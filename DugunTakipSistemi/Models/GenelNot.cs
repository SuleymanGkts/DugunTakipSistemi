using System;

namespace DugunTakipSistemi.Models
{
    public class GenelNot
    {
        public int Id { get; set; }
        public string Icerik { get; set; } // Notun metni
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    }
}