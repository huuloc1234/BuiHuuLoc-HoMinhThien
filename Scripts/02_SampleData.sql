-- ============================================================
-- Script 02: Dữ liệu mẫu kiểm thử hệ thống (Email + Mật khẩu + Ví MetaMask)
-- ============================================================

USE DiemRenLuyenDB;
GO

DELETE FROM dbo.BlockchainLog;
DELETE FROM dbo.ChiTietMuc5;
DELETE FROM dbo.ChiTietMuc4;
DELETE FROM dbo.ChiTietMuc3;
DELETE FROM dbo.ChiTietMuc2;
DELETE FROM dbo.ChiTietMuc1;
DELETE FROM dbo.PhieuDanhGia;
DELETE FROM dbo.TaiKhoan;

DBCC CHECKIDENT ('dbo.TaiKhoan', RESEED, 0);
DBCC CHECKIDENT ('dbo.PhieuDanhGia', RESEED, 0);
DBCC CHECKIDENT ('dbo.ChiTietMuc1', RESEED, 0);
DBCC CHECKIDENT ('dbo.ChiTietMuc2', RESEED, 0);
DBCC CHECKIDENT ('dbo.ChiTietMuc3', RESEED, 0);
DBCC CHECKIDENT ('dbo.ChiTietMuc4', RESEED, 0);
DBCC CHECKIDENT ('dbo.ChiTietMuc5', RESEED, 0);
GO

-- 1. TÀI KHOẢN ADMIN (Email: admin@drl.edu.vn | Mật khẩu: Admin@123 | Ví MetaMask Account 5)
INSERT INTO dbo.TaiKhoan (MSSV, HoTen, Lop, Email, MatKhau, KhoaHoc, DiaChiVi, Role)
VALUES (
    N'ADMIN001', 
    N'Quản Trị Viên (Khoa CNTT)', 
    N'KhoaCNTT', 
    N'admin@drl.edu.vn', 
    N'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7', -- SHA-256("Admin@123")
    N'2023', 
    N'0xD3860e63921eB2778ACBc98b8ED283cf2FaBE8Ba', 
    N'Admin'
);

-- 2. TÀI KHOẢN SINH VIÊN 1 (MSSV: SV230001 | Email: sv230001@drl.edu.vn | Mật khẩu: 123456)
INSERT INTO dbo.TaiKhoan (MSSV, HoTen, Lop, Email, MatKhau, KhoaHoc, NgaySinh, SoDienThoai, DiaChiVi, Role)
VALUES (
    N'SV230001', 
    N'Nguyễn Văn An', 
    N'23DTH01', 
    N'sv230001@drl.edu.vn', 
    N'8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', -- SHA-256("123456")
    N'2023', 
    N'15/09/2005', 
    N'0901234567', 
    NULL, 
    N'SinhVien'
);

-- 3. TÀI KHOẢN SINH VIÊN 2 (MSSV: SV230002 | Email: sv230002@drl.edu.vn | Mật khẩu: 123456)
INSERT INTO dbo.TaiKhoan (MSSV, HoTen, Lop, Email, MatKhau, KhoaHoc, NgaySinh, SoDienThoai, DiaChiVi, Role)
VALUES (
    N'SV230002', 
    N'Trần Thị Bình', 
    N'23DTH02', 
    N'sv230002@drl.edu.vn', 
    N'8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', -- SHA-256("123456")
    N'2023', 
    N'22/11/2005', 
    N'0909876543', 
    NULL, 
    N'SinhVien'
);

-- ============================================================
-- PHIẾU ĐÁNH GIÁ MẪU CHO SV1 (Nguyễn Văn An - HK1 Năm học 2024 - 2025)
-- ============================================================
DECLARE @TkId1 INT = (SELECT Id FROM dbo.TaiKhoan WHERE MSSV = N'SV230001');

INSERT INTO dbo.PhieuDanhGia (
    TaiKhoanId, MSSV, HocKy, NamHoc, 
    DiemSVTuCham, TrangThaiFile, TenFileGoc, DuongDanFileWord, DaChot
)
VALUES (
    @TkId1, N'SV230001', N'HK1_2024-2025', N'2024-2025',
    85, N'Đã nộp', N'MinhChung_RenLuyen_SV230001.docx', N'/uploads/minhchung/MinhChung_RenLuyen_SV230001.docx', 0
);

DECLARE @P1 INT = SCOPE_IDENTITY();

