using System;

namespace PayHelp.Mobile.Maui.ViewModels
{
    /// <summary>
    /// ViewModel para o modal de resolução pelo usuário
    /// </summary>
    public class MarcarResolvidoViewModel : BaseViewModel
    {
        private string _feedbackUsuario = string.Empty;
        private int _notaUsuario = 0;
        private string _notaDescricao = "Selecione uma nota";
        private string _contadorCaracteres = "0 / 2000";

        public string FeedbackUsuario
        {
            get => _feedbackUsuario;
            set
            {
                if (SetProperty(ref _feedbackUsuario, value))
                {
                    AtualizarContador();
                }
            }
        }

        public int NotaUsuario
        {
            get => _notaUsuario;
            set
            {
                if (SetProperty(ref _notaUsuario, value))
                {
                    AtualizarNotaDescricao();
                }
            }
        }

        public string NotaDescricao
        {
            get => _notaDescricao;
            set => SetProperty(ref _notaDescricao, value);
        }

        public string ContadorCaracteres
        {
            get => _contadorCaracteres;
            set => SetProperty(ref _contadorCaracteres, value);
        }

        public Command<int> SelecionarNotaCommand { get; }
        public Command ConfirmarCommand { get; }
        public Command CancelarCommand { get; }

        public event EventHandler<bool>? OnClosed;

        public MarcarResolvidoViewModel()
        {
            SelecionarNotaCommand = new Command<int>(nota => NotaUsuario = nota);
            ConfirmarCommand = new Command(Confirmar);
            CancelarCommand = new Command(Cancelar);
        }

        private void AtualizarNotaDescricao()
        {
            NotaDescricao = _notaUsuario switch
            {
                1 => "⭐ Ruim",
                2 => "⭐⭐ Poderia melhorar",
                3 => "⭐⭐⭐ Bom",
                4 => "⭐⭐⭐⭐ Muito bom",
                5 => "⭐⭐⭐⭐⭐ Excelente!",
                _ => "Selecione uma nota"
            };
        }

        private void AtualizarContador()
        {
            var len = FeedbackUsuario?.Length ?? 0;
            ContadorCaracteres = $"{len} / 2000";
        }

        private async void Confirmar()
        {
            if (NotaUsuario == 0)
            {
                await Application.Current?.MainPage?.DisplayAlert(
                    "⚠️ Avaliação necessária",
                    "Por favor, selecione quantas estrelas você daria para o atendimento (de 1 a 5 estrelas).",
                    "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(FeedbackUsuario) || FeedbackUsuario.Length < 3)
            {
                await Application.Current?.MainPage?.DisplayAlert(
                    "⚠️ Comentário necessário",
                    "Por favor, escreva um comentário sobre sua experiência (mínimo 3 caracteres). Sua opinião é muito importante!",
                    "OK");
                return;
            }

            OnClosed?.Invoke(this, true);
        }

        private void Cancelar()
        {
            OnClosed?.Invoke(this, false);
        }
    }
}
