USE RegistroCivilDigital;
GO

DECLARE @Cantidad INT = 100000;
DECLARE @Inicio INT = 100000;

;WITH Numeros AS
(
    SELECT TOP (@Cantidad)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + @Inicio - 1 AS N
    FROM sys.all_objects A
    CROSS JOIN sys.all_objects B
)
INSERT INTO dbo.ActasNacimiento
(
    NumeroActa,
    DniInscrito,
    ApellidoPaterno,
    ApellidoMaterno,
    Nombres,
    FechaNacimiento,
    Sexo,
    UbigeoNacimiento,
    LugarNacimiento,
    FechaRegistro,
    FechaModificacion,
    Estado
)
SELECT
    CONCAT('ACT-2026-', RIGHT('000000' + CAST(N AS VARCHAR(6)), 6)),
    RIGHT('00000000' + CAST(90000000 + N AS VARCHAR(8)), 8),
    CONCAT('ApellidoPaterno', N),
    CONCAT('ApellidoMaterno', N),
    CONCAT('Persona', N),

    -- Fechas entre 1980 y 2018
    DATEADD(DAY, N % 14000, CAST('1980-01-01' AS DATE)),

    CASE WHEN N % 2 = 0 THEN 'F' ELSE 'M' END,
    '130101',
    'Trujillo',
    SYSDATETIME(),
    SYSDATETIME(),
    1
FROM Numeros
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.ActasNacimiento A
    WHERE A.NumeroActa = CONCAT(
        'ACT-2026-',
        RIGHT('000000' + CAST(N AS VARCHAR(6)), 6)
    )
)
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.ActasNacimiento A
    WHERE A.DniInscrito = RIGHT(
        '00000000' + CAST(90000000 + N AS VARCHAR(8)),
        8
    )
);
GO

SELECT COUNT(*) AS TotalRegistros
FROM dbo.ActasNacimiento;
GO