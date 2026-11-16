using DiemRenLuyen.Data;
using DiemRenLuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace DiemRenLuyen.Services;

public interface IPhieuDanhGiaService
{
    Task<PhieuDanhGia?> LayPhieuTheoMSSV(string mssv, string hocKy);
    Task<PhieuDanhGia>  LayPhieuTheoId(int phieuId);
    Task<PhieuDanhGia>  TaoPhieuMoi(int taiKhoanId, string mssv, string hocKy, string namHoc);
    Task               LuuDiemSV(int phieuId, ChiTietMuc1 m1, ChiTietMuc2 m2,
                                  ChiTietMuc3 m3, ChiTietMuc4 m4, ChiTietMuc5 m5);
    Task               LuuDiemAdmin(int phieuId, ChiTietMuc1 m1, ChiTietMuc2 m2,
                                    ChiTietMuc3 m3, ChiTietMuc4 m4, ChiTietMuc5 m5);
    Task               GuiKhieuNai(int phieuId, string noiDung);
    Task               CapNhatFileWord(int phieuId, string duongDan, string tenFile);
    Task               ChotDiem(int phieuId, string txHash, string dataHash);
    Task<List<PhieuDanhGia>> LayDanhSachTatCa(string? hocKy = null);
    bool               DaChot(PhieuDanhGia phieu);
}

public class PhieuDanhGiaService : IPhieuDanhGiaService
{
    private readonly AppDbContext _db;

    public PhieuDanhGiaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PhieuDanhGia?> LayPhieuTheoMSSV(string mssv, string hocKy)
        => await _db.PhieuDanhGias
                    .Include(p => p.TaiKhoan)
                    .Include(p => p.Muc1).Include(p => p.Muc2)
                    .Include(p => p.Muc3).Include(p => p.Muc4).Include(p => p.Muc5)
                    .FirstOrDefaultAsync(p => p.MSSV == mssv && p.HocKy == hocKy);

    public async Task<PhieuDanhGia> LayPhieuTheoId(int phieuId)
    {
        var phieu = await _db.PhieuDanhGias
                             .Include(p => p.TaiKhoan)
                             .Include(p => p.Muc1).Include(p => p.Muc2)
                             .Include(p => p.Muc3).Include(p => p.Muc4).Include(p => p.Muc5)
                             .FirstOrDefaultAsync(p => p.Id == phieuId);
        return phieu ?? throw new KeyNotFoundException($"Không tìm thấy phiếu Id={phieuId}");
    }

