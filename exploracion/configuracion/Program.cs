// Prueba de concepto de Indagación (Actividad 3): registro de dependencias por interfaces y Swagger.
// Usa un repositorio en memoria: no se conecta a la base del proyecto.
// Ejecutar: dotnet run --project exploracion/configuracion  ->  http://localhost:5000/swagger

using Configuracion;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// Inyección de dependencias: interfaz -> implementación y tiempo de vida.
// ---------------------------------------------------------------------
// Singleton: una sola instancia mientras corre la API. La lista en memoria es el almacenamiento,
// así que tiene que sobrevivir entre solicitudes.
builder.Services.AddSingleton<IRepositorioPersonajes, RepositorioPersonajesEnMemoria>();

// Scoped: una instancia nueva por cada solicitud HTTP. No guarda estado propio.
builder.Services.AddScoped<IServicioPersonajes, ServicioPersonajes>();

// ---------------------------------------------------------------------
// Swagger
// ---------------------------------------------------------------------
// Expone las rutas de Minimal API para que el generador pueda describirlas.
builder.Services.AddEndpointsApiExplorer();

// Genera el documento OpenAPI (swagger.json) con la información de la API.
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Exploración: dependencias y Swagger",
        Version = "v1"
    });
});

var app = builder.Build();

// Publica el documento en /swagger/v1/swagger.json.
app.UseSwagger();

// Publica la interfaz web en /swagger, que lee ese documento.
app.UseSwaggerUI();

// ---------------------------------------------------------------------
// Rutas: reciben el servicio como parámetro; Minimal API lo pide al contenedor
// porque su tipo (IServicioPersonajes) está registrado.
// ---------------------------------------------------------------------
var personajes = app.MapGroup("/api/personajes").WithTags("Personajes");

personajes.MapGet("/", (IServicioPersonajes servicio) => Results.Ok(servicio.ObtenerTodos()))
    .WithSummary("Lista los personajes del repositorio en memoria")
    .Produces<IReadOnlyList<PersonajeRespuesta>>(StatusCodes.Status200OK);

personajes.MapGet("/{id:int}", (int id, IServicioPersonajes servicio) =>
        servicio.ObtenerPorId(id) is { } personaje
            ? Results.Ok(personaje)
            : Results.NotFound(new { mensaje = $"No existe el personaje {id}." }))
    .WithSummary("Obtiene un personaje")
    .Produces<PersonajeRespuesta>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// Sirve para comprobar el tiempo de vida: lo agregado sigue estando en el GET siguiente.
personajes.MapPost("/", (CrearPersonajeSolicitud solicitud, IServicioPersonajes servicio) =>
        servicio.Crear(solicitud) is { } creado
            ? Results.Created($"/api/personajes/{creado.Id}", creado)
            : Results.BadRequest(new { mensaje = "Nombre y tipo son obligatorios." }))
    .WithSummary("Agrega un personaje al repositorio en memoria")
    .Produces<PersonajeRespuesta>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

app.Run();
