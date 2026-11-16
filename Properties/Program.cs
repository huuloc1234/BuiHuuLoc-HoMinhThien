using DiemRenLuyen.Data;
using DiemRenLuyen.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. DATABASE
// ============================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// ============================================================
// 2. AUTHENTICATION - JWT (Session-based sau khi MetaMask ký)
// ============================================================
var jwtConfig = builder.Configuration.GetSection("Jwt");
var secretKey  = jwtConfig["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey chưa được cấu hình trong appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Chỉ dev - bật HTTPS khi production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer           = true,
        ValidIssuer              = jwtConfig["Issuer"],
        ValidateAudience         = true,
        ValidAudience            = jwtConfig["Audience"],
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };

    // Đọc JWT từ Cookie (MVC) hoặc Header (API)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["auth_token"]
                         ?? context.Request.Headers["Authorization"]
                                   .ToString().Replace("Bearer ", "");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",   policy => policy.RequireRole("Admin"));
    options.AddPolicy("SinhVienOnly", policy => policy.RequireRole("SinhVien"));
});

// ============================================================
// 3. SERVICES
// ============================================================
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPhieuDanhGiaService, PhieuDanhGiaService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IBlockchainService, BlockchainService>();
builder.Services.AddHttpContextAccessor();

// ============================================================
// 4. MVC
// ============================================================
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ============================================================
// 5. MIDDLEWARE PIPELINE
// ============================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// 6. ROUTES
// ============================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
