# Indagación

Pruebas pequeñas para entender cómo se conectan el modelo orientado a objetos, Minimal API y MySQL, y contraste con lo que imaginé en [`punto-de-partida.md`](punto-de-partida.md). Las pruebas de concepto están en [`exploracion/`](../exploracion) y no dependen del proyecto final.

---

## Actividad 1: Probar antes de construir

### Prueba de concepto

[`exploracion/Program.cs`](../exploracion/Program.cs) es una Minimal API independiente, sin base de datos:

```bash
dotnet run --project exploracion
```

Swagger queda en `http://localhost:5000/swagger`.

| Ruta | Qué prueba | Respuestas |
| :--- | :--- | :--- |
| `GET /exploracion/personaje` | Una ruta que devuelve un objeto fijo | `200`: `{ "id": 1, "nombre": "Thorin", "tipo": "Guerrero" }` |
| `GET /exploracion/personajes/{id}` | Un dato que viaja en la ruta | `200` con `id` 1; `404` con cualquier otro |
| `POST /exploracion/personajes` | Datos que viajan en el cuerpo | `201` con el personaje y el encabezado `Location`; `400` si falta nombre o tipo |
| `GET /exploracion/polimorfismo` | Guerrero y Mago ante la misma acción | `200` con lo que hizo cada uno |

**Cómo se registra una ruta y sus respuestas**: `app.MapGet(ruta, función)` asocia un método HTTP y una ruta con el código que responde. `Results.Ok`, `Results.Created`, `Results.BadRequest` y `Results.NotFound` eligen el código. `.Produces<T>(código)` le dice a Swagger qué devuelve en cada caso y `.WithSummary` agrega la descripción. Así está hecho también [`src/API/Program.cs`](../src/API/Program.cs).

### Guerrero y Mago ante la misma acción

La ruta `/exploracion/polimorfismo` arma una lista de `Luchador` con un Guerrero y un Mago, y llama a `UsarHabilidad` sobre cada uno contra un muñeco de defensa 5. No hay ningún `if` ni `switch` por tipo: cada clase sobrescribe el método abstracto.

| Luchador | Habilidad | Daño | Por qué |
| :--- | :--- | :---: | :--- |
| Guerrero (ataque 18, escudo 20) | Embestida | 33 | Suma el escudo al ataque y le resta la defensa: 18 + 20 − 5. Se queda sin escudo. |
| Mago (ataque 14, maná 60) | Bola de fuego | 28 | El doble del ataque ignorando la defensa: 14 × 2. Gasta 25 de maná. |

**En el proyecto final** es lo mismo, pero más completo:
- `Personaje.UsarHabilidad` es abstracto, y cada clase le delega la acción a su propia `IHabilidad`: `Embestida`, `BolaDeFuego`, `DisparoDoble` y `GolpeSigiloso`.
- `TurnoService` traduce `"atacar"` o `"habilidad"` con un diccionario, y `Batalla` llama a `Atacar` o `UsarHabilidad` sobre el atacante actual.
- El comportamiento lo elige el tipo real del objeto en tiempo de ejecución.
- El único lugar donde se decide qué clase instanciar es `CatalogoPersonajes`, también con un diccionario y no con un `switch`.

### Preguntas

**¿Qué diferencia hay entre devolver 200, 201, 400 y 404?**

| Código | Significa | En el proyecto |
| :---: | :--- | :--- |
| `200 OK` | La solicitud salió bien y se devuelve el resultado | Todos los `GET` y `PUT`, y `POST /api/combates/{id}/turnos` (así lo pide el contrato) |
| `201 Created` | Se creó un recurso nuevo. Devuelve el recurso y su dirección en el encabezado `Location` | `POST /api/personajes` y `POST /api/combates` |
| `400 Bad Request` | Los datos enviados no son válidos o la acción no se puede hacer en ese momento | Nombre vacío, tipo inexistente, mismo personaje dos veces, acción que no es `atacar` ni `habilidad`, fuera de turno, combate finalizado, sin recursos para la habilidad |
| `404 Not Found` | El recurso pedido no existe | Personaje, combate o turno con un ID inexistente, o un atacante que no participa del combate |

