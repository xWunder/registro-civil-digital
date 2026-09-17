using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using RegistroCivil.Web.Models;

namespace RegistroCivil.Web.FileStorage;

// Un solo escritor por carpeta (también entre procesos). Los lectores no observan
// índices intermedios. El diario permite recuperar una escritura interrumpida.
public sealed class ArchivoActasRepository
{
    private readonly object gate = new();
    public string Directorio { get; }
    private string Datos => Path.Combine(Directorio, "actas.dat");
    private string Dni => Path.Combine(Directorio, "dni.idx");
    private string Numero => Path.Combine(Directorio, "numero.idx");
    private string Diario => Path.Combine(Directorio, "operacion.json");
    private string Pendiente => Path.Combine(Directorio, "indices.pendientes");

    public ArchivoActasRepository(string directorio) => Directorio = Path.GetFullPath(directorio);

    private T Ejecutar<T>(Func<T> accion)
    {
        lock (gate)
        {
            Directory.CreateDirectory(Directorio);
            using var bloqueo = new FileStream(Path.Combine(Directorio, "escritura.lock"),
                FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            Recuperar();
            if (!File.Exists(Datos))
            {
                using var w = new BinaryWriter(File.Create(Datos));
                CabeceraArchivo.Escribir(w, 0);
            }
            if (File.Exists(Pendiente) || !File.Exists(Dni) || !File.Exists(Numero)) Reconstruir();
            return accion();
        }
    }

    private long Cantidad()
    {
        using var r = new BinaryReader(File.OpenRead(Datos));
        return CabeceraArchivo.Leer(r);
    }

    public (long Registros, long Bytes) Estado() => Ejecutar(() => (Cantidad(), new FileInfo(Datos).Length));

    public (List<ActaNacimiento> Actas, long TotalRegistros) Listar(int pagina, int tamano)
        => Ejecutar(() =>
        {
            if (pagina < 1 || tamano is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pagina));
            var total = Cantidad();
            var lista = new List<ActaNacimiento>();
            using var r = new BinaryReader(File.OpenRead(Datos));
            var inicio = total - 1 - checked((long)(pagina - 1) * tamano);
            for (var p = inicio; p >= 0 && lista.Count < tamano; p--)
            {
                r.BaseStream.Position = FormatoArchivoActas.CalcularOffset(p);
                lista.Add(ActaArchivoBinario.Leer(r));
            }
            return (lista, total);
        });

    private long Posicion(string clave, bool porDni) =>
        IndiceBinario.BuscarBinario(porDni ? Dni : Numero, clave, porDni ? 8 : 20);

    private ActaNacimiento Leer(long posicion)
    {
        if (posicion < 0 || posicion >= Cantidad()) throw new InvalidDataException("Posición fuera del archivo.");
        using var r = new BinaryReader(File.OpenRead(Datos));
        r.BaseStream.Position = FormatoArchivoActas.CalcularOffset(posicion);
        return ActaArchivoBinario.Leer(r);
    }

    public ActaNacimiento? Buscar(string clave, bool porDni = false) => Ejecutar(() =>
    {
        var p = Posicion(clave, porDni);
        return p < 0 ? null : Leer(p);
    });

    public static void Validar(ActaNacimiento acta)
    {
        acta.NumeroActa = acta.NumeroActa.Trim().ToUpperInvariant();
        acta.DniInscrito = string.IsNullOrWhiteSpace(acta.DniInscrito) ? null : acta.DniInscrito.Trim();
        acta.ApellidoPaterno = acta.ApellidoPaterno.Trim();
        acta.ApellidoMaterno = string.IsNullOrWhiteSpace(acta.ApellidoMaterno) ? null : acta.ApellidoMaterno.Trim();
        acta.Nombres = acta.Nombres.Trim();
        acta.Sexo = acta.Sexo.Trim().ToUpperInvariant();
        acta.UbigeoNacimiento = acta.UbigeoNacimiento.Trim();
        acta.LugarNacimiento = acta.LugarNacimiento.Trim();
        Validator.ValidateObject(acta, new ValidationContext(acta), true);
    }

