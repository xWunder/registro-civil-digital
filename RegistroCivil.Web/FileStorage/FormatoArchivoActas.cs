namespace RegistroCivil.Web.FileStorage;

public static class FormatoArchivoActas
{
    public const string Firma = "RCDIG001";
    public const int Version = 1;

    public const int TamanoCabecera = 64;
    public const int TamanoRegistro = 512;

    public const int CaracteresNumeroActa = 20;
    public const int CaracteresDni = 8;
    public const int CaracteresApellido = 40;
    public const int CaracteresNombres = 60;
    public const int CaracteresUbigeo = 6;
    public const int CaracteresLugar = 60;

    public const int TamanoEntradaIndiceNumero = 48;
    public const int TamanoEntradaIndiceDni = 24;

    public static long CalcularOffset(long posicion)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(posicion);
        return checked(TamanoCabecera + posicion * TamanoRegistro);
    }
}
