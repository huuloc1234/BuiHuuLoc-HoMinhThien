# Hệ Thống Quản Lý & Xác Thực Điểm Rèn Luyện Sinh Viên Tích Hợp Blockchain

Hệ thống web quản lý quy trình đánh giá điểm rèn luyện sinh viên (theo khung 5 mục – 61 tiêu chí) kết hợp kiến trúc lưu trữ lai (**Hybrid On-chain / Off-chain**) nhằm chống gian lận, ngăn chặn hành vi chỉnh sửa điểm trái phép trong cơ sở dữ liệu và đảm bảo tính minh bạch tuyệt đối.
## 1. Kiến Trúc Hệ Thống (Hybrid Architecture)

**Off-chain (ASP.NET Core 8.0 MVC + SQL Server):**Lưu trữ hồ sơ tài khoản, phiếu đánh giá chi tiết 61 tiêu chí, quy trình khiếu nại trước khi chốt sổ, tệp minh chứng Word (`.doc`/`.docx`) và hệ thống **SQL Server Trigger** tự động ghi nhật ký truy vết mọi thay đổi điểm bất thường.
**On-chain (Ethereum Sepolia Testnet):** Sau khi Khoa duyệt điểm chính thức, Backend tính mã băm mật mã **SHA-256** từ chuỗi chuẩn hóa `MSSV|HocKy|TongDiem|TenFile`. Admin ký giao dịch qua **MetaMask** để ghi vĩnh viễn mã băm này kèm mốc thời gian (`block.timestamp`) lên Smart Contract `DiemRenLuyenTracker.sol`.
**Đối soát toàn vẹn & Điều tra (Forensics):** Bất kỳ khi nào kiểm tra toàn vẹn, hệ thống băm lại dữ liệu hiện tại trong SQL Server và đối chiếu với mã băm gốc trên Blockchain. Nếu phát hiện sai lệch, hệ thống lập tức phát cảnh báo đỏ và truy xuất thủ phạm từ bảng nhật ký Trigger SQL.

## 2. Công Nghệ Sử Dụng

| Thành phần | Công nghệ / Thư viện |
| **Backend** | ASP.NET Core 8.0 MVC, C#, Entity Framework Core 8.0, Dapper |
| **Bảo mật & Xác thực** | JWT (JSON Web Token) kết hợp HttpOnly Cookie, Web3 Signature |
| **Cơ sở dữ liệu** | Microsoft SQL Server (kèm Database Trigger truy vết) |
| **Blockchain & Web3** | Solidity `^0.8.20`, Ethereum Sepolia Testnet (`ChainId: 11155111`), Ethers.js, MetaMask |
| **Frontend** | Razor Views, Bootstrap 5, Bootstrap Icons, `mammoth.js` (xem trực tiếp file Word trên web) |

##  3. Bảng Phân Công Công Việc
**Bùi Hữu Lộc** HTTT2311023 Thiết kế & triển khai Smart Contract `DiemRenLuyenTracker.sol` trên Sepolia.Thiết kế CSDL SQL Server,các dịch vụ lõi (`HashService`, `BlockchainService`, `PhieuDanhGiaService`).Xây dựng `AdminController`, `HomeController` và giao diện Admin (`Index.cshtml` chốt hàng loạt, `ChiTiet.cshtml` chấm 61 tiêu chí & xem trực tiếp file Word). |
**Hồ Minh Thiện** HTTT2311010  Xây dựng cơ chế đăng nhập truyền thống & liên kết ký xác thực ví MetaMask (`AuthController`, `AuthService`, cấp phát JWT Cookie).Xây dựng luồng nghiệp vụ Sinh viên (`SinhVienController`): tự đánh giá 61 tiêu chí, gửi khiếu nại điểm trước khi chốt.Xây dựng dịch vụ xử lý & kiểm duyệt tệp minh chứng Word (`FileService`).Thiết kế giao diện Dashboard Sinh viên, trang Đăng nhập và bố cục dùng chung (`_Layout.cshtml`). |
## 4. Cấu Trúc Thư Mục Dự Án

```text
DiemRenLuyen/
├── Contracts/
│   └── DiemRenLuyenTracker.sol      # Smart Contract lưu trữ và xác minh mã hash SHA-256
├── Scripts/
│   └── 01_CreateDatabase.sql        # Script tạo Database, các bảng dữ liệu và Trigger truy vết
├── Data/
│   └── AppDbContext.cs              # Cấu hình ánh xạ Entity Framework Core
├── Models/
│   └── Entities.cs                  # Các lớp thực thể (TaiKhoan, PhieuDanhGia, ChiTietMuc1..5, BlockchainLog)
├── Services/
│   ├── AuthService.cs               # Xử lý đăng nhập, xác thực chữ ký ví và tạo JWT
│   ├── BlockchainService.cs         # Tính hash, đóng gói dữ liệu gọi hợp đồng và đối soát toàn vẹn
│   ├── FileService.cs               # Kiểm tra định dạng/dung lượng và lưu file Word minh chứng
│   ├── HashService.cs               # Chuẩn hóa chuỗi dữ liệu và băm SHA-256 (64 ký tự hex)
│   └── PhieuDanhGiaService.cs       # Nghiệp vụ tạo phiếu, lưu điểm SV/Admin, chặn trần và khiếu nại
├── Controllers/
│   ├── AdminController.cs           # Điều hướng chấm điểm, chốt hàng loạt, truy vết SQL & đọc file Word
│   ├── AuthController.cs            # Điều hướng đăng nhập / đăng xuất
│   ├── HomeController.cs            # Phân luồng trang chủ theo Role và xử lý trang lỗi
│   └── SinhVienController.cs        # Điều hướng trang tự chấm điểm, nộp file và gửi khiếu nại của SV
├── Views/                           # Giao diện Razor (.cshtml) cho Auth, Admin, SinhVien và Shared
├── wwwroot/uploads/minhchung/       # Thư mục lưu trữ tệp minh chứng .doc / .docx của sinh viên
├── appsettings.json                 # Cấu hình ConnectionString, Blockchain, FileUpload và JWT
├── DiemRenLuyen.csproj              # Khai báo các gói thư viện NuGet (.NET 8.0)
└── Program.cs                       # Cấu hình DI Container, JWT Cookie Authentication và Middleware
```
## 5. Hướng Dẫn Cài Đặt & Chạy Dự Án

