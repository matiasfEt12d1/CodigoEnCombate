using Aplicacion;
using Aplicacion.Dtos;
using Aplicacion.Interfaces;
using Aplicacion.Servicios;
using Microsoft.OpenApi;
using Persistencia;
using Persistencia.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// Conexiones: se leen de ConnectionStrings__Invitado y ConnectionStrings__Administrador (variables de entorno).
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

builder.Services.AddScoped<IPersonajeApiRepository, PersonajeApiRepository>();
builder.Services.AddScoped<ICombateRepository, CombateRepository>();
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();

builder.Services.AddScoped<IPersonajeApiService, PersonajeApiService>();
builder.Services.AddScoped<ICombateService, CombateService>();
builder.Services.AddScoped<ITurnoService, TurnoService>();

// Servicios y repositorios del simulador del bimestre anterior (base codigo_en_combate).
builder.Services.AgregarCodigoEnCombate();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Combate por API",
        Version = "v1",
        Description = "Personajes, combates y turnos de Código en combate."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Combate por API v1");
    opciones.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// ---------------------------------------------------------------------
// Personajes
// ---------------------------------------------------------------------
var personajes = app.MapGroup("/api/personajes").WithTags("Personajes");

personajes.MapGet("/", (IPersonajeApiService servicio) => Results.Ok(servicio.ObtenerTodos()))
    .WithSummary("Lista todos los personajes")
    .Produces<IReadOnlyList<PersonajeRespuesta>>(StatusCodes.Status200OK);

personajes.MapGet("/{id:int}", (int id, IPersonajeApiService servicio) =>
        Respuestas.Ok(servicio.ObtenerPorId(id)))
    .WithSummary("Obtiene un personaje")
    .Produces<PersonajeRespuesta>(StatusCodes.Status200OK)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound);

personajes.MapGet("/{id:int}/combates", (int id, IPersonajeApiService servicio) =>
        Respuestas.Ok(servicio.ObtenerCombates(id)))
    .WithSummary("Lista los combates de un personaje (vacía si no participó)")
    .Produces<IReadOnlyList<CombateResumenRespuesta>>(StatusCodes.Status200OK)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound);

personajes.MapPost("/", (CrearPersonajeSolicitud solicitud, IPersonajeApiService servicio) =>
        Respuestas.Creado(servicio.Crear(solicitud), p => $"/api/personajes/{p.Id}"))
    .WithSummary("Crea un personaje a partir de nombre y tipo (Guerrero, Mago, Arquero o Asesino)")
    .Accepts<CrearPersonajeSolicitud>("application/json")
    .Produces<PersonajeRespuesta>(StatusCodes.Status201Created)
    .Produces<ErrorRespuesta>(StatusCodes.Status400BadRequest);

personajes.MapPut("/{id:int}", (int id, ActualizarPersonajeSolicitud solicitud, IPersonajeApiService servicio) =>
        Respuestas.Ok(servicio.ActualizarNombre(id, solicitud)))
    .WithSummary("Modifica solo el nombre del personaje")
    .Accepts<ActualizarPersonajeSolicitud>("application/json")
    .Produces<PersonajeRespuesta>(StatusCodes.Status200OK)
    .Produces<ErrorRespuesta>(StatusCodes.Status400BadRequest)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound);

personajes.MapDelete("/{id:int}", (int id, IPersonajeApiService servicio) =>
        Respuestas.SinContenido(servicio.Eliminar(id)))
    .WithSummary("Elimina un personaje que no participó en combates")
    .Produces(StatusCodes.Status204NoContent)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound)
    .Produces<ErrorRespuesta>(StatusCodes.Status409Conflict);

// ---------------------------------------------------------------------
// Combates
// ---------------------------------------------------------------------
var combates = app.MapGroup("/api/combates").WithTags("Combates");

combates.MapGet("/", (ICombateService servicio) => Results.Ok(servicio.ObtenerTodos()))
    .WithSummary("Lista los combates con sus participantes y estado")
    .Produces<IReadOnlyList<CombateResumenRespuesta>>(StatusCodes.Status200OK);

combates.MapPost("/", (ParticipantesSolicitud solicitud, ICombateService servicio) =>
        Respuestas.Creado(servicio.Crear(solicitud), c => $"/api/combates/{c.Id}"))
    .WithSummary("Crea un combate entre dos personajes distintos")
    .Accepts<ParticipantesSolicitud>("application/json")
    .Produces<CombateDetalleRespuesta>(StatusCodes.Status201Created)
    .Produces<ErrorRespuesta>(StatusCodes.Status400BadRequest)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound);

