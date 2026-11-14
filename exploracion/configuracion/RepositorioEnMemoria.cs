namespace Configuracion;

public class Personaje
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}

// Contrato del repositorio: el servicio solo conoce esto, no sabe dónde se guardan los datos.
public interface IRepositorioPersonajes
{
    IReadOnlyList<Personaje> ObtenerTodos();
    Personaje? ObtenerPorId(int id);
    Personaje Agregar(string nombre, string tipo);
}

// Implementación en memoria. En el proyecto final la misma idea la cumple PersonajeApiRepository con MySQL.
// La lista es el "almacenamiento": por eso se registra como Singleton (una sola instancia para toda la API).
public class RepositorioPersonajesEnMemoria : IRepositorioPersonajes
{
    private readonly List<Personaje> _personajes =
    [
        new() { Id = 1, Nombre = "Thorin", Tipo = "Guerrero" },
        new() { Id = 2, Nombre = "Merlina", Tipo = "Mago" }
    ];

    // Como es Singleton, varias solicitudes pueden usarla a la vez: el lock evita que se pisen.
    private readonly Lock _bloqueo = new();

    public IReadOnlyList<Personaje> ObtenerTodos()
    {
        lock (_bloqueo) return _personajes.ToList();
    }

    public Personaje? ObtenerPorId(int id)
    {
        lock (_bloqueo) return _personajes.FirstOrDefault(p => p.Id == id);
    }

    public Personaje Agregar(string nombre, string tipo)
    {
        lock (_bloqueo)
        {
            var personaje = new Personaje { Id = _personajes.Max(p => p.Id) + 1, Nombre = nombre, Tipo = tipo };
            _personajes.Add(personaje);
            return personaje;
        }
    }
}
