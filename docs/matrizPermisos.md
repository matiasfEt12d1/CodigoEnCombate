# Matriz de Permisos — Código en Combate

> Documento que describe los permisos de los roles **Usuario** (jugador / usuario estándar de la aplicación) y **Administrador** (administrador del sistema y DBA) dentro del proyecto *Código en Combate*, detallando tanto las operaciones generales como el control de acceso granular por **atributo (campo)** para cada entidad.

---

## 1. Definición de Roles

| Rol | Usuario MySQL Sugerido | Ámbito de Acceso | Principio / Descripción |
|:---|:---|:---|:---|
| **Usuario** | `usuario@localhost` | Base de datos `codigo_en_combate` (restringido a DML operativo y ejecución) | Aplica el **Principio de Menor Privilegio (PoLP)**. Diseñado para interactuar con la aplicación / simulador de combate. Puede consultar datos, crear personajes, iniciar batallas, ejecutar turnos y consultar estadísticas. **No posee permisos DDL**, no puede eliminar registros críticos ni alterar atributos base o historiales. |
| **Administrador** | `administrador@localhost` | Servidor completo (`*.*`) con `WITH GRANT OPTION` | Control total sobre el motor MySQL, la base de datos `codigo_en_combate`, gestión de cuentas, configuración de privilegios, mantenimiento de integridad, balanceo manual de stats y depuración de datos. |

---

## 2. Permisos DDL (Estructura de la Base de Datos)

| Operación DDL | Usuario | Administrador | Justificación / Contexto |
|:---|:---:|:---:|:---|
| Crear base de datos `codigo_en_combate` | ❌ | ✅ | Solo el DBA inicializa el entorno. |
| Eliminar base de datos `codigo_en_combate` (`DROP DATABASE`) | ❌ | ✅ | Previene destrucción total accidental o maliciosa. |
| Crear tablas (`CREATE TABLE`) | ❌ | ✅ | La estructura relacional es administrada exclusivamente por el Administrador. |
| Modificar tablas (`ALTER TABLE`) | ❌ | ✅ | Los cambios de esquema impactan en la aplicación y requieren control DBA. |
| Eliminar tablas (`DROP TABLE`) | ❌ | ✅ | Acción destructiva irreversible. |
| Vaciar tablas (`TRUNCATE TABLE`) | ❌ | ✅ | Acción destructiva que reinicia secuencias autonuméricas. |
| Crear / Modificar Stored Procedures | ❌ | ✅ | La lógica de base de datos está congelada para el usuario final. |
| Eliminar Stored Procedures (`DROP PROCEDURE`) | ❌ | ✅ | Riesgo de indisponibilidad de la lógica de combate. |
| Crear / Eliminar índices (`INDEX`) | ❌ | ✅ | Tarea de optimización y tuning a cargo del Administrador. |

---

## 3. Permisos DML Generales por Tabla (Nivel Físico)

Nivel de privilegios SQL concedidos directamente sobre los objetos de base de datos:

| Tabla | `SELECT` (Leer) | `INSERT` (Crear) | `UPDATE` (Actualizar) | `DELETE` (Eliminar) |
|:---|:---:|:---:|:---:|:---:|
| **Personajes** | **Usuario:** ✅<br>**Admin:** ✅ | **Usuario:** ✅<br>**Admin:** ✅ | **Usuario:** ✅ *(restringido)*<br>**Admin:** ✅ *(total)* | **Usuario:** ❌<br>**Admin:** ✅ |
| **Habilidades** | **Usuario:** ✅<br>**Admin:** ✅ | **Usuario:** ✅<br>**Admin:** ✅ | **Usuario:** ❌<br>**Admin:** ✅ | **Usuario:** ❌<br>**Admin:** ✅ |
| **Batallas** | **Usuario:** ✅<br>**Admin:** ✅ | **Usuario:** ✅<br>**Admin:** ✅ | **Usuario:** ✅ *(restringido)*<br>**Admin:** ✅ *(total)* | **Usuario:** ❌<br>**Admin:** ✅ |
| **HistorialBatallas** | **Usuario:** ✅<br>**Admin:** ✅ | **Usuario:** ✅<br>**Admin:** ✅ | **Usuario:** ❌<br>**Admin:** ✅ | **Usuario:** ❌<br>**Admin:** ✅ |


