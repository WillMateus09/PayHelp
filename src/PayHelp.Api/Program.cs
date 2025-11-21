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
builder.Services.AddSignalR();


// Database provider: Always use SQL Server
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=PayHelp_Banco;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

Console.WriteLine($"[Program] Usando connection string: {connectionString}");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure();
    }));


builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();
builder.Services.AddScoped<ICannedMessageRepository, CannedMessageRepository>();
builder.Services.AddScoped<IFaqRepository, FaqRepository>();
builder.Services.AddScoped<IReportSink, ReportSink>();
builder.Services.AddScoped<ITicketFeedbackRepository, TicketFeedbackRepository>();


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
    // Broad dev policy (no credentials needed)
    options.AddPolicy("AllowDev", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

    // SignalR / protected API policy (credentials require explicit origins)
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    if (origins.Length == 0)
    {
        origins = new[] { "http://localhost:5236", "http://192.168.0.100:5236" }; // fallback examples
    }
    options.AddPolicy("AllowSignalR", policy =>
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
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

// Network binding configuration: read from Hosting section or environment variables
try
{
    var bindAddress = builder.Configuration.GetValue<string>("Hosting:BindAddress")
        ?? Environment.GetEnvironmentVariable("PAYHELP_BIND_ADDRESS")
        ?? "0.0.0.0"; // all interfaces
    var port = builder.Configuration.GetValue<int?>("Hosting:Port")
        ?? (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var envPort) ? envPort : 5236);
    var overrideUrl = Environment.GetEnvironmentVariable("PAYHELP_BIND_URL");
    if (!string.IsNullOrWhiteSpace(overrideUrl))
    {
        app.Logger.LogInformation("Binding override via PAYHELP_BIND_URL: {Url}", overrideUrl);
        app.Urls.Clear();
        app.Urls.Add(overrideUrl);
    }
    else
    {
        var url = $"http://{bindAddress}:{port}";
        app.Logger.LogInformation("Binding API to {Url}", url);
        app.Urls.Clear();
        app.Urls.Add(url);
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[WARN] Failed to configure network binding: {ex.Message}");
}


using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Database.IsSqlite())
        {
            // SQLite (Dev): ensure DB exists and self-heal missing columns by recreating file
            db.Database.EnsureCreated();
            try
            {
                await using var conn = db.Database.GetDbConnection();
                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info([Users]);";
                    var hasIsBlocked = false;
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var colName = reader.GetString(1);
                        if (string.Equals(colName, "IsBlocked", StringComparison.OrdinalIgnoreCase))
                        {
                            hasIsBlocked = true;
                            break;
                        }
                    }
                    if (!hasIsBlocked)
                    {
                        await conn.CloseAsync();
                        var dataDir = Path.Combine(app.Environment.ContentRootPath, "App_Data");
                        Directory.CreateDirectory(dataDir);
                        var sqlitePath = Path.Combine(dataDir, "payhelp.db");
                        try { if (File.Exists(sqlitePath)) File.Delete(sqlitePath); }
                        catch (Exception delEx)
                        {
                            logger.LogWarning(delEx, "Failed to delete SQLite DB file during self-heal");
                        }
                        // Recreate with current model
                        db.Database.EnsureCreated();
                        logger.LogInformation("SQLite database re-created to include missing columns.");
                    }
                }
            }
            catch (Exception healEx)
            {
                logger.LogWarning(healEx, "SQLite self-heal check failed; continuing");
            }
        }
        else
        {
            db.Database.Migrate();

            // SQL Server self-heal for legacy databases missing new tables/columns
            try
            {
                await using var conn = db.Database.GetDbConnection();
                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    // 1) Add Users.IsBlocked if missing
                    cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns 
               WHERE Name = N'IsBlocked' AND Object_ID = Object_ID(N'[dbo].[Users]'))
BEGIN
    ALTER TABLE [dbo].[Users] ADD [IsBlocked] bit NOT NULL CONSTRAINT DF_Users_IsBlocked DEFAULT(0);
END";
                    await cmd.ExecuteNonQueryAsync();
                }

                await using (var cmd2 = conn.CreateCommand())
                {
                    // 2) Create SystemSettings table if missing
                    cmd2.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SystemSettings](
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [Value] nvarchar(2000) NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_SystemSettings_Key] ON [dbo].[SystemSettings]([Key]);
END";
                    await cmd2.ExecuteNonQueryAsync();
                }

                await using (var cmd3 = conn.CreateCommand())
                {
                    // 3) Create TicketFeedbacks table if missing (with FKs)
                    cmd3.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TicketFeedbacks]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TicketFeedbacks](
        [Id] uniqueidentifier NOT NULL,
        [TicketId] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [Nota] int NULL,
        [Comentario] nvarchar(1000) NULL,
        [DataCriacaoUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_TicketFeedbacks] PRIMARY KEY ([Id])
    );
    ALTER TABLE [dbo].[TicketFeedbacks] ADD CONSTRAINT [FK_TicketFeedbacks_Tickets_TicketId]
        FOREIGN KEY([TicketId]) REFERENCES [dbo].[Tickets] ([Id]) ON DELETE CASCADE;
    ALTER TABLE [dbo].[TicketFeedbacks] ADD CONSTRAINT [FK_TicketFeedbacks_Users_UsuarioId]
        FOREIGN KEY([UsuarioId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION;
END";
                    await cmd3.ExecuteNonQueryAsync();
                }
            }
            catch (Exception selfHealEx)
            {
                logger.LogWarning(selfHealEx, "SQL Server self-heal for schema failed; continuing");
            }

            // If for any reason the underlying connection string is empty, mark to skip seeding to avoid EF errors
            var underlyingConnStr = db.Database.GetDbConnection().ConnectionString;
            var skipSeeding = string.IsNullOrWhiteSpace(underlyingConnStr);
            if (skipSeeding)
            {
                logger.LogWarning("Skipping database seeding because the connection string is empty.");
            }
        }

        // Seed defaults idempotently
        if (!string.IsNullOrWhiteSpace(db.Database.GetDbConnection().ConnectionString) && !db.Users.Any())
        {
            db.Users.Add(new PayHelp.Domain.Entities.User
            {
                NumeroInscricao = "0001",
                Nome = "Suporte 1",
                Email = "suporte@payhelp.local",
                SenhaHash = PayHelp.Domain.Security.HashStub.Hash("123456"),
                Role = PayHelp.Domain.Enums.UserRole.Suporte
            });
        }

        // Ensure Master admin exists
        var masterEmail = "payhelp.master@gmail.com";
        if (!string.IsNullOrWhiteSpace(db.Database.GetDbConnection().ConnectionString) && !db.Users.Any(u => u.Email == masterEmail))
        {
            db.Users.Add(new PayHelp.Domain.Entities.User
            {
                NumeroInscricao = "9999",
                Nome = "Master Admin",
                Email = masterEmail,
                SenhaHash = PayHelp.Domain.Security.HashStub.Hash("PayHelp@123"),
                Role = PayHelp.Domain.Enums.UserRole.Master
            });
        }

        // Default system settings (tolerant to missing table in existing DB)
        try
        {
            if (!string.IsNullOrWhiteSpace(db.Database.GetDbConnection().ConnectionString) && !db.Set<PayHelp.Domain.Entities.SystemSetting>().Any())
            {
                db.Set<PayHelp.Domain.Entities.SystemSetting>().Add(new PayHelp.Domain.Entities.SystemSetting{ Key = "SupportVerificationWord", Value = "SUP", UpdatedAtUtc = DateTime.UtcNow});
                db.Set<PayHelp.Domain.Entities.SystemSetting>().Add(new PayHelp.Domain.Entities.SystemSetting{ Key = "PublicBaseUrl", Value = null, UpdatedAtUtc = DateTime.UtcNow});
            }
        }
        catch (Exception missingSettingsEx)
        {
            logger.LogWarning(missingSettingsEx, "SystemSettings table missing; attempting EnsureCreated() and continuing");
            // As a safety net, create schema based on model if migrations are out of sync
            db.Database.EnsureCreated();
            if (!string.IsNullOrWhiteSpace(db.Database.GetDbConnection().ConnectionString) && !db.Set<PayHelp.Domain.Entities.SystemSetting>().Any())
            {
                db.Set<PayHelp.Domain.Entities.SystemSetting>().Add(new PayHelp.Domain.Entities.SystemSetting{ Key = "SupportVerificationWord", Value = "SUP", UpdatedAtUtc = DateTime.UtcNow});
                db.Set<PayHelp.Domain.Entities.SystemSetting>().Add(new PayHelp.Domain.Entities.SystemSetting{ Key = "PublicBaseUrl", Value = null, UpdatedAtUtc = DateTime.UtcNow});
            }
        }

        if (!string.IsNullOrWhiteSpace(db.Database.GetDbConnection().ConnectionString) && !db.CannedMessages.Any())
        {
            db.CannedMessages.AddRange(
                new PayHelp.Domain.Entities.CannedMessage{ Titulo="Senha Expirada", Conteudo="Tente redefinir sua senha em Configurações...", GatilhoPalavraChave="senha,esqueci"},
                new PayHelp.Domain.Entities.CannedMessage{ Titulo="Problemas de Acesso", Conteudo="Verifique sua conexão e tente novamente...", GatilhoPalavraChave="acesso,conexão"},
                new PayHelp.Domain.Entities.CannedMessage{ Titulo="Erro de Pagamento", Conteudo="Confirme os dados de cartão...", GatilhoPalavraChave="pagamento,cartão"}
            );
        }
        if (!string.IsNullOrWhiteSpace(db.Database.GetDbConnection().ConnectionString))
        {
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration/seed failed. API will start without DB migration.");
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
app.UseCors("AllowSignalR");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PayHelp.Api.Hubs.ChatHub>("/hubs/chat");


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
try
{
    app.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[FATAL] API failed to start: {ex}");
    throw;
}
