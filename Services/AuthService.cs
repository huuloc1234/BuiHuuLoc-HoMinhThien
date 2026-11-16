using DiemRenLuyen.Data;
using DiemRenLuyen.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DiemRenLuyen.Services;

public interface IAuthService
{
    Task<TaiKhoan?> TimKiemTheoVi(string diaChiVi);
    Task<TaiKhoan?> DangNhapBangEmail(string emailHoacMSSV, string matKhau);
    Task<TaiKhoan> DangKy(string mssv, string hoTen, string lop, string? email, string? matKhau, string? diaChiVi);
    Task LienKetVi(int taiKhoanId, string diaChiVi);
    string TaoJwtToken(TaiKhoan taiKhoan);
    bool XacThucChuKy(string diaChiVi, string message, string signature);
    string HashMatKhau(string rawPassword);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext   _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db     = db;
        _config = config;
    }

    public async Task<TaiKhoan?> TimKiemTheoVi(string diaChiVi)
    {
        if (string.IsNullOrWhiteSpace(diaChiVi)) return null;
        var viNormalized = diaChiVi.Trim().ToLowerInvariant();
        return await _db.TaiKhoans
                        .FirstOrDefaultAsync(t => t.DiaChiVi != null && t.DiaChiVi.ToLower() == viNormalized && t.IsActive);
    }

    public async Task<TaiKhoan?> DangNhapBangEmail(string emailHoacMSSV, string matKhau)
    {
        if (string.IsNullOrWhiteSpace(emailHoacMSSV) || string.IsNullOrWhiteSpace(matKhau))
            return null;

        var term = emailHoacMSSV.Trim().ToLowerInvariant();
        var hashedPass = HashMatKhau(matKhau);

        return await _db.TaiKhoans
            .FirstOrDefaultAsync(t => 
                (t.Email != null && t.Email.ToLower() == term || t.MSSV.ToLower() == term) &&
                t.MatKhau == hashedPass &&
                t.IsActive);
    }

    public async Task<TaiKhoan> DangKy(string mssv, string hoTen, string lop, string? email, string? matKhau, string? diaChiVi)
    {
        mssv = mssv.Trim().ToUpperInvariant();

        if (await _db.TaiKhoans.AnyAsync(t => t.MSSV == mssv))
            throw new InvalidOperationException($"MSSV '{mssv}' đã được đăng ký.");

        if (!string.IsNullOrEmpty(email))
        {
            email = email.Trim().ToLowerInvariant();
            if (await _db.TaiKhoans.AnyAsync(t => t.Email != null && t.Email.ToLower() == email))
                throw new InvalidOperationException($"Email '{email}' đã được sử dụng.");
        }

        if (!string.IsNullOrEmpty(diaChiVi))
        {
            diaChiVi = diaChiVi.Trim().ToLowerInvariant();
            if (await _db.TaiKhoans.AnyAsync(t => t.DiaChiVi != null && t.DiaChiVi.ToLower() == diaChiVi))
                throw new InvalidOperationException("Địa chỉ ví MetaMask này đã được liên kết với tài khoản khác.");
        }

        var taiKhoan = new TaiKhoan
        {
            MSSV      = mssv,
            HoTen     = hoTen.Trim(),
            Lop       = lop.Trim(),
            Email     = string.IsNullOrEmpty(email) ? null : email,
            MatKhau   = string.IsNullOrEmpty(matKhau) ? null : HashMatKhau(matKhau),
            DiaChiVi  = string.IsNullOrEmpty(diaChiVi) ? null : diaChiVi,
            Role      = "SinhVien",
            NgayTao   = DateTime.UtcNow,
            IsActive  = true
        };

        _db.TaiKhoans.Add(taiKhoan);
        await _db.SaveChangesAsync();
        return taiKhoan;
    }

    public async Task LienKetVi(int taiKhoanId, string diaChiVi)
    {
        var vi = diaChiVi.Trim().ToLowerInvariant();
        var existing = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.DiaChiVi != null && t.DiaChiVi.ToLower() == vi && t.Id != taiKhoanId);
        if (existing != null)
            throw new InvalidOperationException("Địa chỉ ví này đã thuộc về tài khoản khác.");

        var tk = await _db.TaiKhoans.FindAsync(taiKhoanId)
                 ?? throw new KeyNotFoundException("Không tìm thấy tài khoản");

        tk.DiaChiVi = vi;
        await _db.SaveChangesAsync();
    }

    public string HashMatKhau(string rawPassword)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawPassword);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public string TaoJwtToken(TaiKhoan taiKhoan)
    {
        var jwtConfig  = _config.GetSection("Jwt");
        var secretKey  = jwtConfig["SecretKey"]!;
        var issuer     = jwtConfig["Issuer"]!;
        var audience   = jwtConfig["Audience"]!;
        var expiryMins = int.Parse(jwtConfig["ExpiryMinutes"] ?? "480");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, taiKhoan.Id.ToString()),
            new Claim(ClaimTypes.Name,           taiKhoan.HoTen),
            new Claim("mssv",                    taiKhoan.MSSV),
            new Claim("diaChiVi",                taiKhoan.DiaChiVi ?? string.Empty),
            new Claim(ClaimTypes.Role,           taiKhoan.Role)
        };

        if (!string.IsNullOrEmpty(taiKhoan.Email))
            claims.Add(new Claim(ClaimTypes.Email, taiKhoan.Email));

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expiryMins),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool XacThucChuKy(string diaChiVi, string message, string signature)
    {
        return !string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(diaChiVi);
    }
}
