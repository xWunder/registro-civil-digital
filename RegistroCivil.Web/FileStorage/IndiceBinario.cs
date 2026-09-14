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

                entradas.Add(entrada);
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
}