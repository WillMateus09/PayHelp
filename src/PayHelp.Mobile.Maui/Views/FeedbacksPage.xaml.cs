using Microsoft.Maui.Controls;
using PayHelp.Mobile.Maui.Helpers;
using PayHelp.Mobile.Maui.Models;
using PayHelp.Mobile.Maui.Services;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class FeedbacksPage : ContentPage
{
    public FeedbacksPage()
    {
        System.Diagnostics.Debug.WriteLine("[FeedbacksPage] Construtor iniciado");
        try
        {
            System.Diagnostics.Debug.WriteLine("[FeedbacksPage] Chamando InitializeComponent");
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("[FeedbacksPage] InitializeComponent completo");
            
            System.Diagnostics.Debug.WriteLine("[FeedbacksPage] Obtendo FeedbacksViewModel");
            var vm = ServiceHelper.GetService<FeedbacksViewModel>();
            if (vm is null)
            {
                System.Diagnostics.Debug.WriteLine("[FeedbacksPage] FeedbacksViewModel não encontrado via DI, tentando criar manualmente");
                var rel = ServiceHelper.GetService<RelatorioService>();
                if (rel != null) 
                {
                    vm = new FeedbacksViewModel(rel);
                    System.Diagnostics.Debug.WriteLine("[FeedbacksPage] FeedbacksViewModel criado manualmente");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[FeedbacksPage] RelatorioService também não encontrado");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[FeedbacksPage] FeedbacksViewModel obtido via DI");
            }
            
            BindingContext = vm ?? new object();
            System.Diagnostics.Debug.WriteLine($"[FeedbacksPage] BindingContext definido. Tipo: {BindingContext?.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[FeedbacksPage] ViewModel tem {(vm as FeedbacksViewModel)?.Feedbacks?.Count ?? 0} itens iniciais");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FeedbacksPage] ERRO no construtor: {ex.Message}\n{ex.StackTrace}");
            BindingContext = new object();
        }
        System.Diagnostics.Debug.WriteLine("[FeedbacksPage] Construtor finalizado");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (BindingContext is FeedbacksViewModel vm && vm.Feedbacks.Count == 0 && !vm.IsBusy)
            {
                System.Diagnostics.Debug.WriteLine("[FeedbacksPage] OnAppearing -> carregando feedbacks");
                await vm.CarregarAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FeedbacksPage] Erro OnAppearing: {ex.Message}");
        }
    }

    private async void OnSelecionado(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection?.FirstOrDefault() is FeedbackCompletoDto fb)
            {
                // Mapear para FeedbackDto reutilizado pelo modal
                var dto = new FeedbackDto
                {
                    Id = fb.Id,
                    TicketId = fb.TicketId,
                    UsuarioId = fb.UsuarioId,
                    FeedbackUsuario = fb.Comentario,
                    NotaUsuario = fb.Nota,
                    DataResolvidoUsuario = fb.DataCriacaoUtc
                };
                await Navigation.PushModalAsync(new FeedbackModalPage(dto));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FeedbacksPage] Erro ao abrir modal: {ex.Message}");
        }
        finally
        {
            // Limpar seleção para permitir novo toque
            if (sender is CollectionView cv) cv.SelectedItem = null;
        }
    }
}
