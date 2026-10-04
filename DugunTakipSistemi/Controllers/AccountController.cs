using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace DugunTakipSistemi.Controllers
{
    public class AdminCredential
    {
        public string KullaniciAdi { get; set; } = "admin";
        public string Sifre { get; set; } = "1234";
    }

    public class GuncelleModel
    {
        public string MevcutSifre { get; set; }
        public string YeniKullaniciAdi { get; set; }
        public string YeniSifre { get; set; }
    }

    public class AccountController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private string CredentialPath => Path.Combine(_env.ContentRootPath, "admin_auth.json");

        public AccountController(IWebHostEnvironment env)
        {
            _env = env;
        }

        private AdminCredential GetCredentials()
        {
            if (!System.IO.File.Exists(CredentialPath))
            {
                var defaultCred = new AdminCredential();
                System.IO.File.WriteAllText(CredentialPath, JsonSerializer.Serialize(defaultCred));
                return defaultCred;
            }
            var json = System.IO.File.ReadAllText(CredentialPath);
            return JsonSerializer.Deserialize<AdminCredential>(json) ?? new AdminCredential();
        }

        private void SaveCredentials(AdminCredential cred)
        {
            var json = JsonSerializer.Serialize(cred);
            System.IO.File.WriteAllText(CredentialPath, json);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string kullaniciAdi, string sifre)
        {
            var cred = GetCredentials();

            if (kullaniciAdi == cred.KullaniciAdi && sifre == cred.Sifre)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, cred.KullaniciAdi),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Hata = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        // KULLANICI ADI & ŞİFRE GÜNCELLEME İŞLEMİ
        [HttpPost]
        public IActionResult BilgiGuncelle([FromBody] GuncelleModel model)
        {
            if (model == null)
                return Json(new { success = false, mesaj = "Geçersiz istek gönderildi!" });

            var cred = GetCredentials();

            if (model.MevcutSifre != cred.Sifre)
            {
                return Json(new { success = false, mesaj = "Mevcut şifrenizi hatalı girdiniz!" });
            }

            if (string.IsNullOrWhiteSpace(model.YeniKullaniciAdi) || string.IsNullOrWhiteSpace(model.YeniSifre))
            {
                return Json(new { success = false, mesaj = "Kullanıcı adı ve yeni şifre boş bırakılamaz!" });
            }

            cred.KullaniciAdi = model.YeniKullaniciAdi.Trim();
            cred.Sifre = model.YeniSifre.Trim();
            SaveCredentials(cred);

            return Json(new { success = true, mesaj = "Kullanıcı adı ve şifre başarıyla güncellendi!" });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}