    private void ComprobarDuplicados(ActaNacimiento acta, long posicionPermitida = -1)
    {
        var p = Posicion(acta.NumeroActa, false);
        if (p >= 0 && p != posicionPermitida) throw new InvalidOperationException("El número de acta ya está registrado.");
        if (acta.DniInscrito is not null)
        {
            p = Posicion(acta.DniInscrito, true);
            if (p >= 0 && p != posicionPermitida) throw new InvalidOperationException("El DNI ya está asociado a otra acta.");
        }
    }

    public long Agregar(ActaNacimiento acta) => Ejecutar(() =>
    {
        Validar(acta);
        ComprobarDuplicados(acta);
        var n = Cantidad();
        // IDs conservados al importar; los registros permanecen ordenados por ID.
        acta.Id = n == 0 ? 1 : checked(Leer(n - 1).Id + 1);
        acta.Estado = 1;
        acta.FechaRegistro = acta.FechaModificacion = DateTime.Now;
        EscribirTransaccion(n, acta, true);
        return acta.Id;
    });

    public bool Modificar(ActaNacimiento acta) => Ejecutar(() =>
    {
        Validar(acta);
        var p = Posicion(acta.NumeroActa, false);
        if (p < 0) return false;
        var anterior = Leer(p);
        if (anterior.Id != acta.Id || anterior.Estado == 0) return false;
        ComprobarDuplicados(acta, p);
        acta.FechaRegistro = anterior.FechaRegistro;
        acta.FechaModificacion = DateTime.Now;
        acta.Estado = anterior.Estado;
        EscribirTransaccion(p, acta, false);
        return true;
    });

    public bool Anular(long id) => Ejecutar(() =>
    {
        long izquierda = 0, derecha = Cantidad() - 1;
        while (izquierda <= derecha)
        {
            var p = izquierda + (derecha - izquierda) / 2;
            var acta = Leer(p);
            if (acta.Id == id)
            {
                if (acta.Estado == 0) return false;
                acta.Estado = 0;
                acta.FechaModificacion = DateTime.Now;
                EscribirTransaccion(p, acta, false);
                return true;
            }
            if (acta.Id < id) izquierda = p + 1; else derecha = p - 1;
        }
        return false;
    });

    private sealed record Respaldo(long Longitud, long Posicion, byte[] Cabecera, byte[] Registro);

    private void EscribirTransaccion(long posicion, ActaNacimiento acta, bool agregar)
    {
        // Serializar y validar antes de tocar el archivo.
        using var buffer = new MemoryStream();
        using (var w = new BinaryWriter(buffer, System.Text.Encoding.UTF8, true)) ActaArchivoBinario.Escribir(w, acta);
        if (buffer.Length != 512) throw new InvalidDataException("Tamaño de registro inesperado.");
        Respaldo respaldo;
        using (var r = new BinaryReader(File.OpenRead(Datos)))
        {
            var cabecera = r.ReadBytes(64);
            r.BaseStream.Position = FormatoArchivoActas.CalcularOffset(posicion);
            respaldo = new(r.BaseStream.Length, posicion, cabecera, agregar ? [] : r.ReadBytes(512));
        }
        using (var diario = new FileStream(Diario + ".tmp", FileMode.Create, FileAccess.Write))
        {
            JsonSerializer.Serialize(diario, respaldo);
            diario.Flush(true);
        }
        File.Move(Diario + ".tmp", Diario, true);
        try
        {
            File.WriteAllText(Pendiente, "Reconstruir índices al recuperar.");
            using (var f = new FileStream(Datos, FileMode.Open, FileAccess.Write))
            {
                f.Position = FormatoArchivoActas.CalcularOffset(posicion);
                f.Write(buffer.ToArray());
                if (agregar)
                {
                    f.Position = 0;
                    using var w = new BinaryWriter(f, System.Text.Encoding.UTF8, true);
                    CabeceraArchivo.Escribir(w, posicion + 1);
                    w.Flush();
                }
                f.Flush(true);
            }
            Reconstruir();
            File.Delete(Diario); // Confirmación: datos e índices ya están escritos.
        }
        catch { Recuperar(); throw; }
    }

