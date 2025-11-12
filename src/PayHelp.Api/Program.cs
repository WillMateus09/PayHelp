using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Hosting;
using PayHelp.Application.Services;
using PayHelp.Application.Abstractions;
using PayHelp.Infrastructure;
using PayHelp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PayHelp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Server=localhost;Database=PayHelp_Banco;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure();
    }));


builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICannedMessageRepository, CannedMessageRepository>();
builder.Services.AddScoped<IFaqRepository, FaqRepository>();
builder.Services.AddScoped<IReportSink, ReportSink>();


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITriageService, TriageService>();
builder.Services.AddScoped<ICannedMessageService, CannedMessageService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFaqService, FaqService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PayHelp API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] {} }
    });
});


builder.Services.AddCors(options =>
{

    options.AddPolicy("AllowDev", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});


builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? "dev-super-secret-key-please-change");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        db.Users.Add(new PayHelp.Domain.Entities.User
        {
            NumeroInscricao = "0001",
            Nome = "Suporte 1",
            Email = "suporte@payhelp.local",
            SenhaHash = PayHelp.Domain.Security.HashStub.Hash("123456"),
            Role = PayHelp.Domain.Enums.UserRole.Suporte
        });
        db.CannedMessages.AddRange(
            new PayHelp.Domain.Entities.CannedMessage{ Titulo="Senha Expirada", Conteudo="Tente redefinir sua senha em Configurações...", GatilhoPalavraChave="senha,esqueci"},
            new PayHelp.Domain.Entities.CannedMessage{ Titulo="Problemas de Acesso", Conteudo="Verifique sua conexão e tente novamente...", GatilhoPalavraChave="acesso,conexão"},
            new PayHelp.Domain.Entities.CannedMessage{ Titulo="Erro de Pagamento", Conteudo="Confirme os dados de cartão...", GatilhoPalavraChave="pagamento,cartão"}
        );
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", () => Results.Redirect("/swagger"));


    app.MapPost("/admin/dev/purge", async (AppDbContext db) =>
    {

        await db.Database.ExecuteSqlRawAsync("DELETE FROM [FaqEntries]");

        await db.Database.ExecuteSqlRawAsync("DELETE FROM [Tickets]");

        await db.Database.ExecuteSqlRawAsync("DELETE FROM [Users]");
        return Results.Ok(new { ok = true, message = "All tickets and users purged (development)." });
    });
}


if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.MapGet("/__version", (IWebHostEnvironment env) =>
{
    var asm = typeof(Program).Assembly;
    var name = asm.GetName();
    return Results.Json(new
    {
        app = "PayHelp.Api",
        environment = env.EnvironmentName,
        assembly = name.Name,
        version = name.Version?.ToString(),
        location = asm.Location
    });
});


app.MapGet("/health/db", async (AppDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
    var result = new
    {
        provider = db.Database.ProviderName,
        canConnect,
        pendingMigrations = pendingMigrations.ToArray()
    };
    return canConnect ? Results.Json(result) : Results.Problem(title: "Database connection failed", statusCode: 503, extensions: new Dictionary<string, object?> { ["details"] = result });
});

app.MapGet("/health/integrity", async (AppDbContext db) =>
{
    var expectedFks = new[]
    {
        "FK_FaqEntries_Tickets_TicketId",
        "FK_Reports_Tickets_TicketId",
        "FK_TicketMessages_Users_RemetenteUserId",
        "FK_Tickets_Users_SupportUserId",
        "FK_Tickets_Users_UserId"
    };

    var existing = new List<string>();
    await using (var conn = db.Database.GetDbConnection())
    {
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sys.foreign_keys";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            existing.Add(reader.GetString(0));
        }
    }

    var missing = expectedFks.Except(existing, StringComparer.OrdinalIgnoreCase).ToArray();
    var ok = missing.Length == 0;
    var payload = new { ok, expected = expectedFks, existing = existing.ToArray(), missing };
    return ok ? Results.Json(payload) : Results.Problem(title: "Missing foreign keys", statusCode: 500, extensions: new Dictionary<string, object?> { ["details"] = payload });
});
app.Run();
