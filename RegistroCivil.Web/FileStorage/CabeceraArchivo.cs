using System.Text;

namespace RegistroCivil.Web.FileStorage;

public static class CabeceraArchivo
{
    private const int tamañorelleno = 
        FormatoArchivoActas.TamanoCabecera - 8 - 4 - 8;

    public static void Escribir(BinaryWriter escritor, long cantidadRegistros)
    {
        var bytesFirma = Encoding.UTF8.GetBytes(FormatoArchivoActas.Firma);
        escritor.Write(bytesFirma);

        escritor.Write(FormatoArchivoActas.Version);

        escritor.Write(cantidadRegistros);

        escritor.Write(new byte[tamañorelleno]);
    }

    public static long leer(BinaryReader lector)
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

        lector.ReadBytes(tamañorelleno);

        return cantidadRegistros;
    }
}
