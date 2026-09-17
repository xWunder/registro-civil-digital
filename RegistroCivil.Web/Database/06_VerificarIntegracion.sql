-- Diagnóstico de solo lectura. No crea, modifica ni elimina registros.
USE RegistroCivilDigital;
GO
SET NOCOUNT ON;

SELECT DB_NAME() AS BaseActual, @@VERSION AS VersionSQL;
SELECT COUNT_BIG(*) AS TotalRegistros,
       SUM(CASE WHEN Estado = 1 THEN CONVERT(BIGINT,1) ELSE 0 END) AS Activas,
       SUM(CASE WHEN Estado = 0 THEN CONVERT(BIGINT,1) ELSE 0 END) AS Anuladas,
       SUM(CASE WHEN Nombres LIKE N'Persona%' AND ApellidoPaterno LIKE N'ApellidoPaterno%'
                  AND ApellidoMaterno LIKE N'ApellidoMaterno%' THEN CONVERT(BIGINT,1) ELSE 0 END) AS NombresGenericos
FROM dbo.ActasNacimiento;

SELECT NumeroActa, COUNT_BIG(*) AS Repeticiones
FROM dbo.ActasNacimiento GROUP BY NumeroActa HAVING COUNT_BIG(*) > 1;
SELECT DniInscrito, COUNT_BIG(*) AS Repeticiones
FROM dbo.ActasNacimiento WHERE DniInscrito IS NOT NULL
GROUP BY DniInscrito HAVING COUNT_BIG(*) > 1;

SELECT name AS Indice, is_unique AS EsUnico, is_disabled AS Deshabilitado
FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ActasNacimiento') AND index_id > 0;

SELECT COUNT_BIG(*) AS CoincidenciasPrueba
FROM dbo.ActasNacimiento WHERE NumeroActa = 'ACT-2026-199999' AND DniInscrito = '90199999';
GO
