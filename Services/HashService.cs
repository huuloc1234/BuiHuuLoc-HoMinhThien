using System.Security.Cryptography;
using System.Text;

namespace DiemRenLuyen.Services;

public interface IHashService
{
    /// <summary>
    /// Tạo chuỗi dữ liệu gốc để băm (MSSV|HocKy|TongDiem|TenFile)
    /// </summary>
    string TaoChuoiDuLieu(string mssv, string hocKy, decimal tongDiem, string tenFile);

    /// <summary>
    /// Băm SHA-256, trả về 64 ký tự hex lowercase
    /// </summary>
    string BamSHA256(string chuoiDuLieu);

    /// <summary>
    /// Tạo hash trực tiếp từ 4 trường dữ liệu
    /// </summary>
    string TaoDataHash(string mssv, string hocKy, decimal tongDiem, string tenFile);
}

public class HashService : IHashService
{
    private const char Separator = '|';

    /// <inheritdoc />
    public string TaoChuoiDuLieu(string mssv, string hocKy, decimal tongDiem, string tenFile)
    {
        // Format: "HTTT2311023|HK1_2024-2025|82.00|BuiHuuLoc_HTTT2311023_HK1.docx"
        return $"{mssv}{Separator}{hocKy}{Separator}{tongDiem:F2}{Separator}{tenFile}";
    }

    /// <inheritdoc />
    public string BamSHA256(string chuoiDuLieu)
    {
        var bytes = Encoding.UTF8.GetBytes(chuoiDuLieu);
        var hash  = SHA256.HashData(bytes);
        // Chuyển thành hex 64 ký tự lowercase
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <inheritdoc />
    public string TaoDataHash(string mssv, string hocKy, decimal tongDiem, string tenFile)
    {
        var chuoi = TaoChuoiDuLieu(mssv, hocKy, tongDiem, tenFile);
        return BamSHA256(chuoi);
    }
}
