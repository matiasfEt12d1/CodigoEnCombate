-- Tablas del simulador sobre la base combate_api.
-- Reutiliza la tabla personajes de sql/01_estructura.sql: ejecutar ese script antes.
USE combate_api;

DROP TABLE IF EXISTS HistorialBatallas;
DROP TABLE IF EXISTS Habilidades;
DROP TABLE IF EXISTS Batallas;

CREATE TABLE Habilidades (
    id INT AUTO_INCREMENT PRIMARY KEY,
    personaje_id INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    costo_recurso INT NOT NULL,
    potencia INT NOT NULL,
    CONSTRAINT FK_Habilidades_Personajes FOREIGN KEY (personaje_id)
        REFERENCES personajes(id) ON DELETE CASCADE
);

CREATE TABLE Batallas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    combatiente1_id INT NOT NULL,
    combatiente2_id INT NOT NULL,
    ganador_id INT NULL,
    numero_turno INT NOT NULL DEFAULT 1,
    es_finalizada BOOLEAN NOT NULL DEFAULT FALSE,
    fecha_inicio DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Batallas_Combatiente1 FOREIGN KEY (combatiente1_id)
        REFERENCES personajes(id),
    CONSTRAINT FK_Batallas_Combatiente2 FOREIGN KEY (combatiente2_id)
        REFERENCES personajes(id),
    CONSTRAINT FK_Batallas_Ganador FOREIGN KEY (ganador_id)
        REFERENCES personajes(id),
    CONSTRAINT CK_Batallas_Distintos CHECK (combatiente1_id <> combatiente2_id)
);

CREATE TABLE HistorialBatallas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    batalla_id INT NOT NULL,
    numero_turno INT NOT NULL,
    atacante_id INT NOT NULL,
    defensor_id INT NOT NULL,
    habilidad_id INT NULL,
    dano_causado INT NOT NULL DEFAULT 0,
    fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Historial_Batallas FOREIGN KEY (batalla_id)
        REFERENCES Batallas(id) ON DELETE CASCADE,
    CONSTRAINT FK_Historial_Atacante FOREIGN KEY (atacante_id)
        REFERENCES personajes(id),
    CONSTRAINT FK_Historial_Defensor FOREIGN KEY (defensor_id)
        REFERENCES personajes(id),
    CONSTRAINT FK_Historial_Habilidad FOREIGN KEY (habilidad_id)
        REFERENCES Habilidades(id),
    CONSTRAINT CK_Historial_Distintos CHECK (atacante_id <> defensor_id)
);
