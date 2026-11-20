<h1 align="center">E.T. Nº12 D.E. 1º "Libertador Gral. José de San Martín"</h1>

<p align="center">
  <img src="https://et12.edu.ar/imgs/et12.svg" alt="ET12 Logo" width="240">
</p>

# Código en combate

Simulador de batallas por turnos desarrollado en **.NET 10 con C#**, aplicando arquitectura por capas, principios de Programación Orientada a Objetos (Herencia, Polimorfismo, Encapsulamiento) y persistencia en **MySQL** utilizando **Dapper** y **MySqlConnector**. Incluye una Web API con Minimal API y Swagger para administrar personajes, combates y turnos.

Hecho mediante los planes de aprendizaje nivel 2 *Código en combate* (Software 2.3, 3er bimestre) y *Combate por API* (Software 2.4, 4to bimestre), de la especialidad "Computación", cursando 5to año en una escuela técnica de la Ciudad Autónoma de Buenos Aires, Argentina.

![C#](https://img.shields.io/badge/Language-C%23-blue)
![.NET](https://img.shields.io/badge/Framework-.NET%2010.0-purple)
![MySQL](https://img.shields.io/badge/Database-MySQL%208.0-violet)
![Minimal API](https://img.shields.io/badge/Web%20API-Minimal%20API-blue)
![Swagger](https://img.shields.io/badge/Docs-Swagger-green)
![Dapper](https://img.shields.io/badge/ORM-Dapper-orange)
![xUnit](https://img.shields.io/badge/Tests-xUnit-red)

---

## Estructura

```text
CodigoEnCombate/
├── backup/combate_api.sql   Backup de la base de la API
├── docs/                    Matriz de permisos
├── scripts/                 Tablas, procedimientos y usuarios del simulador (en combate_api)
├── sql/                     Base combate_api: estructura, usuarios de la API y datos de muestra
└── src/
    ├── API/                 Programa principal: rutas, Swagger e inyección de dependencias
    ├── Aplicacion/          Servicios, interfaces y DTO
    ├── Persistencia/        Dominio, conexiones y repositorios
    ├── Tests/               Pruebas con xUnit (sin MySQL)
    └── Proyecto.sln
```

Dependencias: `API -> Aplicacion, Persistencia`, `Aplicacion -> Persistencia` y `Tests -> Aplicacion, Persistencia`.

---

## Instalación

Requisitos: **.NET SDK 10.0** y **MySQL 8.0**.

```bash
git clone https://github.com/matiasfEt12d1/CodigoEnCombate.git
cd CodigoEnCombate
dotnet restore src/Proyecto.sln
```

---

## Base de datos `combate_api`

La API y el simulador del bimestre anterior comparten una sola base, `combate_api`, y la misma tabla `personajes`. El simulador agrega sus tablas (`Habilidades`, `Batallas` e `HistorialBatallas`) y sus stored procedures sobre ella.

Los scripts usan contraseñas de ejemplo. **Antes de ejecutarlos**, reemplazar los valores `CAMBIAR_...` en `sql/02_usuarios.sql` y `scripts/usuarios.sql`, y no subir las contraseñas reales al repositorio. Todos los comandos se ejecutan desde la raíz del repositorio, en este orden:

```bash
mysql -u root -p -e "source sql/01_estructura.sql"
mysql -u root -p -e "source sql/02_usuarios.sql"
mysql -u root -p -e "source sql/03_datos_muestra.sql"
mysql -u root -p -e "source scripts/install.sql"
```

1. **Estructura**: crea la base con las tablas `personajes`, `combates` y `turnos`. Borra la base si ya existía, así que después hay que volver a ejecutar el paso 4.
2. **Usuarios de la API**: `combate_admin` con todos los permisos sobre la base y `combate_invitado` con solo `SELECT`.
3. **Datos de muestra**: 5 personajes, 3 combates y 13 turnos.
4. **Simulador**: ejecuta en orden `ddl` (tablas), `sp` (procedimientos de batalla), `estadisticas` (procedimientos de reportes) y `usuarios` (`administrador` con todos los permisos del servidor y `desarrollador` con todos los permisos sobre `combate_api`). Cada uno también puede ejecutarse solo, por ejemplo `mysql -u root -p -e "source scripts/sp.sql"`.

La comparación entre el usuario que juega y el administrador del simulador está en [`docs/matrizPermisos.md`](docs/matrizPermisos.md).

**Backup** (estructura, datos y stored procedures, sin usuarios ni contraseñas):

```bash
mysqldump -u root -p --databases combate_api --routines --single-transaction --skip-dump-date --set-gtid-purged=OFF --result-file=backup/combate_api.sql
```

**Restaurar** (en un servidor nuevo, ejecutar después `sql/02_usuarios.sql` y `scripts/usuarios.sql`):

```bash
mysql -u root -p -e "source backup/combate_api.sql"
```

**Permisos del invitado**: el primer comando lista los personajes; los otros dos fallan con `ERROR 1142 ... command denied` y no cambian nada.

```bash
mysql -u combate_invitado -p combate_api -e "SELECT id, nombre FROM personajes;"
mysql -u combate_invitado -p combate_api -e "INSERT INTO personajes (nombre, tipo, vida_max, ataque, defensa) VALUES ('Intruso', 'Guerrero', 100, 10, 5);"
mysql -u combate_invitado -p combate_api -e "UPDATE turnos SET nota = 'cambio' WHERE id = 1;"
```

---

## Variables de entorno

Las contraseñas no se guardan en el repositorio: cada conexión se define con una variable de entorno.

| Variable | Base | Usuario | Uso |
| :--- | :--- | :--- | :--- |
| `ConnectionStrings__ApiInvitado` | `combate_api` | `combate_invitado` | Consultas `GET` de la API |
| `ConnectionStrings__ApiAdministrador` | `combate_api` | `combate_admin` | Escrituras de la API |
| `ConnectionStrings__Desarrollo` | `combate_api` | `desarrollador` | Servicios del simulador |
| `ConnectionStrings__Administrador` | `combate_api` | `administrador` | Tareas con privilegios globales |

Ejemplo en PowerShell (en Bash: `export NOMBRE="valor"`):

```powershell
$env:ConnectionStrings__ApiInvitado = "Server=localhost;Database=combate_api;Uid=combate_invitado;Pwd=TU_PASSWORD;"
$env:ConnectionStrings__ApiAdministrador = "Server=localhost;Database=combate_api;Uid=combate_admin;Pwd=TU_PASSWORD;"
$env:ConnectionStrings__Desarrollo = "Server=localhost;Database=combate_api;Uid=desarrollador;Pwd=TU_PASSWORD;"
$env:ConnectionStrings__Administrador = "Server=localhost;Database=combate_api;Uid=administrador;Pwd=TU_PASSWORD;"
```

---

## Compilar, probar e iniciar

```bash
dotnet build src/Proyecto.sln
dotnet test src/Proyecto.sln
dotnet run --project src/API/API.csproj
```

Las pruebas no necesitan MySQL. La API queda en `http://localhost:5080` y Swagger en **`http://localhost:5080/swagger`**.

---

## Ejemplos de las quince rutas

En orden, con los datos de muestra recién cargados. En Swagger: **Try it out**, completar y **Execute**.

| # | Solicitud | Cuerpo | Respuesta |
| :---: | :--- | :--- | :--- |
| 1 | `GET /api/personajes` | | `200`: los 5 personajes |
| 2 | `GET /api/personajes/1` | | `200`: Thorin |
| 3 | `GET /api/personajes/1/combates` | | `200`: el combate 1 (`[]` con el personaje 5) |
| 4 | `POST /api/personajes` | `{ "nombre": "Aragorn", "tipo": "Guerrero" }` | `201`: personaje con `id` 6 |
| 5 | `PUT /api/personajes/6` | `{ "nombre": "Aragorn II" }` | `200`: nombre actualizado |
| 6 | `DELETE /api/personajes/6` | | `204` |
| 7 | `GET /api/combates` | | `200`: los 3 combates |
| 8 | `POST /api/combates` | `{ "personaje1Id": 4, "personaje2Id": 5 }` | `201`: combate con `id` 4 |
| 9 | `GET /api/combates/4` | | `200`: participantes y `turnos: []` |
| 10 | `PUT /api/combates/4` | `{ "personaje1Id": 5, "personaje2Id": 1 }` | `200`: participantes nuevos |
| 11 | `DELETE /api/combates/4` | | `204` |
| 12 | `GET /api/combates/2/turnos` | | `200`: 2 turnos en orden |
| 13 | `POST /api/combates/2/turnos` | `{ "atacanteId": 3, "accion": "habilidad" }` | `200`: turno `id` 14 con `danio: 24` y estado del combate |
| 14 | `PUT /api/combates/2/turnos/14` | `{ "nota": "Disparo doble" }` | `200`: nota nueva, sigue `danio: 24` |
| 15 | `DELETE /api/combates/2/turnos/14` | | `204` |

Después del paso 14, `GET /api/combates/2` muestra lo mismo que después del 13: la nota no altera el daño. Después del 15, vuelve al estado previo: Legolia con 55 de vida y 10 flechas, Sombra con 61 y el turno de Legolia.

### Casos 409

| Solicitud | Motivo |
| :--- | :--- |
| `DELETE /api/personajes/1` | Thorin participó en un combate |
| `PUT /api/combates/1` con `{ "personaje1Id": 4, "personaje2Id": 5 }` | El combate ya tiene turnos |
| `DELETE /api/combates/1` | El combate ya tiene turnos |
| `DELETE /api/combates/1/turnos/1` | El turno no es el último |

Todos responden `409` con `{ "mensaje": "..." }` y un `GET` posterior muestra que no cambió nada.
