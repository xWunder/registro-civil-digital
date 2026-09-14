using RegistroCivil.Web.Components;
using RegistroCivil.Web.Services;
using RegistroCivil.Web.FileStorage;
using RegistroCivil.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ActaNacimientoService>();

var app = builder.Build();

// ===== PRUEBA TEMPORAL: crear actas.dat de prueba y generar el índice =====
var rutaActas = "actas.dat";
var rutaIndiceDni = "dni.idx";

using (var escritor = new BinaryWriter(File.Create(rutaActas)))
{
    var actasPrueba = new List<ActaNacimiento>
    {
        new ActaNacimiento
        {
            Id = 1, NumeroActa = "ACT-001", DniInscrito = "10000003",
            ApellidoPaterno = "Cruz", Nombres = "Carlos",
            FechaNacimiento = new DateOnly(2000, 1, 1),
            Sexo = "M", UbigeoNacimiento = "130101", LugarNacimiento = "Trujillo"
        },
        new ActaNacimiento
        {
            Id = 2, NumeroActa = "ACT-002", DniInscrito = "10000001",
            ApellidoPaterno = "Ramos", Nombres = "Ana",
            FechaNacimiento = new DateOnly(2001, 2, 2),
            Sexo = "F", UbigeoNacimiento = "130101", LugarNacimiento = "Trujillo"
        },
        new ActaNacimiento
        {
            Id = 3, NumeroActa = "ACT-003", DniInscrito = "10000002",
            ApellidoPaterno = "Torres", Nombres = "Luis",
            FechaNacimiento = new DateOnly(2002, 3, 3),
            Sexo = "M", UbigeoNacimiento = "130101", LugarNacimiento = "Trujillo"
        },
    };

    CabeceraArchivo.Escribir(escritor, actasPrueba.Count);

    foreach (var acta in actasPrueba)
    {
        ActaArchivoBinario.Escribir(escritor, acta);
    }
}

IndiceBinario.GenerarIndicePorDni(rutaActas, rutaIndiceDni);

using (var lectorIndice = new BinaryReader(File.OpenRead(rutaIndiceDni)))
{
    Console.WriteLine("Contenido del índice (debería estar ordenado):");
    while (lectorIndice.BaseStream.Position < lectorIndice.BaseStream.Length)
    {
        var dni = TextoFijoBinario.Leer(lectorIndice, FormatoArchivoActas.CaracteresDni);
        var posicion = lectorIndice.ReadInt64();
        Console.WriteLine($"  DNI: {dni} -> Posición: {posicion}");
    }
}

Console.WriteLine($"Prueba completada. Archivos creados en: {Directory.GetCurrentDirectory()}");
// ===== FIN DE PRUEBA TEMPORAL =====

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