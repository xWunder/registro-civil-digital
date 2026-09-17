using System.Text;

namespace RegistroCivil.Web.FileStorage;

public static class CabeceraArchivo
{
    private const int tamañorelleno =
        FormatoArchivoActas.TamanoCabecera - 8 - 4 - 8;

    public static void Escribir(BinaryWriter escritor, long cantidadRegistros)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidadRegistros);
        var bytesFirma = Encoding.UTF8.GetBytes(FormatoArchivoActas.Firma);
        escritor.Write(bytesFirma);

        escritor.Write(FormatoArchivoActas.Version);

        escritor.Write(cantidadRegistros);

        escritor.Write(new byte[tamañorelleno]);
    }

    public static long Leer(BinaryReader lector)
    {
        var bytesFirma = lector.ReadBytes(8);
        var firma = Encoding.UTF8.GetString(bytesFirma);

        if(firma != FormatoArchivoActas.Firma)
        {
            throw new InvalidDataException("El archivo no tiene la firma esperada. ¿Es un Archivo de acta valido?");
        }

        var version = lector.ReadInt32();

        if(version !=FormatoArchivoActas.Version)
        {
            throw new InvalidDataException($"Versión de archivo no soportada: {version}.");
        }

        var cantidadRegistros = lector.ReadInt64();

        if (cantidadRegistros < 0 || lector.ReadBytes(tamañorelleno).Length != tamañorelleno)
            throw new InvalidDataException("Cabecera incompleta o cantidad inválida.");

        if (lector.BaseStream.Length != FormatoArchivoActas.CalcularOffset(cantidadRegistros))
            throw new InvalidDataException("El tamaño del archivo no coincide con su cabecera.");

        return cantidadRegistros;
    }
}
