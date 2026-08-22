-- Histórico de instalaciones PWA (Cuidamed / Cuidanet).
-- Ejecutar en la misma base de DefaultConnection de APILISPoblacion.

IF OBJECT_ID(N'dbo.PwaInstalacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PwaInstalacion
    (
        PwaInstalacionId BIGINT IDENTITY(1, 1) NOT NULL
            CONSTRAINT PK_PwaInstalacion PRIMARY KEY,
        FechaUtc DATETIME2(3) NOT NULL
            CONSTRAINT DF_PwaInstalacion_FechaUtc DEFAULT (SYSUTCDATETIME()),
        Evento NVARCHAR(32) NOT NULL,
        Plataforma NVARCHAR(32) NOT NULL,
        ClientInstallId NVARCHAR(64) NOT NULL,
        Cedula NVARCHAR(32) NULL,
        UserAgent NVARCHAR(512) NULL,
        Origen NVARCHAR(64) NULL
    );

    CREATE UNIQUE INDEX UX_PwaInstalacion_Client_Evento
        ON dbo.PwaInstalacion (ClientInstallId, Evento);

    CREATE INDEX IX_PwaInstalacion_FechaUtc
        ON dbo.PwaInstalacion (FechaUtc);

    CREATE INDEX IX_PwaInstalacion_Plataforma_Fecha
        ON dbo.PwaInstalacion (Plataforma, FechaUtc);
END
GO
