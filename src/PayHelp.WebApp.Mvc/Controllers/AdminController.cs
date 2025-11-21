using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.WebApp.Mvc.Services;
using PayHelp.WebApp.Mvc.ViewModels;

namespace PayHelp.WebApp.Mvc.Controllers;

[Authorize(Roles = "Master")]
public class AdminController : Controller
{
    private readonly ApiService _api;

    public AdminController(ApiService api) { _api = api; }

    public IActionResult Index() => RedirectToAction("Users");

    // --- Users ---
    public record ApiUser(Guid Id, string Nome, string Email, string Role, bool IsBlocked);

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var list = await _api.GetAsync<List<ApiUser>>("admin/users") ?? new();
        var vm = list.Select(u => new AdminUserViewModel
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            Role = u.Role,
            IsBlocked = u.IsBlocked
        }).ToList();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(Guid id, bool blocked)
    {
        await _api.PostAsync<object, object>($"admin/users/{id}/block", new { Blocked = blocked });
        TempData["Sucesso"] = blocked ? "Usuário bloqueado." : "Usuário desbloqueado.";
        return RedirectToAction("Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(Guid id, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            TempData["Erro"] = "Informe a nova senha.";
            return RedirectToAction("Users");
        }
        await _api.PostAsync<object, object>($"admin/users/{id}/password", new { NewPassword = newPassword });
        TempData["Sucesso"] = "Senha atualizada.";
        return RedirectToAction("Users");
    }

    // --- Settings ---
    public record ApiSettings(string? SupportVerificationWord, string? PublicBaseUrl);

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var dto = await _api.GetAsync<ApiSettings>("admin/settings") ?? new ApiSettings(null, null);
        var vm = new AdminSettingsViewModel
        {
            SupportVerificationWord = dto.SupportVerificationWord ?? string.Empty,
            PublicBaseUrl = dto.PublicBaseUrl ?? string.Empty
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(AdminSettingsViewModel vm)
    {
        var body = new { SupportVerificationWord = vm.SupportVerificationWord, PublicBaseUrl = vm.PublicBaseUrl };
        await _api.PutAsync("admin/settings", body);
        // Also set this browser session to use the new base immediately
        var baseUrl = (vm.PublicBaseUrl ?? string.Empty).TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            var apiBase = baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase) ? baseUrl + "/" : baseUrl + "/api/";
            HttpContext.Session.SetString("ApiBaseOverride", apiBase);
        }
        TempData["Sucesso"] = "Configurações salvas.";
        return RedirectToAction("Settings");
    }
}
