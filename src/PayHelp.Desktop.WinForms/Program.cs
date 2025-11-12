using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Globalization;
using System.Net;
using System.Diagnostics;

namespace PayHelp.Desktop.WinForms;

public static class AppLog
{
    private static readonly object _lock = new();
    private static readonly string _file = Path.Combine(AppContext.BaseDirectory, "payhelp-winforms.log");
    public static void Write(string message)
    {
        try
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}";
            lock (_lock)
            {
                File.AppendAllText(_file, line + Environment.NewLine);
            }
            Debug.WriteLine(line);
        }
        catch {  }
    }
}

public class ApiClient
{
    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public record LoginRequest(string Email, string Senha);
    public record AuthResponse(Guid UserId, string Nome, string Email, string Role, string? Token);
    public record RegisterSimplesRequest(string NumeroInscricao, string Nome, string Email, string Senha);
    public record RegisterSuporteRequest(string NumeroInscricao, string Nome, string Email, string Senha, string PalavraVerificacao);
    public record TicketDto(Guid Id, string Titulo, string Status, DateTime CriadoEmUtc, DateTime? EncerradoEmUtc, bool? Triaging = null);
    public record AbrirChamadoRequest(Guid SolicitanteId, string Titulo, string Descricao);
    public record EnviarMensagemRequest(Guid RemetenteId, string Conteudo, bool Automatica);
    public record CannedMessageDto(Guid Id, string Titulo, string Conteudo, string? GatilhoPalavraChave);
    public record TicketMessageDto(Guid Id, Guid RemetenteUserId, string Conteudo, DateTime EnviadoEmUtc, bool Automatica);
    public record TicketDetailsDto(Guid Id, string Titulo, string Status, DateTime CriadoEmUtc, DateTime? EncerradoEmUtc, List<TicketMessageDto> Mensagens);
    public record MudarStatusRequest(string NovoStatus, Guid? SupportUserId);
    public record ResolucaoFinalRequest(string Solucao);
    public record CreateCannedRequest(string Titulo, string Conteudo, string? GatilhoPalavraChave);
    public record TriagemRequest(string Texto);
    public record TriagemApiResponse(string? sugestao, string? origem, string? faq);
    public record RelatorioFiltro(DateTime? DeUtc, DateTime? AteUtc, string? Status);
    public record RelatorioDto(Guid TicketId, string Status, string SolicitanteEmail, string SolicitanteRole, TimeSpan? Duracao, DateTime CriadoEmUtc, DateTime? EncerradoEmUtc);
    public record FaqItemDto(int Id, string TituloProblema, string Solucao, DateTime DataCriacao, Guid? TicketId);

