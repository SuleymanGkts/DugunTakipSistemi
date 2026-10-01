using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using DugunTakipSistemi.Models;

namespace DugunTakipSistemi.Controllers
{
    public class FinansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinansController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var rezervasyonlar = _context.Rezervasyonlar.Include(r => r.Musteri).ToList();
            var odemeler = _context.Odemeler.Include(o => o.Rezervasyon).ThenInclude(r => r.Musteri).ToList();
            var finansHareketler = _context.FinansHareketler.Include(f => f.Musteri).ToList();
            var tedarikciler = _context.Tedarikciler.ToList();

            // 1. KART HESAPLAMALARI
            decimal rezTahsilati = odemeler.Sum(o => o.Tutar);
            decimal ekGelirler = finansHareketler.Where(f => f.Tur == "Ek Gelir").Sum(f => f.Tutar);
            decimal toplamGelir = rezTahsilati + ekGelirler;

            decimal toplamGider = finansHareketler.Where(f => f.Tur == "Gider").Sum(f => f.Tutar);
            decimal rezBekleyen = rezervasyonlar.Sum(r => (r.ToplamUcret - r.AlinanKapora));
            decimal manuelAlacaklar = finansHareketler.Where(f => f.Tur == "Manuel Alacak").Sum(f => f.Tutar);

            ViewBag.ToplamRezervasyon = rezervasyonlar.Count;
            ViewBag.ToplamGelir = toplamGelir;
            ViewBag.BekleyenOdemeler = rezBekleyen + manuelAlacaklar;
            ViewBag.ToplamGider = toplamGider;
            ViewBag.NetKar = toplamGelir - toplamGider;

            // 2. SEKMELER İÇİN VERİLER
            ViewBag.TumMusteriler = _context.Musteriler.ToList();
            ViewBag.Tedarikciler = tedarikciler;
            ViewBag.ManuelAlacaklar = finansHareketler.Where(f => f.Tur == "Manuel Alacak").ToList();

            ViewBag.MusteriCarileri = rezervasyonlar
                .GroupBy(r => r.Musteri)
                .Select(g => new {
                    Musteri = g.Key,
                    ToplamIslem = g.Sum(x => x.ToplamUcret),
                    Odenen = g.Sum(x => x.AlinanKapora),
                    Kalan = g.Sum(x => x.ToplamUcret - x.AlinanKapora)
                }).ToList();

            // 3. SON HAREKETLER (Birleşik Liste)
            var islemListesi = new List<dynamic>();
            foreach (var o in odemeler)
            {
                var ciftAd = (o.Rezervasyon.Musteri.GelinAdSoyad != null) ? $"{o.Rezervasyon.Musteri.GelinAdSoyad} & {o.Rezervasyon.Musteri.DamatAdSoyad}" : o.Rezervasyon.Musteri.AdSoyad;
                islemListesi.Add(new { Id = o.Id, Tur = "Tahsilat", Aciklama = ciftAd + " - " + o.IslemTuru, Tutar = o.Tutar, Tarih = o.Tarih, IsPositive = true, IsMusteri = true });
            }
            foreach (var f in finansHareketler)
            {
                if (f.Tur == "Manuel Alacak") continue; // Alacaklar kasaya giren para değildir, borç kaydıdır
                var muhatap = f.MusteriId.HasValue ? f.Musteri.AdSoyad : f.MuhatapIsim;
                islemListesi.Add(new { Id = f.Id, Tur = f.Tur, Aciklama = f.Aciklama + (muhatap != null ? $" ({muhatap})" : ""), Tutar = f.Tutar, Tarih = f.Tarih, IsPositive = f.Tur == "Ek Gelir", IsMusteri = false });
            }
            ViewBag.SonHareketler = islemListesi.OrderByDescending(x => x.Tarih).Take(20).ToList();