    private void Recuperar()
    {
        if (!File.Exists(Diario)) return;
        var backup = JsonSerializer.Deserialize<Respaldo>(File.ReadAllText(Diario))
            ?? throw new InvalidDataException("Diario inválido.");
        using (var f = new FileStream(Datos, FileMode.Open, FileAccess.Write))
        {
            f.SetLength(backup.Longitud);
            f.Position = 0;
            f.Write(backup.Cabecera);
            if (backup.Registro.Length > 0)
            {
                f.Position = FormatoArchivoActas.CalcularOffset(backup.Posicion);
                f.Write(backup.Registro);
            }
            f.Flush(true);
        }
        File.WriteAllText(Pendiente, "Recuperación");
        Reconstruir();
        File.Delete(Diario);
    }

    private void Reconstruir()
    {
        IndiceBinario.GenerarIndicePorDni(Datos, Dni + ".tmp");
        IndiceBinario.GenerarIndicePorNumero(Datos, Numero + ".tmp");
        File.Move(Dni + ".tmp", Dni, true);
        File.Move(Numero + ".tmp", Numero, true);
        if (File.Exists(Pendiente)) File.Delete(Pendiente);
    }

    // Carga inicial explícita. No reemplaza un archivo que ya contiene trabajo.
    public long Preparar(IEnumerable<ActaNacimiento> actas) => Ejecutar(() =>
    {
        if (Cantidad() != 0) throw new InvalidOperationException("El archivo ya contiene datos; no se sobrescribirá.");
        var numeros = new HashSet<string>(StringComparer.Ordinal);
        var dnis = new HashSet<string>(StringComparer.Ordinal);
        long n = 0, ultimoId = 0;
        using (var f = new FileStream(Datos + ".tmp", FileMode.Create, FileAccess.Write))
        {
            using var w = new BinaryWriter(f, System.Text.Encoding.UTF8, true);
            CabeceraArchivo.Escribir(w, 0);
            foreach (var a in actas)
            {
                Validar(a);
                if (a.Id <= ultimoId || !numeros.Add(a.NumeroActa) ||
                    (a.DniInscrito is not null && !dnis.Add(a.DniInscrito)))
                    throw new InvalidDataException("La carga requiere IDs crecientes y claves únicas.");
                ultimoId = a.Id;
                ActaArchivoBinario.Escribir(w, a);
                n++;
            }
            f.Position = 0;
            CabeceraArchivo.Escribir(w, n);
            w.Flush();
            f.Flush(true);
        }
        File.WriteAllText(Pendiente, "Carga inicial");
        File.Move(Datos + ".tmp", Datos, true);
        Reconstruir();
        return n;
    });

    public sealed record Medicion(string Metodo, double MedianaMs, double MinimoMs, double MaximoMs, long? Id);
    public sealed record Comparacion(long Registros, int Repeticiones, string Clave, bool PorDni, Medicion Secuencial, Medicion Indexada);

    public Comparacion Comparar(string clave, bool porDni, int repeticiones = 5) => Ejecutar(() =>
    {
        if (repeticiones is < 1 or > 20) throw new ArgumentOutOfRangeException(nameof(repeticiones));
        clave = clave.Trim().ToUpperInvariant();
        ActaNacimiento? Secuencial() => BusquedaService.Secuencial(Datos, clave, porDni);
        ActaNacimiento? Indexada()
        {
            var p = Posicion(clave, porDni);
            return p < 0 ? null : Leer(p);
        }
        var sec = Medir("Archivo secuencial", Secuencial, repeticiones);
        var idx = Medir("Índice binario + acceso directo", Indexada, repeticiones);
        if (sec.Id != idx.Id) throw new InvalidDataException("Los métodos devolvieron resultados diferentes.");
        return new Comparacion(Cantidad(), repeticiones, clave, porDni, sec, idx);
    });

    private static Medicion Medir(string nombre, Func<ActaNacimiento?> buscar, int repeticiones)
    {
        buscar(); // Calentamiento explícito, excluido de las mediciones.
        var tiempos = new double[repeticiones];
        long? id = null;
        for (var i = 0; i < repeticiones; i++)
        {
            var reloj = Stopwatch.StartNew();
            id = buscar()?.Id;
            tiempos[i] = reloj.Elapsed.TotalMilliseconds;
        }
        Array.Sort(tiempos);
        return new(nombre, tiempos[repeticiones / 2], tiempos[0], tiempos[^1], id);
    }
}
