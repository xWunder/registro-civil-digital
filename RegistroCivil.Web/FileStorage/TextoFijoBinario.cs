using System.Text;

namespace RegistroCivil.Web.FileStorage;

public static class TextoFijoBinario
{
    public static void Escribir(
        BinaryWriter escritor,
        string? valor,
        int cantidadCaracteres)
    {
        var texto = (valor ?? string.Empty).Trim();

        if (texto.Length > cantidadCaracteres)
        {
            texto = texto[..cantidadCaracteres];
        }

        texto = texto.PadRight(cantidadCaracteres, ' ');

        var bytes = Encoding.Unicode.GetBytes(texto);
        escritor.Write(bytes);
    }

    public static string Leer(
        BinaryReader lector,
        int cantidadCaracteres)
    {
        var cantidadBytes = cantidadCaracteres * 2;
        var bytes = lector.ReadBytes(cantidadBytes);

        if (bytes.Length != cantidadBytes)
        {
            throw new EndOfStreamException(
                "El registro binario está incompleto.");
        }

        return Encoding.Unicode
            .GetString(bytes)
            .TrimEnd();
    }
}