Además el proyecto usa `204 No Content` en los `DELETE` (salió bien y no hay nada que devolver) y `409 Conflict` cuando un cambio choca con las restricciones: borrar un personaje que ya peleó, modificar o borrar un combate con turnos, o borrar un turno que no es el último. Los servicios devuelven un `Resultado` con un `TipoError`, y la clase `Respuestas` de `Program.cs` lo traduce a `200/201/204`, `400`, `404` o `409`.

**¿Qué parte de una solicitud corresponde a la ruta y cuál al cuerpo?**

- **Ruta**: identifica *sobre qué recurso* se actúa. En `/api/combates/{id}/turnos/{turnoId}`, `id` y `turnoId` son parámetros de ruta. Minimal API los toma de la URL porque tienen el mismo nombre que los parámetros de la función, y `{id:int}` hace que solo coincida con números.
- **Cuerpo**: lleva *los datos* para crear o modificar, en JSON. En `POST /api/combates/{id}/turnos` el cuerpo es `{ "atacanteId": 3, "accion": "habilidad" }`. Minimal API lo convierte en el objeto `EjecutarTurnoSolicitud` porque es un tipo complejo.
- Los `GET` y `DELETE` no tienen cuerpo: todo lo necesario está en la ruta.

**¿Qué permisos necesita una conexión que solo consulta datos?**

Solo `SELECT` sobre las tablas que consulta. En [`sql/02_usuarios.sql`](../sql/02_usuarios.sql), `combate_invitado` tiene `GRANT SELECT ON combate_api.*`. Al darse sobre la base (`.*`) y no tabla por tabla, alcanza también a las tablas que se creen en el futuro. No tiene `INSERT`, `UPDATE`, `DELETE`, `CREATE` ni `DROP`: si alguien lo intenta, MySQL responde `ERROR 1142 ... command denied`.

En la API, cada conexión es una variable de entorno (`ConnectionStrings__ApiInvitado` y `ConnectionStrings__ApiAdministrador`):
- Los métodos de lectura de los repositorios usan por defecto `TipoConexion.ApiInvitado`, así que todos los `GET` consultan con el invitado.
- Las escrituras usan siempre `TipoConexion.ApiAdministrador`.
- Las lecturas que forman parte de una escritura (por ejemplo, comprobar que el combate existe antes de ejecutar un turno) también usan el administrador, porque pertenecen a esa operación.

**¿Qué datos debe incluir un backup para restaurar el proyecto?**

La estructura y los datos de `combate_api`. El backup [`backup/combate_api.sql`](../backup/combate_api.sql) se genera con `mysqldump --databases combate_api` y contiene:
- `CREATE DATABASE`, para que se pueda restaurar en un servidor vacío.
- Los `CREATE TABLE` de `personajes`, `combates` y `turnos`, con sus claves primarias, claves foráneas, `CHECK` e índices.
- Los `INSERT` con los datos de muestra.

No contiene usuarios ni contraseñas. Los usuarios viven en la base del sistema `mysql`, no en `combate_api`, así que después de restaurar en un servidor nuevo hay que ejecutar `sql/02_usuarios.sql` con contraseñas propias. Tampoco contiene las cadenas de conexión, que están en variables de entorno. `--single-transaction` exporta una foto consistente sin bloquear las tablas, y `--skip-dump-date` y `--set-gtid-purged=OFF` evitan datos propios del servidor que impedirían restaurarlo en otro.

### Correcciones a las hipótesis iniciales

