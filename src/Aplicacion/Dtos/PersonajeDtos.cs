namespace Aplicacion.Dtos
{
    public record CrearPersonajeSolicitud(string? Nombre, string? Tipo);

    public record ActualizarPersonajeSolicitud(string? Nombre);

    public record PersonajeRespuesta(
        int Id,
        string Nombre,
        string Tipo,
        int VidaMax,
        int Ataque,
        int Defensa,
        int? Escudo,
        int? ManaMax,
        int? CantidadFlechas,
        decimal? ProbabilidadCritico);
}
