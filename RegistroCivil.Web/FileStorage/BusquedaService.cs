using System.Diagnostics;
using RegistroCivil.Web.Models;

namespace RegistroCivil.Web.FileStorage;

// El repositorio coordina bloqueos y mide con TotalMilliseconds.
public static class BusquedaService
{
    public static ActaNacimiento? Secuencial(string rutaActas, string clave, bool porDni)
    {
        using var lector = new BinaryReader(File.OpenRead(rutaActas));
        var cantidad = CabeceraArchivo.Leer(lector);
        for (long i = 0; i < cantidad; i++)
        {
            var acta = ActaArchivoBinario.Leer(lector);
            if ((porDni ? acta.DniInscrito : acta.NumeroActa) == clave) return acta;
        }
        return null;
    }
}
