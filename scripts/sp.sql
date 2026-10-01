USE codigo_en_combate;

DROP PROCEDURE IF EXISTS sp_RegistrarBatalla;
DROP PROCEDURE IF EXISTS sp_FinalizarBatalla;
DROP PROCEDURE IF EXISTS sp_ProcesarAccionCombate;
DROP PROCEDURE IF EXISTS sp_ObtenerRankingPorTipoPersonaje;
DROP PROCEDURE IF EXISTS sp_GenerarReporteBatallasDetallado;

DELIMITER //

-- Registrar una batalla completa con sus participantes y estado inicial
CREATE PROCEDURE sp_RegistrarBatalla (
    IN p_combatiente1_id INT,
    IN p_combatiente2_id INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
    END;

    START TRANSACTION;

    INSERT INTO Batallas (combatiente1_id, combatiente2_id, numero_turno, es_finalizada, fecha_inicio)
    VALUES (p_combatiente1_id, p_combatiente2_id, 1, FALSE, NOW());

    SELECT LAST_INSERT_ID() AS BatallaId;

    COMMIT;
END //

-- Cerrar una batalla actualizando resultado, estadísticas e historial
CREATE PROCEDURE sp_FinalizarBatalla (
    IN p_batalla_id INT,
    IN p_ganador_id INT,
    IN p_numero_turno INT,
    IN p_vida_c1 INT,
    IN p_vida_c2 INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
    END;

    START TRANSACTION;

    -- Actualizar estado de la batalla
    UPDATE Batallas
    SET ganador_id = p_ganador_id,
        numero_turno = p_numero_turno,
        es_finalizada = TRUE
    WHERE id = p_batalla_id;

    -- Actualizar la vida final de ambos combatientes
    UPDATE Personajes P
    INNER JOIN Batallas B ON B.id = p_batalla_id
    SET P.vida = CASE 
        WHEN P.id = B.combatiente1_id THEN p_vida_c1
        WHEN P.id = B.combatiente2_id THEN p_vida_c2
        ELSE P.vida
    END
    WHERE P.id IN (B.combatiente1_id, B.combatiente2_id);

    COMMIT;
END //

-- Procesar operaciones transaccionales que afecten varias entidades relacionadas
CREATE PROCEDURE sp_ProcesarAccionCombate (
    IN p_batalla_id INT,
    IN p_atacante_id INT,
    IN p_defensor_id INT,
    IN p_dano_realizado INT,
    IN p_nueva_vida_defensor INT,
    IN p_habilidad_usada VARCHAR(100),
    IN p_numero_turno INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
    END;

    START TRANSACTION;

    -- Insertar el evento en el historial de combate
    INSERT INTO HistorialBatallas (batalla_id, numero_turno, atacante_id, defensor_id, habilidad_usada, dano_causado, fecha_registro)
    VALUES (p_batalla_id, p_numero_turno, p_atacante_id, p_defensor_id, p_habilidad_usada, p_dano_realizado, NOW());

    -- Actualizar estado del defensor
    UPDATE Personajes
    SET vida = p_nueva_vida_defensor
    WHERE id = p_defensor_id;

    -- Actualizar el turno actual en la batalla
    UPDATE Batallas
    SET numero_turno = p_numero_turno
    WHERE id = p_batalla_id;

    COMMIT;
END //

-- Obtener rankings o estadísticas agregadas por período y tipo de personaje
CREATE PROCEDURE sp_ObtenerRankingPorTipoPersonaje (
    IN p_fecha_desde DATETIME,
    IN p_fecha_hasta DATETIME
)
BEGIN
    SELECT 
        P.tipo AS TipoPersonaje,
        COUNT(DISTINCT B.id) AS TotalBatallas,
        SUM(CASE WHEN B.ganador_id = P.id THEN 1 ELSE 0 END) AS TotalVictorias,
        SUM(CASE WHEN B.ganador_id IS NOT NULL AND B.ganador_id != P.id THEN 1 ELSE 0 END) AS TotalDerrotas,
        IFNULL(AVG(H.dano_causado), 0) AS DanoPromedioPorAccion
    FROM Personajes P
    LEFT JOIN Batallas B 
        ON (P.id = B.combatiente1_id OR P.id = B.combatiente2_id)
        AND B.es_finalizada = TRUE
        AND B.fecha_inicio BETWEEN p_fecha_desde AND p_fecha_hasta
    LEFT JOIN HistorialBatallas H 
        ON B.id = H.batalla_id AND H.atacante_id = P.id
    GROUP BY P.tipo
    ORDER BY TotalVictorias DESC, DanoPromedioPorAccion DESC;
END //

-- Generar reportes combinando batallas, participantes y resultados
CREATE PROCEDURE sp_GenerarReporteBatallasDetallado (
    IN p_fecha_desde DATETIME,
    IN p_fecha_hasta DATETIME
)
BEGIN
    SELECT 
        B.id AS BatallaId,
        B.fecha_inicio AS FechaInicio,
        B.numero_turno AS TurnosTotales,
        B.es_finalizada AS Finalizada,
        C1.nombre AS Combatiente1_Nombre,
        C1.tipo AS Combatiente1_Tipo,
        C2.nombre AS Combatiente2_Nombre,
        C2.tipo AS Combatiente2_Tipo,
        IFNULL(G.nombre, 'Sin Ganador / Empate') AS Ganador_Nombre,
        IFNULL(G.tipo, '-') AS Ganador_Tipo
    FROM Batallas B
    INNER JOIN Personajes C1 ON B.combatiente1_id = C1.id
    INNER JOIN Personajes C2 ON B.combatiente2_id = C2.id
    LEFT JOIN Personajes G ON B.ganador_id = G.id
    WHERE B.fecha_inicio BETWEEN p_fecha_desde AND p_fecha_hasta
    ORDER BY B.fecha_inicio DESC;
END //

DELIMITER ;