    public Uri? BaseAddress => _http.BaseAddress;
    public void UseBaseUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var u))
        {
            _http.BaseAddress = u;
            AppLog.Write($"[API] BaseUrl definida para {u}");
        }
    }

    public async Task<(bool ok, string? version, string? error)> PingVersionAsync(Uri? baseOverride = null, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, new Uri(baseOverride ?? _http.BaseAddress!, "__version"));
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return (false, null, $"HTTP {(int)resp.StatusCode}");
            var text = await resp.Content.ReadAsStringAsync(ct);
            AppLog.Write($"[API] __version -> {(string.IsNullOrWhiteSpace(text) ? "(vazio)" : text.Trim())}");
            return (true, string.IsNullOrWhiteSpace(text) ? "" : text.Trim(), null);
        }
        catch (HttpRequestException ex)
        {
            return (false, null, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<AuthResponse?> LoginAsync(string email, string senha)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/login", new LoginRequest(email, senha), JsonOpts);
        if (!resp.IsSuccessStatusCode)
        {
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {

                try
                {
                    var raw = await resp.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        Debug.WriteLine($"Login 401 detalhe: {raw}");
                    }
                }
                catch {  }
                return null;
            }

            string? serverMsg = null;
            try
            {
                var raw = await resp.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(raw) && raw.TrimStart().StartsWith("{"))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(raw);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("detail", out var d) && d.ValueKind == System.Text.Json.JsonValueKind.String)
                            serverMsg = d.GetString();
                        else if (root.TryGetProperty("title", out var t) && t.ValueKind == System.Text.Json.JsonValueKind.String)
                            serverMsg = t.GetString();
                    }
                    catch { }
                }
                if (string.IsNullOrWhiteSpace(serverMsg))
                    serverMsg = !string.IsNullOrWhiteSpace(resp.ReasonPhrase) ? resp.ReasonPhrase : raw;
            }
            catch { }
            var status = (int)resp.StatusCode;
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(serverMsg) ? $"Falha HTTP {status}" : serverMsg);
        }
        return await resp.Content.ReadFromJsonAsync<AuthResponse>(JsonOpts);
    }

    public Task<AuthResponse> RegistrarSimplesAsync(string numero, string nome, string email, string senha)
        => SendOrThrow<AuthResponse>(HttpMethod.Post, "api/auth/register/simples", new RegisterSimplesRequest(numero, nome, email, senha));

    public Task<AuthResponse> RegistrarSuporteAsync(string numero, string nome, string email, string senha, string palavra)
        => SendOrThrow<AuthResponse>(HttpMethod.Post, "api/auth/register/suporte", new RegisterSuporteRequest(numero, nome, email, senha, palavra));

    public Task<List<TicketDto>?> ListarMeusChamadosAsync(Guid userId)
        => Send<List<TicketDto>>(HttpMethod.Get, $"api/chamados/usuario/{userId}");

    public Task<TicketDto?> AbrirChamadoAsync(Guid userId, string titulo, string descricao)
        => Send<TicketDto>(HttpMethod.Post, "api/chamados", new AbrirChamadoRequest(userId, titulo, descricao));

    public Task<List<CannedMessageDto>?> ListarMensagensAutomaticasAsync()
        => Send<List<CannedMessageDto>>(HttpMethod.Get, "api/mensagensautomaticas");

    public Task<TicketDetailsDto?> ObterChamadoAsync(Guid ticketId)
        => Send<TicketDetailsDto>(HttpMethod.Get, $"api/chamados/{ticketId}");

    public async Task<bool> EnviarMensagemAsync(Guid ticketId, Guid remetenteId, string conteudo, bool automatica)
    {

        await SendOrThrow<object>(HttpMethod.Post, $"api/chamados/{ticketId}/mensagens", new EnviarMensagemRequest(remetenteId, conteudo, automatica));
        return true;
    }

    public Task<List<TicketDto>?> ListarChamadosAsync(string? status)
        => Send<List<TicketDto>>(HttpMethod.Get, status is null ? "api/chamados" : $"api/chamados?status={status}");

    public Task<TicketDto?> MudarStatusAsync(Guid ticketId, string novoStatus, Guid? supportUserId)
        => Send<TicketDto>(HttpMethod.Post, $"api/chamados/{ticketId}/status", new MudarStatusRequest(novoStatus, supportUserId));
    public Task<object?> EncerrarChamadoAsync(Guid ticketId)
        => Send<object>(HttpMethod.Post, $"api/chamados/{ticketId}/encerrar");

    public async Task<(bool ok, string? error)> ChamarAtendenteAsync(Guid ticketId)
    {
        var resp = await SendInternalAsync(HttpMethod.Post, $"api/chamados/{ticketId}/chamar-atendente", body: null);
        if (resp.IsSuccessStatusCode) return (true, null);
        string? msg = null;
        try { msg = await resp.Content.ReadAsStringAsync(); } catch { }
        return (false, string.IsNullOrWhiteSpace(msg) ? $"Falha HTTP {(int)resp.StatusCode}" : msg);
    }

    public async Task<(bool ok, string? error)> AssumirChamadoAsync(Guid ticketId, Guid supportUserId)
    {
        var body = new MudarStatusRequest(TicketStatus.EmAtendimento, supportUserId);
        var resp = await SendInternalAsync(HttpMethod.Post, $"api/chamados/{ticketId}/status", body);
        if (resp.IsSuccessStatusCode) return (true, null);
        string? txt = null; try { txt = await resp.Content.ReadAsStringAsync(); } catch { }
        return (false, string.IsNullOrWhiteSpace(txt) ? resp.ReasonPhrase : txt);
    }

    public Task<object?> RegistrarResolucaoFinalAsync(Guid ticketId, string solucao)
        => Send<object>(HttpMethod.Post, $"api/chamados/{ticketId}/resolucao-final", new ResolucaoFinalRequest(solucao));

    public Task<CannedMessageDto?> CriarMensagemAutomaticaAsync(string titulo, string conteudo, string? gatilhos)
        => Send<CannedMessageDto>(HttpMethod.Post, "api/mensagensautomaticas", new CreateCannedRequest(titulo, conteudo, gatilhos));

    public async Task<bool> RemoverMensagemAutomaticaAsync(Guid id)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"api/mensagensautomaticas/{id}");
        var resp = await _http.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }

    public async Task<string?> SugerirAsync(string texto)
    {

        var typed = await Send<TriagemApiResponse>(HttpMethod.Post, "api/triagem", new TriagemRequest(texto));
        if (typed is not null)
        {
            if (!string.IsNullOrWhiteSpace(typed.sugestao)) return typed.sugestao;
            if (!string.IsNullOrWhiteSpace(typed.faq)) return typed.faq;
        }

        var r = await Send<Dictionary<string, string>>(HttpMethod.Post, "api/triagem", new TriagemRequest(texto));
        if (r != null)
        {
            if (r.TryGetValue("sugestao", out var s1)) return s1;
            if (r.TryGetValue("sugestão", out var s2)) return s2;
            if (r.TryGetValue("suggestion", out var s3)) return s3;
            if (r.TryGetValue("faq", out var s4)) return s4;
        }
        return null;
    }

    public Task<List<RelatorioDto>?> GerarRelatorioAsync(DateTime? de, DateTime? ate, string? status)
        => Send<List<RelatorioDto>>(HttpMethod.Post, "api/relatorios", new RelatorioFiltro(de, ate, status));

    public async Task<List<FaqItemDto>> BuscarFaqAsync(string texto)
    {
        var r = await Send<List<FaqItemDto>>(HttpMethod.Post, "api/faq/buscar", new { Texto = texto });
        return r ?? new List<FaqItemDto>();
    }

    private async Task<T?> Send<T>(HttpMethod method, string url, object? body = null)
    {
        var resp = await SendInternalAsync(method, url, body);
        if (!resp.IsSuccessStatusCode)
            return default;
        return await resp.Content.ReadFromJsonAsync<T>(JsonOpts);
    }

    private async Task<T> SendOrThrow<T>(HttpMethod method, string url, object? body = null)
    {
        var resp = await SendInternalAsync(method, url, body);
        if (!resp.IsSuccessStatusCode)
        {
            string? serverMsg = null;
            try
            {

                var raw = await resp.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(raw) && raw.TrimStart().StartsWith("{"))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(raw);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("detail", out var d) && d.ValueKind == System.Text.Json.JsonValueKind.String)
                            serverMsg = d.GetString();
                        else if (root.TryGetProperty("title", out var t) && t.ValueKind == System.Text.Json.JsonValueKind.String)
                            serverMsg = t.GetString();
                    }
                    catch {  }
                }
                if (string.IsNullOrWhiteSpace(serverMsg))
                {

                    serverMsg = !string.IsNullOrWhiteSpace(resp.ReasonPhrase) ? resp.ReasonPhrase : raw;
                }
            }
            catch {  }
            var status = (int)resp.StatusCode;
            var message = string.IsNullOrWhiteSpace(serverMsg) ? $"Falha HTTP {status}" : serverMsg;
            throw new InvalidOperationException(message);
        }
        return await resp.Content.ReadFromJsonAsync<T>(JsonOpts) ?? throw new InvalidOperationException("Resposta inválida do servidor.");
    }


    private async Task<HttpResponseMessage> SendInternalAsync(HttpMethod method, string url, object? body)
    {
        using var req = new HttpRequestMessage(method, url);
        if (body != null)
            req.Content = JsonContent.Create(body, options: JsonOpts);
        var resp = await _http.SendAsync(req);
        if ((int)resp.StatusCode == 307 || (int)resp.StatusCode == 308)
        {
            if (resp.Headers.Location != null)
            {
                var redirectUri = resp.Headers.Location.IsAbsoluteUri ? resp.Headers.Location : new Uri(_http.BaseAddress!, resp.Headers.Location);
                AppLog.Write($"[NET] Redirect {(int)resp.StatusCode} -> {redirectUri}");
                using var req2 = new HttpRequestMessage(method, redirectUri);
                if (body != null)
                    req2.Content = JsonContent.Create(body, options: JsonOpts);
                resp.Dispose();
                resp = await _http.SendAsync(req2);
            }
        }
        return resp;
    }

    private record ProblemDetailsLite(string? type, string? title, int? status, string? detail, string? instance);
}

