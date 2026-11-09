-- =====================================================================
-- 02_usuarios.sql
-- Crea los usuarios de la API sobre la base combate_api.
-- Ejecutar con un usuario que pueda crear usuarios (por ejemplo root),
-- después de 01_estructura.sql.
--
-- IMPORTANTE: las contraseñas son valores de ejemplo.
-- Reemplazarlas antes de ejecutar (ver README.md).
-- =====================================================================

DROP USER IF EXISTS 'combate_admin'@'localhost';
DROP USER IF EXISTS 'combate_invitado'@'localhost';

-- Administrador: todos los permisos sobre combate_api (y solo sobre ella).
CREATE USER 'combate_admin'@'localhost' IDENTIFIED BY 'CAMBIAR_password_admin';
GRANT ALL PRIVILEGES ON combate_api.* TO 'combate_admin'@'localhost';

-- Invitado: solo lectura. Al otorgarse sobre combate_api.* alcanza
-- a todas las tablas presentes y a las que se creen en el futuro.
CREATE USER 'combate_invitado'@'localhost' IDENTIFIED BY 'CAMBIAR_password_invitado';
GRANT SELECT ON combate_api.* TO 'combate_invitado'@'localhost';

FLUSH PRIVILEGES;
