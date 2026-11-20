-- =====================================================================
-- install.sql
-- Agrega el simulador a la base combate_api ejecutando los scripts de
-- la carpeta scripts/ en orden. Antes hay que ejecutar sql/01_estructura.sql.
-- Ejecutar desde la raíz del repositorio con un usuario con permisos
-- de creación (por ejemplo root):
--   mysql -u root -p -e "source scripts/install.sql"
-- =====================================================================

SOURCE scripts/ddl.sql;
SOURCE scripts/sp.sql;
SOURCE scripts/estadisticas.sql;
SOURCE scripts/usuarios.sql;