static class Program
{
    [STAThread]
    static void Main()
    {
        System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.SystemAware);
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);


        Application.ThreadException += (s, e) =>
        {
            MessageBox.Show(e.Exception.Message, "Erro inesperado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AppLog.Write($"[EXC] ThreadException: {e.Exception.GetType().Name} - {e.Exception.Message}");
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppLog.Write($"[EXC] Unhandled: {ex.GetType().Name} - {ex.Message}");
            }
        };


        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();


        var culture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(config);


    var apiBaseFromEnv = Environment.GetEnvironmentVariable("PAYHELP_API_URL");


        services.AddTransient<AuthHeaderHandler>();
        services.AddHttpClient<ApiClient>(c =>
        {
            var baseUrl = apiBaseFromEnv
                ?? config["Api:BaseUrl"]
                ?? "http://192.168.15.105:5236/";
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            c.BaseAddress = new Uri(baseUrl);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            c.Timeout = TimeSpan.FromSeconds(100);
        })

        .ConfigurePrimaryHttpMessageHandler(() => new System.Net.Http.SocketsHttpHandler { AllowAutoRedirect = false })
        .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddSingleton<SessionContext>();
        services.AddTransient<FrmLogin>();
        services.AddTransient<FrmHome>();
        services.AddTransient<FrmCadastro>();
        services.AddTransient<FrmPainelUsuario>();
        services.AddTransient<FrmPainelSuporte>();
        services.AddTransient<FrmChatChamado>();
        services.AddTransient<FrmMensagensAutomaticas>();
        services.AddTransient<FrmRelatorios>();
    services.AddTransient<FrmAbrirChamadoWizard>();

    using var provider = services.BuildServiceProvider();


    var api = provider.GetRequiredService<ApiClient>();
    var cfg = provider.GetRequiredService<IConfiguration>();
    var sessionCtx = provider.GetRequiredService<SessionContext>();
        var baseUrl = apiBaseFromEnv
            ?? cfg["Api:BaseUrl"]
            ?? api.BaseAddress?.ToString()
            ?? "http://192.168.15.105:5236/";
        var ping = api.PingVersionAsync().GetAwaiter().GetResult();
        if (!ping.ok)
        {
            // Only show HTTPS dev-cert helper when explicitly using https://localhost
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var current) &&
                current.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(current.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                var fallbackPort = current.Port == 7236 ? 5236 : current.Port;
                var fallback = new Uri($"http://{current.Host}:{fallbackPort}/");
                var pingHttp = api.PingVersionAsync(fallback).GetAwaiter().GetResult();
                if (pingHttp.ok)
                {
                    var choice = MessageBox.Show(
                        "Não foi possível conectar via HTTPS (certificado de desenvolvimento não confiável).\n" +
                        $"Detectei a API via HTTP em {fallback}. Deseja usar HTTP apenas nesta sessão?\n\n" +
                        "Dica: execute 'dotnet dev-certs https --trust' para confiar no certificado e voltar ao HTTPS.",
                        "Conexão com API",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );
                    if (choice == DialogResult.Yes)
                    {
                        api.UseBaseUrl(fallback.ToString());
                        AppLog.Write($"[NET] Usando fallback HTTP {fallback}");
                    }
                    else
                    {
                        MessageBox.Show(
                            "Conexão HTTPS necessária não estabelecida. Encerrando aplicação.",
                            "API indisponível",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Não foi possível conectar à API. Verifique se o projeto PayHelp.Api está em execução.\n" +
                        $"Erro: {ping.error}",
                        "API indisponível",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    AppLog.Write($"[NET] Falha ao conectar API (HTTPS e HTTP). Erro: {ping.error}");
                    return;
                }
            }
            else
            {
                MessageBox.Show(
                    "Não foi possível conectar à API. Verifique a BaseUrl em appsettings.json e se a API está em execução.",
                    "API indisponível",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                AppLog.Write("[NET] Falha ao conectar API (config não HTTPS localhost). Encerrando.");
                return;
            }
        }


        while (!sessionCtx.IsAuthenticated)
        {
            AppLog.Write("[APP] Abrindo FrmLogin");
            var login = provider.GetRequiredService<FrmLogin>();
            var result = login.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK && !sessionCtx.IsAuthenticated)
            {
                AppLog.Write($"[APP] Login cancelado. IsAuthenticated={sessionCtx.IsAuthenticated}");

                return;
            }
            else
            {
                AppLog.Write($"[APP] DialogResult={result}; IsAuthenticated={sessionCtx.IsAuthenticated}");
            }
        }

        try
        {
            AppLog.Write("[APP] Iniciando FrmHome");
            var home = provider.GetRequiredService<FrmHome>();
            home.Shown += (_, __) => { try { home.Activate(); home.BringToFront(); } catch { } };

            System.Windows.Forms.MessageBox.Show(
                $"Login OK. Usuário: {sessionCtx.CurrentUser?.Email} (Role: {sessionCtx.CurrentUser?.Role}). Abrindo Home...",
                "Sessão",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information
            );
            home.FormClosed += (_, __) => {
                try { System.Diagnostics.Debug.WriteLine("[HOME] FormClosed - encerrando aplicação"); } catch { }
                AppLog.Write("[APP] FrmHome fechado. Encerrando aplicação.");
            };
            System.Windows.Forms.Application.Run(home);
        }
        catch (Exception ex)
        {
            AppLog.Write($"[APP] Falha ao iniciar Home: {ex.GetType().Name} - {ex.Message}");
            System.Windows.Forms.MessageBox.Show($"Falha ao iniciar Home: {ex.Message}", "Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }
    }


}


public class AuthHeaderHandler : DelegatingHandler
{
    private readonly SessionContext _session;
    public AuthHeaderHandler(SessionContext session)
    {
        _session = session;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _session.AccessToken;

        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        var isAuthEndpoint = path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase);
        if (!isAuthEndpoint && !string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await base.SendAsync(request, cancellationToken);

        if (!isAuthEndpoint && (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden))
        {

            _session.CurrentUser = null;
            _session.AccessToken = null;
            try
            {
                System.Windows.Forms.MessageBox.Show(
                    response.StatusCode == HttpStatusCode.Unauthorized ? "Sessão expirada. Faça login novamente." : "Sem permissão para acessar este recurso.",
                    "Autenticação",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning
                );
            }
            catch {  }
        }
        return response;
    }
}
