using Microsoft.AspNetCore.Mvc;

namespace DugunTakipSistemi.Controllers
{
    public class RezervasyonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}