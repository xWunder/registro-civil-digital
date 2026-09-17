using RegistroCivil.Web.Models;

namespace RegistroCivil.Web.FileStorage;

public static class IndiceBinario
{
    public static void GenerarIndicePorDni(string rutaActas, string rutaIndice)
    {
        var entradas = new List<EntradaIndice>();

        using (var lector = new BinaryReader(File.OpenRead(rutaActas)))
        {
            var cantidadRegistros = CabeceraArchivo.Leer(lector);

            for (long i = 0; i < cantidadRegistros; i++)
            {
                var acta = ActaArchivoBinario.Leer(lector);

                var entrada = new EntradaIndice
                {
                    Clave = acta.DniInscrito?? string.Empty,
                    Posicion = i
                };

                if (!string.IsNullOrWhiteSpace(entrada.Clave)) entradas.Add(entrada);
            }
        }

        entradas.Sort((a, b) => string.Compare(a.Clave, b.Clave, StringComparison.Ordinal));

        using (var escritor = new BinaryWriter(File.Create(rutaIndice)))
        {
            foreach (var entrada in entradas)
            {
                TextoFijoBinario.Escribir(
                    escritor, entrada.Clave, FormatoArchivoActas.CaracteresDni);

                escritor.Write(entrada.Posicion);
            }
        }
    }

    public static void GenerarIndicePorNumero(string rutaActas, string rutaIndice)
    {
        var entradas = new List<EntradaIndice>();

        using (var lector = new BinaryReader(File.OpenRead(rutaActas)))
        {
            var cantidadRegistros = CabeceraArchivo.Leer(lector);

            for (long i = 0; i < cantidadRegistros; i++)
            {
                var acta = ActaArchivoBinario.Leer(lector);

                var entrada = new EntradaIndice
                {
                    Clave = acta.NumeroActa,
                    Posicion = i
                };

                entradas.Add(entrada);
            }
        }

        entradas.Sort((a, b) => string.Compare(a.Clave, b.Clave, StringComparison.Ordinal));

        using (var escritor = new BinaryWriter(File.Create(rutaIndice)))
        {
            foreach (var entrada in entradas)
            {
                TextoFijoBinario.Escribir(
                    escritor, entrada.Clave, FormatoArchivoActas.CaracteresNumeroActa);

                escritor.Write(entrada.Posicion);
            }
        }
    }


    public static long BuscarBinario(string rutaIndice, string claveBuscada, int caracteresPorClave)
    {
        if (caracteresPorClave is not (8 or 20)) throw new ArgumentOutOfRangeException(nameof(caracteresPorClave));
        claveBuscada = claveBuscada.Trim().ToUpperInvariant();
        var bytesPorEntrada = (caracteresPorClave * 2) + sizeof(long);

        using (var flujo = new FileStream(rutaIndice, FileMode.Open, FileAccess.Read))
        using (var lector = new BinaryReader(flujo))
        {
            if (flujo.Length % bytesPorEntrada != 0)
                throw new InvalidDataException("Índice incompleto.");
            var totalEntradas = flujo.Length / bytesPorEntrada;

            long inicio = 0;
            long fin = totalEntradas - 1;

            while (inicio <= fin)
            {
                var medio = inicio + (fin - inicio) / 2;

                flujo.Seek(medio * bytesPorEntrada, SeekOrigin.Begin);

                var claveLeida = TextoFijoBinario.Leer(lector, caracteresPorClave);
                var posicion = lector.ReadInt64();

                var comparacion = string.Compare(claveLeida, claveBuscada, StringComparison.Ordinal);

                if (comparacion == 0)
                {
                    return posicion;
                }
                else if (comparacion < 0)
                {
                    inicio = medio + 1;
                }
                else
                {
                    fin = medio - 1;
                }
            }
        }

        return -1;
    }
}
