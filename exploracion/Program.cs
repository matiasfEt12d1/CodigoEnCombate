// Prueba de concepto de Indagación (Actividad 1): rutas de Minimal API documentadas en Swagger
// y comparación polimórfica entre Guerrero y Mago. No usa el proyecto final ni MySQL.
// Ejecutar: dotnet run --project exploracion  ->  http://localhost:5000/swagger

var builder = WebApplication.CreateBuilder(args);

// Generan el documento OpenAPI (swagger.json) a partir de las rutas registradas.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Publican el documento y la interfaz de Swagger.
app.UseSwagger();
app.UseSwaggerUI();

// ---------------------------------------------------------------------
// 1. Ruta GET que devuelve un objeto fijo.
// ---------------------------------------------------------------------
app.MapGet("/exploracion/personaje", () => Results.Ok(new PersonajeFijo(1, "Thorin", "Guerrero")))
    .WithSummary("Devuelve siempre el mismo personaje")
    .Produces<PersonajeFijo>(StatusCodes.Status200OK);

// ---------------------------------------------------------------------
// 2. Ruta con parámetro: el {id} viaja en la ruta. 200 si existe, 404 si no.
// ---------------------------------------------------------------------
app.MapGet("/exploracion/personajes/{id:int}", (int id) =>
        id == 1
            ? Results.Ok(new PersonajeFijo(1, "Thorin", "Guerrero"))
            : Results.NotFound(new { mensaje = $"No existe el personaje {id}." }))
    .WithSummary("200 si el id es 1; 404 en cualquier otro caso")
    .Produces<PersonajeFijo>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// ---------------------------------------------------------------------
// 3. Ruta con cuerpo: los datos viajan en el JSON. 201 si son válidos, 400 si no.
// ---------------------------------------------------------------------
app.MapPost("/exploracion/personajes", (NuevoPersonaje cuerpo) =>
        string.IsNullOrWhiteSpace(cuerpo.Nombre) || string.IsNullOrWhiteSpace(cuerpo.Tipo)
            ? Results.BadRequest(new { mensaje = "Nombre y tipo son obligatorios." })
            : Results.Created("/exploracion/personajes/2", new PersonajeFijo(2, cuerpo.Nombre.Trim(), cuerpo.Tipo.Trim())))
    .WithSummary("201 con el personaje creado; 400 si falta nombre o tipo (no guarda nada)")
    .Produces<PersonajeFijo>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

// ---------------------------------------------------------------------
// 4. Polimorfismo: la misma llamada (UsarHabilidad) y cada clase responde a su manera.
// ---------------------------------------------------------------------
app.MapGet("/exploracion/polimorfismo", () =>
    {
        List<Luchador> luchadores = [new Guerrero("Thorin"), new Mago("Merlina")];

        // Ningún if ni switch por tipo: se llama al método de la clase base.
        var resultados = luchadores
            .Select(l => l.UsarHabilidad(new Muneco(defensa: 5)))
            .ToList();

        return Results.Ok(resultados);
    })
    .WithSummary("Guerrero y Mago usan su habilidad contra el mismo muñeco sin preguntar su tipo")
    .Produces<List<ResultadoAccion>>(StatusCodes.Status200OK);

app.Run();

// ---------------------------------------------------------------------
// Tipos de la prueba
// ---------------------------------------------------------------------
record PersonajeFijo(int Id, string Nombre, string Tipo);

record NuevoPersonaje(string? Nombre, string? Tipo);

record ResultadoAccion(string Luchador, string Clase, string Habilidad, int Danio, string Detalle);

// Objetivo de práctica: solo tiene defensa.
class Muneco(int defensa)
{
    public int Defensa { get; } = defensa;
}

abstract class Luchador(string nombre, int ataque)
{
    public string Nombre { get; } = nombre;
    public int Ataque { get; } = ataque;

    // Cada subclase decide qué hace su habilidad.
    public abstract ResultadoAccion UsarHabilidad(Muneco objetivo);
}

class Guerrero(string nombre) : Luchador(nombre, ataque: 18)
{
    public int Escudo { get; private set; } = 20;

    // Embestida: suma el escudo al ataque, descuenta la defensa y se queda sin escudo.
    public override ResultadoAccion UsarHabilidad(Muneco objetivo)
    {
        int danio = Math.Max(1, Ataque + Escudo - objetivo.Defensa);
        Escudo = 0;
        return new ResultadoAccion(Nombre, nameof(Guerrero), "Embestida", danio, "Suma el escudo y la defensa del objetivo lo reduce.");
    }
}

class Mago(string nombre) : Luchador(nombre, ataque: 14)
{
    public int Mana { get; private set; } = 60;

    // Bola de fuego: gasta 25 de maná y hace el doble del ataque ignorando la defensa.
    public override ResultadoAccion UsarHabilidad(Muneco objetivo)
    {
        Mana -= 25;
        return new ResultadoAccion(Nombre, nameof(Mago), "Bola de fuego", Ataque * 2, $"Ignora la defensa. Le quedan {Mana} de maná.");
    }
}
