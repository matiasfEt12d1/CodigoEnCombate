# Reflexión final

Comparación entre lo que imaginé en [`punto-de-partida.md`](punto-de-partida.md), antes de programar, y el proyecto terminado. El contraste hipótesis por hipótesis está en [`indagacion.md`](indagacion.md#correcciones-a-las-hipótesis-iniciales); acá me quedo con lo que más cambió, lo que aprendí y lo que mejoraría.

---

## 1. Del modelo inicial al proyecto final

| Área | Punto de partida | Proyecto final |
| :--- | :--- | :--- |
| **Polimorfismo** | Cada clase sobrescribía `Atacar` o `RecibirDanio`, pero la habilidad era un dato (`Habilidad` con costo y potencia) y un `switch (tipo)` elegía qué subclase crear. | `UsarHabilidad` es abstracto y cada clase delega en su propia `IHabilidad` (`Embestida`, `BolaDeFuego`, `DisparoDoble`, `GolpeSigiloso`). `Batalla` llama a `Atacar` o `UsarHabilidad` sobre el atacante actual sin preguntar su tipo. `CatalogoPersonajes` reemplaza al `switch` con un diccionario. |
| **Rutas HTTP** | El cliente mandaba atacante, defensor y `habilidadId`. Ejecutar un turno devolvía `201` y atacar fuera de turno `409`. | El cliente manda solo `atacanteId` y `accion` (`atacar` o `habilidad`); el defensor es el otro participante. El turno devuelve `200` con el estado del combate, `400` es para acciones inválidas y `409` queda para los `PUT` y `DELETE` que violan una restricción. Quince rutas con DTO, documentadas en Swagger. |
| **Permisos** | Un único usuario con todos los permisos, como `desarrollador` en 2.3. | `combate_invitado` (solo `SELECT` sobre `combate_api.*`) para los `GET` y `combate_admin` para las escrituras. Cada repositorio elige la conexión con `TipoConexion`, y las cadenas vienen de variables de entorno. |
| **Persistencia** | La vida y el recurso actuales en `personajes`. El turno se guardaba con el stored procedure `sp_ProcesarAccionCombate`, que también calculaba el daño. | `personajes` guarda solo estadísticas base; la vida y el recurso de cada participante están en `combates`. Las reglas viven en el dominio y `TurnoRepository` solo guarda: turno y combate en una transacción de Dapper. |

---

## 2. Decisiones que cambié y qué aprendí

### 2.1. El estado del combate no es del personaje

- **Antes**: la vida, el maná, el escudo y las flechas actuales se guardaban en `personajes`, como en 2.3.
- **Ahora**: `personajes` tiene solo las estadísticas base. `combates` guarda `vida_personaje1`, `recurso_personaje1`, etc., y `CatalogoPersonajes.CrearEnCombate` arma el personaje del dominio con ese estado antes de cada turno.
- **Qué aprendí**: el modelo de datos tiene que separar lo que *es* una entidad de lo que le *pasa* en un contexto. Mi propia duda inicial ("si un personaje pelea en dos combates a la vez, ¿dónde se guarda su vida?") ya mostraba que el diseño de 2.3 no servía: con la vida en `personajes`, un combate le habría bajado la vida en el otro.

### 2.2. La habilidad es comportamiento, no un registro

- **Antes**: una tabla `habilidades` con costo y potencia, elegida con `habilidadId`, y un `switch (tipo)` para crear la subclase.
- **Ahora**: cada clase tiene su `IHabilidad` con reglas propias (la `Embestida` suma el escudo y lo gasta, la `BolaDeFuego` ignora la defensa y gasta maná). El cliente solo dice `"habilidad"`, y `TurnoService` traduce el texto a una acción con un diccionario.
- **Qué aprendí**: si cada habilidad es solo `potencia × algo`, el polimorfismo no aporta nada y las reglas terminan en un `if` o en la base. Poner el comportamiento en las clases permite agregar un tipo nuevo registrándolo en un solo lugar, sin tocar el turno, y probar las reglas sin MySQL.

### 2.3. Dos usuarios en lugar de uno

- **Antes**: alcanzaba con una conexión con todos los permisos.
- **Ahora**: los `GET` usan `combate_invitado` y las escrituras `combate_admin`. El permiso se da sobre `combate_api.*`, así que también alcanza a tablas futuras. Las contraseñas no están en el repositorio: van en `ConnectionStrings__ApiInvitado` y `ConnectionStrings__ApiAdministrador`.
- **Qué aprendí**: el mínimo privilegio no es solo para usuarios humanos. Si una consulta tiene un error o alguien logra inyectar SQL en un `GET`, la base lo rechaza (`ERROR 1142`) porque esa conexión no puede escribir. También aprendí que las lecturas que forman parte de una escritura conviene hacerlas con el administrador, porque pertenecen a la misma operación.

### 2.4. Deshacer un turno guardando el estado, no recalculándolo

- **Antes**: para borrar un turno se recalculaba el daño y se le devolvía al defensor.
- **Ahora**: cada turno guarda la vida y el recurso de ambos participantes *antes* de la acción (`vida_personaje1_antes`, ...). `DELETE` copia esos valores al combate y le devuelve el turno al atacante, todo en una transacción.
- **Qué aprendí**: no todo cálculo se puede invertir. El escudo absorbe parte del daño, el crítico del Asesino es aleatorio y las habilidades gastan recursos. Guardar una foto del estado es más simple y más seguro que intentar deshacer la cuenta, y además deja la nota como único dato editable sin afectar el daño.

---

## 3. Lo que se mantuvo

- Un personaje no puede pelear contra sí mismo (`400`).
- Un ID inexistente da `404`.
- Un personaje derrotado no puede actuar, y el combate termina cuando uno queda sin vida.
- La falta de maná o flechas para la habilidad da `400`.
- `mysqldump` sirve como backup, aunque sin usuarios: después de restaurar hay que ejecutar `sql/02_usuarios.sql`.

---

## 4. Dificultades y cómo las resolví

| Dificultad | Cómo la resolví |
| :--- | :--- |
| El código de 2.3 creaba personajes con un `switch` por tipo, y la consigna prohíbe `if` o `switch` por clase. | `CatalogoPersonajes` registra cada tipo una sola vez en un diccionario: cómo crearlo, cómo leer y escribir su recurso. |
| El daño del turno se medía mal contra el Guerrero, porque el escudo absorbía parte y la vida no bajaba. | El daño registrado es la vida perdida más el recurso perdido al defender. En el dominio, `VidaEfectiva` del Guerrero es vida más escudo. |
| Dos instancias del mismo personaje leído de la base no se reconocían como el mismo, y se podía armar un combate contra sí mismo. | Sobrecarga de `==` en `Personaje`: son el mismo si son el mismo objeto o si ya tienen el mismo `Id`. |
| Dos solicitudes de turno al mismo tiempo podían pisarse. | El `UPDATE` del combate exige el `numero_turno` esperado. Si otro turno ya lo cambió, no actualiza nada y la transacción se revierte. |
| Las reglas estaban mezcladas con el acceso a datos (stored procedures de 2.3). | Separé la resolución del turno en el dominio y dejé los repositorios solo para leer y guardar. Así los tests usan repositorios falsos en memoria. |

---

## 5. Mejora propuesta: autenticación y roles en la API

Hoy los permisos diferenciados protegen la **base**, pero no la **API**: cualquiera que llegue a la API puede llamar a un `DELETE`, y la API lo ejecuta con la conexión del administrador. El invitado solo limita lo que hace cada ruta, no quién puede llamarla.

Si siguiera desarrollando la API, agregaría autenticación con roles que reflejen los usuarios de la base:

- **Autenticación con JWT**: `AddAuthentication().AddJwtBearer(...)` en `Program.cs`, con la clave de firma en una variable de entorno, como las cadenas de conexión.
- **Dos políticas**: `Lector` para los `GET` y `Administrador` para `POST`, `PUT` y `DELETE`, aplicadas por grupo con `.RequireAuthorization("Administrador")` en las rutas de escritura.
- **Respuestas nuevas**: `401` sin token y `403` con un token sin el rol necesario, documentadas con `.Produces(401)` y `.Produces(403)`.
- **Swagger**: `AddSecurityDefinition` para poder cargar el token desde el botón *Authorize* y seguir probando todo desde ahí.
- **Tests**: verificar que una solicitud con rol `Lector` recibe `403` en cada escritura.

Así la separación entre invitado y administrador existiría en las dos capas: la API decide quién puede pedir una escritura, y la base sigue impidiendo que una lectura escriba.
