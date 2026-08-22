-- Medición de instalaciones PWA (Cuidamed / Cuidanet)
-- Tabla: dbo.PwaInstalacion
--
-- Evento 'install'          = instalación nativa (Chrome/Edge Android/PC)
-- Evento 'standalone_open'  = apertura como app (útil para estimar iOS / ya instalada)
-- ClientInstallId           = dispositivo (1 por navegador/app)

USE CuidaNet;
GO

/* ========== 1) Resumen general ========== */
SELECT
    COUNT(*) AS TotalEventos,
    SUM(CASE WHEN Evento = N'install' THEN 1 ELSE 0 END) AS TotalInstalaciones,
    SUM(CASE WHEN Evento = N'standalone_open' THEN 1 ELSE 0 END) AS TotalAperturasApp,
    COUNT(DISTINCT ClientInstallId) AS DispositivosUnicos,
    COUNT(DISTINCT CASE WHEN Evento = N'install' THEN ClientInstallId END) AS DispositivosQueInstalaron,
    COUNT(DISTINCT NULLIF(Cedula, N'')) AS CedulasDistintas
FROM dbo.PwaInstalacion;
GO

/* ========== 2) Instalaciones por día (métrica principal) ========== */
SELECT
    CAST(FechaUtc AS date) AS DiaUtc,
    COUNT(*) AS Instalaciones,
    COUNT(DISTINCT ClientInstallId) AS Dispositivos
FROM dbo.PwaInstalacion
WHERE Evento = N'install'
GROUP BY CAST(FechaUtc AS date)
ORDER BY DiaUtc DESC;
GO

/* ========== 3) Por plataforma ========== */
SELECT
    Plataforma,
    SUM(CASE WHEN Evento = N'install' THEN 1 ELSE 0 END) AS Instalaciones,
    SUM(CASE WHEN Evento = N'standalone_open' THEN 1 ELSE 0 END) AS AperturasApp,
    COUNT(DISTINCT ClientInstallId) AS DispositivosUnicos
FROM dbo.PwaInstalacion
GROUP BY Plataforma
ORDER BY Instalaciones DESC, AperturasApp DESC;
GO

/* ========== 4) Últimos 30 días: install + standalone por día ========== */
SELECT
    CAST(FechaUtc AS date) AS DiaUtc,
    Evento,
    COUNT(*) AS Cantidad
FROM dbo.PwaInstalacion
WHERE FechaUtc >= DATEADD(day, -30, SYSUTCDATETIME())
GROUP BY CAST(FechaUtc AS date), Evento
ORDER BY DiaUtc DESC, Evento;
GO

/* ========== 5) Detalle reciente (auditoría) ========== */
SELECT TOP (100)
    PwaInstalacionId,
    FechaUtc,
    Evento,
    Plataforma,
    ClientInstallId,
    Cedula,
    Origen
FROM dbo.PwaInstalacion
ORDER BY FechaUtc DESC;
GO

/* ========== 6) Rango personalizado (ajusta las fechas) ========== */
/*
DECLARE @Desde date = '2026-08-01';
DECLARE @Hasta date = '2026-09-01'; -- exclusivo

SELECT
    COUNT(DISTINCT CASE WHEN Evento = N'install' THEN ClientInstallId END) AS DispositivosQueInstalaron,
    SUM(CASE WHEN Evento = N'install' THEN 1 ELSE 0 END) AS TotalInstalaciones,
    SUM(CASE WHEN Evento = N'standalone_open' THEN 1 ELSE 0 END) AS TotalAperturasApp
FROM dbo.PwaInstalacion
WHERE FechaUtc >= @Desde
  AND FechaUtc <  @Hasta;
*/
