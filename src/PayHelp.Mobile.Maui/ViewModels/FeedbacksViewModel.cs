using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Models;
using PayHelp.Mobile.Maui.Services;
using System.Collections.ObjectModel;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class FeedbacksViewModel : BaseViewModel
{
    private readonly RelatorioService _relatorios;

    // Período (ainda usado para futuros filtros)
    [ObservableProperty] private DateTime dataInicio;
    [ObservableProperty] private DateTime dataFim;

    // Lista COMPLETA de feedbacks (cada avaliação)
    [ObservableProperty] private ObservableCollection<FeedbackCompletoDto> feedbacks = new();

    // Estatísticas agregadas (opcional - manter para header futuro)
    [ObservableProperty] private ObservableCollection<FeedbackEstatisticaDto> estatisticas = new();

    public FeedbacksViewModel(RelatorioService relatorios)
    {
        _relatorios = relatorios;
        DataInicio = DateTime.UtcNow.AddYears(-10);
        DataFim = DateTime.UtcNow.AddDays(1);
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            System.Diagnostics.Debug.WriteLine("[FeedbacksViewModel] Iniciando CarregarAsync (completo + estatísticas)");
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            // 1. Lista completa
            var listaCompleta = await _relatorios.ListarFeedbacksCompletosAsync(cts.Token);
            System.Diagnostics.Debug.WriteLine($"[FeedbacksViewModel] Feedbacks completos: {listaCompleta.Count}");
            Feedbacks.Clear();
            foreach (var fb in listaCompleta)
            {
                System.Diagnostics.Debug.WriteLine($"[FeedbacksViewModel] FB => Usuario='{fb.UsuarioNome}' Email='{fb.UsuarioEmail}' Ticket='{fb.TicketTitulo}' Nota={fb.Nota} Comentario='{fb.Comentario}'");
                Feedbacks.Add(fb);
            }

            // (Removido cálculo de estatísticas da UI principal a pedido do usuário)
        }
        catch (TaskCanceledException)
        {
            await AlertAsync("Aviso", "Timeout ao carregar feedbacks.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FeedbacksViewModel] Erro geral: {ex.Message}\n{ex.StackTrace}");
            await AlertAsync("Erro", ex.Message);
        }
        finally { IsBusy = false; }
    }
}
