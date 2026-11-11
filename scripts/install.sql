-- =====================================================================
-- install.sql
-- Despliega la base codigo_en_combate ejecutando los scripts de la
-- carpeta scripts/ en orden.
-- Ejecutar desde la raíz del repositorio con un usuario con permisos
-- de creación (por ejemplo root):
--   mysql -u root -p -e "source scripts/install.sql"
-- =====================================================================

SOURCE scripts/ddl.sql;
SOURCE scripts/sp.sql;
SOURCE scripts/estadisticas.sql;
SOURCE scripts/usuarios.sql;
