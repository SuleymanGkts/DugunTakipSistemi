using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DugunTakipSistemi.Models; // Kendi namespace'ine göre burasý deðiþebilir

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context; // 1. DbContext'i tanýmlýyoruz

    // 2. Constructor (Yapýcý metot) ile dependency injection yapýyoruz
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var bugun = DateTime.Today;
        var dun = bugun.AddDays(-1);
        var yarin = bugun.AddDays(1);

        var aktifIsler = _context.Rezervasyonlar
            .Include(r => r.Mekan)
            .Include(r => r.Musteri)
            .Where(r => r.BaslangicTarihi.Date >= dun && r.BaslangicTarihi.Date <= yarin)
            .OrderBy(r => r.BaslangicTarihi)
            .ToList();

        ViewBag.AktifIsler = aktifIsler;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}