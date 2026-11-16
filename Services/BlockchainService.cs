using DiemRenLuyen.Data;
using DiemRenLuyen.Models;

namespace DiemRenLuyen.Services;

// ============================================================
// BlockchainService - Tạo Hash và ghi log giao dịch
// (Việc ký + gửi giao dịch do Ethers.js + MetaMask thực hiện
//  trên Frontend - Backend chỉ: tính hash + lưu TxHash về DB)
// ============================================================

public interface IBlockchainService
{
    /// <summary>
    /// Tính SHA-256 hash dữ liệu cần chốt (C# Backend)
    /// </summary>
    Task<string> TinhDataHash(int phieuId);

    /// <summary>
    /// Gọi sau khi frontend xác nhận TxHash - lưu về DB
    /// </summary>
    Task XacNhanChotDiem(int phieuId, string txHash, string dataHash, string adminAddress);

    /// <summary>
    /// Đối soát tính toàn vẹn - băm lại dữ liệu SQL hiện tại
    /// </summary>
    Task<(bool TrungKhop, string HashHienTai, string HashBlockchain)> KiemTraToanVen(int phieuId);

    /// <summary>
    /// Trả về dữ liệu cần thiết để Frontend gọi Smart Contract
    /// </summary>
    Task<BlockchainCallData> LayDuLieuGoiHopDong(int phieuId);
}

public record BlockchainCallData(
    string Mssv,
    string HocKy,
    string DataHash,
    string ContractAddress,
    string AdminWalletAddress
);

public class BlockchainService : IBlockchainService
{
    private readonly AppDbContext    _db;
    private readonly IHashService    _hashService;
    private readonly IConfiguration  _config;

    public BlockchainService(AppDbContext db, IHashService hashService, IConfiguration config)
    {
        _db          = db;
        _hashService = hashService;
        _config      = config;
    }

    public async Task<string> TinhDataHash(int phieuId)
    {
        var phieu = await _db.PhieuDanhGias.FindAsync(phieuId)
                   ?? throw new KeyNotFoundException($"Không tìm thấy phiếu Id={phieuId}");

        if (phieu.DiemAdminCham == null)
            throw new InvalidOperationException("Admin chưa chấm điểm - không thể tạo hash.");

        var tenFile = phieu.TenFileGoc ?? "khong_co_file";

        return _hashService.TaoDataHash(
            phieu.MSSV,
            phieu.HocKy,
            phieu.DiemAdminCham.Value,
            tenFile
        );
    }

    public async Task XacNhanChotDiem(int phieuId, string txHash, string dataHash, string adminAddress)
    {
        var phieu = await _db.PhieuDanhGias.FindAsync(phieuId)
                   ?? throw new KeyNotFoundException("Phiếu không tồn tại");

        phieu.DaChot      = true;
        phieu.NgayChot    = DateTime.UtcNow;
        phieu.TxHash      = txHash;
        phieu.DataHash    = dataHash;
        phieu.NgayCapNhat = DateTime.UtcNow;

        // Ghi log
        _db.BlockchainLogs.Add(new BlockchainLog
        {
            PhieuId       = phieuId,
            MSSV          = phieu.MSSV,
            HocKy         = phieu.HocKy,
            DataHash      = dataHash,
            TxHash        = txHash,
            AdminAddress  = adminAddress,
            HanhDong      = "LuuBlockchain",
            KetQua        = "ThanhCong",
            NgayThucHien  = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task<(bool TrungKhop, string HashHienTai, string HashBlockchain)>
        KiemTraToanVen(int phieuId)
    {
        var phieu = await _db.PhieuDanhGias.FindAsync(phieuId)
                   ?? throw new KeyNotFoundException("Phiếu không tồn tại");

        if (!phieu.DaChot || string.IsNullOrEmpty(phieu.DataHash))
            throw new InvalidOperationException("Phiếu chưa được chốt lên Blockchain.");

        // Băm lại dữ liệu SQL hiện tại
        var hashHienTai = _hashService.TaoDataHash(
            phieu.MSSV,
            phieu.HocKy,
            phieu.DiemAdminCham ?? 0,
            phieu.TenFileGoc ?? "khong_co_file"
        );

        var hashBlockchain = phieu.DataHash;
        var trungKhop      = string.Equals(hashHienTai, hashBlockchain, StringComparison.OrdinalIgnoreCase);

        // Ghi log kiểm tra
        _db.BlockchainLogs.Add(new BlockchainLog
        {
            PhieuId      = phieuId,
            MSSV         = phieu.MSSV,
            HocKy        = phieu.HocKy,
            DataHash     = hashHienTai,
            AdminAddress = "system",
            HanhDong     = "XacMinh",
            KetQua       = trungKhop ? "ThanhCong" : "ThatBai",
            GhiChu       = trungKhop
                           ? "Dữ liệu toàn vẹn - Hash khớp"
                           : $"CẢNH BÁO: Hash không khớp! DB={hashHienTai} | Chain={hashBlockchain}",
            NgayThucHien = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return (trungKhop, hashHienTai, hashBlockchain);
    }

    public async Task<BlockchainCallData> LayDuLieuGoiHopDong(int phieuId)
    {
        var dataHash = await TinhDataHash(phieuId);
        var phieu    = await _db.PhieuDanhGias.FindAsync(phieuId)!;

        // Đọc từ appsettings.json (KHÔNG hardcode)
        var contractAddress  = _config["Blockchain:ContractAddress"]
            ?? throw new InvalidOperationException("ContractAddress chưa được cấu hình");
        var adminWallet = _config["Blockchain:AdminWalletAddress"]
            ?? throw new InvalidOperationException("AdminWalletAddress chưa được cấu hình");

        return new BlockchainCallData(
            phieu!.MSSV,
            phieu.HocKy,
            dataHash,
            contractAddress,
            adminWallet
        );
    }
}
