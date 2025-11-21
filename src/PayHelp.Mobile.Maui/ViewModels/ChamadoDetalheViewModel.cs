using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Models;
using PayHelp.Mobile.Maui.Services;

namespace PayHelp.Mobile.Maui.ViewModels;

[QueryProperty(nameof(TicketId), "ticketId")]
public partial class ChamadoDetalheViewModel : BaseViewModel
{
    private readonly ChamadoService _chamados;
    private readonly TriagemService _triagem;
    private readonly FaqService _faq;

    [ObservableProperty] private Guid ticketId;
    [ObservableProperty] private TicketDto? ticket;
    [ObservableProperty] private string novaMensagem = string.Empty;
    [ObservableProperty] private List<string> sugestoes = new();
    [ObservableProperty] private string? faqResposta;
    [ObservableProperty] private bool mostrarFaq;
    [ObservableProperty] private bool mostrarTriagem;
    [ObservableProperty] private bool canChat;
    [ObservableProperty] private bool canChamarAtendente;
    [ObservableProperty] private bool isSuporte;
    [ObservableProperty] private bool canAssumir;
    [ObservableProperty] private bool canEncerrar;
    [ObservableProperty] private bool triagemExecutada;
    [ObservableProperty] private bool mostrarBotaoMarcarResolvido;
    [ObservableProperty] private bool mostrarBotaoResolvidoUsuario;
    [ObservableProperty] private bool botaoResolvidoHabilitado;
    [ObservableProperty] private bool atendenteJaChamado;
    [ObservableProperty] private bool mostrarResolucaoFinal;

    public ChamadoDetalheViewModel(ChamadoService chamados, TriagemService triagem, FaqService faq)
    { _chamados = chamados; _triagem = triagem; _faq = faq; }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        if (TicketId == Guid.Empty) return;
        try
        {
            var role = await Microsoft.Maui.Storage.SecureStorage.Default.GetAsync("user_role");
            IsSuporte = (role ?? string.Empty).ToLowerInvariant().Contains("suporte") || (role ?? string.Empty).ToLowerInvariant().Contains("support");
        }
        catch { IsSuporte = false; }

        Ticket = await _chamados.GetDetailsAsync(TicketId);

        var status = Ticket?.Status ?? string.Empty;
        var encerrado = string.Equals(status, "Encerrado", StringComparison.OrdinalIgnoreCase);
        
        // Mostrar Resolução Final APENAS quando encerrado E tem resolução final preenchida
        MostrarResolucaoFinal = encerrado && !string.IsNullOrWhiteSpace(Ticket?.ResolucaoFinal);

        if (IsSuporte)
        {

            MostrarFaq = false;
            MostrarTriagem = false;
            CanChat = !encerrado;
            CanChamarAtendente = false;

            CanAssumir = !encerrado && !string.Equals(status, "EmAtendimento", StringComparison.OrdinalIgnoreCase);
            CanEncerrar = !encerrado && string.Equals(status, "EmAtendimento", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            if (encerrado)
            {
                MostrarFaq = false;
                MostrarTriagem = false;
                CanChat = false;
                CanChamarAtendente = false;
                CanAssumir = false;
                CanEncerrar = false;
                
                // Mostrar botão de marcar como resolvido se ainda não foi avaliado
                MostrarBotaoMarcarResolvido = Ticket?.DataResolvidoUsuario == null;
                MostrarBotaoResolvidoUsuario = false;
                BotaoResolvidoHabilitado = false;
                return;
            }
            
            // Se foi resolvido pelo usuário (IA), bloquear chat e botões
            if (Ticket?.DataResolvidoUsuario.HasValue == true)
            {
                MostrarFaq = false;
                MostrarTriagem = false;
                CanChat = false; // BLOQUEIO: não pode enviar mensagens
                CanChamarAtendente = false; // BLOQUEIO: não pode chamar atendente
                MostrarBotaoResolvidoUsuario = false; // BLOQUEIO: não pode marcar resolvido novamente
                BotaoResolvidoHabilitado = false;
                MostrarBotaoMarcarResolvido = false;
                CanAssumir = false;
                CanEncerrar = false;
                return;
            }
            
            // REGRA 1: Botão "Resolvido" visível APENAS se ainda NÃO foi resolvido pelo usuário
            MostrarBotaoResolvidoUsuario = Ticket?.DataResolvidoUsuario == null;
            
            // REGRA 2: Habilitado APENAS se atendente NÃO foi chamado
            var emAtendimento = string.Equals(status, "EmAtendimento", StringComparison.OrdinalIgnoreCase);
            AtendenteJaChamado = emAtendimento || Ticket?.SupportUserId != null;
            BotaoResolvidoHabilitado = !AtendenteJaChamado && Ticket?.DataResolvidoUsuario == null;

            bool faqFound = false;
            if (Ticket is not null && !string.IsNullOrWhiteSpace(Ticket.Descricao))
            {
                try
                {
                    var items = await _faq.BuscarAsync(Ticket.Descricao);
                    if (items.Count > 0 && !string.IsNullOrWhiteSpace(items[0].Solucao))
                    {
                        FaqResposta = items[0].Solucao;
                        faqFound = true;
                    }
                }
                catch { }
            }

            MostrarFaq = faqFound;
            MostrarTriagem = true && Ticket?.DataResolvidoUsuario == null; // Ocultar triagem se já foi resolvido

            CanChat = TriagemExecutada;
            CanChamarAtendente = TriagemExecutada && !AtendenteJaChamado;
            CanAssumir = false;
            CanEncerrar = false;
            MostrarBotaoMarcarResolvido = false;
        }
    }

