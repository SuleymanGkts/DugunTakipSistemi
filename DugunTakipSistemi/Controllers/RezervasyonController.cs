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
        public string? GelinAdres { get; set; }
        public string? DamatAdres { get; set; }

        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }

        public int MekanId { get; set; }
        public string? MekanAdi { get; set; }
        public int? PaketId { get; set; }
        public decimal PaketFiyati { get; set; }
        public decimal Kapora { get; set; }
        public string? Notlar { get; set; }
        public string? IslemTuru { get; set; }

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

        private string TakvimBaslikOlustur(Musteri m)
        {
            if (m == null) return "İsimsiz Kayıt";
            bool gelinVar = !string.IsNullOrWhiteSpace(m.GelinAdSoyad);
            bool damatVar = !string.IsNullOrWhiteSpace(m.DamatAdSoyad);

            if (gelinVar && damatVar) return $"{m.GelinAdSoyad} & {m.DamatAdSoyad}";
            if (gelinVar) return m.GelinAdSoyad;
            if (damatVar) return m.DamatAdSoyad;
            return !string.IsNullOrWhiteSpace(m.AdSoyad) ? m.AdSoyad : "İsimsiz Kayıt";
        }

        public IActionResult Index(int? seciliRezId)
        {
            ViewBag.Paketler = _context.Paketler.ToList();
            ViewBag.Mekanlar = _context.Mekanlar.ToList();
            ViewBag.SeciliRezId = seciliRezId;
            return View();
        }

        [HttpPost]
        public IActionResult Kaydet([FromBody] RezervasyonDTO veri)
        {
            if (veri == null) return Json(new { success = false, mesaj = "Sisteme veri ulaşmadı." });

            if (!string.IsNullOrWhiteSpace(veri.GelinTC) && veri.GelinTC.Trim().Length != 11)
                return Json(new { success = false, mesaj = "Gelin T.C. Kimlik Numarası 11 haneli olmalıdır!" });

            if (!string.IsNullOrWhiteSpace(veri.DamatTC) && veri.DamatTC.Trim().Length != 11)
                return Json(new { success = false, mesaj = "Damat T.C. Kimlik Numarası 11 haneli olmalıdır!" });

            try
            {
                if (veri.Bitis <= veri.Baslangic)
                {
                    veri.Bitis = veri.Baslangic.AddHours(4);
                }

                string girilenMekanAdi = string.IsNullOrWhiteSpace(veri.MekanAdi) ? "Belirtilmedi" : veri.MekanAdi.Trim();
                var mevcutMekan = _context.Mekanlar.FirstOrDefault(m => m.MekanAdi.ToLower() == girilenMekanAdi.ToLower());
                if (mevcutMekan == null)
                {
                    mevcutMekan = new Mekan { MekanAdi = girilenMekanAdi };
                    _context.Mekanlar.Add(mevcutMekan);
                    _context.SaveChanges();
                }
                veri.MekanId = mevcutMekan.Id;

                Musteri musteri = null;

                if (veri.RezervasyonId.HasValue && veri.RezervasyonId > 0)
                {
                    var mevcutRez = _context.Rezervasyonlar.AsNoTracking().FirstOrDefault(r => r.Id == veri.RezervasyonId.Value);
                    if (mevcutRez != null)
                    {
                        musteri = _context.Musteriler.Find(mevcutRez.MusteriId);
                    }
                }
                else if (veri.MusteriId.HasValue && veri.MusteriId > 0)
                {
                    musteri = _context.Musteriler.Find(veri.MusteriId.Value);
                }

                if (musteri != null)
                {
                    musteri.AdSoyad = veri.AnaAdSoyad;
                    musteri.Telefon = veri.AnaTelefon;
                    if (veri.Email != null) musteri.Email = veri.Email;
                    musteri.GelinAdSoyad = veri.GelinAd;
                    musteri.DamatAdSoyad = veri.DamatAd;
                    musteri.GelinTC = veri.GelinTC;
                    musteri.DamatTC = veri.DamatTC;
                    musteri.GelinTelefon = veri.GelinTel;
                    musteri.DamatTelefon = veri.DamatTel;
                    musteri.GelinAdres = veri.GelinAdres;
                    musteri.DamatAdres = veri.DamatAdres;
                    _context.SaveChanges();
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
                        DamatTelefon = veri.DamatTel,
                        GelinAdres = veri.GelinAdres,
                        DamatAdres = veri.DamatAdres
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
                    rezervasyon.PaketId = (veri.PaketId.HasValue && veri.PaketId > 0) ? veri.PaketId : null;
                    rezervasyon.ToplamUcret = veri.PaketFiyati;
                    rezervasyon.Notlar = veri.Notlar;

                    if (veri.Kapora > rezervasyon.AlinanKapora)
                    {
                        decimal fark = veri.Kapora - rezervasyon.AlinanKapora;
                        _context.Odemeler.Add(new Odeme { RezervasyonId = rezervasyon.Id, Tutar = fark, IslemTuru = "Ara Ödeme / Kapora İlavesi", OdemeYontemi = "Nakit", Tarih = DateTime.Now });
                        rezervasyon.AlinanKapora = veri.Kapora;
                    }
                }
                else
                {
                    rezervasyon = new Rezervasyon
                    {
                        MusteriId = musteri.Id,
                        BaslangicTarihi = veri.Baslangic,
                        BitisTarihi = veri.Bitis,
                        ToplamUcret = veri.PaketFiyati,
                        AlinanKapora = veri.Kapora,
                        MekanId = veri.MekanId,
                        PaketId = (veri.PaketId.HasValue && veri.PaketId > 0) ? veri.PaketId : null,
                        Notlar = veri.Notlar
                    };
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
            var rezervasyonlar = _context.Rezervasyonlar
                .Include(r => r.Musteri)
                .Include(r => r.Mekan)
                .Include(r => r.Paket)
                .Include(r => r.Odemeler)
                .ToList();

            var liste = rezervasyonlar.Select(r => {
                var bitis = r.BitisTarihi <= r.BaslangicTarihi ? r.BaslangicTarihi.AddHours(4) : r.BitisTarihi;
                return new
                {
                    id = r.Id,
                    title = TakvimBaslikOlustur(r.Musteri),
                    start = r.BaslangicTarihi.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = bitis.ToString("yyyy-MM-ddTHH:mm:ss"),
                    extendedProps = new
                    {
                        islemTuru = r.SozlesmeDetayi ?? "Çekim",
                        mekan = r.Mekan != null ? r.Mekan.MekanAdi : "Belirtilmedi",
                        paket = r.Paket != null ? r.Paket.PaketAdi : "",
                        toplam = r.ToplamUcret,
                        kapora = r.AlinanKapora,
                        kalan = r.ToplamUcret - r.AlinanKapora,
                        anaAd = r.Musteri?.AdSoyad,
                        telefon = r.Musteri?.Telefon,
                        notlar = r.Notlar
                    }
                };
            }).ToList();

            return Json(liste);
        }

        [HttpGet]
        public IActionResult GetRezervasyonDetay(int id)
        {
            var r = _context.Rezervasyonlar
                .Include(x => x.Musteri)
                .Include(x => x.Mekan)
                .Include(x => x.Paket)
                .Include(x => x.Odemeler)
                .Include(x => x.Personeller)
                .FirstOrDefault(x => x.Id == id);

            if (r == null) return Json(new { success = false });

            var bitis = r.BitisTarihi <= r.BaslangicTarihi ? r.BaslangicTarihi.AddHours(4) : r.BitisTarihi;

            return Json(new
            {
                success = true,
                data = new
                {
                    id = r.Id,
                    musteriId = r.MusteriId,
                    baslangic = r.BaslangicTarihi.ToString("yyyy-MM-ddTHH:mm"),
                    bitis = bitis.ToString("yyyy-MM-ddTHH:mm"),
                    tarihFormatli = r.BaslangicTarihi.ToString("dd MMMM yyyy dddd"),
                    saatFormatli = $"{r.BaslangicTarihi:HH:mm} - {bitis:HH:mm}",
                    islemTuru = r.SozlesmeDetayi,
                    mekanId = r.MekanId,
                    mekanAd = r.Mekan != null ? r.Mekan.MekanAdi : "-",
                    paketId = r.PaketId,
                    paketAd = r.Paket != null ? $"{r.Paket.PaketAdi} ({r.Paket.Fiyat:N0} ₺)" : "Standart / Özel Anlaşma",
                    toplamUcret = r.ToplamUcret,
                    alinanKapora = r.AlinanKapora,
                    kalanBakiye = r.ToplamUcret - r.AlinanKapora,
                    notlar = r.Notlar,
                    anaAd = r.Musteri?.AdSoyad,
                    telefon = r.Musteri?.Telefon,
                    gelinAd = r.Musteri?.GelinAdSoyad,
                    damatAd = r.Musteri?.DamatAdSoyad,
                    gelinTC = r.Musteri?.GelinTC,
                    damatTC = r.Musteri?.DamatTC,
                    gelinTel = r.Musteri?.GelinTelefon,
                    damatTel = r.Musteri?.DamatTelefon,
                    gelinAdres = r.Musteri?.GelinAdres,
                    damatAdres = r.Musteri?.DamatAdres,
                    sozlesmeDosya = r.SozlesmeDosyaYolu,

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

        [HttpGet] public IActionResult GetMekanlar() { return Json(_context.Mekanlar.Select(m => new { id = m.Id, mekanAdi = m.MekanAdi }).ToList()); }
        [HttpGet] public IActionResult GetPaketler() { return Json(_context.Paketler.Select(p => new { id = p.Id, paketAdi = p.PaketAdi, fiyat = p.Fiyat, sureSaat = p.SureSaat }).ToList()); }

        // --- PERSONEL İŞLEMLERİ ---
        [HttpGet] public IActionResult GetPersoneller() { return Json(_context.Personeller.Select(p => new { id = p.Id, adSoyad = p.AdSoyad, gorev = p.Gorevi, ucret = p.GunlukUcret }).ToList()); }
        [HttpPost] public IActionResult PersonelEkle([FromBody] Personel per) { if (!string.IsNullOrEmpty(per.AdSoyad) && !string.IsNullOrEmpty(per.Gorevi)) { _context.Personeller.Add(per); _context.SaveChanges(); return Json(new { success = true }); } return Json(new { success = false, mesaj = "Ad ve Görev alanları zorunludur!" }); }
        [HttpPost] public IActionResult PersonelAta(int rezervasyonId, int personelId) { var rez = _context.Rezervasyonlar.Include(r => r.Personeller).FirstOrDefault(r => r.Id == rezervasyonId); var per = _context.Personeller.Find(personelId); if (rez != null && per != null) { if (!rez.Personeller.Any(p => p.Id == personelId)) { rez.Personeller.Add(per); _context.SaveChanges(); return Json(new { success = true }); } return Json(new { success = false, mesaj = "Bu personel zaten atanmış!" }); } return Json(new { success = false, mesaj = "Kayıt bulunamadı!" }); }
        [HttpPost] public IActionResult PersonelCikar(int rezervasyonId, int personelId) { var rez = _context.Rezervasyonlar.Include(r => r.Personeller).FirstOrDefault(r => r.Id == rezervasyonId); if (rez != null) { var per = rez.Personeller.FirstOrDefault(p => p.Id == personelId); if (per != null) { rez.Personeller.Remove(per); _context.SaveChanges(); return Json(new { success = true }); } } return Json(new { success = false }); }

        // --- SÖZLEŞME YAZDIR VE YÜKLE ---
        [HttpGet]
        public IActionResult SozlesmeYazdir(int id)
        {
            var rez = _context.Rezervasyonlar.Include(x => x.Musteri).Include(x => x.Mekan).Include(x => x.Paket).FirstOrDefault(x => x.Id == id);
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