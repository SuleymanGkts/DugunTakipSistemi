namespace DugunTakipSistemi.Models
{
    public class Musteri
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string Telefon { get; set; }
        public string? Email { get; set; }
        public string? Adres { get; set; } // Yeni Eklendi

        public string? GelinAdSoyad { get; set; }
        public string? GelinTelefon { get; set; } // Yeni Eklendi

        public string? DamatAdSoyad { get; set; }
        public string? DamatTelefon { get; set; } // Yeni Eklendi

        public string? Notlar { get; set; } // İşletmeye özel iç notlar

        public ICollection<Rezervasyon>? Rezervasyonlar { get; set; }
    }
}