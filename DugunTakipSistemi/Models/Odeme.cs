using System;

namespace DugunTakipSistemi.Models
{
    public class Odeme
    {
        public int Id { get; set; }

        public int RezervasyonId { get; set; }
        public Rezervasyon Rezervasyon { get; set; }

        public decimal Tutar { get; set; }
        public DateTime Tarih { get; set; } = DateTime.Now;

        public string IslemTuru { get; set; } // Örn: "Kapora", "Ara Ödeme", "Kalan Ödeme"
        public string OdemeYontemi { get; set; } // Örn: "Nakit", "Kredi Kartı", "Havale"
        public string? Aciklama { get; set; }
    }
}