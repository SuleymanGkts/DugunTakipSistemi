using Microsoft.AspNetCore.Mvc;
using DugunTakipSistemi.Models;
using System.Linq;

namespace DugunTakipSistemi.Controllers
{
    public class PaketlerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaketlerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Paketleri Listeleme Sayfası
        public IActionResult Index()
        {
            var paketler = _context.Paketler.ToList();
            return View(paketler);
        }

        // Yeni Paket Ekleme İşlemi
        [HttpPost]
        public IActionResult Ekle(string paketAdi, decimal paketFiyati)
        {
            var yeniPaket = new Paket
            {
                PaketAdi = paketAdi,
                Fiyat = paketFiyati,
                SureSaat = 4 // Varsayılan süre
            };

            _context.Paketler.Add(yeniPaket);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // Var olan Paketi Güncelleme İşlemi
        [HttpPost]
        public IActionResult Guncelle(int id, string paketAdi, decimal paketFiyati)
        {
            var paket = _context.Paketler.Find(id);
            if (paket != null)
            {
                paket.PaketAdi = paketAdi;
                paket.Fiyat = paketFiyati;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Paketi Silme İşlemi
        public IActionResult Sil(int id)
        {
            var paket = _context.Paketler.Find(id);
            if (paket != null)
            {
                _context.Paketler.Remove(paket);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}