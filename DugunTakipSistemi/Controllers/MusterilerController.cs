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

        // Takvimden tıklandığında modalı otomatik açmak için 'seciliMusteriId' parametresi ekledik
        public IActionResult Index(string aramaParametresi, int? seciliMusteriId)
        {
            // Müşterileri, Rezervasyonları ve o rezervasyonların Mekanlarını birlikte çekiyoruz
            var sorgu = _context.Musteriler
                .Include(m => m.Rezervasyonlar)
                    .ThenInclude(r => r.Mekan) // Mekan verisini dahil ettik
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(aramaParametresi))
            {
                sorgu = sorgu.Where(m => m.AdSoyad.Contains(aramaParametresi) || m.Telefon.Contains(aramaParametresi));
            }

            var musteriler = sorgu.OrderByDescending(m => m.Id).ToList();

            ViewBag.Arama = aramaParametresi;
            ViewBag.SeciliMusteriId = seciliMusteriId; // Takvim yönlendirmesi için

            return View(musteriler);
        }

        // Müşteri Güncelleme İşlemi
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

        [HttpPost]
        public IActionResult Sil(int id)
        {
            var musteri = _context.Musteriler.Find(id);
            if (musteri != null)
            {
                _context.Musteriler.Remove(musteri);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}