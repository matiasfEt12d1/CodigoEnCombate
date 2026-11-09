-- =====================================================================
-- 01_estructura.sql
-- Crea la base combate_api y las tablas personajes, combates y turnos.
-- Ejecutar con un usuario con permisos de creación (por ejemplo root).
-- =====================================================================

DROP DATABASE IF EXISTS combate_api;
CREATE DATABASE combate_api
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

USE combate_api;

-- ---------------------------------------------------------------------
-- personajes: estadísticas base de cada personaje.
-- El recurso propio de cada tipo queda en su columna (NULL en el resto):
--   Guerrero -> escudo | Mago -> mana_max | Arquero -> cantidad_flechas
--   Asesino  -> probabilidad_critico
-- ---------------------------------------------------------------------
CREATE TABLE personajes (
    id                   INT          NOT NULL AUTO_INCREMENT,
    nombre               VARCHAR(50)  NOT NULL,
    tipo                 ENUM('Guerrero', 'Mago', 'Arquero', 'Asesino') NOT NULL,
    vida_max             INT          NOT NULL,
    ataque               INT          NOT NULL,
    defensa              INT          NOT NULL,
    escudo               INT          NULL,
    mana_max             INT          NULL,
    cantidad_flechas     INT          NULL,
    probabilidad_critico DECIMAL(3,2) NULL,
    fecha_creacion       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_personajes PRIMARY KEY (id),
    CONSTRAINT ck_personajes_nombre   CHECK (CHAR_LENGTH(TRIM(nombre)) > 0),
    CONSTRAINT ck_personajes_vida_max CHECK (vida_max > 0),
    CONSTRAINT ck_personajes_ataque   CHECK (ataque > 0),
    CONSTRAINT ck_personajes_defensa  CHECK (defensa >= 0),
    CONSTRAINT ck_personajes_escudo   CHECK (escudo IS NULL OR escudo >= 0),
    CONSTRAINT ck_personajes_mana     CHECK (mana_max IS NULL OR mana_max > 0),
    CONSTRAINT ck_personajes_flechas  CHECK (cantidad_flechas IS NULL OR cantidad_flechas >= 0),
    CONSTRAINT ck_personajes_critico  CHECK (probabilidad_critico IS NULL OR probabilidad_critico BETWEEN 0 AND 1)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- combates: dos participantes distintos y el estado actual del combate.
-- La vida y el recurso de cada participante se guardan por combate,
-- así un personaje puede pelear en varios combates sin mezclar estados.
-- ---------------------------------------------------------------------
CREATE TABLE combates (
    id                 INT      NOT NULL AUTO_INCREMENT,
    personaje1_id      INT      NOT NULL,
    personaje2_id      INT      NOT NULL,
    estado             ENUM('EnCurso', 'Finalizado') NOT NULL DEFAULT 'EnCurso',
    turno_personaje1   BOOLEAN  NOT NULL DEFAULT TRUE,
    numero_turno       INT      NOT NULL DEFAULT 1,
    vida_personaje1    INT      NOT NULL,
    vida_personaje2    INT      NOT NULL,
    recurso_personaje1 INT      NULL,
    recurso_personaje2 INT      NULL,
    ganador_id         INT      NULL,
    fecha_creacion     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_combates PRIMARY KEY (id),
    CONSTRAINT fk_combates_personaje1 FOREIGN KEY (personaje1_id) REFERENCES personajes (id),
    CONSTRAINT fk_combates_personaje2 FOREIGN KEY (personaje2_id) REFERENCES personajes (id),
    CONSTRAINT fk_combates_ganador    FOREIGN KEY (ganador_id)    REFERENCES personajes (id),
    CONSTRAINT ck_combates_distintos  CHECK (personaje1_id <> personaje2_id),
    CONSTRAINT ck_combates_turno      CHECK (numero_turno >= 1),
    CONSTRAINT ck_combates_vida       CHECK (vida_personaje1 >= 0 AND vida_personaje2 >= 0),
    CONSTRAINT ck_combates_ganador    CHECK (ganador_id IS NULL OR ganador_id IN (personaje1_id, personaje2_id))
) ENGINE = InnoDB;

CREATE INDEX ix_combates_personaje1 ON combates (personaje1_id);
CREATE INDEX ix_combates_personaje2 ON combates (personaje2_id);

-- ---------------------------------------------------------------------
-- turnos: historial de acciones de cada combate.
-- Guarda la vida y el recurso de ambos participantes ANTES del turno,
-- necesarios para deshacer el último turno y restaurar el combate.
-- La nota es el único dato editable.
-- ---------------------------------------------------------------------
CREATE TABLE turnos (
    id                       INT          NOT NULL AUTO_INCREMENT,
    combate_id               INT          NOT NULL,
    numero                   INT          NOT NULL,
    atacante_id              INT          NOT NULL,
    defensor_id              INT          NOT NULL,
    accion                   ENUM('atacar', 'habilidad') NOT NULL,
    danio                    INT          NOT NULL,
    nota                     VARCHAR(255) NULL,
    vida_personaje1_antes    INT          NOT NULL,
    vida_personaje2_antes    INT          NOT NULL,
    recurso_personaje1_antes INT          NULL,
    recurso_personaje2_antes INT          NULL,
    fecha                    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_turnos PRIMARY KEY (id),
    CONSTRAINT uq_turnos_combate_numero UNIQUE (combate_id, numero),
    CONSTRAINT fk_turnos_combate  FOREIGN KEY (combate_id)  REFERENCES combates (id),
    CONSTRAINT fk_turnos_atacante FOREIGN KEY (atacante_id) REFERENCES personajes (id),
    CONSTRAINT fk_turnos_defensor FOREIGN KEY (defensor_id) REFERENCES personajes (id),
    CONSTRAINT ck_turnos_numero    CHECK (numero >= 1),
    CONSTRAINT ck_turnos_distintos CHECK (atacante_id <> defensor_id),
    CONSTRAINT ck_turnos_danio     CHECK (danio >= 0),
    CONSTRAINT ck_turnos_vida      CHECK (vida_personaje1_antes >= 0 AND vida_personaje2_antes >= 0)
) ENGINE = InnoDB;
