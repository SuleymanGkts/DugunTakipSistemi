using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DugunTakipSistemi.Models;

namespace DugunTakipSistemi.Controllers
{
    public class RezervasyonDTO
    {
        public int? RezervasyonId { get; set; }
        public int? MusteriId { get; set; }

        public string AnaAdSoyad { get; set; }
        public string AnaTelefon { get; set; }
        public string? Email { get; set; }
        public string? Adres { get; set; }

        public string? GelinAd { get; set; }
        public string? DamatAd { get; set; }
        public string? GelinTC { get; set; }
        public string? DamatTC { get; set; }
        public string? GelinTel { get; set; }
        public string? DamatTel { get; set; }

        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }

        public int MekanId { get; set; }
        public int? PaketId { get; set; }
        public decimal PaketFiyati { get; set; }
        public decimal Kapora { get; set; }
        public string? Notlar { get; set; }
        public int? KisiSayisi { get; set; }

        public string IslemTuru { get; set; } = "Düğün";
    }

    public class RezervasyonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RezervasyonController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index(int? seciliRezId)
        {
            ViewBag.Paketler = _context.Paketler.ToList();
            ViewBag.SeciliRezId = seciliRezId;
            return View();
        }

        [HttpPost]
        public IActionResult Kaydet([FromBody] RezervasyonDTO veri)
        {
            if (veri == null) return Json(new { success = false, mesaj = "Sisteme veri ulaşmadı." });

            try
            {
                Musteri musteri;

                if (veri.MusteriId.HasValue && veri.MusteriId > 0)
                {
                    musteri = _context.Musteriler.Find(veri.MusteriId.Value);
                    if (musteri == null) return Json(new { success = false, mesaj = "Müşteri bulunamadı!" });

                    musteri.AdSoyad = veri.AnaAdSoyad;
                    musteri.Telefon = veri.AnaTelefon;
                    if (!string.IsNullOrEmpty(veri.Email)) musteri.Email = veri.Email;
                    if (!string.IsNullOrEmpty(veri.GelinAd)) musteri.GelinAdSoyad = veri.GelinAd;
                    if (!string.IsNullOrEmpty(veri.DamatAd)) musteri.DamatAdSoyad = veri.DamatAd;
                    if (!string.IsNullOrEmpty(veri.GelinTel)) musteri.GelinTelefon = veri.GelinTel;
                    if (!string.IsNullOrEmpty(veri.DamatTel)) musteri.DamatTelefon = veri.DamatTel;
                }
                else
                {
                    musteri = new Musteri
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
                        DamatTelefon = veri.DamatTel
                    };
                    _context.Musteriler.Add(musteri);
                    _context.SaveChanges();
                }

                Rezervasyon rezervasyon;

                if (veri.RezervasyonId.HasValue && veri.RezervasyonId > 0)
                {
                    rezervasyon = _context.Rezervasyonlar.Include(r => r.Odemeler).FirstOrDefault(r => r.Id == veri.RezervasyonId);
                    if (rezervasyon == null) return Json(new { success = false, mesaj = "Rezervasyon bulunamadı!" });

                    rezervasyon.BaslangicTarihi = veri.Baslangic;
                    rezervasyon.BitisTarihi = veri.Bitis;
                    rezervasyon.MekanId = veri.MekanId;
                    rezervasyon.PaketId = veri.PaketId;
                    rezervasyon.SozlesmeDetayi = veri.IslemTuru;
                    rezervasyon.ToplamUcret = veri.PaketFiyati;
                    rezervasyon.Notlar = veri.Notlar;
                    rezervasyon.KisiSayisi = veri.KisiSayisi;

                    if (veri.Kapora > rezervasyon.AlinanKapora)
                    {
                        decimal fark = veri.Kapora - rezervasyon.AlinanKapora;
                        _context.Odemeler.Add(new Odeme { RezervasyonId = rezervasyon.Id, Tutar = fark, IslemTuru = "Ara Ödeme / Kapora İlavesi", OdemeYontemi = "Nakit", Tarih = DateTime.Now });
                        rezervasyon.AlinanKapora = veri.Kapora;
                    }
                }
                else
                {
                    rezervasyon = new Rezervasyon { MusteriId = musteri.Id, BaslangicTarihi = veri.Baslangic, BitisTarihi = veri.Bitis, SozlesmeDetayi = veri.IslemTuru, ToplamUcret = veri.PaketFiyati, AlinanKapora = veri.Kapora, MekanId = veri.MekanId, PaketId = veri.PaketId, Notlar = veri.Notlar, KisiSayisi = veri.KisiSayisi };
                    _context.Rezervasyonlar.Add(rezervasyon);
                    _context.SaveChanges();

                    if (veri.Kapora > 0)
                    {
                        _context.Odemeler.Add(new Odeme { RezervasyonId = rezervasyon.Id, Tutar = veri.Kapora, IslemTuru = "İlk Kapora", OdemeYontemi = "Nakit", Tarih = DateTime.Now });
                    }
                }

                _context.SaveChanges();
                return Json(new { success = true, mesaj = "Kayıt Başarılı!", rezId = rezervasyon.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mesaj = "HATA DETAYI: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message) });
            }
        }

        [HttpPost]
        public IActionResult NotGuncelle(int id, string not)
        {
            var r = _context.Rezervasyonlar.Find(id);
            if (r != null) { r.Notlar = not; _context.SaveChanges(); return Json(new { success = true }); }
            return Json(new { success = false });
        }

        [HttpGet]
        public IActionResult GetRezervasyonlar()
        {
            var rezervasyonlar = _context.Rezervasyonlar.Include(r => r.Musteri).Include(r => r.Mekan).Include(r => r.Odemeler).ToList();
            var liste = rezervasyonlar.Select(r => new {
                id = r.Id,
                title = r.Musteri != null ? (r.Musteri.GelinAdSoyad + " & " + r.Musteri.DamatAdSoyad) : "İsimsiz",
                start = r.BaslangicTarihi.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = r.BitisTarihi.ToString("yyyy-MM-ddTHH:mm:ss"),
                extendedProps = new { islemTuru = r.SozlesmeDetayi ?? "Organizasyon", mekan = r.Mekan != null ? r.Mekan.MekanAdi : "Mekan Yok", toplam = r.ToplamUcret, kapora = r.AlinanKapora, kalan = r.ToplamUcret - r.AlinanKapora, anaAd = r.Musteri?.AdSoyad, telefon = r.Musteri?.Telefon, notlar = r.Notlar, kisiSayisi = r.KisiSayisi }
            }).ToList();
            return Json(liste);
        }

        [HttpGet]
        public IActionResult GetRezervasyonDetay(int id)
        {
            var r = _context.Rezervasyonlar.Include(x => x.Musteri).Include(x => x.Mekan).Include(x => x.Odemeler).Include(x => x.Personeller).FirstOrDefault(x => x.Id == id);
            if (r == null) return Json(new { success = false });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = r.Id,
                    musteriId = r.MusteriId,
                    baslangic = r.BaslangicTarihi.ToString("yyyy-MM-ddTHH:mm"),
                    bitis = r.BitisTarihi.ToString("yyyy-MM-ddTHH:mm"),
                    tarihFormatli = r.BaslangicTarihi.ToString("dd MMMM yyyy dddd"),
                    saatFormatli = r.BaslangicTarihi.ToString("HH:mm"),
                    islemTuru = r.SozlesmeDetayi,
                    mekanId = r.MekanId,
                    mekanAd = r.Mekan != null ? r.Mekan.MekanAdi : "-",
                    paketId = r.PaketId,
                    toplamUcret = r.ToplamUcret,
                    alinanKapora = r.AlinanKapora,
                    kalanBakiye = r.ToplamUcret - r.AlinanKapora,
                    notlar = r.Notlar,
                    kisiSayisi = r.KisiSayisi,
                    anaAd = r.Musteri?.AdSoyad,
                    telefon = r.Musteri?.Telefon,
                    gelinAd = r.Musteri?.GelinAdSoyad,
                    damatAd = r.Musteri?.DamatAdSoyad,
                    gelinTel = r.Musteri?.GelinTelefon,
                    damatTel = r.Musteri?.DamatTelefon,
                    sozlesmeDosya = r.SozlesmeDosyaYolu, // SÖZLEŞME EKLENDİ

                    odemeler = r.Odemeler?.OrderByDescending(o => o.Tarih).Select(o => new { tarih = o.Tarih.ToString("dd.MM.yyyy HH:mm"), tutar = o.Tutar, tur = o.IslemTuru, yontem = o.OdemeYontemi }).ToList(),
                    personeller = r.Personeller?.Select(p => new { id = p.Id, adSoyad = p.AdSoyad, gorev = p.Gorevi, ucret = p.GunlukUcret, telefon = p.Telefon }).ToList()
                }
            });
        }

        [HttpPost]
        public IActionResult TahsilatYap(int rezervasyonId, decimal tutar, string yontem, string aciklama)
        {
            var rez = _context.Rezervasyonlar.Find(rezervasyonId);
            if (rez != null && tutar > 0)
            {
                rez.AlinanKapora += tutar;
                _context.Odemeler.Add(new Odeme { RezervasyonId = rezervasyonId, Tutar = tutar, IslemTuru = string.IsNullOrEmpty(aciklama) ? "Ara Ödeme" : aciklama, OdemeYontemi = yontem, Tarih = DateTime.Now });
                _context.SaveChanges(); return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // --- MEKAN İŞLEMLERİ ---
        [HttpGet] public IActionResult GetMekanlar() { return Json(_context.Mekanlar.Select(m => new { id = m.Id, mekanAdi = m.MekanAdi }).ToList()); }
        [HttpPost] public IActionResult MekanEkle([FromBody] Mekan yeniMekan) { if (yeniMekan != null && !string.IsNullOrEmpty(yeniMekan.MekanAdi)) { _context.Mekanlar.Add(yeniMekan); _context.SaveChanges(); return Json(new { success = true }); } return Json(new { success = false }); }
        [HttpPost] public IActionResult MekanSil(int id) { var mekan = _context.Mekanlar.Find(id); if (mekan == null) return Json(new { success = false }); if (_context.Rezervasyonlar.Any(r => r.MekanId == id)) return Json(new { success = false, mesaj = "Mekana ait rezervasyon var!" }); _context.Mekanlar.Remove(mekan); _context.SaveChanges(); return Json(new { success = true }); }

        // --- PERSONEL İŞLEMLERİ ---
        [HttpGet] public IActionResult GetPersoneller() { return Json(_context.Personeller.Select(p => new { id = p.Id, adSoyad = p.AdSoyad, gorev = p.Gorevi, ucret = p.GunlukUcret }).ToList()); }
        [HttpPost] public IActionResult PersonelEkle([FromBody] Personel per) { if (!string.IsNullOrEmpty(per.AdSoyad) && !string.IsNullOrEmpty(per.Gorevi)) { _context.Personeller.Add(per); _context.SaveChanges(); return Json(new { success = true }); } return Json(new { success = false, mesaj = "Ad ve Görev alanları zorunludur!" }); }
        [HttpPost] public IActionResult PersonelAta(int rezervasyonId, int personelId) { var rez = _context.Rezervasyonlar.Include(r => r.Personeller).FirstOrDefault(r => r.Id == rezervasyonId); var per = _context.Personeller.Find(personelId); if (rez != null && per != null) { if (!rez.Personeller.Any(p => p.Id == personelId)) { rez.Personeller.Add(per); _context.SaveChanges(); return Json(new { success = true }); } return Json(new { success = false, mesaj = "Bu personel zaten atanmış!" }); } return Json(new { success = false, mesaj = "Kayıt bulunamadı!" }); }
        [HttpPost] public IActionResult PersonelCikar(int rezervasyonId, int personelId) { var rez = _context.Rezervasyonlar.Include(r => r.Personeller).FirstOrDefault(r => r.Id == rezervasyonId); if (rez != null) { var per = rez.Personeller.FirstOrDefault(p => p.Id == personelId); if (per != null) { rez.Personeller.Remove(per); _context.SaveChanges(); return Json(new { success = true }); } } return Json(new { success = false }); }

        // --- SÖZLEŞME YAZDIR VE YÜKLE ---
        [HttpGet]
        public IActionResult SozlesmeYazdir(int id)
        {
            var rez = _context.Rezervasyonlar.Include(x => x.Musteri).Include(x => x.Mekan).FirstOrDefault(x => x.Id == id);
            if (rez == null) return NotFound("Bulunamadı");
            return View(rez);
        }

        [HttpPost]
        public async Task<IActionResult> SozlesmeYukle(int id, IFormFile dosya)
        {
            var rez = await _context.Rezervasyonlar.FindAsync(id);
            if (rez == null || dosya == null || dosya.Length == 0) return Json(new { success = false, mesaj = "Geçersiz dosya." });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "sozlesmeler");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(dosya.FileName);
            var uniqueFileName = $"sozlesme_{id}_{DateTime.Now.Ticks}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create)) { await dosya.CopyToAsync(stream); }

            rez.SozlesmeDosyaYolu = $"/uploads/sozlesmeler/{uniqueFileName}";
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}