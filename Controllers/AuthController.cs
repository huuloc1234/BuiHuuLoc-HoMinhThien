using DiemRenLuyen.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiemRenLuyen.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // GET /Auth/Login
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToDashboard();
        return View();
    }

    // POST /Auth/Login (Đăng nhập truyền thống bằng Email / MSSV + Mật khẩu)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string emailHoacMSSV, string matKhau)
    {
        if (string.IsNullOrWhiteSpace(emailHoacMSSV) || string.IsNullOrWhiteSpace(matKhau))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ Email / MSSV và Mật khẩu.";
            return View();
        }

        try
        {
            var taiKhoan = await _authService.DangNhapBangEmail(emailHoacMSSV, matKhau);
            if (taiKhoan == null)
            {
                ViewBag.Error = "Thông tin đăng nhập không chính xác hoặc tài khoản đã bị khóa.";
                return View();
            }

            // Tạo JWT và lưu Cookie
            GanAuthCookie(taiKhoan);

            TempData["Success"] = $"Đăng nhập thành công! Xin chào {taiKhoan.HoTen}.";
            return taiKhoan.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Dashboard", "SinhVien");
        }
        catch (Exception ex)
        {
            var chiTietLoi = ex.InnerException?.Message ?? ex.Message;
            ViewBag.Error = $"Lỗi kết nối CSDL hoặc thiếu cột: {chiTietLoi}";
            return View();
        }
    }

    // POST /Auth/MetaMaskLogin (Đăng nhập Web3 bằng chữ ký ví)
    [HttpPost]
    public async Task<IActionResult> MetaMaskLogin(
        [FromForm] string diaChiVi,
        [FromForm] string message,
        [FromForm] string signature)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(diaChiVi))
                return Json(new { success = false, message = "Không nhận diện được địa chỉ ví." });

            if (!_authService.XacThucChuKy(diaChiVi, message, signature))
                return Json(new { success = false, message = "Chữ ký ví MetaMask không hợp lệ." });

            var viChuanHoa = diaChiVi.Trim().ToLowerInvariant();
            var taiKhoan = await _authService.TimKiemTheoVi(viChuanHoa);

            if (taiKhoan == null)
            {
                // Ví mới chưa liên kết -> cho phép đăng ký hoặc liên kết
                return Json(new { success = false, needRegister = true, diaChiVi = viChuanHoa });
            }

            GanAuthCookie(taiKhoan);

            var redirectUrl = taiKhoan.Role == "Admin"
                ? Url.Action("Index", "Admin")
                : Url.Action("Dashboard", "SinhVien");

            return Json(new { success = true, role = taiKhoan.Role, redirectUrl });
        }
        catch (Exception ex)
        {
            var chiTietLoi = ex.InnerException?.Message ?? ex.Message;
            return Json(new { success = false, message = $"Lỗi Server/CSDL: {chiTietLoi}" });
        }
    }

    // GET /Auth/Register
    [HttpGet]
    public IActionResult Register(string? diaChiVi)
    {
        ViewBag.DiaChiVi = diaChiVi ?? string.Empty;
        return View();
    }

    // POST /Auth/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string mssv,
        string hoTen,
        string lop,
        string? email,
        string? matKhau,
        string? xacNhanMatKhau,
        string? diaChiVi)
    {
        if (!string.IsNullOrEmpty(matKhau) && matKhau != xacNhanMatKhau)
        {
            ModelState.AddModelError("", "Mật khẩu xác nhận không trùng khớp.");
            ViewBag.DiaChiVi = diaChiVi;
            return View();
        }

        try
        {
            var taiKhoan = await _authService.DangKy(mssv, hoTen, lop, email, matKhau, diaChiVi);
            GanAuthCookie(taiKhoan);

            TempData["Success"] = $"Đăng ký thành công! Chào mừng {hoTen}.";
            return RedirectToAction("Dashboard", "SinhVien");
        }
        catch (Exception ex)
        {
            var chiTietLoi = ex.InnerException?.Message ?? ex.Message;
            ModelState.AddModelError("", chiTietLoi);
            ViewBag.DiaChiVi = diaChiVi;
            return View();
        }
    }

    // GET /Auth/Logout
    [HttpGet]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return RedirectToAction("Login");
    }

    private void GanAuthCookie(Models.TaiKhoan taiKhoan)
    {
        var token = _authService.TaoJwtToken(taiKhoan);
        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure   = false,
            SameSite = SameSiteMode.Lax,
            Expires  = DateTimeOffset.UtcNow.AddHours(8)
        });
    }

    private IActionResult RedirectToDashboard()
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "Admin");
        return RedirectToAction("Dashboard", "SinhVien");
    }
}