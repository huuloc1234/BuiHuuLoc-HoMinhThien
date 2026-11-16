using DiemRenLuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace DiemRenLuyen.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaiKhoan>      TaiKhoans      { get; set; }
    public DbSet<PhieuDanhGia>  PhieuDanhGias  { get; set; }
    public DbSet<ChiTietMuc1>   ChiTietMuc1s   { get; set; }
    public DbSet<ChiTietMuc2>   ChiTietMuc2s   { get; set; }
    public DbSet<ChiTietMuc3>   ChiTietMuc3s   { get; set; }
    public DbSet<ChiTietMuc4>   ChiTietMuc4s   { get; set; }
    public DbSet<ChiTietMuc5>   ChiTietMuc5s   { get; set; }
    public DbSet<BlockchainLog> BlockchainLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TaiKhoan
        modelBuilder.Entity<TaiKhoan>(e =>
        {
            e.HasIndex(t => t.DiaChiVi).IsUnique().HasFilter("[DiaChiVi] IS NOT NULL");
            e.HasIndex(t => t.MSSV).IsUnique();
            e.Property(t => t.Role)
             .HasConversion<string>()
             .HasDefaultValue("SinhVien");
        });

        // PhieuDanhGia
        modelBuilder.Entity<PhieuDanhGia>(e =>
        {
            e.HasIndex(p => new { p.MSSV, p.HocKy }).IsUnique();
            e.HasOne(p => p.TaiKhoan)
             .WithMany(t => t.PhieuDanhGias)
             .HasForeignKey(p => p.TaiKhoanId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ChiTietMuc1 -> PhieuDanhGia: 1-1
        modelBuilder.Entity<ChiTietMuc1>(e =>
        {
            e.HasOne(m => m.PhieuDanhGia)
             .WithOne(p => p.Muc1)
             .HasForeignKey<ChiTietMuc1>(m => m.PhieuId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChiTietMuc2>(e =>
        {
            e.HasOne(m => m.PhieuDanhGia)
             .WithOne(p => p.Muc2)
             .HasForeignKey<ChiTietMuc2>(m => m.PhieuId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChiTietMuc3>(e =>
        {
            e.HasOne(m => m.PhieuDanhGia)
             .WithOne(p => p.Muc3)
             .HasForeignKey<ChiTietMuc3>(m => m.PhieuId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChiTietMuc4>(e =>
        {
            e.HasOne(m => m.PhieuDanhGia)
             .WithOne(p => p.Muc4)
             .HasForeignKey<ChiTietMuc4>(m => m.PhieuId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChiTietMuc5>(e =>
        {
            e.HasOne(m => m.PhieuDanhGia)
             .WithOne(p => p.Muc5)
             .HasForeignKey<ChiTietMuc5>(m => m.PhieuId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlockchainLog>(e =>
        {
            e.HasOne(b => b.PhieuDanhGia)
             .WithMany()
             .HasForeignKey(b => b.PhieuId)
             .OnDelete(DeleteBehavior.Restrict);
        });
        // Cấu hình để EF Core không dùng mệnh đề OUTPUT trên các bảng có Trigger trong SQL Server
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.GetTableName() != null)
            {
                modelBuilder.Entity(entityType.Name).ToTable(tb => tb.HasTrigger("trg_Audit"));
            }
        }
    }

}
