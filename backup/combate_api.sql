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
INSERT INTO `combates` VALUES (1,1,2,'Finalizado',1,12,51,0,0,0,1,'2026-11-09 12:45:41'),(2,3,4,'EnCurso',1,3,55,61,10,NULL,NULL,'2026-11-09 12:45:41'),(3,2,3,'EnCurso',1,1,80,90,60,12,NULL,'2026-11-09 12:45:41');
/*!40000 ALTER TABLE `combates` ENABLE KEYS */;
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
  `ataque` int NOT NULL,
  `defensa` int NOT NULL,
  `escudo` int DEFAULT NULL,
  `mana_max` int DEFAULT NULL,
  `cantidad_flechas` int DEFAULT NULL,
  `probabilidad_critico` decimal(3,2) DEFAULT NULL,
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  CONSTRAINT `ck_personajes_ataque` CHECK ((`ataque` > 0)),
  CONSTRAINT `ck_personajes_critico` CHECK (((`probabilidad_critico` is null) or (`probabilidad_critico` between 0 and 1))),
  CONSTRAINT `ck_personajes_defensa` CHECK ((`defensa` >= 0)),
  CONSTRAINT `ck_personajes_escudo` CHECK (((`escudo` is null) or (`escudo` >= 0))),
  CONSTRAINT `ck_personajes_flechas` CHECK (((`cantidad_flechas` is null) or (`cantidad_flechas` >= 0))),
  CONSTRAINT `ck_personajes_mana` CHECK (((`mana_max` is null) or (`mana_max` > 0))),
  CONSTRAINT `ck_personajes_nombre` CHECK ((char_length(trim(`nombre`)) > 0)),
  CONSTRAINT `ck_personajes_vida_max` CHECK ((`vida_max` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `personajes`
--

LOCK TABLES `personajes` WRITE;
/*!40000 ALTER TABLE `personajes` DISABLE KEYS */;
INSERT INTO `personajes` VALUES (1,'Thorin','Guerrero',120,18,8,20,NULL,NULL,NULL,'2026-11-09 12:45:41'),(2,'Merlina','Mago',80,14,4,NULL,60,NULL,NULL,'2026-11-09 12:45:41'),(3,'Legolia','Arquero',90,16,5,NULL,NULL,12,NULL,'2026-11-09 12:45:41'),(4,'Sombra','Asesino',85,20,4,NULL,NULL,NULL,0.30,'2026-11-09 12:45:41'),(5,'Brunilda','Guerrero',110,16,7,15,NULL,NULL,NULL,'2026-11-09 12:45:41');
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
INSERT INTO `turnos` VALUES (1,1,1,1,2,'atacar',14,'Primer golpe de Thorin',120,80,20,60,'2026-11-09 12:45:41'),(2,1,2,2,1,'habilidad',28,'Bola de fuego contra el escudo',120,66,20,60,'2026-11-09 12:45:41'),(3,1,3,1,2,'habilidad',14,'Embestida sin escudo',112,66,0,35,'2026-11-09 12:45:41'),(4,1,4,2,1,'atacar',21,NULL,112,52,0,35,'2026-11-09 12:45:41'),(5,1,5,1,2,'atacar',14,NULL,91,52,0,25,'2026-11-09 12:45:41'),(6,1,6,2,1,'habilidad',28,'Última bola de fuego',91,38,0,25,'2026-11-09 12:45:41'),(7,1,7,1,2,'atacar',14,NULL,63,38,0,0,'2026-11-09 12:45:41'),(8,1,8,2,1,'atacar',6,'Merlina sin maná',63,24,0,0,'2026-11-09 12:45:41'),(9,1,9,1,2,'atacar',14,NULL,57,24,0,0,'2026-11-09 12:45:41'),(10,1,10,2,1,'atacar',6,NULL,57,10,0,0,'2026-11-09 12:45:41'),(11,1,11,1,2,'atacar',14,'Golpe final',51,10,0,0,'2026-11-09 12:45:41'),(12,2,1,3,4,'habilidad',24,'Disparo doble',90,85,12,NULL,'2026-11-09 12:45:41'),(13,2,2,4,3,'habilidad',35,'Golpe sigiloso',90,61,10,NULL,'2026-11-09 12:45:41');
/*!40000 ALTER TABLE `turnos` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed
