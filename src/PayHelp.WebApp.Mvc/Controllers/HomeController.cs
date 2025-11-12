using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PayHelp.WebApp.Mvc.Models;

namespace PayHelp.WebApp.Mvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public IActionResult Forbidden()
    {
        Response.StatusCode = 403;
        return View();
    }
}