> Para el **Usuario**, los comandos `INSERT` y `UPDATE` en tablas como `Batallas` y `HistorialBatallas` se realizan a través de los **Stored Procedures** (`sp_RegistrarBatalla`, `sp_ProcesarAccionCombate`, `sp_FinalizarBatalla`), impidiendo la inserción arbitraria de datos inconsistentes. Además, el Usuario tiene bloqueado el comando `DELETE` en todas las tablas para preservar la integridad referencial y el histórico de auditoría.

---

## 4. Matriz Granular de Permisos por Atributo (CRUD a Nivel de Campo)

A continuación se detalla el análisis en profundidad de cada una de las entidades del proyecto (`Personajes`, `Habilidades`, `Batallas` e `HistorialBatallas`), especificando qué atributos específicos puede **Leer (C)**, **Crear (R)**, **Actualizar (U)** o **Eliminar (D)** cada rol:

### 4.1. Entidad: `Personajes`
Representa a los combatientes base (`Personaje`) y sus clases especializadas: **Guerrero**, **Mago**, **Arquero** y **Asesino**.

| Atributo / Campo | Tipo de Dato | Rol | Leer (R) | Crear (C) | Actualizar (U) | Eliminar (D) | Detalle y Restricciones de Negocio |
|:---|:---|:---|:---:|:---:|:---:|:---:|:---|
| **`id`** | `INT (PK)` | **Usuario**<br>**Admin** | ✅<br>✅ | ❌<br>❌ | ❌<br>❌ | ❌<br>✅ | Clave primaria autonumérica generada por el motor MySQL. Inmutable para ambos. El Admin puede eliminar el registro completo. |
| **`nombre`** | `VARCHAR(100)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | El Usuario lo define al crear el personaje; una vez registrado no puede cambiarlo (identificador único de negocio). El Admin puede renombrarlo para moderación o corrección. |
| **`tipo`** | `VARCHAR(20)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Clase del personaje (`Guerrero`, `Mago`, `Arquero`, `Asesino`). El Usuario lo selecciona en la creación. Es inmutable para el Usuario. |
| **`vida_max`** | `INT` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Salud tope del personaje. Inmutable en partida para el Usuario. El Admin puede modificarlo para balance de clases. |
| **`vida`** | `INT` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | Salud actual. Se inicializa en `vida_max`. El Usuario la modifica dinámicamente mediante el combate (daño recibido) gestionado por `sp_ProcesarAccionCombate` y `sp_FinalizarBatalla`. El Admin puede resetearla arbitrariamente. |
| **`ataque`** | `INT` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Estadística base de daño físico. Inmutable para el Usuario. El Admin puede ajustarla por balance. |
| **`defensa`** | `INT` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Mitigación de daño recibida. Inmutable para el Usuario. El Admin puede modificarla. |
| **`escudo`** *(Guerrero)* | `INT (NULL)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | Recurso defensivo del Guerrero. El Usuario lo inicializa al crearlo y se actualiza (consume) dinámicamente en combate al absorber golpes. El Admin puede restaurarlo manualmente. |
| **`mana_max`** *(Mago)* | `INT (NULL)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Límite de maná para el Mago. Inmutable para el Usuario. El Admin puede alterar el valor. |
| **`mana`** *(Mago)* | `INT (NULL)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | Maná actual. El Usuario lo inicializa y lo consume (10 pts por ataque mágico) a través de la lógica de combate. El Admin puede recargarlo o modificarlo. |
| **`cantidad_flechas`** *(Arquero)* | `INT (NULL)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | Munición del Arquero. El Usuario lo inicializa y descuenta 1 unidad por ataque en combate. El Admin puede reabastecer flechas libremente. |
| **`probabilidad_critico`** *(Asesino)* | `DECIMAL(3,2)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Tasa de impacto crítico (0.00 a 1.00). Inmutable para el Usuario. El Admin puede aumentarla o disminuirla para balancear el juego. |

---

### 4.2. Entidad: `Habilidades`
Representa los poderes, hechizos o técnicas especiales asignadas a cada combatiente.

| Atributo / Campo | Tipo de Dato | Rol | Leer (R) | Crear (C) | Actualizar (U) | Eliminar (D) | Detalle y Restricciones de Negocio |
|:---|:---|:---|:---:|:---:|:---:|:---:|:---|
| **`id`** | `INT (PK)` | **Usuario**<br>**Admin** | ✅<br>✅ | ❌<br>❌ | ❌<br>❌ | ❌<br>✅ | Identificador autonumérico autogenerado por la BD. |
| **`personaje_id`** | `INT (FK)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Clave foránea referenciando a `Personajes(id)`. El Usuario la asocia a su personaje al registrar la habilidad. No puede reasignarla a otros personajes. |
| **`nombre`** | `VARCHAR(100)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Nombre descriptivo de la habilidad. Definido en la creación. El Usuario no puede modificarlo luego de creado. |
| **`costo_recurso`** | `INT` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Cantidad de maná/energía requerida. El Usuario lo define según reglas preestablecidas al crear; inmutable posteriormente. El Admin puede alterarlo para balance. |
| **`potencia`** | `INT` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Daño o poder base que inflige la habilidad. Inmutable para el Usuario una vez creada. El Admin puede calibrarla. |

---

### 4.3. Entidad: `Batallas`
Representa los enfrentamientos 1 vs 1 entre combatientes y el estado global de la contienda.

| Atributo / Campo | Tipo de Dato | Rol | Leer (R) | Crear (C) | Actualizar (U) | Eliminar (D) | Detalle y Restricciones de Negocio |
|:---|:---|:---|:---:|:---:|:---:|:---:|:---|
| **`id`** | `INT (PK)` | **Usuario**<br>**Admin** | ✅<br>✅ | ❌<br>❌ | ❌<br>❌ | ❌<br>✅ | Clave primaria autonumérica generada por `sp_RegistrarBatalla`. |
| **`combatiente1_id`** | `INT (FK)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | ID del primer personaje. El Usuario lo selecciona para iniciar la batalla. No puede cambiarse a mitad de una batalla en curso. |
| **`combatiente2_id`** | `INT (FK)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | ID del segundo personaje. El Usuario lo selecciona al crear el combate. Inmutable durante la batalla. |
| **`ganador_id`** | `INT (FK, NULL)` | **Usuario**<br>**Admin** | ✅<br>✅ | ❌<br>✅ | ✅<br>✅ | ❌<br>✅ | Inicialmente `NULL`. El Usuario lo actualiza de forma indirecta al finalizar el combate mediante el SP `sp_FinalizarBatalla`. El Admin puede fijar, cambiar o revocar ganadores a mano. |
| **`numero_turno`** | `INT` | **Usuario**<br>**Admin** | ✅<br>✅ | ❌<br>✅ | ✅<br>✅ | ❌<br>✅ | Se inicializa por defecto en `1`. El Usuario lo incrementa turno a turno mediante la ejecución de combate (`sp_ProcesarAccionCombate` y `BatallaRepository.Actualizar`). El Admin puede alterar el turno directamente. |
| **`es_finalizada`** | `BOOLEAN` | **Usuario**<br>**Admin** | ✅<br>✅ | ❌<br>✅ | ✅<br>✅ | ❌<br>✅ | Inicia en `FALSE`. Pasa a `TRUE` cuando la vida de un personaje llega a 0 (mediante `sp_FinalizarBatalla`). El Admin puede reactivar (`FALSE`) o cerrar (`TRUE`) cualquier batalla. |
| **`fecha_inicio`** | `DATETIME` | **Usuario**<br>**Admin** | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | ❌<br>✅ | Marca de tiempo fijada automáticamente por el servidor (`CURRENT_TIMESTAMP`). Es inmutable para el Usuario para garantizar auditoría cronológica. |

---

### 4.4. Entidad: `HistorialBatallas`
Representa el registro inmutable de turnos, acciones de ataque y daño producido en cada combate (Log de auditoría de combate).

| Atributo / Campo | Tipo de Dato | Rol | Leer (R) | Crear (C) | Actualizar (U) | Eliminar (D) | Detalle y Restricciones de Negocio |
|:---|:---|:---|:---:|:---:|:---:|:---:|:---|
| **`id`** | `INT (PK)` | **Usuario**<br>**Admin** | ✅<br>✅ | ❌<br>❌ | ❌<br>❌ | ❌<br>✅ | Identificador autonumérico autoincremental de cada evento. |
| **`batalla_id`** | `INT (FK)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Batalla a la que corresponde el turno. Insertado mediante `sp_ProcesarAccionCombate`. Inmutable para el Usuario. |
| **`numero_turno`** | `INT` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Turno en el que ocurrió el golpe. Generado en cada turno; inmutable una vez registrado. |
| **`atacante_id`** | `INT (FK)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | ID del personaje que ejecutó el ataque. Provisto por el SP en tiempo de combate; inmutable. |
| **`defensor_id`** | `INT (FK)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | ID del personaje que recibió la acción. Provisto por el SP; inmutable. |
| **`habilidad_usada`** | `VARCHAR(100)` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Nombre de la habilidad o "Ataque Básico". Registrado al momento del impacto; inmutable. |
| **`dano_causado`** | `INT` | **Usuario**<br>**Admin** | ✅<br>✅ | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | Valor del daño real asestado (calculado tras considerar defensas y escudos). Registrado por el SP; inmutable. |
| **`fecha_registro`** | `DATETIME` | **Usuario**<br>**Admin** | ✅<br>✅ | ❌<br>✅ | ❌<br>✅ | ❌<br>✅ | Marca temporal automática fijada con `NOW()`. Inmutable para el Usuario. El Admin puede depurar o corregir logs. |

