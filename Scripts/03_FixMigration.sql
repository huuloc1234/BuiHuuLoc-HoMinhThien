-- ============================================================
-- SCRIPT SỬA LỖI & CẬP NHẬT NHANH CSDL (Không làm mất dữ liệu)
-- Chạy script này trong SQL Server Management Studio (SSMS)
-- ============================================================

USE DiemRenLuyenDB;
GO

-- 1. Thêm cột Email nếu bảng TaiKhoan chưa có
IF COL_LENGTH('dbo.TaiKhoan', 'Email') IS NULL
BEGIN
    ALTER TABLE dbo.TaiKhoan ADD Email NVARCHAR(100) NULL;
    PRINT N'✅ Đã thêm cột Email vào bảng TaiKhoan';
END
GO

-- 2. Thêm cột MatKhau nếu bảng TaiKhoan chưa có
IF COL_LENGTH('dbo.TaiKhoan', 'MatKhau') IS NULL
BEGIN
    ALTER TABLE dbo.TaiKhoan ADD MatKhau NVARCHAR(255) NULL;
    PRINT N'✅ Đã thêm cột MatKhau vào bảng TaiKhoan';
END
GO

-- 3. Tạo index lọc NULL cho DiaChiVi (tránh lỗi trùng khi nhiều SV chưa có ví)
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_TaiKhoan_DiaChiVi' AND object_id = OBJECT_ID('dbo.TaiKhoan'))
BEGIN
    DROP INDEX UQ_TaiKhoan_DiaChiVi ON dbo.TaiKhoan;
END
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_TaiKhoan_DiaChiVi ON dbo.TaiKhoan(DiaChiVi) WHERE DiaChiVi IS NOT NULL;
GO

-- 4. Cập nhật mật khẩu cho Sinh viên SV230001 (Mật khẩu: 123456)
UPDATE dbo.TaiKhoan
SET Email = N'sv230001@drl.edu.vn',
    MatKhau = N'8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', -- Hash của "123456"
    DiaChiVi = NULL
WHERE MSSV = N'SV230001';

-- 5. Cập nhật mật khẩu cho Sinh viên SV230002 (Mật khẩu: 123456)
UPDATE dbo.TaiKhoan
SET Email = N'sv230002@drl.edu.vn',
    MatKhau = N'8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', -- Hash của "123456"
    DiaChiVi = NULL
WHERE MSSV = N'SV230002';

-- 6. Cập nhật mật khẩu & Ví cho Admin (Email: admin@drl.edu.vn | Mật khẩu: Admin@123 | Ví MetaMask Account 5)
UPDATE dbo.TaiKhoan
SET Email = N'admin@drl.edu.vn',
    MatKhau = N'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7', -- Hash của "Admin@123"
    DiaChiVi = N'0xD3860e63921eB2778ACBc98b8ED283cf2FaBE8Ba'
WHERE MSSV = N'ADMIN001' OR Role = 'Admin';
GO

-- 7. Kiểm tra lại dữ liệu sau khi cập nhật
SELECT Id, MSSV, HoTen, Email, MatKhau, DiaChiVi, Role FROM dbo.TaiKhoan;
GO

PRINT N'🎉 HOÀN TẤT CẬP NHẬT CSDL! Bây giờ bạn có thể đăng nhập bằng SV230001 hoặc admin@drl.edu.vn';
GO