            return View();
        }

        [HttpGet]
        public IActionResult GetDonemOzeti(string tip)
        {
            var baslangic = DateTime.Today; var bitis = DateTime.Today.AddDays(1).AddTicks(-1);
            if (tip == "Aylik") { baslangic = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); bitis = baslangic.AddMonths(1).AddTicks(-1); }
            else if (tip == "Yillik") { baslangic = new DateTime(DateTime.Today.Year, 1, 1); bitis = baslangic.AddYears(1).AddTicks(-1); }

            var dOdemeler = _context.Odemeler.Where(o => o.Tarih >= baslangic && o.Tarih <= bitis).Sum(o => o.Tutar);
            var dEkGelir = _context.FinansHareketler.Where(f => f.Tur == "Ek Gelir" && f.Tarih >= baslangic && f.Tarih <= bitis).Sum(f => f.Tutar);
            var dGider = _context.FinansHareketler.Where(f => f.Tur == "Gider" && f.Tarih >= baslangic && f.Tarih <= bitis).Sum(f => f.Tutar);

            var gelir = dOdemeler + dEkGelir; var net = gelir - dGider;
            return Json(new { gelir = gelir.ToString("N2"), gider = dGider.ToString("N2"), net = net.ToString("N2") });
        }

        [HttpPost]
        public IActionResult FinansIslemEkle(string tur, decimal tutar, string aciklama, DateTime tarih, int? musteriId, string muhatapIsim)
        {
            if (tutar <= 0) return Json(new { success = false, mesaj = "Tutar 0'dan büyük olmalıdır." });
            _context.FinansHareketler.Add(new FinansHareket { Tur = tur, Tutar = tutar, Aciklama = aciklama, Tarih = tarih, MusteriId = musteriId, MuhatapIsim = muhatapIsim });
            _context.SaveChanges(); return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult TedarikciEkle(string ad, string telefon, string sektor, decimal bakiye)
        {
            _context.Tedarikciler.Add(new Tedarikci { Ad = ad, Telefon = telefon, Sektor = sektor, Bakiye = bakiye });
            _context.SaveChanges(); return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult TedarikciOdemeYap(int tedarikciId, decimal tutar, string aciklama)
        {
            var t = _context.Tedarikciler.Find(tedarikciId);
            if (t != null && tutar > 0)
            {
                t.Bakiye -= tutar; // Borcumuz azaldı
                _context.FinansHareketler.Add(new FinansHareket { Tur = "Gider", Tutar = tutar, Aciklama = $"Tedarikçi Ödemesi: {t.Ad} - {aciklama}", Tarih = DateTime.Now });
                _context.SaveChanges(); return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult OdemeAl(int musteriId, decimal tutar, string yontem, string aciklama)
        {
            // Müşterinin en eski borçlu rezervasyonunu bulup oradan düşeriz
            var rez = _context.Rezervasyonlar.Where(r => r.MusteriId == musteriId && r.ToplamUcret > r.AlinanKapora).OrderBy(r => r.BaslangicTarihi).FirstOrDefault();
            if (rez != null && tutar > 0)
            {
                rez.AlinanKapora += tutar;
                _context.Odemeler.Add(new Odeme { RezervasyonId = rez.Id, Tutar = tutar, IslemTuru = string.IsNullOrEmpty(aciklama) ? "Cari Tahsilat" : aciklama, OdemeYontemi = yontem, Tarih = DateTime.Now });
                _context.SaveChanges(); return Json(new { success = true });
            }
            return Json(new { success = false, mesaj = "Müşterinin açık bakiyeli işlemi bulunamadı." });
        }

        [HttpPost]
        public IActionResult HareketSil(int id, bool isMusteri)
        {
            if (isMusteri)
            {
                var o = _context.Odemeler.Include(x => x.Rezervasyon).FirstOrDefault(x => x.Id == id);
                if (o != null) { o.Rezervasyon.AlinanKapora -= o.Tutar; _context.Odemeler.Remove(o); _context.SaveChanges(); return Json(new { success = true }); }
            }
            else
            {
                var f = _context.FinansHareketler.Find(id);
                if (f != null) { _context.FinansHareketler.Remove(f); _context.SaveChanges(); return Json(new { success = true }); }
            }
            return Json(new { success = false });
        }
    }
}