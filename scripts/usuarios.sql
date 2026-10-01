USE codigo_en_combate;

-- 1. Usuario "administrador": acceso a todas las bases de datos del sistema
CREATE USER IF NOT EXISTS 'administrador'@'localhost' IDENTIFIED BY 'adminpass123!';
GRANT ALL PRIVILEGES ON *.* TO 'administrador'@'localhost' WITH GRANT OPTION;

-- 2. Usuario "desarrollador": restringido únicamente a la base de datos del proyecto
CREATE USER IF NOT EXISTS 'desarrollador'@'localhost' IDENTIFIED BY 'devpass123!';
GRANT ALL PRIVILEGES ON codigo_en_combate.* TO 'desarrollador'@'localhost';

-- 3. Aplicar los cambios
FLUSH PRIVILEGES;