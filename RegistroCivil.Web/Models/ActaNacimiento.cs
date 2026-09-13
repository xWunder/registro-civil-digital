using System.ComponentModel.DataAnnotations;

namespace RegistroCivil.Web.Models;

public class ActaNacimiento
{
    public long Id { get; set; }

    [Required(ErrorMessage = "El número de acta es obligatorio.")]
    [StringLength(20)]
    public string NumeroActa { get; set; } = string.Empty;

    [RegularExpression(@"^\d{8}$",
        ErrorMessage = "El DNI debe contener exactamente 8 dígitos.")]
    public string? DniInscrito { get; set; }

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(40)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(40)]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(60)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateOnly FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El sexo es obligatorio.")]
    [RegularExpression("^[MF]$",
        ErrorMessage = "El sexo debe ser M o F.")]
    public string Sexo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ubigeo es obligatorio.")]
    [RegularExpression(@"^\d{6}$",
        ErrorMessage = "El ubigeo debe contener exactamente 6 dígitos.")]
    public string UbigeoNacimiento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El lugar de nacimiento es obligatorio.")]
    [StringLength(60)]
    public string LugarNacimiento { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public DateTime FechaModificacion { get; set; } = DateTime.Now;

    public byte Estado { get; set; } = 1;
}