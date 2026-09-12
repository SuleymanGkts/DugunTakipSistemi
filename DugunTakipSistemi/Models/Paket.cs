namespace DugunTakipSistemi.Models
{
    public class Paket
    {
        public int Id { get; set; }
        public string PaketAdi { get; set; }
        public decimal Fiyat { get; set; }
        public int SureSaat { get; set; } // Takvimde kaç saat yer kaplayacağı
    }
}