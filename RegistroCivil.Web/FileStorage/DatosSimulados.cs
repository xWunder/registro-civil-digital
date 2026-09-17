using RegistroCivil.Web.Models;

namespace RegistroCivil.Web.FileStorage;

public static class DatosSimulados
{
    public static IEnumerable<ActaNacimiento> Crear(int cantidad = 100000)
    {
        if (cantidad is < 1 or > 5000000) throw new ArgumentOutOfRangeException(nameof(cantidad));
        string[] mujeres = ["Ana", "Lucía", "Camila", "Valentina", "Mariana", "Elena", "Daniela", "Paula", "Renata", "Sofía"];
        string[] hombres = ["Luis", "Mateo", "Diego", "Gabriel", "Joaquín", "Andrés", "Bruno", "Nicolás", "Adrián", "Santiago"];
        string[] apellidos = ["Aguilar", "Benavides", "Cabrera", "Delgado", "Espinoza", "Flores", "Guzmán", "Herrera", "Ibarra", "Jiménez", "Lozano", "Mendoza", "Navarro", "Ortiz", "Paredes", "Quispe", "Ramírez", "Salazar", "Torres", "Valverde"];
        var fecha = DateTime.Now;
        for (int i = 0; i < cantidad; i++)
        {
            var n = i + 100000;
            var femenino = i % 2 == 0;
            yield return new ActaNacimiento
            {
                Id = i + 1, NumeroActa = $"ACT-2026-{n:D6}", DniInscrito = (90000000 + n).ToString("D8"),
                Nombres = (femenino ? mujeres : hombres)[i / 2 % 10],
                ApellidoPaterno = apellidos[(i * 7 + i / 20) % 20],
                ApellidoMaterno = apellidos[(i * 11 + i / 400 + 3) % 20],
                FechaNacimiento = new DateOnly(1980, 1, 1).AddDays(n % 14000), Sexo = femenino ? "F" : "M",
                UbigeoNacimiento = "130101", LugarNacimiento = "Trujillo",
                FechaRegistro = fecha, FechaModificacion = fecha, Estado = 1
            };
        }
    }
}
