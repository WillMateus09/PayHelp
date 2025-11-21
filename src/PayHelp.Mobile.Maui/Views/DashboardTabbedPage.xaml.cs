using Microsoft.Maui.Controls;
using PayHelp.Mobile.Maui.Helpers;
using PayHelp.Mobile.Maui.Services;
using PayHelp.Mobile.Maui.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PayHelp.Mobile.Maui.Views;

// Agora é uma ContentPage que gerencia manualmente duas páginas internas
[QueryProperty(nameof(InitialTab), "tab")]
public partial class DashboardTabbedPage : ContentPage
{
    private RelatorioService? _relatorios;
    private const int RecentDays = 30;
    private bool _loadingCount = false;
    private string _active = "chamados";

    public string? InitialTab { get; set; }

    public DashboardTabbedPage()
    {
        System.Diagnostics.Debug.WriteLine("[DashboardTabbedPage] ctor iniciado");
        try
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("[DashboardTabbedPage] InitializeComponent OK");
            
            AtualizarEstilosBotoes();
            System.Diagnostics.Debug.WriteLine("[DashboardTabbedPage] ctor finalizado com sucesso");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardTabbedPage] ERRO no ctor: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        if (!string.IsNullOrWhiteSpace(InitialTab))
        {
            if (InitialTab.Equals("feedbacks", StringComparison.OrdinalIgnoreCase))
            {
                MostrarFeedbacks();
            }
            else if (InitialTab.Equals("chamados", StringComparison.OrdinalIgnoreCase))
            {
                MostrarChamados();
            }
        }
    }

    private async void MostrarChamados()
    {
        System.Diagnostics.Debug.WriteLine("[DashboardTabbedPage] MostrarChamados");
        _active = "chamados";
        PageChamados.IsVisible = true;
        PageFeedbacks.IsVisible = false;
        AtualizarEstilosBotoes();
        
        // Carregar dados
        if (PageChamados.BindingContext is ChamadosViewModel vmChamados)
        {
            System.Diagnostics.Debug.WriteLine("[DashboardTabbedPage] Carregando chamados...");
            await vmChamados.CarregarAsync();
            System.Diagnostics.Debug.WriteLine($"[DashboardTabbedPage] Chamados carregados: {vmChamados.Itens?.Count ?? 0}");
        }
    }

    private async void MostrarFeedbacks()
    {
        System.Diagnostics.Debug.WriteLine("[DashboardTabbedPage] MostrarFeedbacks");
        _active = "feedbacks";
        PageChamados.IsVisible = false;
        PageFeedbacks.IsVisible = true;
        AtualizarEstilosBotoes();
        
        System.Diagnostics.Debug.WriteLine($"[DashboardTabbedPage] PageFeedbacks.BindingContext type: {PageFeedbacks.BindingContext?.GetType().Name ?? "null"}");
        
        // Carregar dados através do ViewModel
        if (PageFeedbacks.BindingContext is FeedbacksViewModel vmFeedbacks)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardTabbedPage] ViewModel encontrado. Feedbacks atuais: {vmFeedbacks.Feedbacks?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine("[DashboardTabbedPage] Carregando feedbacks via ViewModel...");
            await vmFeedbacks.CarregarAsync();
            System.Diagnostics.Debug.WriteLine($"[DashboardTabbedPage] Feedbacks carregados: {vmFeedbacks.Feedbacks?.Count ?? 0}");
            
            // Atualizar label com a contagem
            var count = vmFeedbacks.Feedbacks?.Count ?? 0;
            LblStatus.Text = count > 0 ? $"Feedbacks: {count}" : "Sem feedbacks";
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardTabbedPage] ERRO: BindingContext NÃO é FeedbacksViewModel! Tipo: {PageFeedbacks.BindingContext?.GetType().Name ?? "null"}");
        }
    }

    private void AtualizarEstilosBotoes()
    {
        if (BtnChamados != null && BtnFeedbacks != null)
        {
            BtnChamados.Opacity = _active == "chamados" ? 1.0 : 0.6;
            BtnFeedbacks.Opacity = _active == "feedbacks" ? 1.0 : 0.6;
        }
    }

    private async void CarregarContadorFeedbacksAsync()
    {
        if (_loadingCount) return;
        _loadingCount = true;
        try
        {
            if (_relatorios == null) _relatorios = ServiceHelper.GetService<RelatorioService>();
            if (_relatorios == null)
            {
                LblStatus.Text = "Serviço indisponível";
                return;
            }
            var de = DateTime.UtcNow.AddDays(-RecentDays);
            var ate = DateTime.UtcNow.AddDays(1);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var itens = await _relatorios.ListarFeedbacksCompletosAsync(cts.Token);
            var count = itens?.Count ?? 0;
            LblStatus.Text = count > 0 ? $"Feedbacks: {count}" : "Sem feedbacks";
        }
        catch (Exception ex)
        {
            LblStatus.Text = "Erro ao carregar";
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Erro contador feedbacks: {ex.Message}");
        }
        finally { _loadingCount = false; }
    }

    private void ToolbarChamados_Clicked(object sender, EventArgs e) => MostrarChamados();
    private void ToolbarFeedbacks_Clicked(object sender, EventArgs e) => MostrarFeedbacks();

    private async void ContentPage_Appearing(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[DashboardTabbedPage] ContentPage_Appearing");
        
        // Carregar dados iniciais dos chamados ao aparecer
        if (PageChamados.BindingContext is ChamadosViewModel vm)
        {
            System.Diagnostics.Debug.WriteLine("[DashboardTabbedPage] Carregando dados iniciais...");
            await vm.CarregarAsync();
            System.Diagnostics.Debug.WriteLine($"[DashboardTabbedPage] Dados carregados: {vm.Itens?.Count ?? 0}");
        }
    }
}