---

## 5. Ejecución de Stored Procedures

Ambos roles tienen permiso de **ejecución (`EXECUTE`)**, pero su propósito y facultades sobre los procedimientos difieren:
- El **Usuario** los ejecuta como parte del flujo normal de la aplicación (iniciar peleas, aplicar turnos, consultar rankings).
- El **Administrador** además de ejecutarlos, puede modificarlos (`ALTER`), eliminarlos (`DROP`) o recrearlos.

| Stored Procedure | Descripción | Usuario | Administrador | Atributos Modificados / Consultados |
|:---|:---|:---:|:---:|:---|
| `sp_RegistrarBatalla` | Crea una nueva batalla entre dos combatientes | ✅ | ✅ | Inserta en `Batallas` (`combatiente1_id`, `combatiente2_id`, `numero_turno=1`, `es_finalizada=FALSE`, `fecha_inicio=NOW()`). |
| `sp_FinalizarBatalla` | Cierra la batalla, registra el ganador y actualiza la vida final | ✅ | ✅ | Actualiza `Batallas` (`ganador_id`, `numero_turno`, `es_finalizada=TRUE`) y `Personajes` (`vida`). |
| `sp_ProcesarAccionCombate` | Registra el turno en el historial y actualiza vida del defensor y turno | ✅ | ✅ | Inserta en `HistorialBatallas`, actualiza `Personajes` (`vida`) y actualiza `Batallas` (`numero_turno`). |
| `sp_ObtenerRankingPorTipoPersonaje` | Ranking de tipos de personaje por victorias y daño promedio | ✅ | ✅ | Lee de `Personajes`, `Batallas` e `HistorialBatallas`. |
| `sp_GenerarReporteBatallasDetallado` | Reporte detallado de batallas en un rango de fechas | ✅ | ✅ | Lee de `Batallas` y `Personajes`. |
| `sp_ResumenBatallasPorPeriodoYTipo` | Resumen de batallas finalizadas y activas por clase | ✅ | ✅ | Lee de `Personajes` y `Batallas`. |
| `sp_VictoriasYDerrotasPorTipoYPeriodo` | Victorias, derrotas y empates agrupados por tipo | ✅ | ✅ | Lee de `Personajes` y `Batallas`. |
| `sp_RankingPersonajesEfectividad` | Ranking individual por porcentaje de efectividad | ✅ | ✅ | Lee de `Personajes` y `Batallas`. |
| `sp_DanoPromedioPorClaseYHabilidad` | Daño promedio y máximo por clase y habilidad | ✅ | ✅ | Lee de `HistorialBatallas` y `Personajes`. |
| `sp_DanoTotalRealizadoYRecibidoPorPeriodo` | Daño total realizado y recibido por personaje en un período | ✅ | ✅ | Lee de `Personajes` e `HistorialBatallas`. |

