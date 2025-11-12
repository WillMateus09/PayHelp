using Microsoft.Extensions.DependencyInjection;
using PayHelp.Application.Abstractions;
using PayHelp.Application.Services;
using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;
using PayHelp.Domain.Security;

namespace PayHelp.Infrastructure.InMemory;

public static class DependencyInjection
{
    public static IServiceCollection AddPayHelpInMemory(this IServiceCollection services)
    {

        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<ITicketRepository, TicketRepository>();
        services.AddSingleton<ICannedMessageRepository, CannedMessageRepository>();
        services.AddSingleton<IReportSink, ReportSink>();


        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITriageService, TriageService>();
        services.AddScoped<ICannedMessageService, CannedMessageService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }

    public static IServiceCollection SeedPayHelp(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();
        var users = sp.GetRequiredService<IUserRepository>();
        var canned = sp.GetRequiredService<ICannedMessageRepository>();


        var suporte = new User
        {
            NumeroInscricao = "0001",
            Nome = "Suporte 1",
            Email = "suporte@payhelp.local",
            SenhaHash = HashStub.Hash("123456"),
            Role = UserRole.Suporte
        };
        users.AddAsync(suporte).GetAwaiter().GetResult();


        canned.AddAsync(new CannedMessage { Titulo = "Senha Expirada", Conteudo = "Tente redefinir sua senha em Configurações...", GatilhoPalavraChave = "senha,esqueci" }).GetAwaiter().GetResult();
        canned.AddAsync(new CannedMessage { Titulo = "Problemas de Acesso", Conteudo = "Verifique sua conexão e tente novamente...", GatilhoPalavraChave = "acesso,conexão" }).GetAwaiter().GetResult();
        canned.AddAsync(new CannedMessage { Titulo = "Erro de Pagamento", Conteudo = "Confirme os dados de cartão...", GatilhoPalavraChave = "pagamento,cartão" }).GetAwaiter().GetResult();

        return services;
    }
}
