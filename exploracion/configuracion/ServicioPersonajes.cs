namespace Configuracion;

public record PersonajeRespuesta(int Id, string Nombre, string Tipo);

public record CrearPersonajeSolicitud(string? Nombre, string? Tipo);

public interface IServicioPersonajes
{
    IReadOnlyList<PersonajeRespuesta> ObtenerTodos();
    PersonajeRespuesta? ObtenerPorId(int id);
    PersonajeRespuesta? Crear(CrearPersonajeSolicitud solicitud);
}

// Recibe el repositorio por constructor: no sabe qué implementación es ni la crea él.
// El contenedor de dependencias se la entrega según lo registrado en Program.cs.
public class ServicioPersonajes(IRepositorioPersonajes repositorio) : IServicioPersonajes
{
    public IReadOnlyList<PersonajeRespuesta> ObtenerTodos() =>
        repositorio.ObtenerTodos().Select(ARespuesta).ToList();

    public PersonajeRespuesta? ObtenerPorId(int id) =>
        repositorio.ObtenerPorId(id) is { } personaje ? ARespuesta(personaje) : null;

    // Devuelve null si los datos no son válidos.
    public PersonajeRespuesta? Crear(CrearPersonajeSolicitud solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Nombre) || string.IsNullOrWhiteSpace(solicitud.Tipo))
            return null;

        return ARespuesta(repositorio.Agregar(solicitud.Nombre.Trim(), solicitud.Tipo.Trim()));
    }

    private static PersonajeRespuesta ARespuesta(Personaje p) => new(p.Id, p.Nombre, p.Tipo);
}
