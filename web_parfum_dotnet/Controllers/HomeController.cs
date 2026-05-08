using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebParfum.Models;

namespace WebParfum.Controllers;

public class HomeController(ILogger<HomeController> logger) : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
