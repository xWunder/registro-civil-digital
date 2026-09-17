using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using RegistroCivil.Web.FileStorage;
using RegistroCivil.Web.Models;

var carpeta = Path.Combine(Path.GetTempPath(), "RegistroCivil-Pruebas-" + Guid.NewGuid().ToString("N"));
var archivo = new ArchivoActasRepository(carpeta);
var aprobadas = 0;
void Verificar(bool condicion, string caso)
{
    if (!condicion) throw new Exception("FALLO: " + caso);
    Console.WriteLine("APROBADO: " + caso); aprobadas++;
}
void Rechaza(Action accion, string caso)
{
    try { accion(); } catch (Exception ex) when (ex is ValidationException or InvalidOperationException or InvalidDataException or ArgumentException or EndOfStreamException)
    { Verificar(true, caso); return; }
    throw new Exception("FALLO (debió rechazar): " + caso);
}
ActaNacimiento Nueva(string numero, string? dni) => new()
{
    NumeroActa = numero, DniInscrito = dni, ApellidoPaterno = "Muñoz", ApellidoMaterno = "Pérez",
    Nombres = "Lucía", FechaNacimiento = new DateOnly(2001, 5, 20), Sexo = "F",
    UbigeoNacimiento = "130101", LugarNacimiento = "Trujillo"
};

Console.WriteLine($"Entorno: {Environment.OSVersion}; .NET {Environment.Version}; {Environment.ProcessorCount} procesadores lógicos");
Console.WriteLine($"Pruebas aisladas: {carpeta}");
Verificar(archivo.Estado().Registros == 0, "Archivo vacío válido");
var reloj = Stopwatch.StartNew();
Verificar(archivo.Preparar(DatosSimulados.Crear()) == 100000, "Carga de 100000 registros ficticios");
Console.WriteLine($"Carga + índices: {reloj.Elapsed.TotalMilliseconds:F3} ms");
Verificar(archivo.Estado().Bytes == 64L + 100000L * 512, "Cabecera 64 + 100000 registros de 512 bytes");
Verificar(new FileInfo(Path.Combine(carpeta, "dni.idx")).Length == 100000 * 24L, "Índice DNI de 24 bytes por entrada");
Verificar(new FileInfo(Path.Combine(carpeta, "numero.idx")).Length == 100000 * 48L, "Índice acta de 48 bytes por entrada");
foreach (int n in new[] { 100000, 149999, 199999 })
{
    var dni = (90000000 + n).ToString();
    var numero = $"ACT-2026-{n}";
    Verificar(archivo.Buscar(dni, true)?.NumeroActa == numero, "Busca DNI " + dni);
    Verificar(archivo.Buscar(numero)?.DniInscrito == dni, "Busca número " + numero);
}
Verificar(archivo.Buscar("00000000", true) is null && archivo.Buscar("NO-EXISTE") is null, "Claves inexistentes");
Verificar(archivo.Buscar(" act-2026-100000 ")?.Id == 1, "Normalización de clave de acta");
var pagina = archivo.Listar(2, 50);
Verificar(pagina.TotalRegistros == 100000 && pagina.Actas.Count == 50 && pagina.Actas[0].Id == 99950, "Paginación directa sin cargar todos los registros");
Verificar(archivo.Listar(2000, 50).Actas[^1].Id == 1, "Última página");
Rechaza(() => archivo.Preparar(DatosSimulados.Crear()), "No sobrescribir archivo existente");
Rechaza(() => archivo.Agregar(Nueva("ACT-2026-100000", "88000000")), "Rechazo de número duplicado");
Rechaza(() => archivo.Agregar(Nueva("DEMO-1", "90100000")), "Rechazo de DNI duplicado");
Rechaza(() => archivo.Agregar(Nueva("DEMO-1", "123")), "DNI inválido");
Rechaza(() => archivo.Agregar(Nueva("DEMO-1", "１２３４５６７８")), "DNI no ASCII rechazado");
var futura = Nueva("DEMO-1", "88000000"); futura.FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
Rechaza(() => archivo.Agregar(futura), "Fecha futura rechazada");
var larga = Nueva("DEMO-1", "88000000"); larga.Nombres = new string('A', 61);
Rechaza(() => archivo.Agregar(larga), "Longitud excesiva rechazada sin truncar");
var archivoDatos = Path.Combine(carpeta, "actas.dat");
byte[] prefijo;
using (var f = File.OpenRead(archivoDatos)) { f.Position = 64; prefijo = new byte[512]; f.ReadExactly(prefijo); }
var nueva = Nueva("DEMO-1", "88000000");
Verificar(archivo.Agregar(nueva) == 100001, "Inserción al final e ID creciente");
Verificar(archivo.Estado().Bytes == 64 + 100001L * 512, "Inserción agrega exactamente 512 bytes");
using (var f = File.OpenRead(archivoDatos)) { f.Position = 64; var actual = new byte[512]; f.ReadExactly(actual); Verificar(prefijo.SequenceEqual(actual), "Inserción conserva registro anterior"); }
Verificar(archivo.Buscar("88000000", true)?.Nombres == "Lucía", "Índice actualizado y Unicode conservado");
nueva.DniInscrito = "88000001"; nueva.Nombres = "Lucía Elena";
Verificar(archivo.Modificar(nueva), "Edición de acta activa");
Verificar(archivo.Buscar("88000000", true) is null && archivo.Buscar("88000001", true)?.Nombres == "Lucía Elena", "Edición elimina clave anterior y actualiza índice");
var longitudAntes = archivo.Estado().Bytes;
Verificar(archivo.Anular(nueva.Id) && !archivo.Anular(nueva.Id), "Anulación lógica y repetición sin efectos");
Verificar(archivo.Buscar("DEMO-1")?.Estado == 0 && archivo.Estado().Bytes == longitudAntes, "Anulación conserva registro y tamaño");
Verificar(!archivo.Modificar(nueva), "Acta anulada no editable");
Rechaza(() => archivo.Agregar(Nueva("DEMO-2", "88000001")), "DNI de acta anulada sigue reservado");
archivo.Agregar(Nueva("DEMO-SIN-DNI-1", null));
archivo.Agregar(Nueva("DEMO-SIN-DNI-2", null));
Verificar(archivo.Buscar("DEMO-SIN-DNI-1")?.DniInscrito is null, "Múltiples actas sin DNI permitidas");
var reinicio = new ArchivoActasRepository(carpeta);
Verificar(reinicio.Estado().Registros == 100003 && reinicio.Buscar("DEMO-1")?.Estado == 0, "Persistencia al reiniciar repositorio");
File.Delete(Path.Combine(carpeta, "dni.idx"));
Verificar(reinicio.Buscar("90100000", true)?.Id == 1, "Reconstrucción de índice faltante");

