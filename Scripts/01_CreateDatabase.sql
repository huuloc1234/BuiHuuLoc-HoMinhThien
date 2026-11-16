-- ============================================================
-- HỆ THỐNG QUẢN LÝ ĐIỂM RÈN LUYỆN SINH VIÊN - HYBRID BLOCKCHAIN
-- Script: Tạo Database, Tables và Biểu Mẫu 61 Tiêu Chí Chuẩn Quy Chế
-- Database: SQL Server 2019+
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'DiemRenLuyenDB')
BEGIN
    CREATE DATABASE DiemRenLuyenDB;
END
GO

USE DiemRenLuyenDB;
GO

-- ============================================================
-- DỌN DẸP BẢNG CŨ (Theo thứ tự tránh lỗi khóa ngoại Msg 3726)
-- ============================================================
IF OBJECT_ID('dbo.BlockchainLog', 'U') IS NOT NULL DROP TABLE dbo.BlockchainLog;
IF OBJECT_ID('dbo.ChiTietMuc5', 'U') IS NOT NULL DROP TABLE dbo.ChiTietMuc5;
IF OBJECT_ID('dbo.ChiTietMuc4', 'U') IS NOT NULL DROP TABLE dbo.ChiTietMuc4;
IF OBJECT_ID('dbo.ChiTietMuc3', 'U') IS NOT NULL DROP TABLE dbo.ChiTietMuc3;
IF OBJECT_ID('dbo.ChiTietMuc2', 'U') IS NOT NULL DROP TABLE dbo.ChiTietMuc2;
IF OBJECT_ID('dbo.ChiTietMuc1', 'U') IS NOT NULL DROP TABLE dbo.ChiTietMuc1;
IF OBJECT_ID('dbo.PhieuDanhGia', 'U') IS NOT NULL DROP TABLE dbo.PhieuDanhGia;
IF OBJECT_ID('dbo.TaiKhoan', 'U') IS NOT NULL DROP TABLE dbo.TaiKhoan;
GO

-- ============================================================
-- BẢNG 1: TaiKhoan - Hỗ trợ đăng nhập Email/Mật khẩu & MetaMask
-- ============================================================
CREATE TABLE dbo.TaiKhoan (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MSSV            NVARCHAR(20)   NOT NULL UNIQUE,
    HoTen           NVARCHAR(100)  NOT NULL,
    Lop             NVARCHAR(50)   NOT NULL,
    Email           NVARCHAR(100)  NULL,
    MatKhau         NVARCHAR(255)  NULL,              -- SHA-256 hash của mật khẩu
    KhoaHoc         NVARCHAR(20)   NULL,
    NgaySinh        NVARCHAR(20)   NULL,
    SoDienThoai     NVARCHAR(20)   NULL,
    DiaChiVi        NVARCHAR(42)   NULL,              -- Địa chỉ ví MetaMask (tùy chọn liên kết)
    Role            NVARCHAR(10)   NOT NULL DEFAULT 'SinhVien'
                        CHECK (Role IN ('SinhVien', 'Admin')),
    NgayTao         DATETIME2      NOT NULL DEFAULT GETDATE(),
    IsActive        BIT            NOT NULL DEFAULT 1
);
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_TaiKhoan_DiaChiVi ON dbo.TaiKhoan(DiaChiVi) WHERE DiaChiVi IS NOT NULL;
GO

-- ============================================================
-- BẢNG 2: PhieuDanhGia - Phiếu rèn luyện theo học kỳ
-- ============================================================
IF OBJECT_ID('dbo.PhieuDanhGia', 'U') IS NOT NULL DROP TABLE dbo.PhieuDanhGia;
GO

CREATE TABLE dbo.PhieuDanhGia (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanId      INT            NOT NULL REFERENCES dbo.TaiKhoan(Id),
    MSSV            NVARCHAR(20)   NOT NULL,
    HocKy           NVARCHAR(20)   NOT NULL,           -- VD: "HK1_2024-2025"
    NamHoc          NVARCHAR(20)   NOT NULL,           -- VD: "2024-2025"

    -- Điểm tổng hợp
    DiemSVTuCham    DECIMAL(5,2)   NOT NULL DEFAULT 0,
    DiemLopCham     DECIMAL(5,2)   NULL,
    DiemAdminCham   DECIMAL(5,2)   NULL,               -- Khoa đánh giá / Admin chốt

    -- File minh chứng Word
    DuongDanFileWord    NVARCHAR(500)  NULL,
    TenFileGoc          NVARCHAR(255)  NULL,
    TrangThaiFile       NVARCHAR(20)   NOT NULL DEFAULT N'Chưa nộp'
                            CHECK (TrangThaiFile IN (N'Chưa nộp', N'Đã nộp')),

    -- Trạng thái chốt Blockchain
    DaChot          BIT            NOT NULL DEFAULT 0,
    NgayChot        DATETIME2      NULL,
    TxHash          NVARCHAR(100)  NULL,              -- Transaction Hash trên Sepolia
    DataHash        NVARCHAR(64)   NULL,              -- SHA-256 Hash lưu on-chain

    -- Timestamps
    NgayTao         DATETIME2      NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME2      NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_PhieuDanhGia_MSSV_HocKy UNIQUE (MSSV, HocKy)
);
GO

-- ============================================================
-- BẢNG 3: ChiTietMuc1 - Ý thức tham gia học tập (Tối đa 20đ, 12 tiêu chí)
-- ============================================================
IF OBJECT_ID('dbo.ChiTietMuc1', 'U') IS NOT NULL DROP TABLE dbo.ChiTietMuc1;
GO

CREATE TABLE dbo.ChiTietMuc1 (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PhieuId         INT            NOT NULL REFERENCES dbo.PhieuDanhGia(Id) ON DELETE CASCADE,

    TC1_DiemTBHT            DECIMAL(5,2) NOT NULL DEFAULT 0, -- 5 đ/học kỳ
    TC2_ChuyenDeKyNang      DECIMAL(5,2) NOT NULL DEFAULT 0, -- 3 đ/học kỳ
    TC3_HoiThaoToaDam       DECIMAL(5,2) NOT NULL DEFAULT 0, -- 3 đ/lần
    TC4_ThiHocThuatTruong   DECIMAL(5,2) NOT NULL DEFAULT 0, -- 7 đ/lần
    TC5_ThiHocThuatNgoai    DECIMAL(5,2) NOT NULL DEFAULT 0, -- 8 đ/lần
    TC6_BaoCaoKhoaHocKhoa   DECIMAL(5,2) NOT NULL DEFAULT 0, -- 8 đ/lần
    TC7_DeTaiNCKHTruong     DECIMAL(5,2) NOT NULL DEFAULT 0, -- 10 đ/lần
    TC8_BaiBaoKhoaHoc       DECIMAL(5,2) NOT NULL DEFAULT 0, -- 8 đ/lần
    TC9_KhoiNghiepTruong    DECIMAL(5,2) NOT NULL DEFAULT 0, -- 7 đ/lần
    TC10_KhoiNghiepNgoai    DECIMAL(5,2) NOT NULL DEFAULT 0, -- 8 đ/lần
    TC11_ThanhVienCLB       DECIMAL(5,2) NOT NULL DEFAULT 0, -- 2 đ/học kỳ
    TC12_HoatDongKhac       DECIMAL(5,2) NOT NULL DEFAULT 0, -- 3 đ/lần

    TongMuc1SV     DECIMAL(5,2)   NOT NULL DEFAULT 0,
    TongMuc1Lop    DECIMAL(5,2)   NULL,
    TongMuc1Admin  DECIMAL(5,2)   NULL
);
GO

-- ============================================================
-- BẢNG 4: ChiTietMuc2 - Ý thức chấp hành nội quy, quy chế (Tối đa 25đ, 8 tiêu chí)
-- ============================================================
IF OBJECT_ID('dbo.ChiTietMuc2', 'U') IS NOT NULL DROP TABLE dbo.ChiTietMuc2;
GO

CREATE TABLE dbo.ChiTietMuc2 (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PhieuId         INT            NOT NULL REFERENCES dbo.PhieuDanhGia(Id) ON DELETE CASCADE,

    TC1_ThaiDoHocTap        DECIMAL(5,2) NOT NULL DEFAULT 5, -- 5 đ/học kỳ
    TC2_ChapHanhNoiQuy      DECIMAL(5,2) NOT NULL DEFAULT 5, -- 5 đ/học kỳ (-5đ)
    TC3_QuyCheThi           DECIMAL(5,2) NOT NULL DEFAULT 5, -- 5 đ/học kỳ (-5đ)
    TC4_QuyDinhThuVien      DECIMAL(5,2) NOT NULL DEFAULT 5, -- 5 đ/học kỳ (-5đ)
    TC5_QuyDinhPhongHocMay  DECIMAL(5,2) NOT NULL DEFAULT 5, -- 5 đ/học kỳ (-5đ)
    TC6_DangKyNgoaiTru      DECIMAL(5,2) NOT NULL DEFAULT 5, -- 5 đ/học kỳ (-5đ)
    TC7_MacDongPhuc         DECIMAL(5,2) NOT NULL DEFAULT 5, -- 5 đ/học kỳ (-5đ)
    TC8_SinhHoatLopCVHT     DECIMAL(5,2) NOT NULL DEFAULT 5, -- 5 đ/học kỳ (-5đ)

    TongMuc2SV     DECIMAL(5,2)   NOT NULL DEFAULT 25,
    TongMuc2Lop    DECIMAL(5,2)   NULL,
    TongMuc2Admin  DECIMAL(5,2)   NULL
);
GO

-- ============================================================
-- BẢNG 5: ChiTietMuc3 - Tham gia hoạt động chính trị, XH, VH, TDTT (Tối đa 20đ, 15 tiêu chí)
-- ============================================================
IF OBJECT_ID('dbo.ChiTietMuc3', 'U') IS NOT NULL DROP TABLE dbo.ChiTietMuc3;
GO

CREATE TABLE dbo.ChiTietMuc3 (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PhieuId         INT            NOT NULL REFERENCES dbo.PhieuDanhGia(Id) ON DELETE CASCADE,

    TC1_BatBuocKhoaTruong   DECIMAL(5,2) NOT NULL DEFAULT 0, -- 3 đ/lần (-3đ)
    TC2_DaiHoiChiDoan       DECIMAL(5,2) NOT NULL DEFAULT 0, -- 3 đ/lần (-3đ)
    TC3_BaoCaoChuyenDe      DECIMAL(5,2) NOT NULL DEFAULT 0, -- 4 đ/lần
    TC4_NgoaiKhoaThiCLB     DECIMAL(5,2) NOT NULL DEFAULT 0, -- 7 đ/lần
    TC5_NgoaiKhoaCapThanhPho DECIMAL(5,2) NOT NULL DEFAULT 0,-- 8 đ/lần
    TC6_KetNapDoan          DECIMAL(5,2) NOT NULL DEFAULT 0, -- 5 đ
    TC7_KetNapDang          DECIMAL(5,2) NOT NULL DEFAULT 0, -- 8 đ
    TC8_DieuDongDoanHoi     DECIMAL(5,2) NOT NULL DEFAULT 0, -- 4 đ/lần
    TC9_ThanhVienDoiNhom    DECIMAL(5,2) NOT NULL DEFAULT 0, -- 2 đ/học kỳ
    TC10_LyLuanChinhTri     DECIMAL(5,2) NOT NULL DEFAULT 0, -- 4 đ/lần
    TC11_DenOnDapNghia      DECIMAL(5,2) NOT NULL DEFAULT 0, -- 3 đ/lần
    TC12_LaoDongTinhNguyen  DECIMAL(5,2) NOT NULL DEFAULT 0, -- 3 đ/lần
    TC13_KhenThuongPhongTrao DECIMAL(5,2) NOT NULL DEFAULT 0,-- 7 đ/lần
    TC14_TapTheKhenThuong   DECIMAL(5,2) NOT NULL DEFAULT 0, -- 1 đ/lần
    TC15_HoatDongKhac       DECIMAL(5,2) NOT NULL DEFAULT 0, -- 3 đ/lần

    TongMuc3SV     DECIMAL(5,2)   NOT NULL DEFAULT 0,
    TongMuc3Lop    DECIMAL(5,2)   NULL,
    TongMuc3Admin  DECIMAL(5,2)   NULL
);
GO

-- ============================================================
-- BẢNG 6: ChiTietMuc4 - Ý thức công dân trong cộng đồng (Tối đa 25đ, 18 tiêu chí)
-- ============================================================
IF OBJECT_ID('dbo.ChiTietMuc4', 'U') IS NOT NULL DROP TABLE dbo.ChiTietMuc4;
GO

