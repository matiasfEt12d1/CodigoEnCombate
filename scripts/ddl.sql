CREATE DATABASE IF NOT EXISTS codigo_en_combate;
USE codigo_en_combate;

DROP TABLE IF EXISTS HistorialBatallas;
DROP TABLE IF EXISTS Habilidades;
DROP TABLE IF EXISTS Batallas;
DROP TABLE IF EXISTS Personajes;

CREATE TABLE Personajes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    tipo VARCHAR(20) NOT NULL,
    vida_max INT NOT NULL,
    vida INT NOT NULL,
    ataque INT NOT NULL,
    defensa INT NOT NULL,
    escudo INT NULL DEFAULT 0,
    mana_max INT NULL DEFAULT 0,
    mana INT NULL DEFAULT 0,
    cantidad_flechas INT NULL DEFAULT 0,
    probabilidad_critico DECIMAL(3,2) NULL DEFAULT 0.00
);

CREATE TABLE Habilidades (
    id INT AUTO_INCREMENT PRIMARY KEY,
    personaje_id INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    costo_recurso INT NOT NULL,
    potencia INT NOT NULL,
    CONSTRAINT FK_Habilidades_Personajes FOREIGN KEY (personaje_id) 
        REFERENCES Personajes(id) ON DELETE CASCADE
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
        REFERENCES Personajes(id),
    CONSTRAINT FK_Batallas_Combatiente2 FOREIGN KEY (combatiente2_id) 
        REFERENCES Personajes(id),
    CONSTRAINT FK_Batallas_Ganador FOREIGN KEY (ganador_id) 
        REFERENCES Personajes(id)
);

CREATE TABLE HistorialBatallas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    batalla_id INT NOT NULL,
    numero_turno INT NOT NULL,
    atacante_id INT NOT NULL,
    defensor_id INT NOT NULL,
    habilidad_usada VARCHAR(100) NULL,
    dano_causado INT NOT NULL DEFAULT 0,
    fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Historial_Batallas FOREIGN KEY (batalla_id) 
        REFERENCES Batallas(id) ON DELETE CASCADE,
    CONSTRAINT FK_Historial_Atacante FOREIGN KEY (atacante_id) 
        REFERENCES Personajes(id),
    CONSTRAINT FK_Historial_Defensor FOREIGN KEY (defensor_id) 
        REFERENCES Personajes(id)
);