-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: localhost    Database: combate_api
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `combate_api`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `combate_api` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `combate_api`;

--
-- Table structure for table `batallas`
--

DROP TABLE IF EXISTS `batallas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `batallas` (
  `id` int NOT NULL AUTO_INCREMENT,
  `combatiente1_id` int NOT NULL,
  `combatiente2_id` int NOT NULL,
  `ganador_id` int DEFAULT NULL,
  `numero_turno` int NOT NULL DEFAULT '1',
  `es_finalizada` tinyint(1) NOT NULL DEFAULT '0',
  `fecha_inicio` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `FK_Batallas_Combatiente1` (`combatiente1_id`),
  KEY `FK_Batallas_Combatiente2` (`combatiente2_id`),
  KEY `FK_Batallas_Ganador` (`ganador_id`),
  CONSTRAINT `FK_Batallas_Combatiente1` FOREIGN KEY (`combatiente1_id`) REFERENCES `personajes` (`id`),
  CONSTRAINT `FK_Batallas_Combatiente2` FOREIGN KEY (`combatiente2_id`) REFERENCES `personajes` (`id`),
  CONSTRAINT `FK_Batallas_Ganador` FOREIGN KEY (`ganador_id`) REFERENCES `personajes` (`id`),
  CONSTRAINT `CK_Batallas_Distintos` CHECK ((`combatiente1_id` <> `combatiente2_id`))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `batallas`
--

LOCK TABLES `batallas` WRITE;
/*!40000 ALTER TABLE `batallas` DISABLE KEYS */;
/*!40000 ALTER TABLE `batallas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `combates`
--

DROP TABLE IF EXISTS `combates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `combates` (
  `id` int NOT NULL AUTO_INCREMENT,
  `personaje1_id` int NOT NULL,
  `personaje2_id` int NOT NULL,
  `estado` enum('EnCurso','Finalizado') NOT NULL DEFAULT 'EnCurso',
  `turno_personaje1` tinyint(1) NOT NULL DEFAULT '1',
  `numero_turno` int NOT NULL DEFAULT '1',
  `vida_personaje1` int NOT NULL,
  `vida_personaje2` int NOT NULL,
  `recurso_personaje1` int DEFAULT NULL,
  `recurso_personaje2` int DEFAULT NULL,
  `ganador_id` int DEFAULT NULL,
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `fk_combates_ganador` (`ganador_id`),
  KEY `ix_combates_personaje1` (`personaje1_id`),
  KEY `ix_combates_personaje2` (`personaje2_id`),
  CONSTRAINT `fk_combates_ganador` FOREIGN KEY (`ganador_id`) REFERENCES `personajes` (`id`),
  CONSTRAINT `fk_combates_personaje1` FOREIGN KEY (`personaje1_id`) REFERENCES `personajes` (`id`),
  CONSTRAINT `fk_combates_personaje2` FOREIGN KEY (`personaje2_id`) REFERENCES `personajes` (`id`),
  CONSTRAINT `ck_combates_distintos` CHECK ((`personaje1_id` <> `personaje2_id`)),
  CONSTRAINT `ck_combates_ganador` CHECK (((`ganador_id` is null) or (`ganador_id` in (`personaje1_id`,`personaje2_id`)))),
  CONSTRAINT `ck_combates_turno` CHECK ((`numero_turno` >= 1)),
  CONSTRAINT `ck_combates_vida` CHECK (((`vida_personaje1` >= 0) and (`vida_personaje2` >= 0)))
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `combates`
--

LOCK TABLES `combates` WRITE;
/*!40000 ALTER TABLE `combates` DISABLE KEYS */;
INSERT INTO `combates` VALUES (1,1,2,'Finalizado',1,12,51,0,0,0,1,'2026-11-20 08:16:15'),(2,3,4,'EnCurso',1,3,55,61,10,NULL,NULL,'2026-11-20 08:16:15'),(3,2,3,'EnCurso',1,1,80,90,60,12,NULL,'2026-11-20 08:16:15');
/*!40000 ALTER TABLE `combates` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `habilidades`
--

DROP TABLE IF EXISTS `habilidades`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `habilidades` (
  `id` int NOT NULL AUTO_INCREMENT,
  `personaje_id` int NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `costo_recurso` int NOT NULL,
  `potencia` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_Habilidades_Personajes` (`personaje_id`),
  CONSTRAINT `FK_Habilidades_Personajes` FOREIGN KEY (`personaje_id`) REFERENCES `personajes` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `habilidades`
