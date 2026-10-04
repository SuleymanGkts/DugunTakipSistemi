using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using DugunTakipSistemi.Models;

namespace DugunTakipSistemi.Controllers
{
    public class TekliflerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TekliflerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string arama)
        {
            var liste = _context.Teklifler.AsQueryable();

            if (!string.IsNullOrWhiteSpace(arama))
            {
                arama = arama.Trim().ToLower();
                liste = liste.Where(t => t.MusteriAdSoyad.ToLower().Contains(arama)
                                      || (t.Telefon != null && t.Telefon.Contains(arama))
                                      || (t.Aciklama != null && t.Aciklama.ToLower().Contains(arama)));
            }

            ViewBag.Arama = arama;
            var teklifler = liste.OrderByDescending(t => t.Tarih).ToList();
            return View(teklifler);
        }

        [HttpPost]
        public IActionResult Ekle(Teklif teklif)
        {
            if (!string.IsNullOrWhiteSpace(teklif.MusteriAdSoyad) && teklif.Fiyat > 0)
            {
                teklif.Tarih = DateTime.Now;
                _context.Teklifler.Add(teklif);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Sil(int id)
        {
            var t = _context.Teklifler.Find(id);
            if (t != null)
            {
                _context.Teklifler.Remove(t);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}