combates.MapGet("/{id:int}", (int id, ICombateService servicio) =>
        Respuestas.Ok(servicio.ObtenerPorId(id)))
    .WithSummary("Obtiene un combate con participantes, turnos y resultado")
    .Produces<CombateDetalleRespuesta>(StatusCodes.Status200OK)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound);

combates.MapPut("/{id:int}", (int id, ParticipantesSolicitud solicitud, ICombateService servicio) =>
        Respuestas.Ok(servicio.ActualizarParticipantes(id, solicitud)))
    .WithSummary("Cambia los dos participantes (solo antes del primer turno)")
    .Accepts<ParticipantesSolicitud>("application/json")
    .Produces<CombateDetalleRespuesta>(StatusCodes.Status200OK)
    .Produces<ErrorRespuesta>(StatusCodes.Status400BadRequest)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound)
    .Produces<ErrorRespuesta>(StatusCodes.Status409Conflict);

combates.MapDelete("/{id:int}", (int id, ICombateService servicio) =>
        Respuestas.SinContenido(servicio.Eliminar(id)))
    .WithSummary("Elimina un combate sin turnos")
    .Produces(StatusCodes.Status204NoContent)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound)
    .Produces<ErrorRespuesta>(StatusCodes.Status409Conflict);

// ---------------------------------------------------------------------
// Turnos
// ---------------------------------------------------------------------
var turnos = app.MapGroup("/api/combates/{id:int}/turnos").WithTags("Turnos");

turnos.MapGet("/", (int id, ITurnoService servicio) =>
        Respuestas.Ok(servicio.ObtenerPorCombate(id)))
    .WithSummary("Lista los turnos del combate en orden de ejecución")
    .Produces<IReadOnlyList<TurnoRespuesta>>(StatusCodes.Status200OK)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound);

turnos.MapPost("/", (int id, EjecutarTurnoSolicitud solicitud, ITurnoService servicio) =>
        Respuestas.Ok(servicio.Ejecutar(id, solicitud)))
    .WithSummary("Ejecuta un turno: el atacante usa 'atacar' o 'habilidad' contra el otro participante")
    .Accepts<EjecutarTurnoSolicitud>("application/json")
    .Produces<TurnoEjecutadoRespuesta>(StatusCodes.Status200OK)
    .Produces<ErrorRespuesta>(StatusCodes.Status400BadRequest)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound);

turnos.MapPut("/{turnoId:int}", (int id, int turnoId, ActualizarNotaSolicitud solicitud, ITurnoService servicio) =>
        Respuestas.Ok(servicio.ActualizarNota(id, turnoId, solicitud)))
    .WithSummary("Modifica únicamente la nota del turno")
    .Accepts<ActualizarNotaSolicitud>("application/json")
    .Produces<TurnoRespuesta>(StatusCodes.Status200OK)
    .Produces<ErrorRespuesta>(StatusCodes.Status400BadRequest)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound);

turnos.MapDelete("/{turnoId:int}", (int id, int turnoId, ITurnoService servicio) =>
        Respuestas.SinContenido(servicio.EliminarUltimo(id, turnoId)))
    .WithSummary("Borra el último turno y restaura el estado previo del combate")
    .Produces(StatusCodes.Status204NoContent)
    .Produces<ErrorRespuesta>(StatusCodes.Status404NotFound)
    .Produces<ErrorRespuesta>(StatusCodes.Status409Conflict);

app.Run();

// Traduce el Resultado de los servicios a respuestas HTTP.
static class Respuestas
{
    public static IResult Ok<T>(Resultado<T> resultado) =>
        resultado.Exito ? Results.Ok(resultado.Valor) : Error(resultado);

    public static IResult Creado<T>(Resultado<T> resultado, Func<T, string> ubicacion) =>
        resultado.Exito ? Results.Created(ubicacion(resultado.Valor!), resultado.Valor) : Error(resultado);

    public static IResult SinContenido(Resultado resultado) =>
        resultado.Exito ? Results.NoContent() : Error(resultado);

    private static IResult Error(Resultado resultado)
    {
        var cuerpo = new ErrorRespuesta(resultado.Mensaje ?? "Error.");
        return resultado.Error switch
        {
            TipoError.NoEncontrado => Results.NotFound(cuerpo),
            TipoError.Conflicto => Results.Conflict(cuerpo),
            _ => Results.BadRequest(cuerpo)
        };
    }
}

public partial class Program { }