--

LOCK TABLES `habilidades` WRITE;
/*!40000 ALTER TABLE `habilidades` DISABLE KEYS */;
/*!40000 ALTER TABLE `habilidades` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `historialbatallas`
--

DROP TABLE IF EXISTS `historialbatallas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `historialbatallas` (
  `id` int NOT NULL AUTO_INCREMENT,
  `batalla_id` int NOT NULL,
  `numero_turno` int NOT NULL,
  `atacante_id` int NOT NULL,
  `defensor_id` int NOT NULL,
  `habilidad_id` int DEFAULT NULL,
  `dano_causado` int NOT NULL DEFAULT '0',
  `fecha_registro` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `FK_Historial_Batallas` (`batalla_id`),
  KEY `FK_Historial_Atacante` (`atacante_id`),
  KEY `FK_Historial_Defensor` (`defensor_id`),
  KEY `FK_Historial_Habilidad` (`habilidad_id`),
  CONSTRAINT `FK_Historial_Atacante` FOREIGN KEY (`atacante_id`) REFERENCES `personajes` (`id`),
  CONSTRAINT `FK_Historial_Batallas` FOREIGN KEY (`batalla_id`) REFERENCES `batallas` (`id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Historial_Defensor` FOREIGN KEY (`defensor_id`) REFERENCES `personajes` (`id`),
  CONSTRAINT `FK_Historial_Habilidad` FOREIGN KEY (`habilidad_id`) REFERENCES `habilidades` (`id`),
  CONSTRAINT `CK_Historial_Distintos` CHECK ((`atacante_id` <> `defensor_id`))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historialbatallas`
--

LOCK TABLES `historialbatallas` WRITE;
/*!40000 ALTER TABLE `historialbatallas` DISABLE KEYS */;
/*!40000 ALTER TABLE `historialbatallas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `personajes`
--

DROP TABLE IF EXISTS `personajes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `personajes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) NOT NULL,
  `tipo` enum('Guerrero','Mago','Arquero','Asesino') NOT NULL,
  `vida_max` int NOT NULL,
  `vida` int NOT NULL DEFAULT (`vida_max`),
  `ataque` int NOT NULL,
  `defensa` int NOT NULL,
  `escudo` int DEFAULT NULL,
  `escudo_actual` int DEFAULT (`escudo`),
  `mana_max` int DEFAULT NULL,
  `mana` int DEFAULT (`mana_max`),
  `cantidad_flechas` int DEFAULT NULL,
  `flechas_actuales` int DEFAULT (`cantidad_flechas`),
  `probabilidad_critico` decimal(3,2) DEFAULT NULL,
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  CONSTRAINT `ck_personajes_ataque` CHECK ((`ataque` > 0)),
  CONSTRAINT `ck_personajes_critico` CHECK (((`probabilidad_critico` is null) or (`probabilidad_critico` between 0 and 1))),
  CONSTRAINT `ck_personajes_defensa` CHECK ((`defensa` >= 0)),
  CONSTRAINT `ck_personajes_escudo` CHECK (((`escudo` is null) or (`escudo` >= 0))),
  CONSTRAINT `ck_personajes_flechas` CHECK (((`cantidad_flechas` is null) or (`cantidad_flechas` >= 0))),
  CONSTRAINT `ck_personajes_mana` CHECK (((`mana_max` is null) or (`mana_max` >= 0))),
  CONSTRAINT `ck_personajes_nombre` CHECK ((char_length(trim(`nombre`)) > 0)),
  CONSTRAINT `ck_personajes_vida_max` CHECK ((`vida_max` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `personajes`
--

LOCK TABLES `personajes` WRITE;
/*!40000 ALTER TABLE `personajes` DISABLE KEYS */;
INSERT INTO `personajes` VALUES (1,'Thorin','Guerrero',120,120,18,8,20,20,NULL,NULL,NULL,NULL,NULL,'2026-11-20 08:16:15'),(2,'Merlina','Mago',80,80,14,4,NULL,NULL,60,60,NULL,NULL,NULL,'2026-11-20 08:16:15'),(3,'Legolia','Arquero',90,90,16,5,NULL,NULL,NULL,NULL,12,12,NULL,'2026-11-20 08:16:15'),(4,'Sombra','Asesino',85,85,20,4,NULL,NULL,NULL,NULL,NULL,NULL,0.30,'2026-11-20 08:16:15'),(5,'Brunilda','Guerrero',110,110,16,7,15,15,NULL,NULL,NULL,NULL,NULL,'2026-11-20 08:16:15');
/*!40000 ALTER TABLE `personajes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `turnos`
--

DROP TABLE IF EXISTS `turnos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `turnos` (
  `id` int NOT NULL AUTO_INCREMENT,
  `combate_id` int NOT NULL,
  `numero` int NOT NULL,
  `atacante_id` int NOT NULL,
  `defensor_id` int NOT NULL,
  `accion` enum('atacar','habilidad') NOT NULL,
  `danio` int NOT NULL,
  `nota` varchar(255) DEFAULT NULL,
  `vida_personaje1_antes` int NOT NULL,
  `vida_personaje2_antes` int NOT NULL,
  `recurso_personaje1_antes` int DEFAULT NULL,
  `recurso_personaje2_antes` int DEFAULT NULL,
  `fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_turnos_combate_numero` (`combate_id`,`numero`),
  KEY `fk_turnos_atacante` (`atacante_id`),
  KEY `fk_turnos_defensor` (`defensor_id`),
  CONSTRAINT `fk_turnos_atacante` FOREIGN KEY (`atacante_id`) REFERENCES `personajes` (`id`),
  CONSTRAINT `fk_turnos_combate` FOREIGN KEY (`combate_id`) REFERENCES `combates` (`id`),
  CONSTRAINT `fk_turnos_defensor` FOREIGN KEY (`defensor_id`) REFERENCES `personajes` (`id`),
  CONSTRAINT `ck_turnos_danio` CHECK ((`danio` >= 0)),
  CONSTRAINT `ck_turnos_distintos` CHECK ((`atacante_id` <> `defensor_id`)),
  CONSTRAINT `ck_turnos_numero` CHECK ((`numero` >= 1)),
  CONSTRAINT `ck_turnos_vida` CHECK (((`vida_personaje1_antes` >= 0) and (`vida_personaje2_antes` >= 0)))
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `turnos`
--

LOCK TABLES `turnos` WRITE;
/*!40000 ALTER TABLE `turnos` DISABLE KEYS */;
INSERT INTO `turnos` VALUES (1,1,1,1,2,'atacar',14,'Primer golpe de Thorin',120,80,20,60,'2026-11-20 08:16:15'),(2,1,2,2,1,'habilidad',28,'Bola de fuego contra el escudo',120,66,20,60,'2026-11-20 08:16:15'),(3,1,3,1,2,'habilidad',14,'Embestida sin escudo',112,66,0,35,'2026-11-20 08:16:15'),(4,1,4,2,1,'atacar',21,NULL,112,52,0,35,'2026-11-20 08:16:15'),(5,1,5,1,2,'atacar',14,NULL,91,52,0,25,'2026-11-20 08:16:15'),(6,1,6,2,1,'habilidad',28,'Última bola de fuego',91,38,0,25,'2026-11-20 08:16:15'),(7,1,7,1,2,'atacar',14,NULL,63,38,0,0,'2026-11-20 08:16:15'),(8,1,8,2,1,'atacar',6,'Merlina sin maná',63,24,0,0,'2026-11-20 08:16:15'),(9,1,9,1,2,'atacar',14,NULL,57,24,0,0,'2026-11-20 08:16:15'),(10,1,10,2,1,'atacar',6,NULL,57,10,0,0,'2026-11-20 08:16:15'),(11,1,11,1,2,'atacar',14,'Golpe final',51,10,0,0,'2026-11-20 08:16:15'),(12,2,1,3,4,'habilidad',24,'Disparo doble',90,85,12,NULL,'2026-11-20 08:16:15'),(13,2,2,4,3,'habilidad',35,'Golpe sigiloso',90,61,10,NULL,'2026-11-20 08:16:15');
/*!40000 ALTER TABLE `turnos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'combate_api'
--
/*!50003 DROP PROCEDURE IF EXISTS `sp_DanoPromedioPorClaseYHabilidad` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_DanoPromedioPorClaseYHabilidad`()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    SELECT 
        P.tipo AS ClasePersonaje,
        IFNULL(HB.nombre, 'Ataque Básico') AS Habilidad,
        COUNT(H.id) AS CantidadUsos,
        ROUND(AVG(H.dano_causado), 2) AS DanoPromedio,
        MAX(H.dano_causado) AS DanoMaximo
    FROM HistorialBatallas H
    INNER JOIN Personajes P ON H.atacante_id = P.id
    LEFT JOIN Habilidades HB ON H.habilidad_id = HB.id
    GROUP BY P.tipo, IFNULL(HB.nombre, 'Ataque Básico')
    ORDER BY P.tipo, DanoPromedio DESC;

    COMMIT;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_DanoTotalRealizadoYRecibidoPorPeriodo` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_DanoTotalRealizadoYRecibidoPorPeriodo`(
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
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_FinalizarBatalla` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_FinalizarBatalla`(
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
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_GenerarReporteBatallasDetallado` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GenerarReporteBatallasDetallado`(
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
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_ObtenerRankingPorTipoPersonaje` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ObtenerRankingPorTipoPersonaje`(
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
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_ProcesarAccionCombate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ProcesarAccionCombate`(
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
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_RankingPersonajesEfectividad` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_RankingPersonajesEfectividad`()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
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
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_RegistrarBatalla` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_RegistrarBatalla`(
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

        
        UPDATE Personajes
        SET vida = vida_max,
            mana = mana_max
        WHERE id IN (p_combatiente1_id, p_combatiente2_id);

        SELECT LAST_INSERT_ID() AS BatallaId;

    COMMIT;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_RegistrarTurno` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_RegistrarTurno`(
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
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_ResumenBatallasPorPeriodoYTipo` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ResumenBatallasPorPeriodoYTipo`(
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
        SUM(CASE WHEN B.es_finalizada = TRUE THEN 1 ELSE 0 END) AS BatallasFinalizadas,
        SUM(CASE WHEN B.es_finalizada = FALSE THEN 1 ELSE 0 END) AS BatallasEnProgreso
    FROM Personajes P
    INNER JOIN Batallas B ON (P.id = B.combatiente1_id OR P.id = B.combatiente2_id)
    WHERE B.fecha_inicio BETWEEN p_fecha_desde AND p_fecha_hasta
    GROUP BY P.tipo;

    COMMIT;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_VictoriasYDerrotasPorTipoYPeriodo` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_VictoriasYDerrotasPorTipoYPeriodo`(
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
        SUM(CASE WHEN B.ganador_id = P.id THEN 1 ELSE 0 END) AS Victorias,
        SUM(CASE WHEN B.ganador_id IS NOT NULL AND B.ganador_id != P.id THEN 1 ELSE 0 END) AS Derrotas,
        SUM(CASE WHEN B.ganador_id IS NULL THEN 1 ELSE 0 END) AS Empates
    FROM Personajes P
    INNER JOIN Batallas B ON (P.id = B.combatiente1_id OR P.id = B.combatiente2_id)
    WHERE B.es_finalizada = TRUE
      AND B.fecha_inicio BETWEEN p_fecha_desde AND p_fecha_hasta
    GROUP BY P.tipo;

    COMMIT;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed
