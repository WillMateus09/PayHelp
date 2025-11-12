var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Lightweight version endpoint to identify this app instance
app.MapGet("/__version", (IWebHostEnvironment env) =>
{
    var asm = typeof(Program).Assembly;
    var name = asm.GetName();
    return Results.Json(new
    {
        app = "MeuProjeto (root MVC)",
        environment = env.EnvironmentName,
        assembly = name.Name,
        version = name.Version?.ToString(),
        location = asm.Location
    });
});

app.Run();
