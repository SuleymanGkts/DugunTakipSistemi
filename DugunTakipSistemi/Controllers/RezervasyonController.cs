using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using DugunTakipSistemi.Models;

namespace DugunTakipSistemi.Controllers
{
    // Formdan gelen verileri (JSON) C# tarafında karşılayacak olan Taşıyıcı Sınıf (DTO)
    public class RezervasyonDTO
    {
        public string AnaAdSoyad { get; set; }
        public string AnaTelefon { get; set; }
        public string? Email { get; set; }
        public string? Adres { get; set; }

        public string GelinAd { get; set; }
        public string DamatAd { get; set; }
        public string GelinTC { get; set; }
        public string DamatTC { get; set; }
        public string GelinTel { get; set; }
        public string DamatTel { get; set; }

        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }

        public int MekanId { get; set; }       // Formdan gelen seçili Mekan ID'si
        public int PaketId { get; set; }       // Formdan gelen seçili Paket ID'si
        public decimal PaketFiyati { get; set; }
        public decimal Kapora { get; set; }
        public string? Notlar { get; set; }
    }

    public class RezervasyonController : Controller
    {
        // Veritabanı bağlantı nesnesi
        private readonly ApplicationDbContext _context;

        // Constructor (Yapıcı Metot) - Veritabanı context'ini içeri alıyoruz
        public RezervasyonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Takvim ve Rezervasyon sayfasını, veritabanındaki paketlerle birlikte ekrana getiren metot
        public IActionResult Index()
        {
            var paketler = _context.Paketler.ToList();
            return View(paketler);
        }

        // Formdaki Kaydet butonuna basıldığında tetiklenen AJAX (POST) metodu
        [HttpPost]
        public IActionResult YeniKayit([FromBody] RezervasyonDTO veri)
        {
            try
            {
                // 1. ADIM: Önce Müşteriyi SQL'e ekleyelim
                var yeniMusteri = new Musteri
                {
                    AdSoyad = veri.AnaAdSoyad,
                    Telefon = veri.AnaTelefon,
                    Email = veri.Email,
                    Adres = veri.Adres,

                    GelinAdSoyad = veri.GelinAd,
                    DamatAdSoyad = veri.DamatAd,
                    GelinTC = veri.GelinTC,
                    DamatTC = veri.DamatTC,
                    GelinTelefon = veri.GelinTel,
                    DamatTelefon = veri.DamatTel,
                    Notlar = veri.Notlar
                };

                _context.Musteriler.Add(yeniMusteri);
                _context.SaveChanges(); // Müşteri kaydedildi ve ID'si oluştu.

                // 2. ADIM: Rezervasyon kaydını oluşturalım
                var yeniRezervasyon = new Rezervasyon
                {
                    MusteriId = yeniMusteri.Id,
                    BaslangicTarihi = veri.Baslangic,
                    BitisTarihi = veri.Bitis,
                    SozlesmeDetayi = "Düğün Organizasyonu",
                    ToplamUcret = veri.PaketFiyati,
                    AlinanKapora = veri.Kapora,
                    MekanId = veri.MekanId,
                    PaketId = veri.PaketId > 0 ? veri.PaketId : 1
                };

                _context.Rezervasyonlar.Add(yeniRezervasyon);
                _context.SaveChanges(); // Rezervasyon kaydedildi.

                return Json(new { success = true, mesaj = "Kayıt Başarılı!" });
            }
            catch (Exception ex)
            {
                string hataDetayi = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, mesaj = "HATA DETAYI: " + hataDetayi });
            }
        }

        // Takvimin okuyabilmesi için SQL'deki rezervasyonları JSON formatında veren metot
        [HttpGet]
        public IActionResult GetRezervasyonlar()
        {
            var rezervasyonlar = _context.Rezervasyonlar
                .Include(r => r.Musteri)
                .ToList();

            var liste = rezervasyonlar.Select(r => new {
                id = r.Id,
                title = "Düğün Organizasyonu",
                start = r.BaslangicTarihi.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = r.BitisTarihi.ToString("yyyy-MM-ddTHH:mm:ss"),
                extendedProps = new
                {
                    gelin = r.Musteri != null ? r.Musteri.GelinAdSoyad : "Misafir",
                    damat = r.Musteri != null ? r.Musteri.DamatAdSoyad : ""
                },
                backgroundColor = "#0d6efd"
            }).ToList();

            return Json(liste);
        }

        // Mekanları JSON olarak döndüren metot
        [HttpGet]
        public IActionResult GetMekanlar()
        {
            var mekanlar = _context.Mekanlar.Select(m => new { id = m.Id, mekanAdi = m.MekanAdi }).ToList();
            return Json(mekanlar);
        }

        // Hızlıca yeni mekan ekleyen metot
        [HttpPost]
        public IActionResult MekanEkle([FromBody] Mekan yeniMekan)
        {
            if (yeniMekan != null && !string.IsNullOrEmpty(yeniMekan.MekanAdi))
            {
                _context.Mekanlar.Add(yeniMekan);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public IActionResult MekanSil(int id)
        {
            var mekan = _context.Mekanlar.Find(id);
            if (mekan == null)
            {
                return Json(new { success = false, mesaj = "Mekan bulunamadı." });
            }

            // İsteğe bağlı: Bu mekana ait rezervasyon var mı kontrolü eklenebilir
            bool rezinVar = _context.Rezervasyonlar.Any(r => r.MekanId == id);
            if (rezinVar)
            {
                return Json(new { success = false, mesaj = "Bu mekana ait aktif rezervasyonlar olduğu için silinemez!" });
            }

            _context.Mekanlar.Remove(mekan);
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}