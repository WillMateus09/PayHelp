using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.WebApp.Mvc.Services;
using PayHelp.WebApp.Mvc.ViewModels;

namespace PayHelp.WebApp.Mvc.Controllers;

[Authorize(Roles = "Suporte")]
public class CannedMessagesController : Controller
{
    private readonly ApiService _api;

    public CannedMessagesController(ApiService api)
    {
        _api = api;
    }

    public record ApiItem(Guid Id, string Titulo, string Conteudo, string? GatilhoPalavraChave);

    public async Task<IActionResult> Index()
    {
        try
        {
            var list = await _api.GetAsync<List<ApiItem>>("mensagensautomaticas") ?? new();
            var vm = list.Select(x => new CannedMessageViewModel { Id = x.Id, Titulo = x.Titulo, Conteudo = x.Conteudo, GatilhoPalavraChave = x.GatilhoPalavraChave });
            return View(vm);
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(Enumerable.Empty<CannedMessageViewModel>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CannedMessageViewModel vm)
    {
        if (!ModelState.IsValid) return RedirectToAction("Index");
        await _api.PostAsync<object, object>("mensagensautomaticas", new { vm.Titulo, vm.Conteudo, vm.GatilhoPalavraChave });
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _api.DeleteAsync($"mensagensautomaticas/{id}");
        return RedirectToAction("Index");
    }
}