// Simular interrupción después de modificar un registro y antes de confirmar.
byte[] cabecera, registro;
long longitud;
using (var f = File.OpenRead(archivoDatos)) { longitud = f.Length; cabecera = new byte[64]; registro = new byte[512]; f.ReadExactly(cabecera); f.ReadExactly(registro); }
File.WriteAllText(Path.Combine(carpeta, "operacion.json"), JsonSerializer.Serialize(new { Longitud = longitud, Posicion = 0L, Cabecera = cabecera, Registro = registro }));
using (var f = new FileStream(archivoDatos, FileMode.Open, FileAccess.Write)) { f.Position = 64; f.Write(new byte[512]); }
Verificar(new ArchivoActasRepository(carpeta).Buscar("90100000", true)?.Id == 1, "Recuperación de escritura interrumpida desde diario");
using (var memoria = new MemoryStream(new byte[63]))
using (var lector = new BinaryReader(memoria)) Rechaza(() => CabeceraArchivo.Leer(lector), "Cabecera truncada rechazada");
using (var memoria = new MemoryStream())
using (var w = new BinaryWriter(memoria)) Rechaza(() => TextoFijoBinario.Escribir(w, "ABCDEFGHI", 8), "Texto binario nunca truncado silenciosamente");

// Medir sobre exactamente 100000 actas en otro archivo independiente del CRUD.
var rendimiento = new ArchivoActasRepository(Path.Combine(carpeta, "rendimiento"));
rendimiento.Preparar(DatosSimulados.Crear());
foreach (var (clave, dni) in new[] { ("90100000", true), ("90149999", true), ("90199999", true), ("00000000", true), ("ACT-2026-199999", false), ("NO-EXISTE", false) })
{
    var r = rendimiento.Comparar(clave, dni);
    Verificar(r.Secuencial.Id == r.Indexada.Id, "Equivalencia de métodos: " + clave);
    Console.WriteLine($"MEDICION | {clave} | n={r.Registros} | secuencial mediana={r.Secuencial.MedianaMs:F4} min={r.Secuencial.MinimoMs:F4} max={r.Secuencial.MaximoMs:F4} ms | indexada mediana={r.Indexada.MedianaMs:F4} min={r.Indexada.MinimoMs:F4} max={r.Indexada.MaximoMs:F4} ms");
}
Console.WriteLine($"RESULTADO: {aprobadas} pruebas aprobadas. No se modificó SQL ni el archivo de uso de la aplicación.");
Console.WriteLine("Los archivos temporales de prueba se conservan en: " + carpeta);
