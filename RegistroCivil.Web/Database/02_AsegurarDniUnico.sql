USE RegistroCivilDigital;
GO

IF EXISTS
(
    SELECT DniInscrito
    FROM dbo.ActasNacimiento
    WHERE DniInscrito IS NOT NULL
    GROUP BY DniInscrito
    HAVING COUNT(*) > 1
)
BEGIN
    THROW 51000,
        'Existen DNI duplicados. Corríjalos antes de crear el índice único.',
        1;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ActasNacimiento_DniInscrito'
      AND object_id = OBJECT_ID('dbo.ActasNacimiento')
      AND is_unique = 0
)
BEGIN
    DROP INDEX IX_ActasNacimiento_DniInscrito
        ON dbo.ActasNacimiento;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ActasNacimiento_DniInscrito'
      AND object_id = OBJECT_ID('dbo.ActasNacimiento')
      AND is_unique = 1
)
BEGIN
    CREATE UNIQUE INDEX IX_ActasNacimiento_DniInscrito
        ON dbo.ActasNacimiento (DniInscrito)
        WHERE DniInscrito IS NOT NULL;
END;
GO

SELECT
    name AS NombreIndice,
    is_unique AS EsUnico
FROM sys.indexes
WHERE name = 'IX_ActasNacimiento_DniInscrito'
  AND object_id = OBJECT_ID('dbo.ActasNacimiento');
GO
