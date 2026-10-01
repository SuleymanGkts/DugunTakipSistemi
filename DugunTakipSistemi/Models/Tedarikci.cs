namespace DugunTakipSistemi.Models
{
    public class Tedarikci
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public string? Telefon { get; set; }
        public string? Sektor { get; set; }
        public decimal Bakiye { get; set; } // Bizim ona olan borcumuz
    }
}