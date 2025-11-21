using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace PayHelp.Client;

public sealed class ChatClientService
{
    private readonly ITokenStore _tokenStore;
    private readonly string _hubUrl; // full hub url without trailing slash
    private HubConnection? _connection;

    public event Action? OnConnected;
    public event Action? OnDisconnected;
    public event Action<string, string>? OnMessageReceived; // (user, message)

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public ChatClientService(IOptions<ApiOptions> options, ITokenStore tokenStore)
    {
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        var rawBase = options?.Value?.BaseUrl;
        var baseUrl = ApiBaseUrlHelper.NormalizeBaseUrl(rawBase);
        var root = ApiBaseUrlHelper.ExtractRoot(baseUrl);
        _hubUrl = root.TrimEnd('/') + "/hubs/chat"; // Ajustar se o hub estiver em rota diferente
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        if (_connection is not null && _connection.State is HubConnectionState.Connected or HubConnectionState.Connecting)
            return;

        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, opts =>
            {
                opts.AccessTokenProvider = async () => await _tokenStore.GetAsync().ConfigureAwait(false);
            })
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            })
            .Build();

        _connection.Reconnecting += _ => Task.CompletedTask;
        _connection.Reconnected += _ => { OnConnected?.Invoke(); return Task.CompletedTask; };
        _connection.Closed += _ => { OnDisconnected?.Invoke(); return Task.CompletedTask; };

        // Ajustar o nome do método conforme o hub real (ex.: "ReceiveMessage")
        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            OnMessageReceived?.Invoke(user, message);
        });

        await _connection.StartAsync(ct).ConfigureAwait(false);
        OnConnected?.Invoke();
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_connection is null) return;
        try
        {
            await _connection.StopAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
            OnDisconnected?.Invoke();
        }
    }
}
