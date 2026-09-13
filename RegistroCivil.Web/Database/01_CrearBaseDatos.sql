USE master;
GO

IF DB_ID('RegistroCivilDigital') IS NULL
BEGIN
    CREATE DATABASE RegistroCivilDigital;
END;
GO

USE RegistroCivilDigital;
GO

IF OBJECT_ID('dbo.ActasNacimiento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ActasNacimiento
    (
        Id BIGINT IDENTITY(1,1) NOT NULL,
        NumeroActa VARCHAR(20) NOT NULL,
        DniInscrito CHAR(8) NULL,
        ApellidoPaterno NVARCHAR(40) NOT NULL,
        ApellidoMaterno NVARCHAR(40) NULL,
        Nombres NVARCHAR(60) NOT NULL,
        FechaNacimiento DATE NOT NULL,
        Sexo CHAR(1) NOT NULL,
        UbigeoNacimiento CHAR(6) NOT NULL,
        LugarNacimiento NVARCHAR(60) NOT NULL,
        FechaRegistro DATETIME2 NOT NULL
            CONSTRAINT DF_Actas_FechaRegistro DEFAULT SYSDATETIME(),
        FechaModificacion DATETIME2 NOT NULL
            CONSTRAINT DF_Actas_FechaModificacion DEFAULT SYSDATETIME(),
        Estado TINYINT NOT NULL
            CONSTRAINT DF_Actas_Estado DEFAULT 1,

        CONSTRAINT PK_ActasNacimiento
            PRIMARY KEY (Id),

        CONSTRAINT UQ_ActasNacimiento_NumeroActa
            UNIQUE (NumeroActa),

        CONSTRAINT CK_ActasNacimiento_Dni
            CHECK (
                DniInscrito IS NULL
                OR (
                    LEN(DniInscrito) = 8
                    AND DniInscrito NOT LIKE '%[^0-9]%'
                )
            ),

        CONSTRAINT CK_ActasNacimiento_Sexo
            CHECK (Sexo IN ('M', 'F')),

        CONSTRAINT CK_ActasNacimiento_Ubigeo
            CHECK (
                LEN(UbigeoNacimiento) = 6
                AND UbigeoNacimiento NOT LIKE '%[^0-9]%'
            ),

        CONSTRAINT CK_ActasNacimiento_Fecha
            CHECK (FechaNacimiento <= CONVERT(DATE, GETDATE())),

        CONSTRAINT CK_ActasNacimiento_Estado
            CHECK (Estado IN (0, 1))
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ActasNacimiento_DniInscrito'
      AND object_id = OBJECT_ID('dbo.ActasNacimiento')
)
BEGIN
    CREATE INDEX IX_ActasNacimiento_DniInscrito
        ON dbo.ActasNacimiento (DniInscrito)
        WHERE DniInscrito IS NOT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.ActasNacimiento
    WHERE NumeroActa = 'ACT-2026-000001'
)
BEGIN
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
        LugarNacimiento
    )
    VALUES
    (
        'ACT-2026-000001',
        '12345678',
        N'Pérez',
        N'García',
        N'Persona de Prueba',
        '2005-05-15',
        'M',
        '150101',
        N'Lima'
    );
END;
GO

SELECT * FROM dbo.ActasNacimiento;
GO