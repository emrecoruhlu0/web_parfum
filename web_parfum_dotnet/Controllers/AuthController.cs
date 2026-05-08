using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Models;
using WebParfum.ViewModels;

namespace WebParfum.Controllers;

public class AuthController(AppDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        await SignInUser(user);
        return RedirectToAction("Index", "Feed");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await db.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Bu e-posta zaten kullanılıyor.");
            return View(model);
        }
        if (await db.Users.AnyAsync(u => u.Username == model.Username))
        {
            ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
            return View(model);
        }

        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        await SignInUser(user);
        return RedirectToAction("Index", "Feed");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private async Task SignInUser(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
