using DiemRenLuyen.Models;
using DiemRenLuyen.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace DiemRenLuyen.Controllers;

[Authorize(Roles = "SinhVien")]
public class SinhVienController : Controller
{
    private readonly IPhieuDanhGiaService _phieuService;
    private readonly IFileService          _fileService;
    private readonly IConfiguration        _config;

    public SinhVienController(
        IPhieuDanhGiaService phieuService,
        IFileService fileService,
        IConfiguration config)
    {
        _phieuService = phieuService;
        _fileService  = fileService;
        _config       = config;
    }

    private string GetMSSV() => User.FindFirstValue("mssv") ?? string.Empty;
    private int    GetTaiKhoanId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    private decimal ParseScore(string key)
    {
        var raw = Request.Form[key].ToString().Trim();
        if (string.IsNullOrEmpty(raw)) return 0m;
        raw = raw.Replace(',', '.');
        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0m;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(string hocKy = "HK1_2024-2025")
    {
        var mssv  = GetMSSV();
        var phieu = await _phieuService.LayPhieuTheoMSSV(mssv, hocKy);

        if (phieu == null)
        {
            phieu = await _phieuService.TaoPhieuMoi(GetTaiKhoanId(), mssv, hocKy, "2024-2025");
            phieu = await _phieuService.LayPhieuTheoMSSV(mssv, hocKy);
        }

        ViewBag.HocKy           = hocKy;
        ViewBag.ContractAddress = _config["Blockchain:ContractAddress"];
        return View(phieu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuiKhieuNai(int phieuId, string noiDungKhieuNai)
    {
        try
        {
            var phieu = await _phieuService.LayPhieuTheoId(phieuId);
            if (phieu.MSSV != GetMSSV()) return Forbid();

            if (string.IsNullOrWhiteSpace(noiDungKhieuNai))
            {
                TempData["Error"] = "❌ Vui lòng nhập nội dung cần khiếu nại hoặc phản hồi.";
                return RedirectToAction("Dashboard");
            }

            await _phieuService.GuiKhieuNai(phieuId, noiDungKhieuNai);
            TempData["Success"] = "✅ Đã gửi phản hồi/khiếu nại tới Khoa! Vui lòng chờ Khoa xem xét trước khi chốt Blockchain.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"❌ {ex.Message}";
        }

        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LuuDiem(int phieuId)
    {
        try
        {
            var phieu = await _phieuService.LayPhieuTheoId(phieuId);
            if (phieu.MSSV != GetMSSV())
                return Forbid();

            var muc1 = new ChiTietMuc1
            {
                TC1_DiemTBHT          = ParseScore("muc1.TC1_DiemTBHT"),
                TC2_ChuyenDeKyNang    = ParseScore("muc1.TC2_ChuyenDeKyNang"),
                TC3_HoiThaoToaDam     = ParseScore("muc1.TC3_HoiThaoToaDam"),
                TC4_ThiHocThuatTruong = ParseScore("muc1.TC4_ThiHocThuatTruong"),
                TC5_ThiHocThuatNgoai  = ParseScore("muc1.TC5_ThiHocThuatNgoai"),
                TC6_BaoCaoKhoaHocKhoa = ParseScore("muc1.TC6_BaoCaoKhoaHocKhoa"),
                TC7_DeTaiNCKHTruong   = ParseScore("muc1.TC7_DeTaiNCKHTruong"),
                TC8_BaiBaoKhoaHoc     = ParseScore("muc1.TC8_BaiBaoKhoaHoc"),
                TC9_KhoiNghiepTruong  = ParseScore("muc1.TC9_KhoiNghiepTruong"),
                TC10_KhoiNghiepNgoai  = ParseScore("muc1.TC10_KhoiNghiepNgoai"),
                TC11_ThanhVienCLB     = ParseScore("muc1.TC11_ThanhVienCLB"),
                TC12_HoatDongKhac     = ParseScore("muc1.TC12_HoatDongKhac")
            };

            var muc2 = new ChiTietMuc2
            {
                TC1_ThaiDoHocTap       = ParseScore("muc2.TC1_ThaiDoHocTap"),
                TC2_ChapHanhNoiQuy     = ParseScore("muc2.TC2_ChapHanhNoiQuy"),
                TC3_QuyCheThi          = ParseScore("muc2.TC3_QuyCheThi"),
                TC4_QuyDinhThuVien     = ParseScore("muc2.TC4_QuyDinhThuVien"),
                TC5_QuyDinhPhongHocMay = ParseScore("muc2.TC5_QuyDinhPhongHocMay"),
                TC6_DangKyNgoaiTru     = ParseScore("muc2.TC6_DangKyNgoaiTru"),
                TC7_MacDongPhuc        = ParseScore("muc2.TC7_MacDongPhuc"),
                TC8_SinhHoatLopCVHT    = ParseScore("muc2.TC8_SinhHoatLopCVHT")
            };

            var muc3 = new ChiTietMuc3
            {
                TC1_BatBuocKhoaTruong    = ParseScore("muc3.TC1_BatBuocKhoaTruong"),
                TC2_DaiHoiChiDoan        = ParseScore("muc3.TC2_DaiHoiChiDoan"),
                TC3_BaoCaoChuyenDe       = ParseScore("muc3.TC3_BaoCaoChuyenDe"),
                TC4_NgoaiKhoaThiCLB      = ParseScore("muc3.TC4_NgoaiKhoaThiCLB"),
                TC5_NgoaiKhoaCapThanhPho = ParseScore("muc3.TC5_NgoaiKhoaCapThanhPho"),
                TC6_KetNapDoan           = ParseScore("muc3.TC6_KetNapDoan"),
                TC7_KetNapDang           = ParseScore("muc3.TC7_KetNapDang"),
                TC8_DieuDongDoanHoi      = ParseScore("muc3.TC8_DieuDongDoanHoi"),
                TC9_ThanhVienDoiNhom     = ParseScore("muc3.TC9_ThanhVienDoiNhom"),
                TC10_LyLuanChinhTri      = ParseScore("muc3.TC10_LyLuanChinhTri"),
                TC11_DenOnDapNghia       = ParseScore("muc3.TC11_DenOnDapNghia"),
                TC12_LaoDongTinhNguyen   = ParseScore("muc3.TC12_LaoDongTinhNguyen"),
                TC13_KhenThuongPhongTrao = ParseScore("muc3.TC13_KhenThuongPhongTrao"),
                TC14_TapTheKhenThuong    = ParseScore("muc3.TC14_TapTheKhenThuong"),
                TC15_HoatDongKhac        = ParseScore("muc3.TC15_HoatDongKhac")
            };

            var muc4 = new ChiTietMuc4
            {
                TC1_ChapHanhPhapLuat      = ParseScore("muc4.TC1_ChapHanhPhapLuat"),
                TC2_HanhViTotSeChia       = ParseScore("muc4.TC2_HanhViTotSeChia"),
                TC3_BieuDuongKhenThuongXH = ParseScore("muc4.TC3_BieuDuongKhenThuongXH"),
                TC4_GiaoLuuCLBDoiNhom     = ParseScore("muc4.TC4_GiaoLuuCLBDoiNhom"),
                TC5_TuVanTuyenSinh        = ParseScore("muc4.TC5_TuVanTuyenSinh"),
                TC6_CongTacNhapHoc        = ParseScore("muc4.TC6_CongTacNhapHoc"),
                TC7_KhamSucKhoeDauKhoa    = ParseScore("muc4.TC7_KhamSucKhoeDauKhoa"),
                TC8_NgayHoiViecLam        = ParseScore("muc4.TC8_NgayHoiViecLam"),
                TC9_LeTotNghiep           = ParseScore("muc4.TC9_LeTotNghiep"),
                TC10_KiemTraHoSoSV        = ParseScore("muc4.TC10_KiemTraHoSoSV"),
                TC11_PhienGiaoDichViecLam = ParseScore("muc4.TC11_PhienGiaoDichViecLam"),
                TC12_HienMauTinhNguyen    = ParseScore("muc4.TC12_HienMauTinhNguyen"),
                TC13_XuanTinhNguyen       = ParseScore("muc4.TC13_XuanTinhNguyen"),
                TC14_MuaHeXanh            = ParseScore("muc4.TC14_MuaHeXanh"),
                TC15_ChuNhatXanh          = ParseScore("muc4.TC15_ChuNhatXanh"),
                TC16_ThuBayTinhNguyen     = ParseScore("muc4.TC16_ThuBayTinhNguyen"),
                TC17_ChaoDonTanSV         = ParseScore("muc4.TC17_ChaoDonTanSV"),
                TC18_TrachNhiemXaHoi      = ParseScore("muc4.TC18_TrachNhiemXaHoi")
            };

            var muc5 = new ChiTietMuc5
            {
                TC1_TichCucPhongTrao        = ParseScore("muc5.TC1_TichCucPhongTrao"),
                TC2_CanBoChiDoanLop         = ParseScore("muc5.TC2_CanBoChiDoanLop"),
                TC3_GiaiThuongHocTapNCKH    = ParseScore("muc5.TC3_GiaiThuongHocTapNCKH"),
                TC4_BangKhenUBNDTinh        = ParseScore("muc5.TC4_BangKhenUBNDTinh"),
                TC5_SV5TotCapTruong         = ParseScore("muc5.TC5_SV5TotCapTruong"),
                TC6_SV5TotCapThanhTrungUong = ParseScore("muc5.TC6_SV5TotCapThanhTrungUong"),
                TC7_DoanVienUuTu            = ParseScore("muc5.TC7_DoanVienUuTu"),
                TC8_GiayKhenTapTheDoan      = ParseScore("muc5.TC8_GiayKhenTapTheDoan")
            };

            await _phieuService.LuuDiemSV(phieuId, muc1, muc2, muc3, muc4, muc5);

            TempData["Success"] = "✅ Đã lưu điểm tự chấm thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"❌ {ex.Message}";
        }

        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadFile(int phieuId, IFormFile fileWord)
    {
        try
        {
            var phieu = await _phieuService.LayPhieuTheoId(phieuId);
            if (phieu.MSSV != GetMSSV()) return Forbid();

            if (phieu.DaChot)
            {
                TempData["Error"] = "❌ Phiếu đã chốt, không thể thay đổi file minh chứng.";
                return RedirectToAction("Dashboard");
            }

            var (duongDan, tenFile) = await _fileService.LuuFileWord(fileWord, phieu.MSSV, phieu.HocKy);
            await _phieuService.CapNhatFileWord(phieuId, duongDan, tenFile);

            TempData["Success"] = $"✅ Đã upload file '{tenFile}' thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"❌ Lỗi upload file: {ex.Message}";
        }

        return RedirectToAction("Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> XemKetQua(string hocKy = "HK1_2024-2025")
    {
        var phieu = await _phieuService.LayPhieuTheoMSSV(GetMSSV(), hocKy);
        if (phieu == null) return NotFound("Chưa có phiếu đánh giá cho học kỳ này.");

        ViewBag.ContractAddress = _config["Blockchain:ContractAddress"];
        return View(phieu);
    }
}