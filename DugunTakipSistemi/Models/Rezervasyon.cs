using System.ComponentModel.DataAnnotations;

namespace DugunTakipSistemi.Models
{
    public class Rezervasyon
    {
        public int Id { get; set; }

        // Müşteri Bağlantısı
        public int MusteriId { get; set; }
        public Musteri Musteri { get; set; }

        // Paket/Hizmet Bağlantısı
        public int PaketId { get; set; }
        public Paket Paket { get; set; }

        public DateTime BaslangicTarihi { get; set; } // Takvim başlangıç
        public DateTime BitisTarihi { get; set; }   // Takvim bitiş

        public decimal ToplamUcret { get; set; }
        public decimal AlinanKapora { get; set; }
        public decimal KalanBakiye => ToplamUcret - AlinanKapora;

        public string? SozlesmeDetayi { get; set; }
        public int MekanId { get; set; }
        public Mekan? Mekan { get; set; }
        [DataType(DataType.MultilineText)]
        public string? Notlar { get; set; }

    }
}