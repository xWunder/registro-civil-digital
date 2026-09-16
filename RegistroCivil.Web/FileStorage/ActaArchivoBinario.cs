using RegistroCivil.Web.Models;

namespace RegistroCivil.Web.FileStorage;

public static class ActaArchivoBinario
{
    private const int TamanoRelleno = 14;

    public static void Escribir(BinaryWriter escritor, ActaNacimiento acta)
    {
        escritor.Write(acta.Id);

        TextoFijoBinario.Escribir(
            escritor, acta.NumeroActa, FormatoArchivoActas.CaracteresNumeroActa);

        TextoFijoBinario.Escribir(
            escritor, acta.DniInscrito, FormatoArchivoActas.CaracteresDni);

        TextoFijoBinario.Escribir(
            escritor, acta.ApellidoPaterno, FormatoArchivoActas.CaracteresApellido);

        TextoFijoBinario.Escribir(
            escritor, acta.ApellidoMaterno, FormatoArchivoActas.CaracteresApellido);

        TextoFijoBinario.Escribir(
            escritor, acta.Nombres, FormatoArchivoActas.CaracteresNombres);

        escritor.Write(acta.FechaNacimiento.DayNumber);

        var sexoByte = (byte)(acta.Sexo == "F" ? 'F' : 'M');
        escritor.Write(sexoByte);

        TextoFijoBinario.Escribir(
            escritor, acta.UbigeoNacimiento, FormatoArchivoActas.CaracteresUbigeo);

        TextoFijoBinario.Escribir(
            escritor, acta.LugarNacimiento, FormatoArchivoActas.CaracteresLugar);

        escritor.Write(acta.FechaRegistro.Ticks);
        escritor.Write(acta.FechaModificacion.Ticks);
        escritor.Write(acta.Estado);

        escritor.Write(new byte[TamanoRelleno]);
    }

    public static ActaNacimiento Leer(BinaryReader lector)
    {
        var acta = new ActaNacimiento
        {
            Id = lector.ReadInt64(),
            NumeroActa = TextoFijoBinario.Leer(
                lector, FormatoArchivoActas.CaracteresNumeroActa),
            DniInscrito = TextoFijoBinario.Leer(
                lector, FormatoArchivoActas.CaracteresDni),
            ApellidoPaterno = TextoFijoBinario.Leer(
                lector, FormatoArchivoActas.CaracteresApellido),
            ApellidoMaterno = TextoFijoBinario.Leer(
                lector, FormatoArchivoActas.CaracteresApellido),
            Nombres = TextoFijoBinario.Leer(
                lector, FormatoArchivoActas.CaracteresNombres),
        };

        var dayNumber = lector.ReadInt32();
        acta.FechaNacimiento = DateOnly.FromDayNumber(dayNumber);

        var sexoByte = lector.ReadByte();
        acta.Sexo = ((char)sexoByte).ToString();

        acta.UbigeoNacimiento = TextoFijoBinario.Leer(
            lector, FormatoArchivoActas.CaracteresUbigeo);

        acta.LugarNacimiento = TextoFijoBinario.Leer(
            lector, FormatoArchivoActas.CaracteresLugar);

        acta.FechaRegistro = new DateTime(lector.ReadInt64());
        acta.FechaModificacion = new DateTime(lector.ReadInt64());
        acta.Estado = lector.ReadByte();

        lector.ReadBytes(TamanoRelleno);

        return acta;
    }

     
    public static List<ActaNacimiento> ListarTodas(string rutaActas)
    {
        var actas = new List<ActaNacimiento>();

        using (var lector = new BinaryReader(File.OpenRead(rutaActas)))
        {
            var cantidadRegistros = CabeceraArchivo.Leer(lector);

            for (long i = 0; i < cantidadRegistros; i++)
            {
                actas.Add(Leer(lector));
            }
        }

        return actas;
    }

    
    public static void Agregar(string rutaActas, ActaNacimiento acta)
    {
        long cantidadActual;

        using (var lector = new BinaryReader(File.OpenRead(rutaActas)))
        {
            cantidadActual = CabeceraArchivo.Leer(lector);
        }

        using (var flujo = new FileStream(rutaActas, FileMode.Open, FileAccess.Write))
        using (var escritor = new BinaryWriter(flujo))
        {
            var offsetNuevoRegistro = FormatoArchivoActas.CalcularOffset(cantidadActual);
            flujo.Seek(offsetNuevoRegistro, SeekOrigin.Begin);

            Escribir(escritor, acta);

            flujo.Seek(0, SeekOrigin.Begin);
            CabeceraArchivo.Escribir(escritor, cantidadActual + 1);
        }
    }

    
    public static void ModificarPorPosicion(string rutaActas, long posicion, ActaNacimiento actaActualizada)
    {
        using (var flujo = new FileStream(rutaActas, FileMode.Open, FileAccess.Write))
        using (var escritor = new BinaryWriter(flujo))
        {
            var offset = FormatoArchivoActas.CalcularOffset(posicion);
            flujo.Seek(offset, SeekOrigin.Begin);

            Escribir(escritor, actaActualizada);
        }
    }
}