| # | Hipótesis inicial | Qué resultó | Dónde se ve |
| :---: | :--- | :--- | :--- |
| 1 | La vida y el recurso actuales se guardan en `personajes` | **Cambió.** Un personaje puede pelear en varios combates, así que `personajes` guarda solo las estadísticas base. La vida y el recurso de cada participante están en `combates` (`vida_personaje1`, `recurso_personaje1`, etc.). | `sql/01_estructura.sql` |
| 2 | Las habilidades son filas con costo y potencia, y se eligen con `habilidadId` | **Cambió.** La habilidad es comportamiento, no un dato: cada clase tiene su `IHabilidad` (composición) y el cliente solo manda `"accion": "habilidad"`. | `Entidades/Habilidades/`, `IHabilidad.cs` |
| 3 | El cliente indica atacante y defensor | **Cambió.** El defensor es siempre el otro participante (`Batalla.DefensorActual`). El cliente manda solo `atacanteId` y `accion`. | `EjecutarTurnoSolicitud`, `Batalla.cs` |
| 4 | El `POST` de personajes recibe todas las estadísticas | **Cambió.** Recibe solo nombre y tipo. Las estadísticas base de cada tipo salen de `CatalogoPersonajes`, así el cliente no puede crear un personaje con vida 9999. | `CrearPersonajeSolicitud`, `CatalogoPersonajes.cs` |
| 5 | La API devuelve la entidad `Personaje` tal cual | **Cambió.** Recibe y devuelve DTO (`PersonajeRespuesta`, `TurnoRespuesta`...). Las entidades no salen de la capa de aplicación. | `Aplicacion/Dtos/`, `MapeoDtos.cs` |
| 6 | Un `switch (tipo)` decide qué subclase crear | **Cambió.** En 2.3 había un `switch` en `PersonajeRepository`. Ahora un diccionario en `CatalogoPersonajes` asocia cada tipo con cómo crearlo, y el turno usa polimorfismo. | `CatalogoPersonajes.cs`, `TurnoService.cs` |
| 7 | Alcanza con una sola conexión con todos los permisos | **Cambió.** Hay dos usuarios: `combate_invitado` (solo `SELECT`) para los `GET` y `combate_admin` para las escrituras. Si un `GET` tuviera un error, no podría modificar datos. | `sql/02_usuarios.sql`, `TipoConexion.cs` |
| 8 | Atacar fuera de turno, con el combate terminado o derrotado es `409` | **Cambió.** El contrato pide `400` para acciones inválidas o fuera de turno. `409` queda para los `PUT` y `DELETE` que violan una restricción. | `TurnoService.Ejecutar` |
| 9 | Ejecutar un turno devuelve `201` | **Cambió.** El contrato pide `200` con el turno y el estado del combate actualizado, incluido el ganador. | `src/API/Program.cs` |
| 10 | Para deshacer un turno se recalcula el daño | **Cambió.** No se puede recalcular de forma segura: el escudo absorbe parte del daño, el crítico del Asesino es aleatorio y las habilidades gastan recursos. Cada turno guarda la vida y el recurso de ambos *antes* de la acción, y el `DELETE` los copia de vuelta. | `turnos.vida_personaje1_antes`, `TurnoService.EliminarUltimo` |
| 11 | El turno se guarda con un stored procedure, como `sp_ProcesarAccionCombate` | **Cambió.** Las reglas están en las clases del dominio (`Batalla`, `Personaje`) y `TurnoRepository` guarda el turno y actualiza el combate en una transacción de Dapper. Así se pueden probar las reglas sin MySQL. | `TurnoRepository.Registrar` |
| 12 | Con `mysqldump` se restaura todo, usuarios incluidos | **Cambió.** El backup trae estructura y datos, pero no usuarios: hay que ejecutar `02_usuarios.sql` después. | `README.md`, sección Bases de datos |

También se confirmaron estas ideas:
- Un personaje no puede pelear contra sí mismo (`400`).
- Un ID inexistente da `404`.
- Un personaje derrotado no puede actuar.
- La falta de maná o de flechas para la habilidad da `400`.

---

## Actividad 2: DTO entre la API y el servicio

### Qué es un DTO

Un **DTO** (*Data Transfer Object*) es un objeto que solo transporta datos entre dos capas, en este caso entre el cliente y el servicio. No tiene reglas ni comportamiento: define el **contrato**, es decir, qué datos entran y qué datos salen.

| | DTO | Entidad del dominio |
| :--- | :--- | :--- |
| Para qué está | Transportar datos hacia o desde el cliente | Representar el problema y sus reglas |
| Comportamiento | Ninguno | Sí: `Atacar`, `RecibirDanio`, `UsarHabilidad`... |
| Validaciones | No: se validan en el servicio | Sí: en sus propiedades (vida no negativa, nombre no vacío) |
| Forma | La que necesita cada solicitud o respuesta | La que necesita el modelo |
| Si cambia | Cambia el contrato con el cliente | No debería afectar al cliente |