CREATE TABLE dbo.ChiTietMuc4 (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PhieuId         INT            NOT NULL REFERENCES dbo.PhieuDanhGia(Id) ON DELETE CASCADE,

    TC1_ChapHanhPhapLuat    DECIMAL(5,2) NOT NULL DEFAULT 10, -- 10 đ/lần (-5đ)
    TC2_HanhViTotSeChia     DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC3_BieuDuongKhenThuongXH DECIMAL(5,2) NOT NULL DEFAULT 0,-- 5 đ/lần
    TC4_GiaoLuuCLBDoiNhom   DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC5_TuVanTuyenSinh      DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC6_CongTacNhapHoc      DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC7_KhamSucKhoeDauKhoa  DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC8_NgayHoiViecLam      DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC9_LeTotNghiep         DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC10_KiemTraHoSoSV      DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC11_PhienGiaoDichViecLam DECIMAL(5,2) NOT NULL DEFAULT 0,-- 3 đ/lần
    TC12_HienMauTinhNguyen  DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 10 đ/lần
    TC13_XuanTinhNguyen     DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC14_MuaHeXanh          DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 7 đ/lần
    TC15_ChuNhatXanh        DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC16_ThuBayTinhNguyen   DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC17_ChaoDonTanSV       DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 5 đ/lần
    TC18_TrachNhiemXaHoi    DECIMAL(5,2) NOT NULL DEFAULT 0,  -- 3 đ/lần

    TongMuc4SV     DECIMAL(5,2)   NOT NULL DEFAULT 0,
    TongMuc4Lop    DECIMAL(5,2)   NULL,
    TongMuc4Admin  DECIMAL(5,2)   NULL
);
GO

-- ============================================================
-- BẢNG 7: ChiTietMuc5 - Cán bộ lớp & thành tích đặc biệt (Tối đa 10đ, 8 tiêu chí)
-- ============================================================
IF OBJECT_ID('dbo.ChiTietMuc5', 'U') IS NOT NULL DROP TABLE dbo.ChiTietMuc5;
GO

CREATE TABLE dbo.ChiTietMuc5 (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PhieuId         INT            NOT NULL REFERENCES dbo.PhieuDanhGia(Id) ON DELETE CASCADE,

    TC1_TichCucPhongTrao    DECIMAL(5,2) NOT NULL DEFAULT 0, -- 3 đ/học kỳ
    TC2_CanBoChiDoanLop     DECIMAL(5,2) NOT NULL DEFAULT 0, -- 5 đ/học kỳ
    TC3_GiaiThuongHocTapNCKH DECIMAL(5,2) NOT NULL DEFAULT 0,-- 7 đ/lần
    TC4_BangKhenUBNDTinh    DECIMAL(5,2) NOT NULL DEFAULT 0, -- 5 đ/lần
    TC5_SV5TotCapTruong     DECIMAL(5,2) NOT NULL DEFAULT 0, -- 6 đ/lần
    TC6_SV5TotCapThanhTrungUong DECIMAL(5,2) NOT NULL DEFAULT 0,-- 10 đ/lần
    TC7_DoanVienUuTu        DECIMAL(5,2) NOT NULL DEFAULT 0, -- 6 đ/lần
    TC8_GiayKhenTapTheDoan  DECIMAL(5,2) NOT NULL DEFAULT 0, -- 2 đ/SV

    TongMuc5SV     DECIMAL(5,2)   NOT NULL DEFAULT 0,
    TongMuc5Lop    DECIMAL(5,2)   NULL,
    TongMuc5Admin  DECIMAL(5,2)   NULL
);
GO

-- ============================================================
-- BẢNG 8: BlockchainLog - Lịch sử giao dịch On-chain
-- ============================================================
IF OBJECT_ID('dbo.BlockchainLog', 'U') IS NOT NULL DROP TABLE dbo.BlockchainLog;
GO