    partial void OnNovaMensagemChanged(string value)
    {
        _ = BuscarSugestoesAsync(value);
    }

    private async Task BuscarSugestoesAsync(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) { Sugestoes = new(); return; }
        try
        {
            var res = await _triagem.SugerirAsync(texto);
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(res?.Faq)) list.Add(res!.Faq!);
            if (!string.IsNullOrWhiteSpace(res?.Sugestao))
            {
                var s = res!.Sugestao!;
                if (list.Count == 0 || !string.Equals(list[0], s, StringComparison.OrdinalIgnoreCase))
                    list.Add(s);
            }
            Sugestoes = list;
        }
        catch { }
    }

    [RelayCommand]
    public async Task EnviarMensagemAsync()
    {
        if (string.IsNullOrWhiteSpace(NovaMensagem) || TicketId == Guid.Empty) return;
        if (string.Equals(Ticket?.Status, "Encerrado", StringComparison.OrdinalIgnoreCase))
        {
            await AlertAsync("Encerrado", "Este chamado está encerrado. Não é possível enviar novas mensagens.");
            return;
        }
        if (await _chamados.SendMessageAsync(TicketId, new TicketMessageRequest { Texto = NovaMensagem }))
        {
            NovaMensagem = string.Empty;
            await CarregarAsync();
        }
        else
        {
            await AlertAsync("Erro", "Não foi possível enviar a mensagem.");
        }
    }

    [RelayCommand]
    public void AplicarSugestao(string? s)
    {
        if (!string.IsNullOrWhiteSpace(s))
            NovaMensagem = s;
    }

    [RelayCommand]
    public async Task ChamarAtendenteAsync()
    {
        if (TicketId == Guid.Empty || !CanChamarAtendente) return;
        var (ok, error) = await _chamados.CallAtendenteAsync(TicketId);
        if (!ok)
        {
            await AlertAsync("Erro", string.IsNullOrWhiteSpace(error) ? "Não foi possível chamar um atendente." : error!);
            return;
        }
        await _chamados.SendMessageAsync(TicketId, new TicketMessageRequest { Texto = "Um atendente foi acionado para este chamado." }, automatic: true);
        
        // REGRA: Após chamar atendente, desabilitar botão Resolvido
        AtendenteJaChamado = true;
        BotaoResolvidoHabilitado = false;
        
        await CarregarAsync();
        await AlertAsync("Enviado", "Um atendente foi acionado para este chamado.");
    }

    [RelayCommand]
    public void ProsseguirParaTriagem()
    {

        MostrarFaq = false;
        MostrarTriagem = true;
        CanChat = false;
        CanChamarAtendente = false;
    }

    [RelayCommand]
    public async Task ExecutarTriagemAsync()
    {
        var texto = Ticket?.Descricao ?? string.Empty;
        if (string.IsNullOrWhiteSpace(texto)) { Sugestoes = new(); return; }
        try
        {
            var result = await _triagem.SugerirAsync(texto);
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(result?.Faq)) list.Add(result!.Faq!);
            if (!string.IsNullOrWhiteSpace(result?.Sugestao))
            {
                var s = result!.Sugestao!;
                if (list.Count == 0 || !string.Equals(list[0], s, StringComparison.OrdinalIgnoreCase))
                    list.Add(s);
            }

            if (list.Count > 0)
            {
                foreach (var s in list)
                {
                    await _chamados.SendMessageAsync(TicketId, new TicketMessageRequest
                    {
                        Texto = s
                    }, automatic: true);
                }
            }
            if (list.Count == 0)
            {

                await _chamados.SendMessageAsync(TicketId, new TicketMessageRequest
                {
                    Texto = "Não encontramos mensagens prontas para sua resolução. Chame um atendente para ajudá-lo."
                }, automatic: true);
            }

            TriagemExecutada = true;
            CanChamarAtendente = true;
            CanChat = true;
            await CarregarAsync();
        }
        catch
        {

            await _chamados.SendMessageAsync(TicketId, new TicketMessageRequest
            {
                Texto = "Não foi possível executar a triagem no momento. Chame um atendente para ajudá-lo."
            }, automatic: true);
            TriagemExecutada = true;
            CanChamarAtendente = true;
            CanChat = true;
            await CarregarAsync();
        }
    }

    [RelayCommand]
    public void NaoEncontreiNaTriagem()
    {
        CanChamarAtendente = true;
        CanChat = true;
    }

    [RelayCommand]
    public async Task AssumirAsync()
    {

    if (!IsSuporte || TicketId == Guid.Empty || !CanAssumir) return;
        var ok = await _chamados.AssumirAsync(TicketId);
        if (!ok)
        {
            await AlertAsync("Erro", "Não foi possível assumir o chamado.");
            return;
        }

    CanAssumir = false;
    await CarregarAsync();
    }

    [RelayCommand]
    public async Task EncerrarAsync()
    {
        if (!IsSuporte || TicketId == Guid.Empty || !CanEncerrar) return;
        
        // Se foi resolvido pelo usuário (tem DataResolvidoUsuario), não pedir resolução final (já tem feedback)
        var resolvidoPeloUsuario = Ticket?.DataResolvidoUsuario.HasValue == true;
        
        System.Diagnostics.Debug.WriteLine($"[ENCERRAR] DataResolvidoUsuario={Ticket?.DataResolvidoUsuario}, resolvidoPeloUsuario={resolvidoPeloUsuario}");
        
        // Se foi resolvido pelo usuário, só encerra sem pedir resolução
        if (resolvidoPeloUsuario)
        {
            var closedIA = await _chamados.CloseAsync(TicketId);
            if (!closedIA)
            {
                await AlertAsync("Erro", "Falha ao encerrar o chamado.");
                return;
            }
            await CarregarAsync();
            return;
        }
        
        // Se NÃO foi resolvido pelo usuário, pede resolução final ANTES de encerrar
        var resolucao = await PromptAsync("Resolução final", "Descreva a solução aplicada para este chamado:", "Escreva a solução...");
        
        // Se cancelou o prompt, não encerra
        if (string.IsNullOrWhiteSpace(resolucao))
        {
            return;
        }
        
        // Fecha o chamado
        var closedSupport = await _chamados.CloseAsync(TicketId);
        if (!closedSupport)
        {
            await AlertAsync("Erro", "Falha ao encerrar o chamado.");
            return;
        }
        
        // Registra a resolução
        var (ok, error) = await _chamados.RegistrarResolucaoFinalAsync(TicketId, resolucao!);
        if (!ok)
        {
            await AlertAsync("Aviso", string.IsNullOrWhiteSpace(error) ? "Resolução não registrada." : error!);
        }
        
        await CarregarAsync();
    }
    
    [RelayCommand]
    public async Task MarcarResolvidoAsync()
    {
        if (TicketId == Guid.Empty) return;
        try
        {
            await Shell.Current.GoToAsync($"marcar-resolvido?ticketId={TicketId}");
            // Após retornar do modal, recarregar para atualizar status
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            await AlertAsync("Erro", $"Falha ao abrir página: {ex.Message}");
        }
    }
    
    [RelayCommand]
    public async Task ResolverPeloUsuarioAsync()
    {
        if (TicketId == Guid.Empty || !BotaoResolvidoHabilitado) return;
        try
        {
            await Shell.Current.GoToAsync($"marcar-resolvido?ticketId={TicketId}");
            // Após feedback, recarregar chamado
            await Task.Delay(500);
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            await AlertAsync("Erro", $"Falha ao abrir feedback: {ex.Message}");
        }
    }
}
