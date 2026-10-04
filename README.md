<h1 align="center">E.T. Nº12 D.E. 1º "Libertador Gral. José de San Martín"</h1>

<p align="center">
  <img src="https://et12.edu.ar/imgs/et12.svg" alt="ET12 Logo" width="240">
</p>

# Código en combate

Simulador de batallas por turnos desarrollado en **.NET 8.0 con C#**, aplicando arquitectura por capas, principios de Programación Orientada a Objetos (Herencia, Polimorfismo, Encapsulamiento) y persistencia en **MySQL** utilizando **Dapper** y **MySqlConnector**.

Hecho mediante un plan de aprendizaje nivel 2, de la especialidad "Computación" del 3er bimestre, cursando 5to año en una escuela técnica de la Ciudad Autónoma de Buenos Aires, Argentina.

---

## Estructura del Proyecto

```text
CodigoEnCombate/
├── scripts/
├── src/
│   ├── Aplicacion/
│   │   ├── Interfaces/
│   │   └── Servicios/
│   ├── Persistencia/
│   │   ├── Entidades/
│   │   └── Repositorios/
│   └── Tests/
├── Proyecto.sln
├── .gitignore
└── README.md
```

---

### Proyectos de la Solución

- **`Aplicacion`**: Contiene la lógica de negocio, servicios (`BatallaService`, `PersonajeService`, `EstadisticasService`) e interfaces.
- **`Persistencia`**: Contiene el modelo de dominio (`Personaje`, `Guerrero`, `Mago`, `Arquero`, `Asesino`, `Batalla`, `Habilidad`), el contexto de Dapper y la implementación de los repositorios con MySQL.
- **`Tests`**: Suite de pruebas unitarias implementadas con **xUnit** utilizando repositorios simulados y Theory para la cobertura.

---

## Dependencias

```text
Aplicacion -> Persistencia
Tests -> Aplicacion
Tests -> Persistencia
```

---

## Despliegue

![C#](https://img.shields.io/badge/Language-C%23-blue)
![.NET](https://img.shields.io/badge/Framework-.NET%208.0-purple)
![MySQL](https://img.shields.io/badge/Database-MySQL-violet)


### Requisitos

| Nombre | Versión | Descripción |
| :--- | :---: | :--- |
| .NET SDK | `8.0` | Entorno de ejecución, compilación y testing |
| MySQL | `8.0` | Motor de base de datos relacional |
| Dapper | `2.1.89` | Micro-ORM síncrono para mapeo de datos |
| MySqlConnector | `2.6.2` | Driver para conexión con MySQL |
| Microsoft.Extensions.Configuration | `10.0.12` | Soporte para gestión de configuraciones |
| Microsoft.Extensions.Configuration.Json | `10.0.12` | Lectura de ajustes desde `appsettings.json` |
| Microsoft.Extensions.DependencyInjection | `10.0.12` | Contenedor para inyección de dependencias |

---

### Clonar e Instalar

```bash
git clone [https://github.com/TU_USUARIO/codigo-en-combate.git](https://github.com/TU_USUARIO/codigo-en-combate.git)
cd codigo-en-combate
dotnet restore
```

---

### Configuración de la Aplicación

1. Ejecutar en tu servidor MySQL los scripts SQL de la carpeta `scripts/`.
2. El archivo `src/Aplicacion/appsettings.json` está configurado para conectarse mediante los usuarios definidos en dicho script (`desarrollador` y `administrador`):

```json
{
  "ConnectionStrings": {
    "Desarrollo": "Server=localhost;Database=codigo_en_combate;Uid=desarrollador;Pwd=devpass123!;",
    "Administrador": "Server=localhost;Database=codigo_en_combate;Uid=administrador;Pwd=adminpass123!;"
  }
}
```

---

## Comandos Útiles

**Compilar la solución:**
```bash
dotnet build
```

**Ejecutar la aplicación principal:**
```bash
dotnet run --project src/Aplicacion/Aplicacion.csproj
```

**Ejecutar las pruebas unitarias:**
```bash
dotnet test
```