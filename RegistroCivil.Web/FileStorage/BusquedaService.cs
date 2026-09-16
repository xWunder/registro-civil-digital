using System.Diagnostics;
using RegistroCivil.Web.Models;

namespace RegistroCivil.Web.FileStorage;

public static class BusquedaService
{
    public static (ActaNacimiento? Acta, long TiempoMs) BuscarPorDniIndexado(
        string rutaActas, string rutaIndiceDni, string dniBuscado)
    {
        var cronometro = Stopwatch.StartNew();

        var posicion = IndiceBinario.BuscarBinario(
            rutaIndiceDni, dniBuscado, FormatoArchivoActas.CaracteresDni);

        ActaNacimiento? acta = null;

        if (posicion != -1)
        {
            using var flujo = new FileStream(rutaActas, FileMode.Open, FileAccess.Read);
            using var lector = new BinaryReader(flujo);

            flujo.Seek(FormatoArchivoActas.CalcularOffset(posicion), SeekOrigin.Begin);
            acta = ActaArchivoBinario.Leer(lector);
        }

        cronometro.Stop();
        return (acta, cronometro.ElapsedMilliseconds);
    }

    public static (ActaNacimiento? Acta, long TiempoMs) BuscarPorDniSecuencial(
        string rutaActas, string dniBuscado)
    {
        var cronometro = Stopwatch.StartNew();

        ActaNacimiento? encontrada = null;

        using (var lector = new BinaryReader(File.OpenRead(rutaActas)))
        {
            var cantidadRegistros = CabeceraArchivo.Leer(lector);

            for (long i = 0; i < cantidadRegistros; i++)
            {
                var acta = ActaArchivoBinario.Leer(lector);

                if (acta.DniInscrito == dniBuscado)
                {
                    encontrada = acta;
                    break; // Ya la encontramos, no seguimos leyendo el resto
                }
            }
        }

        cronometro.Stop();
        return (encontrada, cronometro.ElapsedMilliseconds);
    }
}