Sirve para:
- Decidir exactamente qué datos acepta cada ruta.
- No mostrar datos internos.
- Poder cambiar las clases del dominio o las tablas sin romper a los clientes.

### Prueba de concepto

- [`exploracion/dto/PersonajeDtos.cs`](../exploracion/dto/PersonajeDtos.cs) define dos DTO:
  - `CrearPersonajeSolicitud(Nombre, Tipo)`, el de entrada.
  - `PersonajeCreadoRespuesta(Id, Nombre)`, el de salida.
- [`exploracion/dto/ServicioPersonajes.cs`](../exploracion/dto/ServicioPersonajes.cs) recibe el primero, arma la entidad `Personaje` con las estadísticas base del tipo, la guarda en una lista en memoria y devuelve el segundo. La entidad es una clase privada del servicio, así que no puede salir de él.

Los dos archivos se compilan con `exploracion/Exploracion.csproj`. Los ejemplos de abajo salen de ejecutar el servicio y serializar con las mismas opciones JSON que usa Minimal API.

### Ejemplo JSON

**Entrada.** El cliente manda de más:

```json
{ "nombre": "Thorin", "tipo": "Guerrero", "vidaMax": 9999, "id": 50 }
```

**Salida:**

```json
{ "id": 1, "nombre": "Thorin" }
```

`vidaMax` e `id` se ignoran porque `CrearPersonajeSolicitud` no los tiene. Si el servicio recibiera la entidad directamente, el cliente podría elegir su propia vida o su ID. Con un tipo que no existe (`"tipo": "Dragon"`), el servicio rechaza la solicitud con *"El tipo debe ser uno de: Guerrero, Mago, Arquero, Asesino."*

### Qué datos no deberían exponerse

- **Datos de control internos**: la fecha de creación, y en el proyecto final la vida y el recurso *antes* de cada turno (`vida_personaje1_antes`...). Solo sirven para deshacer el último turno: están en `TurnoRegistro` pero no en `TurnoRespuesta`.
- **Representaciones internas**: el combate guarda `turno_personaje1` como booleano. La respuesta lo traduce a `turnoDeId`, el ID de quien actúa, que es lo que le sirve al cliente.
- **Comportamiento y estado del objeto**: métodos, listas privadas o referencias a otros objetos. Al serializar una entidad podría salir de más o fallar por referencias circulares.
- **Datos que el cliente no debe poder fijar**: en la entrada no van `Id`, `VidaMax`, `Ataque` ni `Defensa`. Los decide el servidor.
- **Credenciales**: ninguna respuesta debe incluir contraseñas ni cadenas de conexión.

### Dónde se transforman los datos

En el **servicio**, nunca en la ruta ni en el repositorio:
1. **Entrada (DTO → entidad)**: la ruta solo recibe el DTO. El servicio lo valida y arma la entidad, completando lo que el cliente no puede elegir.
2. **Salida (entidad → DTO)**: el servicio arma el DTO de respuesta con los campos que corresponden y lo devuelve. La ruta solo lo convierte en JSON con el código HTTP.

**En el proyecto final**:

| Paso | Prueba de concepto | Proyecto final |
| :--- | :--- | :--- |
| DTO de entrada | `CrearPersonajeSolicitud(Nombre, Tipo)` | `CrearPersonajeSolicitud(Nombre, Tipo)` en `Aplicacion/Dtos/PersonajeDtos.cs` |
| DTO → entidad | `ServicioPersonajes.Crear` | `PersonajeApiService.Crear` valida, y `CatalogoPersonajes.Nuevo` arma el `PersonajeRegistro` con las estadísticas del tipo |
| Entidad → DTO | `new PersonajeCreadoRespuesta(Id, Nombre)` | `MapeoDtos` (clase `internal` de `Aplicacion`) arma `PersonajeRespuesta`, `CombateDetalleRespuesta` y `TurnoRespuesta` |
| DTO de salida | `PersonajeCreadoRespuesta(Id, Nombre)` | `PersonajeRespuesta` con ID, nombre, tipo y estadísticas, sin fecha de creación |

La respuesta del proyecto final tiene más campos que la de la prueba. El contrato pide devolver *"personaje creado y su ID"*, y las estadísticas son información del juego que el cliente necesita para mostrar el personaje.
