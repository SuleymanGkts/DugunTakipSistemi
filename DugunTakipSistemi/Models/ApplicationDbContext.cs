using Microsoft.EntityFrameworkCore;

namespace DugunTakipSistemi.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Veritabanında oluşacak tabloların isimleri
        public DbSet<Musteri> Musteriler { get; set; }
        public DbSet<Paket> Paketler { get; set; }
        public DbSet<Rezervasyon> Rezervasyonlar { get; set; }
        public DbSet<Gider> Giderler { get; set; }
        public DbSet<Mekan> Mekanlar { get; set; }

        // Decimal alanların SQL hassasiyet (precision) ayarları
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // YENİ: Mekanlar tablosunun adını SQL'de kesin olarak sabitliyoruz
            modelBuilder.Entity<Mekan>().ToTable("Mekanlar");

            modelBuilder.Entity<Paket>()
                .Property(p => p.Fiyat)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Rezervasyon>()
                .Property(r => r.AlinanKapora)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Rezervasyon>()
                .Property(r => r.ToplamUcret)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Gider>()
                .Property(g => g.Tutar)
                .HasColumnType("decimal(18,2)");
        }
    }
}