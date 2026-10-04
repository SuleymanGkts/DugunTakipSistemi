namespace DugunTakipSistemi.Models
{
    public class Musteri
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string Telefon { get; set; }
        public string? Email { get; set; }
        public string? Adres { get; set; } // Hataları kesmek için geri koyduk

        public string? GelinAdSoyad { get; set; }
        public string? GelinTelefon { get; set; }
        public string? GelinTC { get; set; }
        public string? GelinAdres { get; set; } // Gelin Evi

        public string? DamatAdSoyad { get; set; }
        public string? DamatTelefon { get; set; }
        public string? DamatTC { get; set; }
        public string? DamatAdres { get; set; } // Damat Evi

        public string? Notlar { get; set; }
        public ICollection<Rezervasyon>? Rezervasyonlar { get; set; }
    }
}