    public async Task<PhieuDanhGia> TaoPhieuMoi(int taiKhoanId, string mssv, string hocKy, string namHoc)
    {
        var existing = await _db.PhieuDanhGias.FirstOrDefaultAsync(p => p.MSSV == mssv && p.HocKy == hocKy);
        if (existing != null) return existing;

        var phieu = new PhieuDanhGia
        {
            TaiKhoanId    = taiKhoanId,
            MSSV          = mssv,
            HocKy         = hocKy,
            NamHoc        = namHoc,
            DiemSVTuCham  = 0,
            TrangThaiFile = "Chưa nộp"
        };

        _db.PhieuDanhGias.Add(phieu);
        await _db.SaveChangesAsync();

        _db.ChiTietMuc1s.Add(new ChiTietMuc1 { PhieuId = phieu.Id, TongMuc1SV = 0 });
        _db.ChiTietMuc2s.Add(new ChiTietMuc2 { PhieuId = phieu.Id, TongMuc2SV = 0 });
        _db.ChiTietMuc3s.Add(new ChiTietMuc3 { PhieuId = phieu.Id, TongMuc3SV = 0 });
        _db.ChiTietMuc4s.Add(new ChiTietMuc4 { PhieuId = phieu.Id, TongMuc4SV = 0 });
        _db.ChiTietMuc5s.Add(new ChiTietMuc5 { PhieuId = phieu.Id, TongMuc5SV = 0 });
        await _db.SaveChangesAsync();

        // Ép cứng toàn bộ cột về 0 trong SQL để tránh EF Core HasDefaultValue(5)/(10)
        await _db.Database.ExecuteSqlRawAsync(@"
            UPDATE ChiTietMuc2
            SET TC1_ThaiDoHocTap = 0, TC2_ChapHanhNoiQuy = 0, TC3_QuyCheThi = 0,
                TC4_QuyDinhThuVien = 0, TC5_QuyDinhPhongHocMay = 0, TC6_DangKyNgoaiTru = 0,
                TC7_MacDongPhuc = 0, TC8_SinhHoatLopCVHT = 0, TongMuc2SV = 0
            WHERE PhieuId = {0};

            UPDATE ChiTietMuc4
            SET TC1_ChapHanhPhapLuat = 0, TongMuc4SV = 0
            WHERE PhieuId = {0};", phieu.Id);

        return phieu;
    }

    public async Task LuuDiemSV(int phieuId, ChiTietMuc1 m1, ChiTietMuc2 m2,
                                  ChiTietMuc3 m3, ChiTietMuc4 m4, ChiTietMuc5 m5)
    {
        var phieu = await LayPhieuTheoId(phieuId);
        if (phieu.DaChot) throw new InvalidOperationException("Phiếu đã chốt trên Blockchain, không thể chỉnh sửa.");

        if (phieu.Muc1 == null) { phieu.Muc1 = new ChiTietMuc1 { PhieuId = phieuId }; _db.ChiTietMuc1s.Add(phieu.Muc1); }
        if (phieu.Muc2 == null) { phieu.Muc2 = new ChiTietMuc2 { PhieuId = phieuId }; _db.ChiTietMuc2s.Add(phieu.Muc2); }
        if (phieu.Muc3 == null) { phieu.Muc3 = new ChiTietMuc3 { PhieuId = phieuId }; _db.ChiTietMuc3s.Add(phieu.Muc3); }
        if (phieu.Muc4 == null) { phieu.Muc4 = new ChiTietMuc4 { PhieuId = phieuId }; _db.ChiTietMuc4s.Add(phieu.Muc4); }
        if (phieu.Muc5 == null) { phieu.Muc5 = new ChiTietMuc5 { PhieuId = phieuId }; _db.ChiTietMuc5s.Add(phieu.Muc5); }

        CopyMuc1SV(m1, phieu.Muc1);
        CopyMuc2SV(m2, phieu.Muc2);
        CopyMuc3SV(m3, phieu.Muc3);
        CopyMuc4SV(m4, phieu.Muc4);
        CopyMuc5SV(m5, phieu.Muc5);

        phieu.DiemSVTuCham = phieu.Muc1.TongMuc1SV + phieu.Muc2.TongMuc2SV +
                             phieu.Muc3.TongMuc3SV + phieu.Muc4.TongMuc4SV +
                             phieu.Muc5.TongMuc5SV;
        phieu.NgayCapNhat  = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task LuuDiemAdmin(int phieuId, ChiTietMuc1 m1, ChiTietMuc2 m2,
                                    ChiTietMuc3 m3, ChiTietMuc4 m4, ChiTietMuc5 m5)
    {
        var phieu = await LayPhieuTheoId(phieuId);
        if (phieu.DaChot) throw new InvalidOperationException("Phiếu đã chốt trên Blockchain, không thể chỉnh sửa.");

        if (phieu.Muc1 == null) { phieu.Muc1 = new ChiTietMuc1 { PhieuId = phieuId }; _db.ChiTietMuc1s.Add(phieu.Muc1); }
        if (phieu.Muc2 == null) { phieu.Muc2 = new ChiTietMuc2 { PhieuId = phieuId }; _db.ChiTietMuc2s.Add(phieu.Muc2); }
        if (phieu.Muc3 == null) { phieu.Muc3 = new ChiTietMuc3 { PhieuId = phieuId }; _db.ChiTietMuc3s.Add(phieu.Muc3); }
        if (phieu.Muc4 == null) { phieu.Muc4 = new ChiTietMuc4 { PhieuId = phieuId }; _db.ChiTietMuc4s.Add(phieu.Muc4); }
        if (phieu.Muc5 == null) { phieu.Muc5 = new ChiTietMuc5 { PhieuId = phieuId }; _db.ChiTietMuc5s.Add(phieu.Muc5); }

        CopyMuc1Admin(m1, phieu.Muc1);
        CopyMuc2Admin(m2, phieu.Muc2);
        CopyMuc3Admin(m3, phieu.Muc3);
        CopyMuc4Admin(m4, phieu.Muc4);
        CopyMuc5Admin(m5, phieu.Muc5);

        phieu.DiemAdminCham = (phieu.Muc1.TongMuc1Admin ?? 0) + (phieu.Muc2.TongMuc2Admin ?? 0) +
                              (phieu.Muc3.TongMuc3Admin ?? 0) + (phieu.Muc4.TongMuc4Admin ?? 0) +
                              (phieu.Muc5.TongMuc5Admin ?? 0);

        if (phieu.TrangThaiKhieuNai == "ChoXuLy")
            phieu.TrangThaiKhieuNai = "DaXuLy";

        phieu.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task GuiKhieuNai(int phieuId, string noiDung)
    {
        var phieu = await LayPhieuTheoId(phieuId);
        if (phieu.DaChot) throw new InvalidOperationException("Phiếu đã chốt lên Blockchain, không thể gửi khiếu nại.");
        if (phieu.DiemAdminCham == null) throw new InvalidOperationException("Khoa chưa chấm điểm cho phiếu này.");

        phieu.NoiDungKhieuNai   = noiDung.Trim();
        phieu.TrangThaiKhieuNai = "ChoXuLy";
        phieu.NgayKhieuNai      = DateTime.UtcNow;
        phieu.NgayCapNhat       = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task CapNhatFileWord(int phieuId, string duongDan, string tenFile)
    {
        var phieu = await LayPhieuTheoId(phieuId);
        if (phieu.DaChot) throw new InvalidOperationException("Phiếu đã chốt, không thể thay đổi file minh chứng.");

        phieu.DuongDanFileWord = duongDan;
        phieu.TenFileGoc       = tenFile;
        phieu.TrangThaiFile    = "Đã nộp";
        phieu.NgayCapNhat      = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task ChotDiem(int phieuId, string txHash, string dataHash)
    {
        var phieu = await LayPhieuTheoId(phieuId);
        phieu.DaChot      = true;
        phieu.NgayChot    = DateTime.UtcNow;
        phieu.TxHash      = txHash;
        phieu.DataHash    = dataHash;
        phieu.NgayCapNhat = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<PhieuDanhGia>> LayDanhSachTatCa(string? hocKy = null)
    {
        var query = _db.PhieuDanhGias
                       .Include(p => p.TaiKhoan)
                       .Include(p => p.Muc1).Include(p => p.Muc2)
                       .Include(p => p.Muc3).Include(p => p.Muc4).Include(p => p.Muc5)
                       .AsQueryable();

        if (!string.IsNullOrEmpty(hocKy)) query = query.Where(p => p.HocKy == hocKy);
        return await query.OrderBy(p => p.MSSV).ToListAsync();
    }

    public bool DaChot(PhieuDanhGia phieu) => phieu.DaChot;

    // --- COPY ĐIỂM SINH VIÊN ---
    private static void CopyMuc1SV(ChiTietMuc1 src, ChiTietMuc1 dst)
    {
        dst.TC1_DiemTBHT          = src.TC1_DiemTBHT;
        dst.TC2_ChuyenDeKyNang    = src.TC2_ChuyenDeKyNang;
        dst.TC3_HoiThaoToaDam     = src.TC3_HoiThaoToaDam;
        dst.TC4_ThiHocThuatTruong = src.TC4_ThiHocThuatTruong;
        dst.TC5_ThiHocThuatNgoai  = src.TC5_ThiHocThuatNgoai;
        dst.TC6_BaoCaoKhoaHocKhoa = src.TC6_BaoCaoKhoaHocKhoa;
        dst.TC7_DeTaiNCKHTruong   = src.TC7_DeTaiNCKHTruong;
        dst.TC8_BaiBaoKhoaHoc     = src.TC8_BaiBaoKhoaHoc;
        dst.TC9_KhoiNghiepTruong  = src.TC9_KhoiNghiepTruong;
        dst.TC10_KhoiNghiepNgoai  = src.TC10_KhoiNghiepNgoai;
        dst.TC11_ThanhVienCLB     = src.TC11_ThanhVienCLB;
        dst.TC12_HoatDongKhac     = src.TC12_HoatDongKhac;

        var tong = dst.TC1_DiemTBHT + dst.TC2_ChuyenDeKyNang + dst.TC3_HoiThaoToaDam +
                   dst.TC4_ThiHocThuatTruong + dst.TC5_ThiHocThuatNgoai + dst.TC6_BaoCaoKhoaHocKhoa +
                   dst.TC7_DeTaiNCKHTruong + dst.TC8_BaiBaoKhoaHoc + dst.TC9_KhoiNghiepTruong +
                   dst.TC10_KhoiNghiepNgoai + dst.TC11_ThanhVienCLB + dst.TC12_HoatDongKhac;
        dst.TongMuc1SV = Math.Min(20m, Math.Max(0m, tong));
    }

    private static void CopyMuc2SV(ChiTietMuc2 src, ChiTietMuc2 dst)
    {
        dst.TC1_ThaiDoHocTap       = src.TC1_ThaiDoHocTap;
        dst.TC2_ChapHanhNoiQuy     = src.TC2_ChapHanhNoiQuy;
        dst.TC3_QuyCheThi          = src.TC3_QuyCheThi;
        dst.TC4_QuyDinhThuVien     = src.TC4_QuyDinhThuVien;
        dst.TC5_QuyDinhPhongHocMay = src.TC5_QuyDinhPhongHocMay;
        dst.TC6_DangKyNgoaiTru     = src.TC6_DangKyNgoaiTru;
        dst.TC7_MacDongPhuc        = src.TC7_MacDongPhuc;
        dst.TC8_SinhHoatLopCVHT    = src.TC8_SinhHoatLopCVHT;

        var tong = dst.TC1_ThaiDoHocTap + dst.TC2_ChapHanhNoiQuy + dst.TC3_QuyCheThi +
                   dst.TC4_QuyDinhThuVien + dst.TC5_QuyDinhPhongHocMay + dst.TC6_DangKyNgoaiTru +
                   dst.TC7_MacDongPhuc + dst.TC8_SinhHoatLopCVHT;
        dst.TongMuc2SV = Math.Min(25m, Math.Max(0m, tong));
    }

    private static void CopyMuc3SV(ChiTietMuc3 src, ChiTietMuc3 dst)
    {
        dst.TC1_BatBuocKhoaTruong    = src.TC1_BatBuocKhoaTruong;
        dst.TC2_DaiHoiChiDoan        = src.TC2_DaiHoiChiDoan;
        dst.TC3_BaoCaoChuyenDe       = src.TC3_BaoCaoChuyenDe;
        dst.TC4_NgoaiKhoaThiCLB      = src.TC4_NgoaiKhoaThiCLB;
        dst.TC5_NgoaiKhoaCapThanhPho = src.TC5_NgoaiKhoaCapThanhPho;
        dst.TC6_KetNapDoan           = src.TC6_KetNapDoan;
        dst.TC7_KetNapDang           = src.TC7_KetNapDang;
        dst.TC8_DieuDongDoanHoi      = src.TC8_DieuDongDoanHoi;
        dst.TC9_ThanhVienDoiNhom     = src.TC9_ThanhVienDoiNhom;
        dst.TC10_LyLuanChinhTri      = src.TC10_LyLuanChinhTri;
        dst.TC11_DenOnDapNghia       = src.TC11_DenOnDapNghia;
        dst.TC12_LaoDongTinhNguyen   = src.TC12_LaoDongTinhNguyen;
        dst.TC13_KhenThuongPhongTrao = src.TC13_KhenThuongPhongTrao;
        dst.TC14_TapTheKhenThuong    = src.TC14_TapTheKhenThuong;
        dst.TC15_HoatDongKhac        = src.TC15_HoatDongKhac;

        var tong = dst.TC1_BatBuocKhoaTruong + dst.TC2_DaiHoiChiDoan + dst.TC3_BaoCaoChuyenDe +
                   dst.TC4_NgoaiKhoaThiCLB + dst.TC5_NgoaiKhoaCapThanhPho + dst.TC6_KetNapDoan +
                   dst.TC7_KetNapDang + dst.TC8_DieuDongDoanHoi + dst.TC9_ThanhVienDoiNhom +
                   dst.TC10_LyLuanChinhTri + dst.TC11_DenOnDapNghia + dst.TC12_LaoDongTinhNguyen +
                   dst.TC13_KhenThuongPhongTrao + dst.TC14_TapTheKhenThuong + dst.TC15_HoatDongKhac;
        dst.TongMuc3SV = Math.Min(20m, Math.Max(0m, tong));
    }

    private static void CopyMuc4SV(ChiTietMuc4 src, ChiTietMuc4 dst)
    {
        dst.TC1_ChapHanhPhapLuat      = src.TC1_ChapHanhPhapLuat;
        dst.TC2_HanhViTotSeChia       = src.TC2_HanhViTotSeChia;
        dst.TC3_BieuDuongKhenThuongXH = src.TC3_BieuDuongKhenThuongXH;
        dst.TC4_GiaoLuuCLBDoiNhom     = src.TC4_GiaoLuuCLBDoiNhom;
        dst.TC5_TuVanTuyenSinh        = src.TC5_TuVanTuyenSinh;
        dst.TC6_CongTacNhapHoc        = src.TC6_CongTacNhapHoc;
        dst.TC7_KhamSucKhoeDauKhoa    = src.TC7_KhamSucKhoeDauKhoa;
        dst.TC8_NgayHoiViecLam        = src.TC8_NgayHoiViecLam;
        dst.TC9_LeTotNghiep           = src.TC9_LeTotNghiep;
        dst.TC10_KiemTraHoSoSV        = src.TC10_KiemTraHoSoSV;
        dst.TC11_PhienGiaoDichViecLam = src.TC11_PhienGiaoDichViecLam;
        dst.TC12_HienMauTinhNguyen    = src.TC12_HienMauTinhNguyen;
        dst.TC13_XuanTinhNguyen       = src.TC13_XuanTinhNguyen;
        dst.TC14_MuaHeXanh            = src.TC14_MuaHeXanh;
        dst.TC15_ChuNhatXanh          = src.TC15_ChuNhatXanh;
        dst.TC16_ThuBayTinhNguyen     = src.TC16_ThuBayTinhNguyen;
        dst.TC17_ChaoDonTanSV         = src.TC17_ChaoDonTanSV;
        dst.TC18_TrachNhiemXaHoi      = src.TC18_TrachNhiemXaHoi;

        var tong = dst.TC1_ChapHanhPhapLuat + dst.TC2_HanhViTotSeChia + dst.TC3_BieuDuongKhenThuongXH +
                   dst.TC4_GiaoLuuCLBDoiNhom + dst.TC5_TuVanTuyenSinh + dst.TC6_CongTacNhapHoc +
                   dst.TC7_KhamSucKhoeDauKhoa + dst.TC8_NgayHoiViecLam + dst.TC9_LeTotNghiep +
                   dst.TC10_KiemTraHoSoSV + dst.TC11_PhienGiaoDichViecLam + dst.TC12_HienMauTinhNguyen +
                   dst.TC13_XuanTinhNguyen + dst.TC14_MuaHeXanh + dst.TC15_ChuNhatXanh +
                   dst.TC16_ThuBayTinhNguyen + dst.TC17_ChaoDonTanSV + dst.TC18_TrachNhiemXaHoi;
        dst.TongMuc4SV = Math.Min(25m, Math.Max(0m, tong));
    }

    private static void CopyMuc5SV(ChiTietMuc5 src, ChiTietMuc5 dst)
    {
        dst.TC1_TichCucPhongTrao        = src.TC1_TichCucPhongTrao;
        dst.TC2_CanBoChiDoanLop         = src.TC2_CanBoChiDoanLop;
        dst.TC3_GiaiThuongHocTapNCKH    = src.TC3_GiaiThuongHocTapNCKH;
        dst.TC4_BangKhenUBNDTinh        = src.TC4_BangKhenUBNDTinh;
        dst.TC5_SV5TotCapTruong         = src.TC5_SV5TotCapTruong;
        dst.TC6_SV5TotCapThanhTrungUong = src.TC6_SV5TotCapThanhTrungUong;
        dst.TC7_DoanVienUuTu            = src.TC7_DoanVienUuTu;
        dst.TC8_GiayKhenTapTheDoan      = src.TC8_GiayKhenTapTheDoan;

        var tong = dst.TC1_TichCucPhongTrao + dst.TC2_CanBoChiDoanLop + dst.TC3_GiaiThuongHocTapNCKH +
                   dst.TC4_BangKhenUBNDTinh + dst.TC5_SV5TotCapTruong + dst.TC6_SV5TotCapThanhTrungUong +
                   dst.TC7_DoanVienUuTu + dst.TC8_GiayKhenTapTheDoan;
        dst.TongMuc5SV = Math.Min(10m, Math.Max(0m, tong));
    }

    // --- COPY ĐIỂM KHOA / ADMIN ---
    private static void CopyMuc1Admin(ChiTietMuc1 src, ChiTietMuc1 dst)
    {
        dst.Admin_TC1_DiemTBHT          = src.Admin_TC1_DiemTBHT ?? 0;
        dst.Admin_TC2_ChuyenDeKyNang    = src.Admin_TC2_ChuyenDeKyNang ?? 0;
        dst.Admin_TC3_HoiThaoToaDam     = src.Admin_TC3_HoiThaoToaDam ?? 0;
        dst.Admin_TC4_ThiHocThuatTruong = src.Admin_TC4_ThiHocThuatTruong ?? 0;
        dst.Admin_TC5_ThiHocThuatNgoai  = src.Admin_TC5_ThiHocThuatNgoai ?? 0;
        dst.Admin_TC6_BaoCaoKhoaHocKhoa = src.Admin_TC6_BaoCaoKhoaHocKhoa ?? 0;
        dst.Admin_TC7_DeTaiNCKHTruong   = src.Admin_TC7_DeTaiNCKHTruong ?? 0;
        dst.Admin_TC8_BaiBaoKhoaHoc     = src.Admin_TC8_BaiBaoKhoaHoc ?? 0;
        dst.Admin_TC9_KhoiNghiepTruong  = src.Admin_TC9_KhoiNghiepTruong ?? 0;
        dst.Admin_TC10_KhoiNghiepNgoai  = src.Admin_TC10_KhoiNghiepNgoai ?? 0;
        dst.Admin_TC11_ThanhVienCLB     = src.Admin_TC11_ThanhVienCLB ?? 0;
        dst.Admin_TC12_HoatDongKhac     = src.Admin_TC12_HoatDongKhac ?? 0;

        var tong = (dst.Admin_TC1_DiemTBHT ?? 0) + (dst.Admin_TC2_ChuyenDeKyNang ?? 0) + (dst.Admin_TC3_HoiThaoToaDam ?? 0) +
                   (dst.Admin_TC4_ThiHocThuatTruong ?? 0) + (dst.Admin_TC5_ThiHocThuatNgoai ?? 0) + (dst.Admin_TC6_BaoCaoKhoaHocKhoa ?? 0) +
                   (dst.Admin_TC7_DeTaiNCKHTruong ?? 0) + (dst.Admin_TC8_BaiBaoKhoaHoc ?? 0) + (dst.Admin_TC9_KhoiNghiepTruong ?? 0) +
                   (dst.Admin_TC10_KhoiNghiepNgoai ?? 0) + (dst.Admin_TC11_ThanhVienCLB ?? 0) + (dst.Admin_TC12_HoatDongKhac ?? 0);
        dst.TongMuc1Admin = Math.Min(20m, Math.Max(0m, tong));
    }

    private static void CopyMuc2Admin(ChiTietMuc2 src, ChiTietMuc2 dst)
    {
        dst.Admin_TC1_ThaiDoHocTap       = src.Admin_TC1_ThaiDoHocTap ?? 0;
        dst.Admin_TC2_ChapHanhNoiQuy     = src.Admin_TC2_ChapHanhNoiQuy ?? 0;
        dst.Admin_TC3_QuyCheThi          = src.Admin_TC3_QuyCheThi ?? 0;
        dst.Admin_TC4_QuyDinhThuVien     = src.Admin_TC4_QuyDinhThuVien ?? 0;
        dst.Admin_TC5_QuyDinhPhongHocMay = src.Admin_TC5_QuyDinhPhongHocMay ?? 0;
        dst.Admin_TC6_DangKyNgoaiTru     = src.Admin_TC6_DangKyNgoaiTru ?? 0;
        dst.Admin_TC7_MacDongPhuc        = src.Admin_TC7_MacDongPhuc ?? 0;
        dst.Admin_TC8_SinhHoatLopCVHT    = src.Admin_TC8_SinhHoatLopCVHT ?? 0;

        var tong = (dst.Admin_TC1_ThaiDoHocTap ?? 0) + (dst.Admin_TC2_ChapHanhNoiQuy ?? 0) + (dst.Admin_TC3_QuyCheThi ?? 0) +
                   (dst.Admin_TC4_QuyDinhThuVien ?? 0) + (dst.Admin_TC5_QuyDinhPhongHocMay ?? 0) + (dst.Admin_TC6_DangKyNgoaiTru ?? 0) +
                   (dst.Admin_TC7_MacDongPhuc ?? 0) + (dst.Admin_TC8_SinhHoatLopCVHT ?? 0);
        dst.TongMuc2Admin = Math.Min(25m, Math.Max(0m, tong));
    }

    private static void CopyMuc3Admin(ChiTietMuc3 src, ChiTietMuc3 dst)
    {
        dst.Admin_TC1_BatBuocKhoaTruong    = src.Admin_TC1_BatBuocKhoaTruong ?? 0;
        dst.Admin_TC2_DaiHoiChiDoan        = src.Admin_TC2_DaiHoiChiDoan ?? 0;
        dst.Admin_TC3_BaoCaoChuyenDe       = src.Admin_TC3_BaoCaoChuyenDe ?? 0;
        dst.Admin_TC4_NgoaiKhoaThiCLB      = src.Admin_TC4_NgoaiKhoaThiCLB ?? 0;
        dst.Admin_TC5_NgoaiKhoaCapThanhPho = src.Admin_TC5_NgoaiKhoaCapThanhPho ?? 0;
        dst.Admin_TC6_KetNapDoan           = src.Admin_TC6_KetNapDoan ?? 0;
        dst.Admin_TC7_KetNapDang           = src.Admin_TC7_KetNapDang ?? 0;
        dst.Admin_TC8_DieuDongDoanHoi      = src.Admin_TC8_DieuDongDoanHoi ?? 0;
        dst.Admin_TC9_ThanhVienDoiNhom     = src.Admin_TC9_ThanhVienDoiNhom ?? 0;
        dst.Admin_TC10_LyLuanChinhTri      = src.Admin_TC10_LyLuanChinhTri ?? 0;
        dst.Admin_TC11_DenOnDapNghia       = src.Admin_TC11_DenOnDapNghia ?? 0;
        dst.Admin_TC12_LaoDongTinhNguyen   = src.Admin_TC12_LaoDongTinhNguyen ?? 0;
        dst.Admin_TC13_KhenThuongPhongTrao = src.Admin_TC13_KhenThuongPhongTrao ?? 0;
        dst.Admin_TC14_TapTheKhenThuong    = src.Admin_TC14_TapTheKhenThuong ?? 0;
        dst.Admin_TC15_HoatDongKhac        = src.Admin_TC15_HoatDongKhac ?? 0;

        var tong = (dst.Admin_TC1_BatBuocKhoaTruong ?? 0) + (dst.Admin_TC2_DaiHoiChiDoan ?? 0) + (dst.Admin_TC3_BaoCaoChuyenDe ?? 0) +
                   (dst.Admin_TC4_NgoaiKhoaThiCLB ?? 0) + (dst.Admin_TC5_NgoaiKhoaCapThanhPho ?? 0) + (dst.Admin_TC6_KetNapDoan ?? 0) +
                   (dst.Admin_TC7_KetNapDang ?? 0) + (dst.Admin_TC8_DieuDongDoanHoi ?? 0) + (dst.Admin_TC9_ThanhVienDoiNhom ?? 0) +
                   (dst.Admin_TC10_LyLuanChinhTri ?? 0) + (dst.Admin_TC11_DenOnDapNghia ?? 0) + (dst.Admin_TC12_LaoDongTinhNguyen ?? 0) +
                   (dst.Admin_TC13_KhenThuongPhongTrao ?? 0) + (dst.Admin_TC14_TapTheKhenThuong ?? 0) + (dst.Admin_TC15_HoatDongKhac ?? 0);
        dst.TongMuc3Admin = Math.Min(20m, Math.Max(0m, tong));
    }

    private static void CopyMuc4Admin(ChiTietMuc4 src, ChiTietMuc4 dst)
    {
        dst.Admin_TC1_ChapHanhPhapLuat      = src.Admin_TC1_ChapHanhPhapLuat ?? 0;
        dst.Admin_TC2_HanhViTotSeChia       = src.Admin_TC2_HanhViTotSeChia ?? 0;
        dst.Admin_TC3_BieuDuongKhenThuongXH = src.Admin_TC3_BieuDuongKhenThuongXH ?? 0;
        dst.Admin_TC4_GiaoLuuCLBDoiNhom     = src.Admin_TC4_GiaoLuuCLBDoiNhom ?? 0;
        dst.Admin_TC5_TuVanTuyenSinh        = src.Admin_TC5_TuVanTuyenSinh ?? 0;
        dst.Admin_TC6_CongTacNhapHoc        = src.Admin_TC6_CongTacNhapHoc ?? 0;
        dst.Admin_TC7_KhamSucKhoeDauKhoa    = src.Admin_TC7_KhamSucKhoeDauKhoa ?? 0;
        dst.Admin_TC8_NgayHoiViecLam        = src.Admin_TC8_NgayHoiViecLam ?? 0;
        dst.Admin_TC9_LeTotNghiep           = src.Admin_TC9_LeTotNghiep ?? 0;
        dst.Admin_TC10_KiemTraHoSoSV        = src.Admin_TC10_KiemTraHoSoSV ?? 0;
        dst.Admin_TC11_PhienGiaoDichViecLam = src.Admin_TC11_PhienGiaoDichViecLam ?? 0;
        dst.Admin_TC12_HienMauTinhNguyen    = src.Admin_TC12_HienMauTinhNguyen ?? 0;
        dst.Admin_TC13_XuanTinhNguyen       = src.Admin_TC13_XuanTinhNguyen ?? 0;
        dst.Admin_TC14_MuaHeXanh            = src.Admin_TC14_MuaHeXanh ?? 0;
        dst.Admin_TC15_ChuNhatXanh          = src.Admin_TC15_ChuNhatXanh ?? 0;
        dst.Admin_TC16_ThuBayTinhNguyen     = src.Admin_TC16_ThuBayTinhNguyen ?? 0;
        dst.Admin_TC17_ChaoDonTanSV         = src.Admin_TC17_ChaoDonTanSV ?? 0;
        dst.Admin_TC18_TrachNhiemXaHoi      = src.Admin_TC18_TrachNhiemXaHoi ?? 0;

        var tong = (dst.Admin_TC1_ChapHanhPhapLuat ?? 0) + (dst.Admin_TC2_HanhViTotSeChia ?? 0) + (dst.Admin_TC3_BieuDuongKhenThuongXH ?? 0) +
                   (dst.Admin_TC4_GiaoLuuCLBDoiNhom ?? 0) + (dst.Admin_TC5_TuVanTuyenSinh ?? 0) + (dst.Admin_TC6_CongTacNhapHoc ?? 0) +
                   (dst.Admin_TC7_KhamSucKhoeDauKhoa ?? 0) + (dst.Admin_TC8_NgayHoiViecLam ?? 0) + (dst.Admin_TC9_LeTotNghiep ?? 0) +
                   (dst.Admin_TC10_KiemTraHoSoSV ?? 0) + (dst.Admin_TC11_PhienGiaoDichViecLam ?? 0) + (dst.Admin_TC12_HienMauTinhNguyen ?? 0) +
                   (dst.Admin_TC13_XuanTinhNguyen ?? 0) + (dst.Admin_TC14_MuaHeXanh ?? 0) + (dst.Admin_TC15_ChuNhatXanh ?? 0) +
                   (dst.Admin_TC16_ThuBayTinhNguyen ?? 0) + (dst.Admin_TC17_ChaoDonTanSV ?? 0) + (dst.Admin_TC18_TrachNhiemXaHoi ?? 0);
        dst.TongMuc4Admin = Math.Min(25m, Math.Max(0m, tong));
    }

    private static void CopyMuc5Admin(ChiTietMuc5 src, ChiTietMuc5 dst)
    {
        dst.Admin_TC1_TichCucPhongTrao        = src.Admin_TC1_TichCucPhongTrao ?? 0;
        dst.Admin_TC2_CanBoChiDoanLop         = src.Admin_TC2_CanBoChiDoanLop ?? 0;
        dst.Admin_TC3_GiaiThuongHocTapNCKH    = src.Admin_TC3_GiaiThuongHocTapNCKH ?? 0;
        dst.Admin_TC4_BangKhenUBNDTinh        = src.Admin_TC4_BangKhenUBNDTinh ?? 0;
        dst.Admin_TC5_SV5TotCapTruong         = src.Admin_TC5_SV5TotCapTruong ?? 0;
        dst.Admin_TC6_SV5TotCapThanhTrungUong = src.Admin_TC6_SV5TotCapThanhTrungUong ?? 0;
        dst.Admin_TC7_DoanVienUuTu            = src.Admin_TC7_DoanVienUuTu ?? 0;
        dst.Admin_TC8_GiayKhenTapTheDoan      = src.Admin_TC8_GiayKhenTapTheDoan ?? 0;

        var tong = (dst.Admin_TC1_TichCucPhongTrao ?? 0) + (dst.Admin_TC2_CanBoChiDoanLop ?? 0) + (dst.Admin_TC3_GiaiThuongHocTapNCKH ?? 0) +
                   (dst.Admin_TC4_BangKhenUBNDTinh ?? 0) + (dst.Admin_TC5_SV5TotCapTruong ?? 0) + (dst.Admin_TC6_SV5TotCapThanhTrungUong ?? 0) +
                   (dst.Admin_TC7_DoanVienUuTu ?? 0) + (dst.Admin_TC8_GiayKhenTapTheDoan ?? 0);
        dst.TongMuc5Admin = Math.Min(10m, Math.Max(0m, tong));
    }
}