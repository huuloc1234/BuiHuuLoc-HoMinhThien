namespace DiemRenLuyen.Services;

public interface IFileService
{
    Task<(string DuongDan, string TenFile)> LuuFileWord(IFormFile file, string mssv, string hocKy);
    void XoaFile(string duongDan);
    bool FileHopLe(IFormFile file);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration      _config;
    private readonly string[]            _allowedExts;
    private readonly long                _maxBytes;

    public FileService(IWebHostEnvironment env, IConfiguration config)
    {
        _env         = env;
        _config      = config;
        _allowedExts = config.GetSection("FileUpload:AllowedExtensions").Get<string[]>()
                       ?? new[] { ".doc", ".docx" };
        var maxMb    = config.GetValue<int>("FileUpload:MaxFileSizeMB", 10);
        _maxBytes    = maxMb * 1024L * 1024L;
    }

    public async Task<(string DuongDan, string TenFile)> LuuFileWord(IFormFile file, string mssv, string hocKy)
    {
        if (!FileHopLe(file))
            throw new InvalidOperationException("File không hợp lệ. Chỉ chấp nhận .doc và .docx, tối đa 10MB.");

        var uploadFolder = _config["FileUpload:UploadFolder"] ?? "wwwroot/uploads/minhchung";
        var fullFolder   = Path.Combine(_env.ContentRootPath, uploadFolder);

        if (!Directory.Exists(fullFolder))
            Directory.CreateDirectory(fullFolder);

        var ext      = Path.GetExtension(file.FileName).ToLowerInvariant();
        // Tên file chuẩn hóa: MSSV_HocKy_timestamp.docx
        var hocKySafe = hocKy.Replace(" ", "_").Replace("/", "-");
        var tenFile   = $"{mssv}_{hocKySafe}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{ext}";
        var fullPath  = Path.Combine(fullFolder, tenFile);

        using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        // Trả về đường dẫn tương đối (web path)
        var webPath = $"/uploads/minhchung/{tenFile}";
        return (webPath, file.FileName);
    }

    public void XoaFile(string duongDan)
    {
        if (string.IsNullOrEmpty(duongDan)) return;
        var fullPath = Path.Combine(_env.ContentRootPath, "wwwroot", duongDan.TrimStart('/'));
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    public bool FileHopLe(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;
        if (file.Length > _maxBytes) return false;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExts.Contains(ext);
    }
}