-- Mục I: 18đ
INSERT INTO dbo.ChiTietMuc1 (
    PhieuId, TC1_DiemTBHT, TC2_ChuyenDeKyNang, TC3_HoiThaoToaDam, TC4_ThiHocThuatTruong,
    TC5_ThiHocThuatNgoai, TC6_BaoCaoKhoaHocKhoa, TC7_DeTaiNCKHTruong, TC8_BaiBaoKhoaHoc,
    TC9_KhoiNghiepTruong, TC10_KhoiNghiepNgoai, TC11_ThanhVienCLB, TC12_HoatDongKhac,
    TongMuc1SV, TongMuc1Admin
)
VALUES (@P1, 5, 3, 3, 7, 0, 0, 0, 0, 0, 0, 0, 0, 18, 18);

-- Mục II: 25đ
INSERT INTO dbo.ChiTietMuc2 (
    PhieuId, TC1_ThaiDoHocTap, TC2_ChapHanhNoiQuy, TC3_QuyCheThi, TC4_QuyDinhThuVien,
    TC5_QuyDinhPhongHocMay, TC6_DangKyNgoaiTru, TC7_MacDongPhuc, TC8_SinhHoatLopCVHT,
    TongMuc2SV, TongMuc2Admin
)
VALUES (@P1, 5, 5, 5, 5, 5, 5, 5, 5, 25, 25);

-- Mục III: 15đ
INSERT INTO dbo.ChiTietMuc3 (
    PhieuId, TC1_BatBuocKhoaTruong, TC2_DaiHoiChiDoan, TC3_BaoCaoChuyenDe, TC4_NgoaiKhoaThiCLB,
    TC5_NgoaiKhoaCapThanhPho, TC6_KetNapDoan, TC7_KetNapDang, TC8_DieuDongDoanHoi,
    TC9_ThanhVienDoiNhom, TC10_LyLuanChinhTri, TC11_DenOnDapNghia, TC12_LaoDongTinhNguyen,
    TC13_KhenThuongPhongTrao, TC14_TapTheKhenThuong, TC15_HoatDongKhac,
    TongMuc3SV, TongMuc3Admin
)
VALUES (@P1, 3, 3, 4, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 15);

-- Mục IV: 22đ
INSERT INTO dbo.ChiTietMuc4 (
    PhieuId, TC1_ChapHanhPhapLuat, TC2_HanhViTotSeChia, TC3_BieuDuongKhenThuongXH,
    TC4_GiaoLuuCLBDoiNhom, TC5_TuVanTuyenSinh, TC6_CongTacNhapHoc, TC7_KhamSucKhoeDauKhoa,
    TC8_NgayHoiViecLam, TC9_LeTotNghiep, TC10_KiemTraHoSoSV, TC11_PhienGiaoDichViecLam,
    TC12_HienMauTinhNguyen, TC13_XuanTinhNguyen, TC14_MuaHeXanh, TC15_ChuNhatXanh,
    TC16_ThuBayTinhNguyen, TC17_ChaoDonTanSV, TC18_TrachNhiemXaHoi,
    TongMuc4SV, TongMuc4Admin
)
VALUES (@P1, 10, 0, 0, 3, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 22, 22);

-- Mục V: 5đ
INSERT INTO dbo.ChiTietMuc5 (
    PhieuId, TC1_TichCucPhongTrao, TC2_CanBoChiDoanLop, TC3_GiaiThuongHocTapNCKH,
    TC4_BangKhenUBNDTinh, TC5_SV5TotCapTruong, TC6_SV5TotCapThanhTrungUong,
    TC7_DoanVienUuTu, TC8_GiayKhenTapTheDoan,
    TongMuc5SV, TongMuc5Admin
)
VALUES (@P1, 3, 2, 0, 0, 0, 0, 0, 0, 5, 5);

-- Chạy Stored Procedure tính tổng (18 + 25 + 15 + 22 + 5 = 85 điểm -> Loại Tốt)
EXEC dbo.SP_TinhTongDiem @PhieuId = @P1, @IsAdminCham = 1;

PRINT N'✅ Khởi tạo dữ liệu mẫu thành công!';
PRINT N'👉 Admin: admin@drl.edu.vn / Mật khẩu: Admin@123';
PRINT N'👉 Sinh viên: SV230001 (hoặc sv230001@drl.edu.vn) / Mật khẩu: 123456';
GO