### Bước 1: Chuẩn bị môi trường
1. Cài đặt **.NET 8.0 SDK** và **Visual Studio 2022** (hoặc VS Code).
2. Cài đặt **Microsoft SQL Server** và **SQL Server Management Studio (SSMS)**.
3. Cài đặt tiện ích mở rộng **MetaMask** trên trình duyệt (Chrome/Edge), chuyển sang mạng thử nghiệm **Sepolia Testnet** và chuẩn bị sẵn một ít **Sepolia ETH** (lấy miễn phí từ các vòi Faucet) để làm phí gas chốt điểm.

### Bước 2: Khởi tạo Cơ sở dữ liệu SQL Server
1. Mở **SSMS**, kết nối vào SQL Server local của bạn.
2. Mở file script SQL trong thư mục `Scripts/` và bấm **Execute (F5)** để tạo cơ sở dữ liệu `DiemRenLuyenDB`, các bảng dữ liệu mẫu cùng **Trigger truy vết sửa điểm**.

### Bước 3: Deploy Smart Contract lên Sepolia Testnet
1. Truy cập [Remix IDE](https://remix.ethereum.org) và tạo file `DiemRenLuyenTracker.sol` từ thư mục `Contracts/`.
2. Ở tab **Solidity Compiler**, chọn phiên bản **`0.8.20`**, bật `Optimization: 200` và bấm **Compile**.
3. Ở tab **Deploy & Run Transactions**, chọn Environment là **Injected Provider - MetaMask** (đảm bảo ví đang ở mạng **Sepolia Testnet**).
4. Bấm **Deploy** và xác nhận trên MetaMask. Sau khi deploy thành công, copy lại:
   * **Địa chỉ Smart Contract** (`ContractAddress`).
   * **Địa chỉ ví MetaMask đã dùng để deploy** (`AdminWalletAddress` – ví duy nhất có quyền `onlyAdmin` ghi điểm).

### Bước 4: Cấu hình file `appsettings.json`
Mở file `appsettings.json` ở thư mục gốc và điền đầy đủ thông tin môi trường của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TEN_SERVER_CUA_BAN;Database=DiemRenLuyenDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Blockchain": {
    "ContractAddress": "0xDiaChiSmartContractSauKhiDeploy",
    "AdminWalletAddress": "0xDiaChiViMetaMaskCuaAdmin",
    "RpcUrl": "[https://sepolia.infura.io/v3/YOUR_API_KEY](https://sepolia.infura.io/v3/YOUR_API_KEY)",
    "ChainId": 11155111,
    "NetworkName": "Sepolia Testnet"
  },
  "FileUpload": {
    "AllowedExtensions": [ ".doc", ".docx" ],
    "MaxFileSizeMB": 10,
    "UploadFolder": "wwwroot/uploads/minhchung"
  },
  "Jwt": {
    "SecretKey": "ChuoiKhoaBiMatNgauNhienDaiItNhat32KyTu_DiemRenLuyen2025!",
    "Issuer": "DiemRenLuyenApp",
    "Audience": "DiemRenLuyenUsers",
    "ExpiryMinutes": 480
  }
}
```

### Bước 5: Chạy ứng dụng
**Cách 1 (Visual Studio):** Mở file giải pháp `.sln` hoặc `DiemRenLuyen.csproj`, nhấn **`Ctrl + F5`** (hoặc nút Run) để khởi chạy.
**Cách 2 (Terminal / Command Prompt):** Di chuyển vào thư mục chứa file `DiemRenLuyen.csproj` và chạy lần lượt:
  ```bash
  dotnet restore
  dotnet run
  ```
* Truy cập trình duyệt theo địa chỉ hiển thị trên Terminal (ví dụ: `https://localhost:7xxx` hoặc `http://localhost:5xxx`).

---

## 6.quy trình Kiểm Thử Tính Toàn Vẹn (Demo Bảo Vệ Đồ Án)

1. **Sinh viên đánh giá:** Đăng nhập tài khoản Sinh viênTự chấm điểm 61 tiêu chí Tải lên file minh chứng `.docx`.
2. **Khoa duyệt & Chốt sổ:** Đăng nhập tài khoản Admin Xem trực tiếp minh chứng Word ngay trên trình duyệt Duyệt điểm Tích chọn sinh viên và bấm **"Chốt Đã Chọn Lên Blockchain"** để ký MetaMask.
3. **Giả lập tấn công sửa điểm SQL:** Mở SSMS, chạy lệnh `UPDATE PhieuDanhGias SET DiemAdminCham = 100 WHERE MSSV = '...'` để sửa lén điểm của một phiếu đã chốt.
4. **Phát hiện & Truy vết:** Quay lại trang Admin Dashboard, hệ thống lập tức tô đỏ dòng phiếu bị sửa và báo **"Sai lệch Hash"**. Bấm nút chiếc khiên màu đỏ để xem chi tiết so sánh 2 mã SHA-256 và danh tính tài khoản SQL đã thực hiện hành vi can thiệp.
