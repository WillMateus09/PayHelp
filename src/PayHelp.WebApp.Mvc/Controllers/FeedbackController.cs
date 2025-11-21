using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.DTOs;
using PayHelp.WebApp.Mvc.Services;

namespace PayHelp.WebApp.Mvc.Controllers;

[Authorize(Roles = "Suporte")]
public class FeedbackController : Controller
{
    private readonly ApiService _api;
    public FeedbackController(ApiService api) { _api = api; }

    [HttpGet]
    public async Task<IActionResult> Usuarios()
    {
        var data = await _api.GetAsync<List<UserFeedbackSummaryDto>>("feedback/usuarios") ?? new();
        return View(data);
    }
}
