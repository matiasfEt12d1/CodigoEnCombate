USE codigo_en_combate;

DROP PROCEDURE IF EXISTS sp_ResumenBatallasPorPeriodoYTipo;
DROP PROCEDURE IF EXISTS sp_VictoriasYDerrotasPorTipoYPeriodo;
DROP PROCEDURE IF EXISTS sp_RankingPersonajesEfectividad;
DROP PROCEDURE IF EXISTS sp_DanoPromedioPorClaseYHabilidad;
DROP PROCEDURE IF EXISTS sp_DanoTotalRealizadoYRecibidoPorPeriodo;

DELIMITER //

-- Resumen de batallas por período y tipo de personaje
CREATE PROCEDURE sp_ResumenBatallasPorPeriodoYTipo (
    IN p_fecha_desde DATETIME,
    IN p_fecha_hasta DATETIME
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
    END;

    START TRANSACTION;

    SELECT 
        P.tipo AS TipoPersonaje,
        COUNT(DISTINCT B.id) AS TotalBatallas,
        SUM(CASE WHEN B.es_finalizada = TRUE THEN 1 ELSE 0 END) AS BatallasFinalizadas,
        SUM(CASE WHEN B.es_finalizada = FALSE THEN 1 ELSE 0 END) AS BatallasEnProgreso
    FROM Personajes P
    INNER JOIN Batallas B ON (P.id = B.combatiente1_id OR P.id = B.combatiente2_id)
    WHERE B.fecha_inicio BETWEEN p_fecha_desde AND p_fecha_hasta
    GROUP BY P.tipo;

    COMMIT;
END //

-- Victorias y derrotas agrupadas por tipo de personaje y período
CREATE PROCEDURE sp_VictoriasYDerrotasPorTipoYPeriodo (
    IN p_fecha_desde DATETIME,
    IN p_fecha_hasta DATETIME
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
    END;

    START TRANSACTION;

    SELECT 
        P.tipo AS TipoPersonaje,
        COUNT(DISTINCT B.id) AS TotalBatallas,
        SUM(CASE WHEN B.ganador_id = P.id THEN 1 ELSE 0 END) AS Victorias,
        SUM(CASE WHEN B.ganador_id IS NOT NULL AND B.ganador_id != P.id THEN 1 ELSE 0 END) AS Derrotas,
        SUM(CASE WHEN B.ganador_id IS NULL THEN 1 ELSE 0 END) AS Empates
    FROM Personajes P
    INNER JOIN Batallas B ON (P.id = B.combatiente1_id OR P.id = B.combatiente2_id)
    WHERE B.es_finalizada = TRUE
      AND B.fecha_inicio BETWEEN p_fecha_desde AND p_fecha_hasta
    GROUP BY P.tipo;

    COMMIT;
END //

-- Ranking de personajes según victorias y porcentaje de efectividad
CREATE PROCEDURE sp_RankingPersonajesEfectividad ()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
    END;

    START TRANSACTION;

    SELECT 
        P.id AS PersonajeId,
        P.nombre AS Nombre,
        P.tipo AS Tipo,
        COUNT(DISTINCT B.id) AS TotalBatallas,
        SUM(CASE WHEN B.ganador_id = P.id THEN 1 ELSE 0 END) AS Victorias,
        SUM(CASE WHEN B.ganador_id IS NOT NULL AND B.ganador_id != P.id THEN 1 ELSE 0 END) AS Derrotas,
        ROUND(
            IF(COUNT(DISTINCT B.id) > 0, 
               (SUM(CASE WHEN B.ganador_id = P.id THEN 1 ELSE 0 END) * 100.0) / COUNT(DISTINCT B.id), 
               0.00
            ), 2
        ) AS PorcentajeEfectividad
    FROM Personajes P
    LEFT JOIN Batallas B ON (P.id = B.combatiente1_id OR P.id = B.combatiente2_id) AND B.es_finalizada = TRUE
    GROUP BY P.id, P.nombre, P.tipo
    ORDER BY Victorias DESC, PorcentajeEfectividad DESC;

    COMMIT;
END //

-- Daño promedio por clase de personaje y tipo de habilidad
CREATE PROCEDURE sp_DanoPromedioPorClaseYHabilidad ()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
    END;

    START TRANSACTION;

    SELECT 
        P.tipo AS ClasePersonaje,
        IFNULL(H.habilidad_usada, 'Ataque Básico') AS Habilidad,
        COUNT(H.id) AS CantidadUsos,
        ROUND(AVG(H.dano_causado), 2) AS DanoPromedio,
        MAX(H.dano_causado) AS DanoMaximo
    FROM HistorialBatallas H
    INNER JOIN Personajes P ON H.atacante_id = P.id
    GROUP BY P.tipo, IFNULL(H.habilidad_usada, 'Ataque Básico')
    ORDER BY P.tipo, DanoPromedio DESC;

    COMMIT;
END //

-- Daño total realizado y recibido por personaje en un período
CREATE PROCEDURE sp_DanoTotalRealizadoYRecibidoPorPeriodo (
    IN p_fecha_desde DATETIME,
    IN p_fecha_hasta DATETIME
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
    END;

    START TRANSACTION;

    SELECT 
        P.id AS PersonajeId,
        P.nombre AS NombrePersonaje,
        P.tipo AS TipoPersonaje,
        IFNULL(SUM(CASE WHEN H.atacante_id = P.id THEN H.dano_causado ELSE 0 END), 0) AS DanoTotalRealizado,
        IFNULL(SUM(CASE WHEN H.defensor_id = P.id THEN H.dano_causado ELSE 0 END), 0) AS DanoTotalRecibido
    FROM Personajes P
    LEFT JOIN HistorialBatallas H ON (P.id = H.atacante_id OR P.id = H.defensor_id)
                                 AND H.fecha_registro BETWEEN p_fecha_desde AND p_fecha_hasta
    GROUP BY P.id, P.nombre, P.tipo
    ORDER BY DanoTotalRealizado DESC;

    COMMIT;
END //

DELIMITER ;