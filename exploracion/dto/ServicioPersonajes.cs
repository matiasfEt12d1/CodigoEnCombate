// Prueba de concepto de Indagación (Actividad 2): servicio que recibe un DTO y devuelve otro.
// Los datos quedan en memoria: no usa MySQL ni el proyecto final.

namespace Exploracion.Dto;

public class ServicioPersonajes
{
    // Estadísticas base de cada tipo, las mismas que usa CatalogoPersonajes en el proyecto final.
    private static readonly Dictionary<string, (int VidaMax, int Ataque, int Defensa)> EstadisticasBase =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Guerrero"] = (120, 18, 8),
            ["Mago"] = (80, 14, 4),
            ["Arquero"] = (90, 16, 5),
            ["Asesino"] = (85, 20, 4)
        };

    private readonly List<Personaje> _personajes = [];
    private int _ultimoId;

    public PersonajeCreadoRespuesta Crear(CrearPersonajeSolicitud solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        if (string.IsNullOrWhiteSpace(solicitud.Nombre))
            throw new ArgumentException("El nombre es obligatorio.");

        string tipo = solicitud.Tipo?.Trim() ?? string.Empty;
        if (!EstadisticasBase.TryGetValue(tipo, out var estadisticas))
            throw new ArgumentException($"El tipo debe ser uno de: {string.Join(", ", EstadisticasBase.Keys)}.");

        // DTO -> entidad: el servicio completa lo que el cliente no puede elegir.
        var personaje = new Personaje
        {
            Id = ++_ultimoId,
            Nombre = solicitud.Nombre.Trim(),
            Tipo = tipo,
            VidaMax = estadisticas.VidaMax,
            Vida = estadisticas.VidaMax,
            Ataque = estadisticas.Ataque,
            Defensa = estadisticas.Defensa,
            FechaCreacion = DateTime.Now
        };
        _personajes.Add(personaje);

        // Entidad -> DTO: solo sale el ID y el nombre; la entidad nunca deja el servicio.
        return new PersonajeCreadoRespuesta(personaje.Id, personaje.Nombre);
    }

    public IReadOnlyList<PersonajeCreadoRespuesta> ObtenerTodos() =>
        _personajes.Select(p => new PersonajeCreadoRespuesta(p.Id, p.Nombre)).ToList();

    // Entidad del ejemplo. Es privada del servicio: fuera de él solo se ven los DTO.
    private class Personaje
    {
        public int Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string Tipo { get; init; } = string.Empty;
        public int VidaMax { get; init; }
        public int Vida { get; set; }
        public int Ataque { get; init; }
        public int Defensa { get; init; }
        public DateTime FechaCreacion { get; init; }
    }
}
