using System.Data;
using Microsoft.Data.SqlClient;
using RegistroCivil.Web.Models;

namespace RegistroCivil.Web.Services;

public class ActaNacimientoService
{
    private readonly string _connectionString;

    public ActaNacimientoService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("RegistroCivil")
            ?? throw new InvalidOperationException(
                "No se encontró la conexión RegistroCivil.");
    }

    public async Task<(List<ActaNacimiento> Actas, long TotalRegistros)>
        ListarPaginadoAsync(int pagina, int tamanoPagina)
    {
        if (pagina < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pagina), "La página debe ser mayor que cero.");
        }

        if (tamanoPagina is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tamanoPagina),
                "El tamaño de página debe estar entre 1 y 500.");
        }

        const string sql = """
            SELECT COUNT_BIG(*)
            FROM dbo.ActasNacimiento;

            SELECT
                Id, NumeroActa, DniInscrito, ApellidoPaterno,
                ApellidoMaterno, Nombres, FechaNacimiento, Sexo,
                UbigeoNacimiento, LugarNacimiento, FechaRegistro,
                FechaModificacion, Estado
            FROM dbo.ActasNacimiento
            ORDER BY Id DESC
            OFFSET @Offset ROWS
            FETCH NEXT @TamanoPagina ROWS ONLY;
            """;

        var actas = new List<ActaNacimiento>();
        var offset = checked((pagina - 1) * tamanoPagina);

        await using var conexion = new SqlConnection(_connectionString);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
        comando.Parameters.Add("@TamanoPagina", SqlDbType.Int)
            .Value = tamanoPagina;

        await conexion.OpenAsync();
        await using var lector = await comando.ExecuteReaderAsync();

        await lector.ReadAsync();
        var totalRegistros = lector.GetInt64(0);

        await lector.NextResultAsync();

        while (await lector.ReadAsync())
        {
            actas.Add(LeerActa(lector));
        }

        return (actas, totalRegistros);
    }

    public async Task<ActaNacimiento?> BuscarPorNumeroAsync(string numeroActa)
    {
        const string sql = """
            SELECT
                Id, NumeroActa, DniInscrito, ApellidoPaterno,
                ApellidoMaterno, Nombres, FechaNacimiento, Sexo,
                UbigeoNacimiento, LugarNacimiento, FechaRegistro,
                FechaModificacion, Estado
            FROM dbo.ActasNacimiento
            WHERE NumeroActa = @NumeroActa;
            """;

        await using var conexion = new SqlConnection(_connectionString);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@NumeroActa", SqlDbType.VarChar, 20)
            .Value = numeroActa.Trim();

        await conexion.OpenAsync();
        await using var lector = await comando.ExecuteReaderAsync();

        return await lector.ReadAsync() ? LeerActa(lector) : null;
    }

    public async Task<List<ActaNacimiento>> BuscarPorDniAsync(string dni)
    {
        const string sql = """
            SELECT
                Id, NumeroActa, DniInscrito, ApellidoPaterno,
                ApellidoMaterno, Nombres, FechaNacimiento, Sexo,
                UbigeoNacimiento, LugarNacimiento, FechaRegistro,
                FechaModificacion, Estado
            FROM dbo.ActasNacimiento
            WHERE DniInscrito = @Dni
            ORDER BY FechaRegistro DESC;
            """;

        var actas = new List<ActaNacimiento>();

        await using var conexion = new SqlConnection(_connectionString);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Dni", SqlDbType.Char, 8).Value = dni.Trim();

        await conexion.OpenAsync();
        await using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            actas.Add(LeerActa(lector));
        }

        return actas;
    }

    public async Task<long> RegistrarAsync(ActaNacimiento acta)
    {
        const string sql = """
            INSERT INTO dbo.ActasNacimiento
            (
                NumeroActa, DniInscrito, ApellidoPaterno,
                ApellidoMaterno, Nombres, FechaNacimiento, Sexo,
                UbigeoNacimiento, LugarNacimiento
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @NumeroActa, @DniInscrito, @ApellidoPaterno,
                @ApellidoMaterno, @Nombres, @FechaNacimiento, @Sexo,
                @UbigeoNacimiento, @LugarNacimiento
            );
            """;

        await using var conexion = new SqlConnection(_connectionString);
        await using var comando = new SqlCommand(sql, conexion);

        AgregarParametros(comando, acta);

        await conexion.OpenAsync();
        var resultado = await comando.ExecuteScalarAsync();

        return Convert.ToInt64(resultado);
    }

    public async Task<bool> ModificarAsync(ActaNacimiento acta)
    {
        const string sql = """
            UPDATE dbo.ActasNacimiento
            SET
                DniInscrito = @DniInscrito,
                ApellidoPaterno = @ApellidoPaterno,
                ApellidoMaterno = @ApellidoMaterno,
                Nombres = @Nombres,
                FechaNacimiento = @FechaNacimiento,
                Sexo = @Sexo,
                UbigeoNacimiento = @UbigeoNacimiento,
                LugarNacimiento = @LugarNacimiento,
                FechaModificacion = SYSDATETIME()
            WHERE Id = @Id AND Estado = 1;
            """;

        await using var conexion = new SqlConnection(_connectionString);
        await using var comando = new SqlCommand(sql, conexion);

        AgregarParametros(comando, acta);
        comando.Parameters.Add("@Id", SqlDbType.BigInt).Value = acta.Id;

        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> AnularAsync(long id)
    {
        const string sql = """
            UPDATE dbo.ActasNacimiento
            SET Estado = 0,
                FechaModificacion = SYSDATETIME()
            WHERE Id = @Id AND Estado = 1;
            """;

        await using var conexion = new SqlConnection(_connectionString);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;

        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() == 1;
    }

    private static void AgregarParametros(
        SqlCommand comando,
        ActaNacimiento acta)
    {
        comando.Parameters.Add("@NumeroActa", SqlDbType.VarChar, 20)
            .Value = acta.NumeroActa.Trim();

        comando.Parameters.Add("@DniInscrito", SqlDbType.Char, 8)
            .Value = string.IsNullOrWhiteSpace(acta.DniInscrito)
                ? DBNull.Value
                : acta.DniInscrito.Trim();

        comando.Parameters.Add("@ApellidoPaterno", SqlDbType.NVarChar, 40)
            .Value = acta.ApellidoPaterno.Trim();

        comando.Parameters.Add("@ApellidoMaterno", SqlDbType.NVarChar, 40)
            .Value = string.IsNullOrWhiteSpace(acta.ApellidoMaterno)
                ? DBNull.Value
                : acta.ApellidoMaterno.Trim();

        comando.Parameters.Add("@Nombres", SqlDbType.NVarChar, 60)
            .Value = acta.Nombres.Trim();

        comando.Parameters.Add("@FechaNacimiento", SqlDbType.Date)
            .Value = acta.FechaNacimiento.ToDateTime(TimeOnly.MinValue);

        comando.Parameters.Add("@Sexo", SqlDbType.Char, 1)
            .Value = acta.Sexo.Trim().ToUpperInvariant();

        comando.Parameters.Add("@UbigeoNacimiento", SqlDbType.Char, 6)
            .Value = acta.UbigeoNacimiento.Trim();

        comando.Parameters.Add("@LugarNacimiento", SqlDbType.NVarChar, 60)
            .Value = acta.LugarNacimiento.Trim();
    }

    private static ActaNacimiento LeerActa(SqlDataReader lector)
    {
        return new ActaNacimiento
        {
            Id = lector.GetInt64(lector.GetOrdinal("Id")),
            NumeroActa = lector.GetString(lector.GetOrdinal("NumeroActa")),
            DniInscrito = lector.IsDBNull(lector.GetOrdinal("DniInscrito"))
                ? null
                : lector.GetString(lector.GetOrdinal("DniInscrito")).Trim(),
            ApellidoPaterno =
                lector.GetString(lector.GetOrdinal("ApellidoPaterno")),
            ApellidoMaterno =
                lector.IsDBNull(lector.GetOrdinal("ApellidoMaterno"))
                    ? null
                    : lector.GetString(
                        lector.GetOrdinal("ApellidoMaterno")),
            Nombres = lector.GetString(lector.GetOrdinal("Nombres")),
            FechaNacimiento = DateOnly.FromDateTime(
                lector.GetDateTime(lector.GetOrdinal("FechaNacimiento"))),
            Sexo = lector.GetString(lector.GetOrdinal("Sexo")).Trim(),
            UbigeoNacimiento =
                lector.GetString(
                    lector.GetOrdinal("UbigeoNacimiento")).Trim(),
            LugarNacimiento =
                lector.GetString(lector.GetOrdinal("LugarNacimiento")),
            FechaRegistro =
                lector.GetDateTime(lector.GetOrdinal("FechaRegistro")),
            FechaModificacion =
                lector.GetDateTime(lector.GetOrdinal("FechaModificacion")),
            Estado = lector.GetByte(lector.GetOrdinal("Estado"))
        };
    }
}
