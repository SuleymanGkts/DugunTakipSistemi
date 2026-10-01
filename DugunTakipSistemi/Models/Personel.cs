using System.Collections.Generic;

namespace DugunTakipSistemi.Models
{
    public class Personel
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string? Telefon { get; set; }
        public string Gorevi { get; set; }

        // YENİ EKLENEN SÜTUN: Personelin aldığı ücret/yevmiye
        public decimal GunlukUcret { get; set; }

        public ICollection<Rezervasyon>? Rezervasyonlar { get; set; }
    }
}