using Persistencia.Entidades;

namespace Persistencia;

// Traduce entre las filas de personajes y las clases del dominio.
// Cada tipo se registra una sola vez en un diccionario: no hay if ni switch por clase.
public static class CatalogoPersonajes
{
    private sealed record MapeoTipo(
        Func<string, PersonajeRegistro> Nuevo,
        Func<PersonajeRegistro, Personaje> Crear,
        Func<Personaje, int?> LeerRecurso,
        Action<Personaje, int> EscribirRecurso);

    private static readonly Dictionary<string, MapeoTipo> Tipos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Guerrero"] = new MapeoTipo(
            nombre => new PersonajeRegistro { Nombre = nombre, Tipo = "Guerrero", VidaMax = 120, Ataque = 18, Defensa = 8, Escudo = 20 },
            r => new Guerrero(r.Nombre, r.VidaMax, r.Ataque, r.Defensa, r.Escudo ?? 0),
            p => ((Guerrero)p).Escudo,
            (p, valor) => ((Guerrero)p).Escudo = valor),

        ["Mago"] = new MapeoTipo(
            nombre => new PersonajeRegistro { Nombre = nombre, Tipo = "Mago", VidaMax = 80, Ataque = 14, Defensa = 4, ManaMax = 60 },
            r => new Mago(r.Nombre, r.VidaMax, r.Ataque, r.Defensa, r.ManaMax ?? 1),
            p => ((Mago)p).Mana,
            (p, valor) => ((Mago)p).Mana = valor),

        ["Arquero"] = new MapeoTipo(
            nombre => new PersonajeRegistro { Nombre = nombre, Tipo = "Arquero", VidaMax = 90, Ataque = 16, Defensa = 5, CantidadFlechas = 12 },
            r => new Arquero(r.Nombre, r.VidaMax, r.Ataque, r.Defensa, r.CantidadFlechas ?? 0),
            p => ((Arquero)p).CantidadFlechas,
            (p, valor) => ((Arquero)p).CantidadFlechas = valor),

        ["Asesino"] = new MapeoTipo(
            nombre => new PersonajeRegistro { Nombre = nombre, Tipo = "Asesino", VidaMax = 85, Ataque = 20, Defensa = 4, ProbabilidadCritico = 0.30m },
            r => new Asesino(r.Nombre, r.VidaMax, r.Ataque, r.Defensa, (double)(r.ProbabilidadCritico ?? 0m)),
            _ => null,
            (_, _) => { })
    };

    public static IEnumerable<string> TiposValidos => Tipos.Keys;

    public static bool EsTipoValido(string? tipo) => tipo != null && Tipos.ContainsKey(tipo.Trim());

    // Registro con las estadísticas base del tipo, listo para insertar.
    public static PersonajeRegistro Nuevo(string nombre, string tipo) => Mapeo(tipo).Nuevo(nombre);

    public static int? RecursoInicial(PersonajeRegistro registro)
    {
        var mapeo = Mapeo(registro.Tipo);
        return mapeo.LeerRecurso(mapeo.Crear(registro));
    }

    // Arma el personaje del dominio con la vida y el recurso que tiene dentro de un combate.
    public static Personaje CrearEnCombate(PersonajeRegistro registro, int vida, int? recurso)
    {
        var mapeo = Mapeo(registro.Tipo);
        var personaje = mapeo.Crear(registro);
        personaje.Id = registro.Id;
        personaje.Vida = vida;
        if (recurso.HasValue) mapeo.EscribirRecurso(personaje, recurso.Value);
        return personaje;
    }

    public static int? LeerRecurso(PersonajeRegistro registro, Personaje personaje) => Mapeo(registro.Tipo).LeerRecurso(personaje);

    private static MapeoTipo Mapeo(string tipo) =>
        Tipos.TryGetValue(tipo.Trim(), out var mapeo)
            ? mapeo
            : throw new InvalidOperationException($"Tipo de personaje desconocido: {tipo}");
}
