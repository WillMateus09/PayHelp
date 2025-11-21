using PayHelp.Mobile.Maui.ViewModels;
using PayHelp.Mobile.Maui.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using PayHelp.Mobile.Maui.Helpers;

namespace PayHelp.Mobile.Maui.Views
{
    [QueryProperty(nameof(TicketIdString), "ticketId")]
    public partial class MarcarResolvidoPage : ContentPage
    {
        private readonly TaskCompletionSource<(bool confirmado, string feedback, int nota)> _tcs;
        private Guid _ticketId;
        private string? _ticketIdString;
        public string? TicketIdString
        {
            get => _ticketIdString;
            set
            {
                _ticketIdString = value;
                if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var g))
                    _ticketId = g;
                else
                    _ticketId = Guid.Empty;
            }
        }

        public MarcarResolvidoPage()
        {
            InitializeComponent();
            _tcs = new TaskCompletionSource<(bool, string, int)>();

            if (BindingContext is MarcarResolvidoViewModel vm)
            {
                vm.OnClosed += async (s, confirmado) =>
                {
                    if (confirmado)
                    {
                        _tcs.SetResult((true, vm.FeedbackUsuario, vm.NotaUsuario));

                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"[MarcarResolvido] TicketId: {_ticketId}, Nota: {vm.NotaUsuario}, Feedback: {vm.FeedbackUsuario}");
                            
                            var svc = ServiceHelper.GetService<ChamadoService>();
                            if (svc is not null && _ticketId != Guid.Empty)
                            {
                                var (ok, error) = await svc.ResolverPorUsuarioAsync(_ticketId, vm.NotaUsuario, vm.FeedbackUsuario);
                                
                                System.Diagnostics.Debug.WriteLine($"[MarcarResolvido] Resultado: ok={ok}, error={error}");
                                
                                if (!ok)
                                {
                                    await DisplayAlert("Erro", string.IsNullOrWhiteSpace(error) ? "Falha ao marcar como resolvido." : error!, "OK");
                                }
                                else
                                {
                                    await DisplayAlert("Obrigado!", "Seu feedback foi registrado e o chamado marcado como resolvido.", "OK");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[MarcarResolvido] Serviço nulo ou ticketId vazio. svc={svc != null}, ticketId={_ticketId}");
                                await DisplayAlert("Erro", "Serviço não disponível ou ID do ticket inválido.", "OK");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[MarcarResolvido] Exception: {ex}");
                            await DisplayAlert("Erro", $"Falha ao enviar feedback: {ex.Message}", "OK");
                        }

                        // Ir para Home do usuário simples
                        await Shell.Current.GoToAsync("//home");
                    }
                    else
                    {
                        _tcs.SetResult((false, string.Empty, 0));
                        if (Shell.Current?.Navigation?.NavigationStack?.Count > 1)
                            await Shell.Current.GoToAsync("..");
                        else
                            await Navigation.PopAsync();
                    }
                };
            }
        }

        public Task<(bool confirmado, string feedback, int nota)> GetResultAsync()
        {
            return _tcs.Task;
        }
    }
}
