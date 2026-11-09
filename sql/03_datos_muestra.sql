-- =====================================================================
-- 03_datos_muestra.sql
-- Carga datos de muestra en combate_api.
-- Ejecutar después de 01_estructura.sql (con root o combate_admin).
-- Los valores siguen las reglas del dominio (Persistencia/Entidades).
-- =====================================================================

USE combate_api;

START TRANSACTION;

INSERT INTO personajes (id, nombre, tipo, vida_max, ataque, defensa, escudo, mana_max, cantidad_flechas, probabilidad_critico) VALUES
    (1, 'Thorin',   'Guerrero', 120, 18, 8, 20,   NULL, NULL, NULL),
    (2, 'Merlina',  'Mago',      80, 14, 4, NULL, 60,   NULL, NULL),
    (3, 'Legolia',  'Arquero',   90, 16, 5, NULL, NULL, 12,   NULL),
    (4, 'Sombra',   'Asesino',   85, 20, 4, NULL, NULL, NULL, 0.30),
    (5, 'Brunilda', 'Guerrero', 110, 16, 7, 15,   NULL, NULL, NULL);

-- Combate 1: finalizado, ganó Thorin.
-- Combate 2: en curso, le toca a Legolia (personaje 1).
-- Combate 3: recién creado, sin turnos.
-- Brunilda no participó en ningún combate.
INSERT INTO combates (id, personaje1_id, personaje2_id, estado, turno_personaje1, numero_turno,
                      vida_personaje1, vida_personaje2, recurso_personaje1, recurso_personaje2, ganador_id) VALUES
    (1, 1, 2, 'Finalizado', TRUE, 12, 51,  0, 0,  0,    1),
    (2, 3, 4, 'EnCurso',    TRUE,  3, 55, 61, 10, NULL, NULL),
    (3, 2, 3, 'EnCurso',    TRUE,  1, 80, 90, 60, 12,   NULL);

INSERT INTO turnos (combate_id, numero, atacante_id, defensor_id, accion, danio, nota,
                    vida_personaje1_antes, vida_personaje2_antes, recurso_personaje1_antes, recurso_personaje2_antes) VALUES
    (1,  1, 1, 2, 'atacar',    14, 'Primer golpe de Thorin',          120, 80, 20, 60),
    (1,  2, 2, 1, 'habilidad', 28, 'Bola de fuego contra el escudo',  120, 66, 20, 60),
    (1,  3, 1, 2, 'habilidad', 14, 'Embestida sin escudo',            112, 66,  0, 35),
    (1,  4, 2, 1, 'atacar',    21, NULL,                              112, 52,  0, 35),
    (1,  5, 1, 2, 'atacar',    14, NULL,                               91, 52,  0, 25),
    (1,  6, 2, 1, 'habilidad', 28, 'Última bola de fuego',             91, 38,  0, 25),
    (1,  7, 1, 2, 'atacar',    14, NULL,                               63, 38,  0,  0),
    (1,  8, 2, 1, 'atacar',     6, 'Merlina sin maná',                 63, 24,  0,  0),
    (1,  9, 1, 2, 'atacar',    14, NULL,                               57, 24,  0,  0),
    (1, 10, 2, 1, 'atacar',     6, NULL,                               57, 10,  0,  0),
    (1, 11, 1, 2, 'atacar',    14, 'Golpe final',                      51, 10,  0,  0),
    (2,  1, 3, 4, 'habilidad', 24, 'Disparo doble',                    90, 85, 12, NULL),
    (2,  2, 4, 3, 'habilidad', 35, 'Golpe sigiloso',                   90, 61, 10, NULL);

COMMIT;
