namespace DugunTakipSistemi.Models
{
    public class Gider
    {
        public int Id { get; set; }
        public string Baslik { get; set; } // Örn: Araç Yakıtı, Dış Çekim İzni
        public decimal Tutar { get; set; }
        public DateTime Tarih { get; set; }
        public string? Aciklama { get; set; }
    }
}