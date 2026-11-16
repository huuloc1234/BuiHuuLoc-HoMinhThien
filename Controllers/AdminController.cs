using DiemRenLuyen.Models;
using DiemRenLuyen.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Security.Claims;

namespace DiemRenLuyen.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IPhieuDanhGiaService _phieuService;
    private readonly IBlockchainService   _blockchainService;
    private readonly IFileService         _fileService;
    private readonly IConfiguration       _config;
    private readonly IWebHostEnvironment  _env;

    public AdminController(
        IPhieuDanhGiaService phieuService,
        IBlockchainService blockchainService,
        IFileService fileService,
        IConfiguration config,
        IWebHostEnvironment env)
    {
        _phieuService      = phieuService;
        _blockchainService = blockchainService;
        _fileService       = fileService;
        _config            = config;
        _env               = env;
    }

    private string GetAdminAddress()
    {
        var claimAddr = User.FindFirstValue("diaChiVi");
        return !string.IsNullOrWhiteSpace(claimAddr)
            ? claimAddr
            : (_config["Blockchain:AdminWalletAddress"] ?? string.Empty);
    }

    private decimal ParseScore(string key)
    {
        var raw = Request.Form[key].ToString().Trim();
        if (string.IsNullOrEmpty(raw)) return 0m;
        raw = raw.Replace(',', '.');
        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0m;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string hocKy = "HK1_2024-2025")
    {
        var danhSach = await _phieuService.LayDanhSachTatCa(hocKy);

        var danhSachBiSua = new HashSet<int>();
        foreach (var p in danhSach)
        {
            if (p.DaChot)
            {
                var idObj = p.GetType().GetProperty("Id")?.GetValue(p)
                         ?? p.GetType().GetProperty("PhieuId")?.GetValue(p);

                if (idObj != null && int.TryParse(idObj.ToString(), out int phieuId))
                {
                    try
                    {
                        var (trungKhop, _, _) = await _blockchainService.KiemTraToanVen(phieuId);
                        if (!trungKhop)
                        {
                            danhSachBiSua.Add(phieuId);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        ViewBag.DanhSachBiSua   = danhSachBiSua;
        ViewBag.HocKy           = hocKy;
        ViewBag.ContractAddress = _config["Blockchain:ContractAddress"];
        ViewBag.AdminWallet     = _config["Blockchain:AdminWalletAddress"];
        return View(danhSach);
    }

    [HttpGet("Admin/ChiTiet/{phieuId}")]
    public async Task<IActionResult> ChiTiet(int phieuId)
    {
        var phieu = await _phieuService.LayPhieuTheoId(phieuId);
        ViewBag.ContractAddress = _config["Blockchain:ContractAddress"];
        return View(phieu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChamDiem(int phieuId)
    {
        try
        {
            var muc1 = new ChiTietMuc1
            {
                Admin_TC1_DiemTBHT          = ParseScore("muc1.Admin_TC1_DiemTBHT"),
                Admin_TC2_ChuyenDeKyNang    = ParseScore("muc1.Admin_TC2_ChuyenDeKyNang"),
                Admin_TC3_HoiThaoToaDam     = ParseScore("muc1.Admin_TC3_HoiThaoToaDam"),
                Admin_TC4_ThiHocThuatTruong = ParseScore("muc1.Admin_TC4_ThiHocThuatTruong"),
                Admin_TC5_ThiHocThuatNgoai  = ParseScore("muc1.Admin_TC5_ThiHocThuatNgoai"),
                Admin_TC6_BaoCaoKhoaHocKhoa = ParseScore("muc1.Admin_TC6_BaoCaoKhoaHocKhoa"),
                Admin_TC7_DeTaiNCKHTruong   = ParseScore("muc1.Admin_TC7_DeTaiNCKHTruong"),
                Admin_TC8_BaiBaoKhoaHoc     = ParseScore("muc1.Admin_TC8_BaiBaoKhoaHoc"),
                Admin_TC9_KhoiNghiepTruong  = ParseScore("muc1.Admin_TC9_KhoiNghiepTruong"),
                Admin_TC10_KhoiNghiepNgoai  = ParseScore("muc1.Admin_TC10_KhoiNghiepNgoai"),
                Admin_TC11_ThanhVienCLB     = ParseScore("muc1.Admin_TC11_ThanhVienCLB"),
                Admin_TC12_HoatDongKhac     = ParseScore("muc1.Admin_TC12_HoatDongKhac")
            };

            var muc2 = new ChiTietMuc2
            {
                Admin_TC1_ThaiDoHocTap       = ParseScore("muc2.Admin_TC1_ThaiDoHocTap"),
                Admin_TC2_ChapHanhNoiQuy     = ParseScore("muc2.Admin_TC2_ChapHanhNoiQuy"),
                Admin_TC3_QuyCheThi          = ParseScore("muc2.Admin_TC3_QuyCheThi"),
                Admin_TC4_QuyDinhThuVien     = ParseScore("muc2.Admin_TC4_QuyDinhThuVien"),
                Admin_TC5_QuyDinhPhongHocMay = ParseScore("muc2.Admin_TC5_QuyDinhPhongHocMay"),
                Admin_TC6_DangKyNgoaiTru     = ParseScore("muc2.Admin_TC6_DangKyNgoaiTru"),
                Admin_TC7_MacDongPhuc        = ParseScore("muc2.Admin_TC7_MacDongPhuc"),
                Admin_TC8_SinhHoatLopCVHT    = ParseScore("muc2.Admin_TC8_SinhHoatLopCVHT")
            };

            var muc3 = new ChiTietMuc3
            {
                Admin_TC1_BatBuocKhoaTruong    = ParseScore("muc3.Admin_TC1_BatBuocKhoaTruong"),
                Admin_TC2_DaiHoiChiDoan        = ParseScore("muc3.Admin_TC2_DaiHoiChiDoan"),
                Admin_TC3_BaoCaoChuyenDe       = ParseScore("muc3.Admin_TC3_BaoCaoChuyenDe"),
                Admin_TC4_NgoaiKhoaThiCLB      = ParseScore("muc3.Admin_TC4_NgoaiKhoaThiCLB"),
                Admin_TC5_NgoaiKhoaCapThanhPho = ParseScore("muc3.Admin_TC5_NgoaiKhoaCapThanhPho"),
                Admin_TC6_KetNapDoan           = ParseScore("muc3.Admin_TC6_KetNapDoan"),
                Admin_TC7_KetNapDang           = ParseScore("muc3.Admin_TC7_KetNapDang"),
                Admin_TC8_DieuDongDoanHoi      = ParseScore("muc3.Admin_TC8_DieuDongDoanHoi"),
                Admin_TC9_ThanhVienDoiNhom     = ParseScore("muc3.Admin_TC9_ThanhVienDoiNhom"),
                Admin_TC10_LyLuanChinhTri      = ParseScore("muc3.Admin_TC10_LyLuanChinhTri"),
                Admin_TC11_DenOnDapNghia       = ParseScore("muc3.Admin_TC11_DenOnDapNghia"),
                Admin_TC12_LaoDongTinhNguyen   = ParseScore("muc3.Admin_TC12_LaoDongTinhNguyen"),
                Admin_TC13_KhenThuongPhongTrao = ParseScore("muc3.Admin_TC13_KhenThuongPhongTrao"),
                Admin_TC14_TapTheKhenThuong    = ParseScore("muc3.Admin_TC14_TapTheKhenThuong"),
                Admin_TC15_HoatDongKhac        = ParseScore("muc3.Admin_TC15_HoatDongKhac")
            };

            var muc4 = new ChiTietMuc4
            {
                Admin_TC1_ChapHanhPhapLuat      = ParseScore("muc4.Admin_TC1_ChapHanhPhapLuat"),
                Admin_TC2_HanhViTotSeChia       = ParseScore("muc4.Admin_TC2_HanhViTotSeChia"),
                Admin_TC3_BieuDuongKhenThuongXH = ParseScore("muc4.Admin_TC3_BieuDuongKhenThuongXH"),
                Admin_TC4_GiaoLuuCLBDoiNhom     = ParseScore("muc4.Admin_TC4_GiaoLuuCLBDoiNhom"),
                Admin_TC5_TuVanTuyenSinh        = ParseScore("muc4.Admin_TC5_TuVanTuyenSinh"),
                Admin_TC6_CongTacNhapHoc        = ParseScore("muc4.Admin_TC6_CongTacNhapHoc"),
                Admin_TC7_KhamSucKhoeDauKhoa    = ParseScore("muc4.Admin_TC7_KhamSucKhoeDauKhoa"),
                Admin_TC8_NgayHoiViecLam        = ParseScore("muc4.Admin_TC8_NgayHoiViecLam"),
                Admin_TC9_LeTotNghiep           = ParseScore("muc4.Admin_TC9_LeTotNghiep"),
                Admin_TC10_KiemTraHoSoSV        = ParseScore("muc4.Admin_TC10_KiemTraHoSoSV"),
                Admin_TC11_PhienGiaoDichViecLam = ParseScore("muc4.Admin_TC11_PhienGiaoDichViecLam"),
                Admin_TC12_HienMauTinhNguyen    = ParseScore("muc4.Admin_TC12_HienMauTinhNguyen"),
                Admin_TC13_XuanTinhNguyen       = ParseScore("muc4.Admin_TC13_XuanTinhNguyen"),
                Admin_TC14_MuaHeXanh            = ParseScore("muc4.Admin_TC14_MuaHeXanh"),
                Admin_TC15_ChuNhatXanh          = ParseScore("muc4.Admin_TC15_ChuNhatXanh"),
                Admin_TC16_ThuBayTinhNguyen     = ParseScore("muc4.Admin_TC16_ThuBayTinhNguyen"),
                Admin_TC17_ChaoDonTanSV         = ParseScore("muc4.Admin_TC17_ChaoDonTanSV"),
                Admin_TC18_TrachNhiemXaHoi      = ParseScore("muc4.Admin_TC18_TrachNhiemXaHoi")
            };

            var muc5 = new ChiTietMuc5
            {
                Admin_TC1_TichCucPhongTrao        = ParseScore("muc5.Admin_TC1_TichCucPhongTrao"),
                Admin_TC2_CanBoChiDoanLop         = ParseScore("muc5.Admin_TC2_CanBoChiDoanLop"),
                Admin_TC3_GiaiThuongHocTapNCKH    = ParseScore("muc5.Admin_TC3_GiaiThuongHocTapNCKH"),
                Admin_TC4_BangKhenUBNDTinh        = ParseScore("muc5.Admin_TC4_BangKhenUBNDTinh"),
                Admin_TC5_SV5TotCapTruong         = ParseScore("muc5.Admin_TC5_SV5TotCapTruong"),
                Admin_TC6_SV5TotCapThanhTrungUong = ParseScore("muc5.Admin_TC6_SV5TotCapThanhTrungUong"),
                Admin_TC7_DoanVienUuTu            = ParseScore("muc5.Admin_TC7_DoanVienUuTu"),
                Admin_TC8_GiayKhenTapTheDoan      = ParseScore("muc5.Admin_TC8_GiayKhenTapTheDoan")
            };

            await _phieuService.LuuDiemAdmin(phieuId, muc1, muc2, muc3, muc4, muc5);
            TempData["Success"] = "✅ Đã lưu điểm Admin chấm thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"❌ {ex.Message}";
        }

        return RedirectToAction("ChiTiet", new { phieuId });
    }

    [HttpGet("Admin/LayDuLieuHash/{phieuId}")]
    public async Task<IActionResult> LayDuLieuHash(int phieuId)
    {
        try
        {
            var data = await _blockchainService.LayDuLieuGoiHopDong(phieuId);
            return Json(new
            {
                success         = true,
                mssv            = data.Mssv,
                hocKy           = data.HocKy,
                dataHash        = data.DataHash,
                contractAddress = data.ContractAddress,
                adminWallet     = data.AdminWalletAddress
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> XacNhanChotDiem([FromBody] ChotDiemRequest request)
    {
        try
        {
            await _blockchainService.XacNhanChotDiem(
                request.PhieuId,
                request.TxHash,
                request.DataHash,
                GetAdminAddress()
            );

            return Json(new { success = true, message = "✅ Đã chốt điểm lên Blockchain thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("Admin/KiemTraToanVen/{phieuId}")]
    public async Task<IActionResult> KiemTraToanVen(int phieuId)
    {
        try
        {
            var (trungKhop, hashHienTai, hashBlockchain) =
                await _blockchainService.KiemTraToanVen(phieuId);

            object? truyVet = null;

            if (!trungKhop)
            {
                truyVet = await LayThongTinTruyVetSQL(phieuId);
            }

            return Json(new
            {
                success        = true,
                trungKhop,
                hashHienTai,
                hashBlockchain,
                truyVet,
                thongBao = trungKhop
                    ? "✅ TOÀN VẸN: Dữ liệu không bị thay đổi"
                    : "🚨 CẢNH BÁO: Dữ liệu đã bị can thiệp trái phép!"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private async Task<object?> LayThongTinTruyVetSQL(int phieuId)
    {
        try
        {
            var connStr = _config.GetConnectionString("DefaultConnection")
                       ?? _config.GetSection("ConnectionStrings").GetChildren().FirstOrDefault()?.Value;

            if (string.IsNullOrEmpty(connStr)) return null;

            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            const string sql = @"
                SELECT TOP 1 DiemCu, DiemMoi, TaiKhoanSQL, PhanMemThucHien, TenMayTinh, ThoiGianSua
                FROM NhatKyTruyVetSQL
                WHERE PhieuDanhGiaId = @phieuId
                ORDER BY ThoiGianSua DESC";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@phieuId", phieuId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new
                {
                    diemCu      = reader["DiemCu"]?.ToString() ?? "N/A",
                    diemMoi     = reader["DiemMoi"]?.ToString() ?? "N/A",
                    taiKhoanSQL = reader["TaiKhoanSQL"]?.ToString() ?? "Không xác định",
                    phanMem     = reader["PhanMemThucHien"]?.ToString() ?? "Không xác định",
                    mayTinh     = reader["TenMayTinh"]?.ToString() ?? "Không xác định",
                    thoiGian    = reader["ThoiGianSua"] != DBNull.Value
                                  ? Convert.ToDateTime(reader["ThoiGianSua"]).ToString("HH:mm:ss dd/MM/yyyy")
                                  : ""
                };
            }
        }
        catch
        {
        }

        return null;
    }

    [HttpGet("Admin/TaiFileMinhChung/{phieuId}")]
    public async Task<IActionResult> TaiFileMinhChung(int phieuId)
    {
        var phieu = await _phieuService.LayPhieuTheoId(phieuId);

        if (string.IsNullOrEmpty(phieu.DuongDanFileWord))
        {
            TempData["Error"] = "❌ Sinh viên này chưa nộp file minh chứng.";
            return RedirectToAction("Index");
        }

        var relativePath = phieu.DuongDanFileWord.Replace("\\", "/").TrimStart('/');
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var fullPath = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(fullPath))
        {
            TempData["Error"] = $"❌ Không tìm thấy file vật lý trên server.";
            return RedirectToAction("Index");
        }

        var bytes       = await System.IO.File.ReadAllBytesAsync(fullPath);
        var ext         = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = ext == ".doc" ? "application/msword" : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        var fileName    = phieu.TenFileGoc ?? Path.GetFileName(fullPath);

        return File(bytes, contentType, fileName);
    }

    [HttpGet("Admin/XemFileMinhChung/{phieuId}")]
    public async Task<IActionResult> XemFileMinhChung(int phieuId)
    {
        var phieu = await _phieuService.LayPhieuTheoId(phieuId);
        if (string.IsNullOrEmpty(phieu.DuongDanFileWord)) return NotFound("Sinh viên chưa nộp file minh chứng.");

        var relativePath = phieu.DuongDanFileWord.Replace("\\", "/").TrimStart('/');
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var fullPath = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(fullPath)) return NotFound("Không tìm thấy file vật lý trên server.");

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
    }
}

public record ChotDiemRequest(int PhieuId, string TxHash, string DataHash);