using Microsoft.AspNetCore.Mvc;
using DugunTakipSistemi.Models;
using System.Linq;
using System;

namespace DugunTakipSistemi.Controllers
{
    public class GenelNotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GenelNotController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var notlar = _context.GenelNotlar.OrderByDescending(n => n.OlusturulmaTarihi).ToList();
            return View(notlar);
        }

        [HttpPost]
        public IActionResult HizliNotEkle(string icerik)
        {
            if (!string.IsNullOrWhiteSpace(icerik))
            {
                var yeniNot = new GenelNot
                {
                    Icerik = icerik,
                    OlusturulmaTarihi = DateTime.Now
                };

                _context.GenelNotlar.Add(yeniNot);
                _context.SaveChanges();

                return Json(new { success = true, message = "Not başarıyla eklendi." });
            }
            return Json(new { success = false, message = "Not içeriği boş olamaz." });
        }

        [HttpPost]
        public IActionResult Sil(int id)
        {
            var not = _context.GenelNotlar.Find(id);
            if (not != null)
            {
                _context.GenelNotlar.Remove(not);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Ekle(string icerik)
        {
            if (!string.IsNullOrWhiteSpace(icerik))
            {
                var yeniNot = new GenelNot
                {
                    Icerik = icerik,
                    OlusturulmaTarihi = DateTime.Now
                };
                _context.GenelNotlar.Add(yeniNot);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Guncelle(int id, string icerik)
        {
            var not = _context.GenelNotlar.Find(id);
            if (not != null && !string.IsNullOrWhiteSpace(icerik))
            {
                not.Icerik = icerik;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}