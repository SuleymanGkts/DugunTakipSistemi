using Microsoft.EntityFrameworkCore;

namespace DugunTakipSistemi.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Mevcut Tablolar
        public DbSet<Musteri> Musteriler { get; set; }
        public DbSet<Paket> Paketler { get; set; }
        public DbSet<Rezervasyon> Rezervasyonlar { get; set; }
        public DbSet<Gider> Giderler { get; set; }
        public DbSet<Mekan> Mekanlar { get; set; }

        // -------- YENİ EKLENEN TABLOLAR --------
        public DbSet<Odeme> Odemeler { get; set; }
        public DbSet<Personel> Personeller { get; set; }
        public DbSet<GenelNot> GenelNotlar { get; set; }
        public DbSet<FinansHareket> FinansHareketler { get; set; }
        public DbSet<Tedarikci> Tedarikciler { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Mekan>().ToTable("Mekanlar");

            modelBuilder.Entity<Paket>().Property(p => p.Fiyat).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Rezervasyon>().Property(r => r.AlinanKapora).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Rezervasyon>().Property(r => r.ToplamUcret).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Gider>().Property(g => g.Tutar).HasColumnType("decimal(18,2)");

            // YENİ: Ödeme tablosundaki tutar hassasiyeti
            modelBuilder.Entity<Odeme>().Property(o => o.Tutar).HasColumnType("decimal(18,2)");
        }
    }
}