---

## 6. Operaciones desde la Capa de Aplicación (C# / Dapper)

Mapeo entre las operaciones expuestas en los repositorios de .NET y los permisos permitidos a cada rol:

| Repositorio | Método C# | Operación SQL Subyacente | Usuario | Administrador | Observaciones |
|:---|:---|:---|:---:|:---:|:---|
| **PersonajeRepository** | `ObtenerTodos()` | `SELECT id FROM Personajes` | ✅ | ✅ | Lectura pública de combatientes. |
| | `ObtenerPorId(id)` | `SELECT ... FROM Personajes WHERE id = @Id` | ✅ | ✅ | Consulta ficha de personaje y habilidades. |
| | `ObtenerPorNombre(nombre)` | `SELECT id FROM Personajes WHERE nombre = @Nombre` | ✅ | ✅ | Búsqueda por nombre de combatiente. |
| | `ObtenerPorTipo(tipo)` | `SELECT id FROM Personajes WHERE tipo = @Tipo` | ✅ | ✅ | Filtrado por clase. |
| | `Agregar(personaje)` | `INSERT INTO Personajes (...)` | ✅ | ✅ | Creación de nuevos personajes por parte de los jugadores o el admin. |
| | `Actualizar(personaje)` | `UPDATE Personajes SET vida, escudo, mana, cantidad_flechas...` | ✅ | ✅ | El Usuario lo ejecuta a través de la lógica de combate; el Admin puede actualizar cualquier stat. |
| | `Eliminar(id)` | `DELETE FROM Personajes WHERE id = @Id` | ❌ | ✅ | **Bloqueado para el Usuario** para evitar romper integridad referencial con batallas existentes. |
| | `AgregarHabilidad(pId, hab)` | `INSERT INTO Habilidades (...)` | ✅ | ✅ | Asignación de habilidades al personaje. |
| **BatallaRepository** | `ObtenerPorId(id)` | `SELECT ... FROM Batallas WHERE id = @Id` | ✅ | ✅ | Consulta de estado de combate. |
| | `ObtenerTodos()` | `SELECT id FROM Batallas` | ✅ | ✅ | Listado general de batallas. |
| | `ObtenerBatallasActivas()` | `SELECT id FROM Batallas WHERE es_finalizada = 0` | ✅ | ✅ | Consulta de combates en curso. |
| | `Agregar(batalla)` | `CALL sp_RegistrarBatalla(...)` | ✅ | ✅ | Inicio de un nuevo enfrentamiento. |
| | `Actualizar(batalla)` | `UPDATE Batallas SET numero_turno, es_finalizada WHERE id = @Id` | ✅ | ✅ | Sincronización del estado del combate. |
| | `Eliminar(id)` | `DELETE FROM Batallas WHERE id = @Id` | ❌ | ✅ | **Bloqueado para el Usuario**. Solo el Admin puede purgar historiales. |
| | `FinalizarBatalla(...)` | `CALL sp_FinalizarBatalla(...)` | ✅ | ✅ | Cierre oficial de batalla y cómputo de ganador. |
| | `ProcesarAccionCombate(...)` | `CALL sp_ProcesarAccionCombate(...)` | ✅ | ✅ | Ejecución del turno y aplicación de daño. |
| **EstadisticasRepository** | *(Todos los métodos de lectura y reportes)* | `CALL sp_...` / `SELECT COUNT(*)...` | ✅ | ✅ | Acceso a métricas, rankings y tableros estadísticos. |

