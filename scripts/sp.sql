USE combate_api;

DROP PROCEDURE IF EXISTS sp_RegistrarBatalla;
DROP PROCEDURE IF EXISTS sp_FinalizarBatalla;
DROP PROCEDURE IF EXISTS sp_ProcesarAccionCombate;
DROP PROCEDURE IF EXISTS sp_RegistrarTurno;
DROP PROCEDURE IF EXISTS sp_ObtenerRankingPorTipoPersonaje;
DROP PROCEDURE IF EXISTS sp_GenerarReporteBatallasDetallado;

DELIMITER //

CREATE PROCEDURE sp_RegistrarBatalla (
    IN p_combatiente1_id INT,
    IN p_combatiente2_id INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

        INSERT INTO Batallas (combatiente1_id, combatiente2_id, numero_turno, es_finalizada, fecha_inicio)
        VALUES (p_combatiente1_id, p_combatiente2_id, 1, FALSE, NOW());

        -- Estado inicial: los dos combatientes empiezan con la vida y el maná completos.
        UPDATE Personajes
        SET vida = vida_max,
            mana = mana_max
        WHERE id IN (p_combatiente1_id, p_combatiente2_id);

        SELECT LAST_INSERT_ID() AS BatallaId;

    COMMIT;
END //

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
        RESIGNAL;
    END;

    START TRANSACTION;

    UPDATE Batallas
    SET ganador_id = p_ganador_id,
        numero_turno = p_numero_turno,
        es_finalizada = TRUE
    WHERE id = p_batalla_id;

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

CREATE PROCEDURE sp_ProcesarAccionCombate (
    IN p_batalla_id INT,
    IN p_atacante_id INT,
    IN p_defensor_id INT,
    IN p_dano_realizado INT,
    IN p_nueva_vida_defensor INT,
    IN p_habilidad_id INT,
    IN p_numero_turno INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

        INSERT INTO HistorialBatallas (batalla_id, numero_turno, atacante_id, defensor_id, habilidad_id, dano_causado, fecha_registro)
        VALUES (p_batalla_id, p_numero_turno, p_atacante_id, p_defensor_id, p_habilidad_id, p_dano_realizado, NOW());

        UPDATE Personajes
        SET vida = p_nueva_vida_defensor
        WHERE id = p_defensor_id;

        UPDATE Batallas
        SET numero_turno = p_numero_turno
        WHERE id = p_batalla_id;

    COMMIT;
END //

-- Registra un turno completo en una sola transacción: la acción de cada combatiente
-- en el historial, el estado de los dos personajes y el avance o cierre de la batalla.
-- Si el combatiente 2 no llegó a actuar, p_dano_c2 llega en NULL.
CREATE PROCEDURE sp_RegistrarTurno (
    IN p_batalla_id INT,
    IN p_numero_turno INT,
    IN p_dano_c1 INT,
    IN p_habilidad_c1_id INT,
    IN p_dano_c2 INT,
    IN p_habilidad_c2_id INT,
    IN p_vida_c1 INT,
    IN p_escudo_c1 INT,
    IN p_mana_c1 INT,
    IN p_flechas_c1 INT,
    IN p_vida_c2 INT,
    IN p_escudo_c2 INT,
    IN p_mana_c2 INT,
    IN p_flechas_c2 INT,
    IN p_finalizada BOOLEAN,
    IN p_ganador_id INT
)
BEGIN
    DECLARE v_c1 INT;
    DECLARE v_c2 INT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

        SELECT combatiente1_id, combatiente2_id INTO v_c1, v_c2
        FROM Batallas
        WHERE id = p_batalla_id AND es_finalizada = FALSE
        FOR UPDATE;

        IF v_c1 IS NULL THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La batalla no existe o ya finalizó.';
        END IF;

        INSERT INTO HistorialBatallas (batalla_id, numero_turno, atacante_id, defensor_id, habilidad_id, dano_causado, fecha_registro)
        VALUES (p_batalla_id, p_numero_turno, v_c1, v_c2, p_habilidad_c1_id, p_dano_c1, NOW());

        IF p_dano_c2 IS NOT NULL THEN
            INSERT INTO HistorialBatallas (batalla_id, numero_turno, atacante_id, defensor_id, habilidad_id, dano_causado, fecha_registro)
            VALUES (p_batalla_id, p_numero_turno, v_c2, v_c1, p_habilidad_c2_id, p_dano_c2, NOW());
        END IF;

        -- Estado actual: escudo y cantidad_flechas son valores base que usa la API.
        UPDATE Personajes
        SET vida = p_vida_c1, escudo_actual = p_escudo_c1, mana = p_mana_c1, flechas_actuales = p_flechas_c1
        WHERE id = v_c1;

        UPDATE Personajes
        SET vida = p_vida_c2, escudo_actual = p_escudo_c2, mana = p_mana_c2, flechas_actuales = p_flechas_c2
        WHERE id = v_c2;

        UPDATE Batallas
        SET numero_turno = IF(p_finalizada, p_numero_turno, p_numero_turno + 1),
            es_finalizada = p_finalizada,
            ganador_id = p_ganador_id
        WHERE id = p_batalla_id;

    COMMIT;
END //

CREATE PROCEDURE sp_ObtenerRankingPorTipoPersonaje (
    IN p_fecha_desde DATETIME,
    IN p_fecha_hasta DATETIME
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

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

    COMMIT;
END //

CREATE PROCEDURE sp_GenerarReporteBatallasDetallado (
    IN p_fecha_desde DATETIME,
    IN p_fecha_hasta DATETIME
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

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

    COMMIT;
END //

DELIMITER ;