USE codigo_en_combate;

-- IMPORTANTE: las contraseñas son valores de ejemplo.
-- Reemplazarlas antes de ejecutar y no subir al repositorio el script con las contraseñas reales.

-- 1. Usuario "administrador": acceso a todas las bases de datos del sistema
CREATE USER IF NOT EXISTS 'administrador'@'localhost' IDENTIFIED BY 'CAMBIAR_password_administrador';
GRANT ALL PRIVILEGES ON *.* TO 'administrador'@'localhost' WITH GRANT OPTION;

-- 2. Usuario "desarrollador": restringido únicamente a la base de datos del proyecto
CREATE USER IF NOT EXISTS 'desarrollador'@'localhost' IDENTIFIED BY 'CAMBIAR_password_desarrollador';
GRANT ALL PRIVILEGES ON codigo_en_combate.* TO 'desarrollador'@'localhost';

-- 3. Aplicar los cambios
FLUSH PRIVILEGES;