---

## 7. Permisos Administrativos del Servidor MySQL

Operaciones a nivel de servidor MySQL (`*.*`):

| Operación a Nivel de Servidor | Usuario | Administrador | Justificación |
|:---|:---:|:---:|:---|
| Crear bases de datos nuevas | ❌ | ✅ | Privilegio global exclusivo de DBA. |
| Eliminar bases de datos ajenas o del sistema | ❌ | ✅ | Protección contra eliminación accidental o sabotaje. |
| Crear usuarios MySQL (`CREATE USER`) | ❌ | ✅ | Gestión de credenciales y accesos. |
| Modificar usuarios MySQL (`ALTER USER`) | ❌ | ✅ | Gestión de contraseñas y expiración. |
| Eliminar usuarios MySQL (`DROP USER`) | ❌ | ✅ | Revocación total de acceso. |
| Otorgar privilegios (`GRANT`) | ❌ | ✅ | Solo el Admin posee `WITH GRANT OPTION`. |
| Revocar privilegios (`REVOKE`) | ❌ | ✅ | Control de seguridad. |
| Ejecutar `FLUSH PRIVILEGES` | ❌ | ✅ | Recarga de tablas de permisos en memoria. |
| Acceder a otras bases de datos del servidor | ❌ | ✅ | Aislamiento entre entornos y aplicaciones. |
| Ver procesos de otros usuarios (`SHOW PROCESSLIST`) | ❌ | ✅ | Auditoría y control de bloqueos / concurrencia. |
| Administrar variables globales del servidor (`SET GLOBAL`) | ❌ | ✅ | Configuración del motor InnoDB, buffers y timeouts. |
| Apagar el servidor MySQL (`SHUTDOWN`) | ❌ | ✅ | Control operativo de infraestructura. |