CREATE TABLE dbo.BlockchainLog (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PhieuId         INT            NOT NULL REFERENCES dbo.PhieuDanhGia(Id),
    MSSV            NVARCHAR(20)   NOT NULL,
    HocKy           NVARCHAR(20)   NOT NULL,
    DataHash        NVARCHAR(64)   NOT NULL,
    TxHash          NVARCHAR(100)  NULL,
    AdminAddress    NVARCHAR(42)   NOT NULL,
    HanhDong        NVARCHAR(50)   NOT NULL DEFAULT N'LuuBlockchain',
    KetQua          NVARCHAR(20)   NOT NULL DEFAULT N'ThanhCong',
    GhiChu          NVARCHAR(500)  NULL,
    NgayThucHien    DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- STORED PROCEDURE: Tính tổng điểm có áp trần chuẩn xác
-- ============================================================
IF OBJECT_ID('dbo.SP_TinhTongDiem', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_TinhTongDiem;
GO

CREATE PROCEDURE dbo.SP_TinhTongDiem
    @PhieuId INT,
    @IsAdminCham BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @M1SV DECIMAL(5,2), @M2SV DECIMAL(5,2), @M3SV DECIMAL(5,2),
            @M4SV DECIMAL(5,2), @M5SV DECIMAL(5,2);
    DECLARE @M1A DECIMAL(5,2), @M2A DECIMAL(5,2), @M3A DECIMAL(5,2),
            @M4A DECIMAL(5,2), @M5A DECIMAL(5,2);

    SELECT @M1SV = ISNULL(TongMuc1SV, 0), @M1A = TongMuc1Admin FROM dbo.ChiTietMuc1 WHERE PhieuId = @PhieuId;
    SELECT @M2SV = ISNULL(TongMuc2SV, 0), @M2A = TongMuc2Admin FROM dbo.ChiTietMuc2 WHERE PhieuId = @PhieuId;
    SELECT @M3SV = ISNULL(TongMuc3SV, 0), @M3A = TongMuc3Admin FROM dbo.ChiTietMuc3 WHERE PhieuId = @PhieuId;
    SELECT @M4SV = ISNULL(TongMuc4SV, 0), @M4A = TongMuc4Admin FROM dbo.ChiTietMuc4 WHERE PhieuId = @PhieuId;
    SELECT @M5SV = ISNULL(TongMuc5SV, 0), @M5A = TongMuc5Admin FROM dbo.ChiTietMuc5 WHERE PhieuId = @PhieuId;

    -- Áp trần từng mục
    SET @M1SV = CASE WHEN @M1SV > 20 THEN 20 WHEN @M1SV < 0 THEN 0 ELSE @M1SV END;
    SET @M2SV = CASE WHEN @M2SV > 25 THEN 25 WHEN @M2SV < 0 THEN 0 ELSE @M2SV END;
    SET @M3SV = CASE WHEN @M3SV > 20 THEN 20 WHEN @M3SV < 0 THEN 0 ELSE @M3SV END;
    SET @M4SV = CASE WHEN @M4SV > 25 THEN 25 WHEN @M4SV < 0 THEN 0 ELSE @M4SV END;
    SET @M5SV = CASE WHEN @M5SV > 10 THEN 10 WHEN @M5SV < 0 THEN 0 ELSE @M5SV END;

    -- Cập nhật điểm SV tự chấm
    UPDATE dbo.PhieuDanhGia
    SET DiemSVTuCham = @M1SV + @M2SV + @M3SV + @M4SV + @M5SV,
        NgayCapNhat  = GETDATE()
    WHERE Id = @PhieuId;

    -- Cập nhật điểm Admin / Khoa nếu đã chấm
    IF @IsAdminCham = 1 AND @M1A IS NOT NULL AND @M2A IS NOT NULL
        AND @M3A IS NOT NULL AND @M4A IS NOT NULL AND @M5A IS NOT NULL
    BEGIN
        SET @M1A = CASE WHEN @M1A > 20 THEN 20 WHEN @M1A < 0 THEN 0 ELSE @M1A END;
        SET @M2A = CASE WHEN @M2A > 25 THEN 25 WHEN @M2A < 0 THEN 0 ELSE @M2A END;
        SET @M3A = CASE WHEN @M3A > 20 THEN 20 WHEN @M3A < 0 THEN 0 ELSE @M3A END;
        SET @M4A = CASE WHEN @M4A > 25 THEN 25 WHEN @M4A < 0 THEN 0 ELSE @M4A END;
        SET @M5A = CASE WHEN @M5A > 10 THEN 10 WHEN @M5A < 0 THEN 0 ELSE @M5A END;

        UPDATE dbo.PhieuDanhGia
        SET DiemAdminCham = @M1A + @M2A + @M3A + @M4A + @M5A,
            NgayCapNhat   = GETDATE()
        WHERE Id = @PhieuId;
    END
END
GO
PRINT N'✅ Database DiemRenLuyenDB với biểu mẫu 61 tiêu chí chuẩn quy chế đã sẵn sàng!';
GO
