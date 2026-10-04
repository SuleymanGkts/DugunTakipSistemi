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

        private string MusteriIsmiFormatla(Musteri m)
        {
            if (m == null) return "Bilinmeyen Müşteri";
            bool gelinVar = !string.IsNullOrWhiteSpace(m.GelinAdSoyad);
            bool damatVar = !string.IsNullOrWhiteSpace(m.DamatAdSoyad);

            if (gelinVar && damatVar) return $"{m.GelinAdSoyad} & {m.DamatAdSoyad} ({m.AdSoyad})";
            if (gelinVar) return $"{m.GelinAdSoyad} ({m.AdSoyad})";
            if (damatVar) return $"{m.DamatAdSoyad} ({m.AdSoyad})";
            return !string.IsNullOrWhiteSpace(m.AdSoyad) ? m.AdSoyad : "İsimsiz Müşteri";
        }

        public IActionResult Index()
        {
            var rezervasyonlar = _context.Rezervasyonlar.Include(r => r.Musteri).Include(r => r.Mekan).ToList();
            var odemeler = _context.Odemeler.Include(o => o.Rezervasyon).ThenInclude(r => r.Musteri).ToList();
            var finansHareketler = _context.FinansHareketler.Include(f => f.Musteri).ToList();
            var tedarikciler = _context.Tedarikciler.ToList();

            // 1. ÜST KART HESAPLAMALARI
            decimal rezTahsilati = odemeler.Sum(o => o.Tutar);
            decimal ekGelirler = finansHareketler.Where(f => f.Tur == "Ek Gelir").Sum(f => f.Tutar);
            decimal toplamGelir = rezTahsilati + ekGelirler;

            decimal toplamGider = finansHareketler.Where(f => f.Tur == "Gider").Sum(f => f.Tutar);

            decimal rezBekleyen = rezervasyonlar.Sum(r => Math.Max(0, r.ToplamUcret - r.AlinanKapora));
            decimal manuelAlacaklar = finansHareketler.Where(f => f.Tur == "Manuel Alacak").Sum(f => f.Tutar);
            decimal tedarikciBorclari = tedarikciler.Sum(t => t.Bakiye);

            ViewBag.ToplamRezervasyon = rezervasyonlar.Count;
            ViewBag.ToplamGelir = toplamGelir;
            ViewBag.BekleyenOdemeler = rezBekleyen + manuelAlacaklar;
            ViewBag.ToplamGider = toplamGider;
            ViewBag.TedarikciBorclari = tedarikciBorclari;
            ViewBag.NetKar = toplamGelir - toplamGider;

            // 2. MÜŞTERİDEN ALINACAK BORÇLAR LİSTESİ
            var musteriBorcListesi = rezervasyonlar
                .Where(r => r.Musteri != null)
                .GroupBy(r => r.Musteri)
                .Select(g => new {
                    MusteriId = g.Key.Id,
                    AdSoyad = g.Key.AdSoyad,
                    GorunenIsim = MusteriIsmiFormatla(g.Key),
                    Telefon = g.Key.Telefon ?? "-",
                    ToplamBorc = g.Sum(x => x.ToplamUcret),
                    Odenen = g.Sum(x => x.AlinanKapora),
                    Kalan = g.Sum(x => Math.Max(0, x.ToplamUcret - x.AlinanKapora))
                })
                .OrderByDescending(x => x.Kalan)
                .ToList();

            ViewBag.MusteriBorclari = musteriBorcListesi;
            ViewBag.TumMusteriler = _context.Musteriler.ToList();
            ViewBag.Tedarikciler = tedarikciler.OrderByDescending(t => t.Bakiye).ToList();
            ViewBag.ManuelAlacaklar = finansHareketler.Where(f => f.Tur == "Manuel Alacak").OrderByDescending(f => f.Tarih).ToList();

            // 3. SON HAREKETLER (Kasa & Gider Geçmişi - Tarihe Göre Sıralı ve Detaylı)
            var islemListesi = new List<dynamic>();
            foreach (var o in odemeler)
            {
                var mIsim = MusteriIsmiFormatla(o.Rezervasyon?.Musteri);
                var mekanAd = o.Rezervasyon?.Mekan != null ? o.Rezervasyon.Mekan.MekanAdi : "Belirtilmemiş";
                var orgTuru = o.Rezervasyon?.SozlesmeDetayi ?? "Organizasyon";

                islemListesi.Add(new
                {
                    Id = o.Id,
                    Tur = "Müşteri Tahsilatı",
                    Baslik = $"{mIsim} — {o.IslemTuru}",
                    Muhatap = mIsim,
                    Yontem = o.OdemeYontemi ?? "Nakit",
                    DetayBilgi = $"{orgTuru} Organizasyonu | Ödeme Yöntemi: {o.OdemeYontemi ?? "Nakit"} | Açıklama: {o.IslemTuru}",
                    Tutar = o.Tutar,
                    Tarih = o.Tarih,
                    IsPositive = true,
                    IsMusteri = true
                });
            }
            foreach (var f in finansHareketler)
            {
                if (f.Tur == "Manuel Alacak") continue; // Alacak kaydı kasa hareketi değildir, tahsil edilince kasaya girer
                var muhatap = f.MusteriId.HasValue && f.Musteri != null ? f.Musteri.AdSoyad : (!string.IsNullOrWhiteSpace(f.MuhatapIsim) ? f.MuhatapIsim : "Genel İşlem");

                islemListesi.Add(new
                {
                    Id = f.Id,
                    Tur = f.Tur,
                    Baslik = !string.IsNullOrWhiteSpace(muhatap) && muhatap != "Genel İşlem" ? $"{f.Aciklama} ({muhatap})" : f.Aciklama,
                    Muhatap = muhatap,
                    Yontem = "Kasa İşlemi",
                    DetayBilgi = $"İşlem Türü: {f.Tur} | İlgili Kişi/Kurum: {muhatap} | Açıklama: {f.Aciklama}",
                    Tutar = f.Tutar,
                    Tarih = f.Tarih,
                    IsPositive = f.Tur == "Ek Gelir",
                    IsMusteri = false
                });
            }
            ViewBag.SonHareketler = islemListesi.OrderByDescending(x => x.Tarih).ToList();

            return View();
        }

        // --- DÖNEM ÖZETİ (GÜNLÜK / AYLIK / YILLIK + DÖNEM HAREKETLERİ) ---
        [HttpGet]
        public IActionResult GetDonemOzeti(string tip, int? yil, int? ay, DateTime? gun)
        {
            DateTime baslangic;
            DateTime bitis;
            string donemAciklama = "";

            int seciliYil = yil ?? DateTime.Today.Year;
            int seciliAy = ay ?? DateTime.Today.Month;

            if (tip == "Gunluk")
            {
                baslangic = gun.HasValue ? gun.Value.Date : DateTime.Today;
                bitis = baslangic.AddDays(1).AddTicks(-1);
                donemAciklama = baslangic.ToString("dd.MM.yyyy") + " Günlük Özeti";
            }
            else if (tip == "Yillik")
            {
                baslangic = new DateTime(seciliYil, 1, 1);
                bitis = baslangic.AddYears(1).AddTicks(-1);
                donemAciklama = seciliYil + " Yılı Genel Özeti";
            }
            else // Aylık
            {
                baslangic = new DateTime(seciliYil, seciliAy, 1);
                bitis = baslangic.AddMonths(1).AddTicks(-1);
                string[] aylar = { "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
                donemAciklama = $"{aylar[seciliAy]} {seciliYil} Dönem Özeti";
            }

            var odemeler = _context.Odemeler
                .Include(o => o.Rezervasyon).ThenInclude(r => r.Musteri)
                .Where(o => o.Tarih >= baslangic && o.Tarih <= bitis)
                .ToList();

            var finanslar = _context.FinansHareketler
                .Include(f => f.Musteri)
                .Where(f => (f.Tur == "Ek Gelir" || f.Tur == "Gider") && f.Tarih >= baslangic && f.Tarih <= bitis)
                .ToList();

            decimal dOdemeler = odemeler.Sum(o => o.Tutar);
            decimal dEkGelir = finanslar.Where(f => f.Tur == "Ek Gelir").Sum(f => f.Tutar);
            decimal dGider = finanslar.Where(f => f.Tur == "Gider").Sum(f => f.Tutar);

            decimal gelir = dOdemeler + dEkGelir;
            decimal net = gelir - dGider;

            // O döneme ait hareketlerin listesi
            var hareketler = new List<object>();
            foreach (var o in odemeler)
            {
                hareketler.Add(new
                {
                    tarih = o.Tarih.ToString("dd.MM.yyyy HH:mm"),
                    baslik = MusteriIsmiFormatla(o.Rezervasyon?.Musteri) + " - " + o.IslemTuru,
                    tur = "Tahsilat (" + (o.OdemeYontemi ?? "Nakit") + ")",
                    tutar = o.Tutar.ToString("N2"),
                    isPositive = true,
                    rawDate = o.Tarih
                });
            }
            foreach (var f in finanslar)
            {
                var muhatap = f.MusteriId.HasValue && f.Musteri != null ? f.Musteri.AdSoyad : f.MuhatapIsim;
                hareketler.Add(new
                {
                    tarih = f.Tarih.ToString("dd.MM.yyyy HH:mm"),
                    baslik = f.Aciklama + (!string.IsNullOrWhiteSpace(muhatap) ? $" ({muhatap})" : ""),
                    tur = f.Tur,
                    tutar = f.Tutar.ToString("N2"),
                    isPositive = f.Tur == "Ek Gelir",
                    rawDate = f.Tarih
                });
            }

            var siraliHareketler = hareketler.OrderByDescending(x => ((dynamic)x).rawDate).ToList();

            return Json(new
            {
                gelir = gelir.ToString("N2"),
                gider = dGider.ToString("N2"),
                net = net.ToString("N2"),
                isPositive = net >= 0,
                aciklama = donemAciklama,
                islemSayisi = siraliHareketler.Count,
                hareketler = siraliHareketler
            });
        }

        // --- GELİR / GİDER / BORÇ EKLEME ---
        [HttpPost]
        public IActionResult FinansIslemEkle(string tur, decimal tutar, string aciklama, DateTime tarih, int? musteriId, string muhatapIsim)
        {
            if (tutar <= 0) return Json(new { success = false, mesaj = "Tutar 0'dan büyük olmalıdır." });
            if (string.IsNullOrWhiteSpace(aciklama)) return Json(new { success = false, mesaj = "Açıklama alanı zorunludur." });

            var islemTarihi = tarih.Date == DateTime.Today ? DateTime.Now : tarih;

            _context.FinansHareketler.Add(new FinansHareket
            {
                Tur = tur,
                Tutar = tutar,
                Aciklama = aciklama,
                Tarih = islemTarihi,
                MusteriId = musteriId,
                MuhatapIsim = muhatapIsim
            });
            _context.SaveChanges();
            return Json(new { success = true });
        }

        // --- DIŞARIDAN EKLENEN ALACAĞI TAHSİL ETME ---
        [HttpPost]
        public IActionResult ManuelAlacakTahsilEt(int id, decimal tutar, string yontem)
        {
            var alacak = _context.FinansHareketler.Include(f => f.Musteri).FirstOrDefault(f => f.Id == id && f.Tur == "Manuel Alacak");
            if (alacak == null || tutar <= 0) return Json(new { success = false, mesaj = "Geçersiz işlem." });

            var muhatap = alacak.MusteriId.HasValue && alacak.Musteri != null ? alacak.Musteri.AdSoyad : alacak.MuhatapIsim;

            // Kasaya Ek Gelir (Tahsilat) olarak sokuyoruz
            _context.FinansHareketler.Add(new FinansHareket
            {
                Tur = "Ek Gelir",
                Tutar = tutar,
                Aciklama = $"Alacak Tahsilatı ({yontem}): {alacak.Aciklama}",
                Tarih = DateTime.Now,
                MusteriId = alacak.MusteriId,
                MuhatapIsim = muhatap
            });

            if (tutar >= alacak.Tutar)
                _context.FinansHareketler.Remove(alacak); // Borç tamamen bitti
            else
                alacak.Tutar -= tutar; // Kısmi ödeme düştü

            _context.SaveChanges();
            return Json(new { success = true });
        }

        // --- TEDARİKÇİ İŞLEMLERİ ---
        [HttpPost]
        public IActionResult TedarikciEkle(string ad, string telefon, string sektor, decimal bakiye)
        {
            if (string.IsNullOrWhiteSpace(ad)) return Json(new { success = false, mesaj = "Firma/Kişi adı zorunludur." });
            _context.Tedarikciler.Add(new Tedarikci { Ad = ad, Telefon = telefon, Sektor = sektor, Bakiye = bakiye });
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult TedarikciOdemeYap(int tedarikciId, decimal tutar, string aciklama)
        {
            var t = _context.Tedarikciler.Find(tedarikciId);
            if (t != null && tutar > 0)
            {
                t.Bakiye = Math.Max(0, t.Bakiye - tutar);
                _context.FinansHareketler.Add(new FinansHareket
                {
                    Tur = "Gider",
                    Tutar = tutar,
                    Aciklama = string.IsNullOrWhiteSpace(aciklama) ? $"Tedarikçi Ödemesi" : $"Tedarikçi Ödemesi: {aciklama}",
                    MuhatapIsim = t.Ad,
                    Tarih = DateTime.Now
                });
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult TedarikciSil(int id)
        {
            var t = _context.Tedarikciler.Find(id);
            if (t != null) { _context.Tedarikciler.Remove(t); _context.SaveChanges(); return Json(new { success = true }); }
            return Json(new { success = false });
        }

        // --- MÜŞTERİDEN BORÇ TAHSİLATI ---
        [HttpPost]
        public IActionResult OdemeAl(int musteriId, decimal tutar, string yontem, string aciklama)
        {
            var rezler = _context.Rezervasyonlar
                .Where(r => r.MusteriId == musteriId && r.ToplamUcret > r.AlinanKapora)
                .OrderBy(r => r.BaslangicTarihi)
                .ToList();

            if (!rezler.Any() || tutar <= 0)
                return Json(new { success = false, mesaj = "Müşterinin bekleyen borcu bulunamadı veya geçersiz tutar girildi." });

            var hedefRez = rezler.First();
            hedefRez.AlinanKapora += tutar;

            _context.Odemeler.Add(new Odeme
            {
                RezervasyonId = hedefRez.Id,
                Tutar = tutar,
                IslemTuru = string.IsNullOrWhiteSpace(aciklama) ? "Borç Tahsilatı" : aciklama,
                OdemeYontemi = yontem ?? "Nakit",
                Tarih = DateTime.Now
            });

            _context.SaveChanges();
            return Json(new { success = true });
        }

        // --- HAREKET SİLME ---
        [HttpPost]
        public IActionResult HareketSil(int id, bool isMusteri)
        {
            if (isMusteri)
            {
                var o = _context.Odemeler.Include(x => x.Rezervasyon).FirstOrDefault(x => x.Id == id);
                if (o != null)
                {
                    if (o.Rezervasyon != null) o.Rezervasyon.AlinanKapora = Math.Max(0, o.Rezervasyon.AlinanKapora - o.Tutar);
                    _context.Odemeler.Remove(o);
                    _context.SaveChanges();
                    return Json(new { success = true });
                }
            }
            else
            {
                var f = _context.FinansHareketler.Find(id);
                if (f != null)
                {
                    _context.FinansHareketler.Remove(f);
                    _context.SaveChanges();
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false });
        }
    }
}