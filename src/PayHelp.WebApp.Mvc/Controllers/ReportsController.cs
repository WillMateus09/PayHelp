using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.WebApp.Mvc.ViewModels;
using PayHelp.WebApp.Mvc.Services;
using PayHelp.Domain.Enums;
using PayHelp.Application.Services;
using System.Text.Json;

namespace PayHelp.WebApp.Mvc.Controllers;

[Authorize(Roles = "Suporte")]
public class ReportsController : Controller
{
    private readonly ApiService _api;
    private readonly IReportService _reports;

    public ReportsController(ApiService api, IReportService reports)
    {
        _api = api;
        _reports = reports;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vm = new ReportFilterViewModel();
        if (TempData["Filter"] is string filterJson)
        {
            try { vm = JsonSerializer.Deserialize<ReportFilterViewModel>(filterJson) ?? vm; } catch { }
        }


        if (vm.De.HasValue || vm.Ate.HasValue || vm.Status.HasValue)
        {
            var (result, usedFallback, unauthorizedError) = await GenerateReportAsync(vm);
            ViewBag.Resultado = result;
            if (usedFallback)
                TempData["Aviso"] = "API indisponível; exibindo resultados locais.";
            if (!string.IsNullOrWhiteSpace(unauthorizedError))
                ModelState.AddModelError(string.Empty, unauthorizedError);
        }

        return View(vm);
    }

    private static DateTime? ToUtc(DateTime? local)
    {
        if (!local.HasValue) return null;
        var withKind = DateTime.SpecifyKind(local.Value, DateTimeKind.Local);
        return withKind.ToUniversalTime();
    }

    private sealed class ApiReportItem
    {
        public Guid TicketId { get; set; }
        public string? Status { get; set; }
        public string? SolicitanteEmail { get; set; }
        public string? SolicitanteRole { get; set; }
        public TimeSpan? Duracao { get; set; }
        public DateTime CriadoEmUtc { get; set; }
        public DateTime? EncerradoEmUtc { get; set; }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Gerar(ReportFilterViewModel vm)
    {

        TempData["Filter"] = JsonSerializer.Serialize(vm);
        return RedirectToAction("Index");
    }

    private async Task<(List<ReportEntryViewModel> result, bool usedFallback, string? unauthorizedError)> GenerateReportAsync(ReportFilterViewModel vm)
    {
        var filtro = new
        {
            DeUtc = ToUtc(vm.De),
            AteUtc = ToUtc(vm.Ate),
            Status = vm.Status?.ToString()
        };

        try
        {
            var items = await _api.PostAsync<object, List<ApiReportItem>>("relatorios", filtro)
                        ?? new List<ApiReportItem>();
            var mapped = items.Select(r => new ReportEntryViewModel
            {
                TicketId = r.TicketId,
                SolicitanteEmail = r.SolicitanteEmail ?? string.Empty,
                StatusFinal = Enum.TryParse<TicketStatus>(r.Status ?? string.Empty, true, out var st) ? st : TicketStatus.Aberto,
                CriadoEm = r.CriadoEmUtc.ToLocalTime(),
                EncerradoEm = r.EncerradoEmUtc?.ToLocalTime(),
                Duracao = r.Duracao
            }).ToList();
            return (mapped, false, null);
        }
        catch (UnauthorizedAccessException ex)
        {
            return (new List<ReportEntryViewModel>(), false, ex.Message);
        }
        catch (HttpRequestException)
        {
            var deUtc = ToUtc(vm.De);
            var ateUtc = ToUtc(vm.Ate);
            var status = vm.Status;
            var localItems = await _reports.GerarRelatorioAsync(deUtc, ateUtc, status) ?? Enumerable.Empty<Domain.Entities.ReportEntry>();
            var mapped = localItems.Select(r => new ReportEntryViewModel
            {
                TicketId = r.TicketId,
                SolicitanteEmail = r.SolicitanteEmail ?? string.Empty,
                StatusFinal = r.StatusFinal,
                CriadoEm = r.CriadoEmUtc.ToLocalTime(),
                EncerradoEm = r.EncerradoEmUtc?.ToLocalTime(),
                Duracao = r.Duracao
            }).ToList();
            return (mapped, true, null);
        }
    }
}
