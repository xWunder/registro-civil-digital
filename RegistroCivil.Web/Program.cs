using RegistroCivil.Web.Components;
using RegistroCivil.Web.Services;
using RegistroCivil.Web.FileStorage;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Opción exclusiva de pruebas locales: claves nuevas en memoria, sin acceder
// a las claves DPAPI de otra cuenta. No se habilita en ejecución normal.
if (builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Verification:EphemeralKeys"))
{
    builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ActaNacimientoService>();
builder.Services.AddSingleton(sp => new ArchivoActasRepository(
    Path.Combine(builder.Environment.ContentRootPath,
        builder.Configuration["Archivos:Directorio"] ?? "Data/ArchivoCivil")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
