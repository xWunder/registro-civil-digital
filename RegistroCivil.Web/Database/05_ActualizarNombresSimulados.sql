-- Ejecutar en SSMS una vez después de actualizar el repositorio.
-- Solo transforma el patrón exacto del generador antiguo. No altera actas manuales.
-- Datos ficticios: cualquier coincidencia con personas reales es casual.
USE RegistroCivilDigital;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
UPDATE A
SET ApellidoPaterno = CHOOSE(CONVERT(INT, ((N - 100000) * 7 + (N - 100000) / 20) % 20 + 1), N'Aguilar', N'Benavides', N'Cabrera', N'Delgado', N'Espinoza', N'Flores', N'Guzmán', N'Herrera', N'Ibarra', N'Jiménez', N'Lozano', N'Mendoza', N'Navarro', N'Ortiz', N'Paredes', N'Quispe', N'Ramírez', N'Salazar', N'Torres', N'Valverde'),
    ApellidoMaterno = CHOOSE(CONVERT(INT, ((N - 100000) * 11 + (N - 100000) / 400 + 3) % 20 + 1), N'Aguilar', N'Benavides', N'Cabrera', N'Delgado', N'Espinoza', N'Flores', N'Guzmán', N'Herrera', N'Ibarra', N'Jiménez', N'Lozano', N'Mendoza', N'Navarro', N'Ortiz', N'Paredes', N'Quispe', N'Ramírez', N'Salazar', N'Torres', N'Valverde'),
    Nombres = CASE WHEN N % 2 = 0 THEN
        CHOOSE(CONVERT(INT, ((N - 100000) / 2) % 10 + 1), N'Ana', N'Lucía', N'Camila', N'Valentina', N'Mariana', N'Elena', N'Daniela', N'Paula', N'Renata', N'Sofía')
    ELSE
        CHOOSE(CONVERT(INT, ((N - 100000) / 2) % 10 + 1), N'Luis', N'Mateo', N'Diego', N'Gabriel', N'Joaquín', N'Andrés', N'Bruno', N'Nicolás', N'Adrián', N'Santiago')
    END,
    FechaModificacion = SYSDATETIME()
FROM dbo.ActasNacimiento A
CROSS APPLY (SELECT TRY_CONVERT(BIGINT, SUBSTRING(NumeroActa, 10, 20)) AS N) S
WHERE NumeroActa LIKE 'ACT-2026-%'
  AND N BETWEEN 100000 AND 199999
  AND Nombres = CONCAT('Persona', N)
  AND ApellidoPaterno = CONCAT('ApellidoPaterno', N)
  AND ApellidoMaterno = CONCAT('ApellidoMaterno', N);
DECLARE @Filas INT = @@ROWCOUNT;
COMMIT;
SELECT @Filas AS FilasActualizadas;
GO
