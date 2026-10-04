using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DugunTakipSistemi.Models;
using System.Linq;

namespace DugunTakipSistemi.Controllers
{
    public class MusterilerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MusterilerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string aramaParametresi, int? seciliMusteriId)
        {
            var sorgu = _context.Musteriler
                .Include(m => m.Rezervasyonlar)
                    .ThenInclude(r => r.Mekan)
                .Include(m => m.Rezervasyonlar)
                    .ThenInclude(r => r.Paket)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(aramaParametresi))
            {
                sorgu = sorgu.Where(m =>
                    m.AdSoyad.Contains(aramaParametresi) ||
                    m.Telefon.Contains(aramaParametresi) ||
                    (m.GelinAdSoyad != null && m.GelinAdSoyad.Contains(aramaParametresi)) ||
                    (m.DamatAdSoyad != null && m.DamatAdSoyad.Contains(aramaParametresi)));
            }

            var musteriler = sorgu.OrderByDescending(m => m.Id).ToList();

            ViewBag.Arama = aramaParametresi;
            ViewBag.SeciliMusteriId = seciliMusteriId;

            return View(musteriler);
        }

        // YENİ MÜŞTERİ EKLEME İŞLEMİ
        [HttpPost]
        public IActionResult Ekle(Musteri yeniMusteri)
        {
            if (yeniMusteri != null && !string.IsNullOrWhiteSpace(yeniMusteri.AdSoyad))
            {
                _context.Musteriler.Add(yeniMusteri);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // MÜŞTERİ GÜNCELLEME İŞLEMİ
        [HttpPost]
        public IActionResult Guncelle(Musteri guncelVeri)
        {
            var musteri = _context.Musteriler.Find(guncelVeri.Id);
            if (musteri != null)
            {
                musteri.AdSoyad = guncelVeri.AdSoyad;
                musteri.Telefon = guncelVeri.Telefon;
                musteri.Email = guncelVeri.Email;
                musteri.Adres = guncelVeri.Adres;
                musteri.GelinAdSoyad = guncelVeri.GelinAdSoyad;
                musteri.GelinTelefon = guncelVeri.GelinTelefon;
                musteri.GelinTC = guncelVeri.GelinTC;
                musteri.DamatAdSoyad = guncelVeri.DamatAdSoyad;
                musteri.DamatTelefon = guncelVeri.DamatTelefon;
                musteri.DamatTC = guncelVeri.DamatTC;
                musteri.Notlar = guncelVeri.Notlar;

                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // MÜŞTERİ SİLME İŞLEMİ
        [HttpPost]
        public IActionResult Sil(int id)
        {
            var musteri = _context.Musteriler
                .Include(m => m.Rezervasyonlar)
                .FirstOrDefault(m => m.Id == id);

            if (musteri != null)
            {
                _context.Musteriler.Remove(musteri);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}