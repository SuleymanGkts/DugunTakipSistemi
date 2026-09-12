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
    }
}