---

## 8. Resumen Comparativo de Arquitectura

```
========================================================================================
ADMINISTRADOR (DBA / Superusuario)
========================================================================================
• Ámbito: *.* (Nivel Servidor y todas las bases de datos) con GRANT OPTION.
• DDL: Control total (CREATE, ALTER, DROP, TRUNCATE sobre BD, tablas, SPs e índices).
• DML: CRUD completo sin restricciones en todas las tablas y todos los atributos.
• Operaciones: Purgar historiales, eliminar registros con dependencias, recalibrar estadísticas
               base de personajes (vida_max, ataque, defensa, probabilidad_critico),
               gestionar usuarios y optimizar el motor MySQL.

========================================================================================
USUARIO (Jugador / Aplicación Cliente)
========================================================================================
• Ámbito: codigo_en_combate.* (Restringido a la base de datos de la aplicación).
• DDL: Ninguno (❌ Bloqueado para cualquier modificación estructural).
• DML: 
    - Leer (SELECT): Todos los atributos de Personajes, Habilidades, Batallas e Historial.
    - Crear (INSERT): Nuevos Personajes, Habilidades y Batallas (a través de lógica controlada).
    - Actualizar (UPDATE): Limitado estrictamente a los atributos dinámicos de combate:
         * Personajes: vida, escudo, mana, cantidad_flechas.
         * Batallas: numero_turno, es_finalizada, ganador_id (mediante SPs).
         * Habilidades e HistorialBatallas: Inmutables (❌ No actualizables).
    - Eliminar (DELETE): ❌ Prohibido en todas las tablas para garantizar la integridad
                         referencial y la persistencia de auditoría histórica.
• SPs: Ejecución permitida de todos los procedimientos para jugar y ver reportes.
========================================================================================
```
