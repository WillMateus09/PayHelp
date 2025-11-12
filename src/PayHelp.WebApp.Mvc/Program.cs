using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using PayHelp.WebApp.Mvc.Hubs;
using PayHelp.WebApp.Mvc.Services;
using PayHelp.Infrastructure.InMemory;

var builder = WebApplication.CreateBuilder(args);


var apiBaseFromEnv = Environment.GetEnvironmentVariable("PAYHELP_API_URL");
if (!string.IsNullOrWhiteSpace(apiBaseFromEnv))
{

    if (apiBaseFromEnv.EndsWith("/")) apiBaseFromEnv = apiBaseFromEnv.TrimEnd('/');
    builder.Configuration["Api:BaseUrl"] = apiBaseFromEnv;
}

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.LogoutPath = "/Account/Logout";
        o.AccessDeniedPath = "/Home/Forbidden";
        o.Cookie.Name = "PayHelp.Auth";
        o.Events = new CookieAuthenticationEvents
        {
            OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.Redirect("/Home/Forbidden");
                return Task.CompletedTask;
            },
            OnRedirectToLogin = ctx =>
            {
                var returnUrl = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
                ctx.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSession();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddSignalR();
 
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));
builder.Services.AddHttpClient<ApiService>((sp, client) =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiOptions>>().Value;
    var baseUrl = string.IsNullOrWhiteSpace(opts?.BaseUrl)
        ? "http://192.168.15.105:5236/api"
        : opts!.BaseUrl!;
    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new System.Net.Http.SocketsHttpHandler
{
    AllowAutoRedirect = false
});

builder.Services.AddSingleton<ITriageTracker, TriageTracker>();


builder.Services
    .AddPayHelpInMemory()
    .SeedPayHelp();

var app = builder.Build();

var culture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (UnauthorizedAccessException)
    {
    try { context.Session.Remove("ApiJwt"); } catch { }

        if (!context.Response.HasStarted)
        {
            var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
            context.Response.Redirect($"/Account/Login?expired=1&returnUrl={returnUrl}");
            return;
        }
        throw;
    }

    if (context.Response.StatusCode == 401)
    {
        var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
        context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
    }
    else if (context.Response.StatusCode == 403)
    {
        context.Response.Redirect("/Home/Forbidden");
    }
});

app.MapHub<ChatHub>("/hubs/chat");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/__version", (IWebHostEnvironment env) =>
{
    var asm = typeof(Program).Assembly;
    var name = asm.GetName();
    return Results.Json(new
    {
        app = "PayHelp.WebApp.Mvc",
        environment = env.EnvironmentName,
        assembly = name.Name,
        version = name.Version?.ToString(),
        location = asm.Location
    });
});

app.Run();
