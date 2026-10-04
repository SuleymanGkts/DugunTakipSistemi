using System;

namespace DugunTakipSistemi.Models
{
    public class Teklif
    {
        public int Id { get; set; }
        public string MusteriAdSoyad { get; set; }
        public string? Telefon { get; set; }
        public decimal Fiyat { get; set; }
        public string? Aciklama { get; set; }
        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}