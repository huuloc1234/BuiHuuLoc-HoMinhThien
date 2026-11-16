using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiemRenLuyen.Models;

[Table("TaiKhoan")]
public class TaiKhoan
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string MSSV { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Lop { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? MatKhau { get; set; }

    [MaxLength(20)]
    public string? KhoaHoc { get; set; }

    [MaxLength(20)]
    public string? NgaySinh { get; set; }

    [MaxLength(20)]
    public string? SoDienThoai { get; set; }

    [MaxLength(42)]
    public string? DiaChiVi { get; set; }

    [Required, MaxLength(10)]
    public string Role { get; set; } = "SinhVien";

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<PhieuDanhGia> PhieuDanhGias { get; set; } = new List<PhieuDanhGia>();
}

[Table("PhieuDanhGia")]
public class PhieuDanhGia
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("TaiKhoan")]
    public int TaiKhoanId { get; set; }

    [Required, MaxLength(20)]
    public string MSSV { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string HocKy { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string NamHoc { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    public decimal DiemSVTuCham { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DiemLopCham { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DiemAdminCham { get; set; }

    [MaxLength(500)]
    public string? DuongDanFileWord { get; set; }

    [MaxLength(255)]
    public string? TenFileGoc { get; set; }

    [MaxLength(20)]
    public string TrangThaiFile { get; set; } = "Chưa nộp";

    public bool DaChot { get; set; } = false;
    public DateTime? NgayChot { get; set; }

    [MaxLength(100)]
    public string? TxHash { get; set; }

    [MaxLength(64)]
    public string? DataHash { get; set; }

    [MaxLength(1000)]
    public string? NoiDungKhieuNai { get; set; }

    [MaxLength(50)]
    public string? TrangThaiKhieuNai { get; set; } // "ChoXuLy" | "DaXuLy" | null

    public DateTime? NgayKhieuNai { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

    public TaiKhoan? TaiKhoan { get; set; }
    public ChiTietMuc1? Muc1 { get; set; }
    public ChiTietMuc2? Muc2 { get; set; }
    public ChiTietMuc3? Muc3 { get; set; }
    public ChiTietMuc4? Muc4 { get; set; }
    public ChiTietMuc5? Muc5 { get; set; }

    [NotMapped]
    public string XepLoai => (DiemAdminCham ?? DiemSVTuCham) switch
    {
        >= 90 => "Xuất sắc",
        >= 80 => "Tốt",
        >= 65 => "Khá",
        >= 50 => "Trung bình",
        >= 35 => "Yếu",
        _      => "Kém"
    };

    [NotMapped]
    public string XepLoaiCssClass => XepLoai switch
    {
        "Xuất sắc"   => "badge bg-success",
        "Tốt"        => "badge bg-primary",
        "Khá"        => "badge bg-info text-dark",
        "Trung bình" => "badge bg-warning text-dark",
        "Yếu"        => "badge bg-danger",
        _             => "badge bg-secondary"
    };
}

[Table("ChiTietMuc1")]
public class ChiTietMuc1
{
    [Key] public int Id { get; set; }
    [ForeignKey("PhieuDanhGia")] public int PhieuId { get; set; }

    // Điểm SV tự chấm
    [Column(TypeName = "decimal(5,2)")] public decimal TC1_DiemTBHT { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC2_ChuyenDeKyNang { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC3_HoiThaoToaDam { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC4_ThiHocThuatTruong { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC5_ThiHocThuatNgoai { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC6_BaoCaoKhoaHocKhoa { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC7_DeTaiNCKHTruong { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC8_BaiBaoKhoaHoc { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC9_KhoiNghiepTruong { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC10_KhoiNghiepNgoai { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC11_ThanhVienCLB { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC12_HoatDongKhac { get; set; } = 0;

    // Điểm Khoa/Admin chấm riêng
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC1_DiemTBHT { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC2_ChuyenDeKyNang { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC3_HoiThaoToaDam { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC4_ThiHocThuatTruong { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC5_ThiHocThuatNgoai { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC6_BaoCaoKhoaHocKhoa { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC7_DeTaiNCKHTruong { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC8_BaiBaoKhoaHoc { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC9_KhoiNghiepTruong { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC10_KhoiNghiepNgoai { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC11_ThanhVienCLB { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC12_HoatDongKhac { get; set; }

    [Column(TypeName = "decimal(5,2)")] public decimal TongMuc1SV { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal? TongMuc1Lop { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? TongMuc1Admin { get; set; }

    public PhieuDanhGia? PhieuDanhGia { get; set; }

    [NotMapped] public const decimal DiemToiDa = 20m;

    [NotMapped]
    public decimal TinhTongSV => Math.Min(
        TC1_DiemTBHT + TC2_ChuyenDeKyNang + TC3_HoiThaoToaDam + TC4_ThiHocThuatTruong +
        TC5_ThiHocThuatNgoai + TC6_BaoCaoKhoaHocKhoa + TC7_DeTaiNCKHTruong + TC8_BaiBaoKhoaHoc +
        TC9_KhoiNghiepTruong + TC10_KhoiNghiepNgoai + TC11_ThanhVienCLB + TC12_HoatDongKhac,
        DiemToiDa);

    [NotMapped]
    public decimal TinhTongAdmin => Math.Min(
        (Admin_TC1_DiemTBHT ?? 0) + (Admin_TC2_ChuyenDeKyNang ?? 0) + (Admin_TC3_HoiThaoToaDam ?? 0) +
        (Admin_TC4_ThiHocThuatTruong ?? 0) + (Admin_TC5_ThiHocThuatNgoai ?? 0) + (Admin_TC6_BaoCaoKhoaHocKhoa ?? 0) +
        (Admin_TC7_DeTaiNCKHTruong ?? 0) + (Admin_TC8_BaiBaoKhoaHoc ?? 0) + (Admin_TC9_KhoiNghiepTruong ?? 0) +
        (Admin_TC10_KhoiNghiepNgoai ?? 0) + (Admin_TC11_ThanhVienCLB ?? 0) + (Admin_TC12_HoatDongKhac ?? 0),
        DiemToiDa);
}

[Table("ChiTietMuc2")]
public class ChiTietMuc2
{
    [Key] public int Id { get; set; }
    [ForeignKey("PhieuDanhGia")] public int PhieuId { get; set; }

    [Column(TypeName = "decimal(5,2)")] public decimal TC1_ThaiDoHocTap { get; set; } = 5;
    [Column(TypeName = "decimal(5,2)")] public decimal TC2_ChapHanhNoiQuy { get; set; } = 5;
    [Column(TypeName = "decimal(5,2)")] public decimal TC3_QuyCheThi { get; set; } = 5;
    [Column(TypeName = "decimal(5,2)")] public decimal TC4_QuyDinhThuVien { get; set; } = 5;
    [Column(TypeName = "decimal(5,2)")] public decimal TC5_QuyDinhPhongHocMay { get; set; } = 5;
    [Column(TypeName = "decimal(5,2)")] public decimal TC6_DangKyNgoaiTru { get; set; } = 5;
    [Column(TypeName = "decimal(5,2)")] public decimal TC7_MacDongPhuc { get; set; } = 5;
    [Column(TypeName = "decimal(5,2)")] public decimal TC8_SinhHoatLopCVHT { get; set; } = 5;

    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC1_ThaiDoHocTap { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC2_ChapHanhNoiQuy { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC3_QuyCheThi { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC4_QuyDinhThuVien { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC5_QuyDinhPhongHocMay { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC6_DangKyNgoaiTru { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC7_MacDongPhuc { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC8_SinhHoatLopCVHT { get; set; }

    [Column(TypeName = "decimal(5,2)")] public decimal TongMuc2SV { get; set; } = 25;
    [Column(TypeName = "decimal(5,2)")] public decimal? TongMuc2Lop { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? TongMuc2Admin { get; set; }

    public PhieuDanhGia? PhieuDanhGia { get; set; }

    [NotMapped] public const decimal DiemToiDa = 25m;

    [NotMapped]
    public decimal TinhTongSV => Math.Min(
        Math.Clamp(TC1_ThaiDoHocTap, 0, 5) + Math.Clamp(TC2_ChapHanhNoiQuy, 0, 5) +
        Math.Clamp(TC3_QuyCheThi, 0, 5) + Math.Clamp(TC4_QuyDinhThuVien, 0, 5) +
        Math.Clamp(TC5_QuyDinhPhongHocMay, 0, 5) + Math.Clamp(TC6_DangKyNgoaiTru, 0, 5) +
        Math.Clamp(TC7_MacDongPhuc, 0, 5) + Math.Clamp(TC8_SinhHoatLopCVHT, 0, 5),
        DiemToiDa);

    [NotMapped]
    public decimal TinhTongAdmin => Math.Min(
        Math.Clamp(Admin_TC1_ThaiDoHocTap ?? 0, 0, 5) + Math.Clamp(Admin_TC2_ChapHanhNoiQuy ?? 0, 0, 5) +
        Math.Clamp(Admin_TC3_QuyCheThi ?? 0, 0, 5) + Math.Clamp(Admin_TC4_QuyDinhThuVien ?? 0, 0, 5) +
        Math.Clamp(Admin_TC5_QuyDinhPhongHocMay ?? 0, 0, 5) + Math.Clamp(Admin_TC6_DangKyNgoaiTru ?? 0, 0, 5) +
        Math.Clamp(Admin_TC7_MacDongPhuc ?? 0, 0, 5) + Math.Clamp(Admin_TC8_SinhHoatLopCVHT ?? 0, 0, 5),
        DiemToiDa);
}

[Table("ChiTietMuc3")]
public class ChiTietMuc3
{
    [Key] public int Id { get; set; }
    [ForeignKey("PhieuDanhGia")] public int PhieuId { get; set; }

    [Column(TypeName = "decimal(5,2)")] public decimal TC1_BatBuocKhoaTruong { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC2_DaiHoiChiDoan { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC3_BaoCaoChuyenDe { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC4_NgoaiKhoaThiCLB { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC5_NgoaiKhoaCapThanhPho { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC6_KetNapDoan { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC7_KetNapDang { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC8_DieuDongDoanHoi { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC9_ThanhVienDoiNhom { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC10_LyLuanChinhTri { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC11_DenOnDapNghia { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC12_LaoDongTinhNguyen { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC13_KhenThuongPhongTrao { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC14_TapTheKhenThuong { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC15_HoatDongKhac { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC1_BatBuocKhoaTruong { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC2_DaiHoiChiDoan { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC3_BaoCaoChuyenDe { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC4_NgoaiKhoaThiCLB { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC5_NgoaiKhoaCapThanhPho { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC6_KetNapDoan { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC7_KetNapDang { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC8_DieuDongDoanHoi { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC9_ThanhVienDoiNhom { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC10_LyLuanChinhTri { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC11_DenOnDapNghia { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC12_LaoDongTinhNguyen { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC13_KhenThuongPhongTrao { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC14_TapTheKhenThuong { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC15_HoatDongKhac { get; set; }

    [Column(TypeName = "decimal(5,2)")] public decimal TongMuc3SV { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal? TongMuc3Lop { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? TongMuc3Admin { get; set; }

    public PhieuDanhGia? PhieuDanhGia { get; set; }

    [NotMapped] public const decimal DiemToiDa = 20m;

    [NotMapped]
    public decimal TinhTongSV => Math.Min(
        TC1_BatBuocKhoaTruong + TC2_DaiHoiChiDoan + TC3_BaoCaoChuyenDe + TC4_NgoaiKhoaThiCLB +
        TC5_NgoaiKhoaCapThanhPho + TC6_KetNapDoan + TC7_KetNapDang + TC8_DieuDongDoanHoi +
        TC9_ThanhVienDoiNhom + TC10_LyLuanChinhTri + TC11_DenOnDapNghia + TC12_LaoDongTinhNguyen +
        TC13_KhenThuongPhongTrao + TC14_TapTheKhenThuong + TC15_HoatDongKhac,
        DiemToiDa);

    [NotMapped]
    public decimal TinhTongAdmin => Math.Min(
        (Admin_TC1_BatBuocKhoaTruong ?? 0) + (Admin_TC2_DaiHoiChiDoan ?? 0) + (Admin_TC3_BaoCaoChuyenDe ?? 0) +
        (Admin_TC4_NgoaiKhoaThiCLB ?? 0) + (Admin_TC5_NgoaiKhoaCapThanhPho ?? 0) + (Admin_TC6_KetNapDoan ?? 0) +
        (Admin_TC7_KetNapDang ?? 0) + (Admin_TC8_DieuDongDoanHoi ?? 0) + (Admin_TC9_ThanhVienDoiNhom ?? 0) +
        (Admin_TC10_LyLuanChinhTri ?? 0) + (Admin_TC11_DenOnDapNghia ?? 0) + (Admin_TC12_LaoDongTinhNguyen ?? 0) +
        (Admin_TC13_KhenThuongPhongTrao ?? 0) + (Admin_TC14_TapTheKhenThuong ?? 0) + (Admin_TC15_HoatDongKhac ?? 0),
        DiemToiDa);
}

[Table("ChiTietMuc4")]
public class ChiTietMuc4
{
    [Key] public int Id { get; set; }
    [ForeignKey("PhieuDanhGia")] public int PhieuId { get; set; }

    [Column(TypeName = "decimal(5,2)")] public decimal TC1_ChapHanhPhapLuat { get; set; } = 10;
    [Column(TypeName = "decimal(5,2)")] public decimal TC2_HanhViTotSeChia { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC3_BieuDuongKhenThuongXH { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC4_GiaoLuuCLBDoiNhom { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC5_TuVanTuyenSinh { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC6_CongTacNhapHoc { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC7_KhamSucKhoeDauKhoa { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC8_NgayHoiViecLam { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC9_LeTotNghiep { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC10_KiemTraHoSoSV { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC11_PhienGiaoDichViecLam { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC12_HienMauTinhNguyen { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC13_XuanTinhNguyen { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC14_MuaHeXanh { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC15_ChuNhatXanh { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC16_ThuBayTinhNguyen { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC17_ChaoDonTanSV { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC18_TrachNhiemXaHoi { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC1_ChapHanhPhapLuat { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC2_HanhViTotSeChia { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC3_BieuDuongKhenThuongXH { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC4_GiaoLuuCLBDoiNhom { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC5_TuVanTuyenSinh { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC6_CongTacNhapHoc { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC7_KhamSucKhoeDauKhoa { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC8_NgayHoiViecLam { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC9_LeTotNghiep { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC10_KiemTraHoSoSV { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC11_PhienGiaoDichViecLam { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC12_HienMauTinhNguyen { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC13_XuanTinhNguyen { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC14_MuaHeXanh { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC15_ChuNhatXanh { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC16_ThuBayTinhNguyen { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC17_ChaoDonTanSV { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC18_TrachNhiemXaHoi { get; set; }

    [Column(TypeName = "decimal(5,2)")] public decimal TongMuc4SV { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal? TongMuc4Lop { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? TongMuc4Admin { get; set; }

    public PhieuDanhGia? PhieuDanhGia { get; set; }

    [NotMapped] public const decimal DiemToiDa = 25m;

    [NotMapped]
    public decimal TinhTongSV => Math.Min(
        TC1_ChapHanhPhapLuat + TC2_HanhViTotSeChia + TC3_BieuDuongKhenThuongXH + TC4_GiaoLuuCLBDoiNhom +
        TC5_TuVanTuyenSinh + TC6_CongTacNhapHoc + TC7_KhamSucKhoeDauKhoa + TC8_NgayHoiViecLam +
        TC9_LeTotNghiep + TC10_KiemTraHoSoSV + TC11_PhienGiaoDichViecLam + TC12_HienMauTinhNguyen +
        TC13_XuanTinhNguyen + TC14_MuaHeXanh + TC15_ChuNhatXanh + TC16_ThuBayTinhNguyen +
        TC17_ChaoDonTanSV + TC18_TrachNhiemXaHoi,
        DiemToiDa);

    [NotMapped]
    public decimal TinhTongAdmin => Math.Min(
        (Admin_TC1_ChapHanhPhapLuat ?? 0) + (Admin_TC2_HanhViTotSeChia ?? 0) + (Admin_TC3_BieuDuongKhenThuongXH ?? 0) +
        (Admin_TC4_GiaoLuuCLBDoiNhom ?? 0) + (Admin_TC5_TuVanTuyenSinh ?? 0) + (Admin_TC6_CongTacNhapHoc ?? 0) +
        (Admin_TC7_KhamSucKhoeDauKhoa ?? 0) + (Admin_TC8_NgayHoiViecLam ?? 0) + (Admin_TC9_LeTotNghiep ?? 0) +
        (Admin_TC10_KiemTraHoSoSV ?? 0) + (Admin_TC11_PhienGiaoDichViecLam ?? 0) + (Admin_TC12_HienMauTinhNguyen ?? 0) +
        (Admin_TC13_XuanTinhNguyen ?? 0) + (Admin_TC14_MuaHeXanh ?? 0) + (Admin_TC15_ChuNhatXanh ?? 0) +
        (Admin_TC16_ThuBayTinhNguyen ?? 0) + (Admin_TC17_ChaoDonTanSV ?? 0) + (Admin_TC18_TrachNhiemXaHoi ?? 0),
        DiemToiDa);
}

[Table("ChiTietMuc5")]
public class ChiTietMuc5
{
    [Key] public int Id { get; set; }
    [ForeignKey("PhieuDanhGia")] public int PhieuId { get; set; }

    [Column(TypeName = "decimal(5,2)")] public decimal TC1_TichCucPhongTrao { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC2_CanBoChiDoanLop { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC3_GiaiThuongHocTapNCKH { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC4_BangKhenUBNDTinh { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC5_SV5TotCapTruong { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC6_SV5TotCapThanhTrungUong { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC7_DoanVienUuTu { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal TC8_GiayKhenTapTheDoan { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC1_TichCucPhongTrao { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC2_CanBoChiDoanLop { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC3_GiaiThuongHocTapNCKH { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC4_BangKhenUBNDTinh { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC5_SV5TotCapTruong { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC6_SV5TotCapThanhTrungUong { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC7_DoanVienUuTu { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? Admin_TC8_GiayKhenTapTheDoan { get; set; }

    [Column(TypeName = "decimal(5,2)")] public decimal TongMuc5SV { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal? TongMuc5Lop { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? TongMuc5Admin { get; set; }

    public PhieuDanhGia? PhieuDanhGia { get; set; }

    [NotMapped] public const decimal DiemToiDa = 10m;

    [NotMapped]
    public decimal TinhTongSV => Math.Min(
        TC1_TichCucPhongTrao + TC2_CanBoChiDoanLop + TC3_GiaiThuongHocTapNCKH +
        TC4_BangKhenUBNDTinh + TC5_SV5TotCapTruong + TC6_SV5TotCapThanhTrungUong +
        TC7_DoanVienUuTu + TC8_GiayKhenTapTheDoan,
        DiemToiDa);

    [NotMapped]
    public decimal TinhTongAdmin => Math.Min(
        (Admin_TC1_TichCucPhongTrao ?? 0) + (Admin_TC2_CanBoChiDoanLop ?? 0) + (Admin_TC3_GiaiThuongHocTapNCKH ?? 0) +
        (Admin_TC4_BangKhenUBNDTinh ?? 0) + (Admin_TC5_SV5TotCapTruong ?? 0) + (Admin_TC6_SV5TotCapThanhTrungUong ?? 0) +
        (Admin_TC7_DoanVienUuTu ?? 0) + (Admin_TC8_GiayKhenTapTheDoan ?? 0),
        DiemToiDa);
}

[Table("BlockchainLog")]
public class BlockchainLog
{
    [Key] public int Id { get; set; }
    [ForeignKey("PhieuDanhGia")] public int PhieuId { get; set; }

    [Required, MaxLength(20)]  public string MSSV { get; set; } = string.Empty;
    [Required, MaxLength(20)]  public string HocKy { get; set; } = string.Empty;
    [Required, MaxLength(64)]  public string DataHash { get; set; } = string.Empty;
    [MaxLength(100)]           public string? TxHash { get; set; }
    [Required, MaxLength(42)]  public string AdminAddress { get; set; } = string.Empty;
    [MaxLength(50)]            public string HanhDong { get; set; } = "LuuBlockchain";
    [MaxLength(20)]            public string KetQua { get; set; } = "ThanhCong";
    [MaxLength(500)]           public string? GhiChu { get; set; }
    public DateTime NgayThucHien { get; set; } = DateTime.UtcNow;

    public PhieuDanhGia? PhieuDanhGia { get; set; }
}