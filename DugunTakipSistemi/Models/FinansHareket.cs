using System;

namespace DugunTakipSistemi.Models
{
    public class FinansHareket
    {
        public int Id { get; set; }
        public string Tur { get; set; } // "Gider", "Ek Gelir", "Manuel Alacak"
        public decimal Tutar { get; set; }
        public string Aciklama { get; set; }
        public DateTime Tarih { get; set; }

        // YENİ EKLENENLER: Bu işlem kiminle yapıldı?
        public string? MuhatapIsim { get; set; } // Dışarıdan manuel biri için
        public int? MusteriId { get; set; }      // Sistemdeki müşteri için
        public Musteri? Musteri { get; set; }
    }
}