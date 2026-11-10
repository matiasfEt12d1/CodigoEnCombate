namespace Persistencia.Entidades
{
    // Filas de las tablas de combate_api. No tienen reglas de combate:
    // el dominio (Personaje, Batalla) se arma a partir de ellas.

    public class PersonajeRegistro
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int VidaMax { get; set; }
        public int Ataque { get; set; }
        public int Defensa { get; set; }
        public int? Escudo { get; set; }
        public int? ManaMax { get; set; }
        public int? CantidadFlechas { get; set; }
        public decimal? ProbabilidadCritico { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public static class EstadoCombate
    {
        public const string EnCurso = "EnCurso";
        public const string Finalizado = "Finalizado";
    }

    public class CombateRegistro
    {
        public int Id { get; set; }
        public int Personaje1Id { get; set; }
        public int Personaje2Id { get; set; }
        public string Estado { get; set; } = EstadoCombate.EnCurso;
        public bool TurnoPersonaje1 { get; set; } = true;
        public int NumeroTurno { get; set; } = 1;
        public int VidaPersonaje1 { get; set; }
        public int VidaPersonaje2 { get; set; }
        public int? RecursoPersonaje1 { get; set; }
        public int? RecursoPersonaje2 { get; set; }
        public int? GanadorId { get; set; }
        public DateTime FechaCreacion { get; set; }

        public bool EstaFinalizado => Estado == EstadoCombate.Finalizado;
        public bool Participa(int personajeId) => personajeId == Personaje1Id || personajeId == Personaje2Id;
        public int? IdQueActua => EstaFinalizado ? null : (TurnoPersonaje1 ? Personaje1Id : Personaje2Id);
    }

    public class TurnoRegistro
    {
        public int Id { get; set; }
        public int CombateId { get; set; }
        public int Numero { get; set; }
        public int AtacanteId { get; set; }
        public int DefensorId { get; set; }
        public string Accion { get; set; } = string.Empty;
        public int Danio { get; set; }
        public string? Nota { get; set; }
        public int VidaPersonaje1Antes { get; set; }
        public int VidaPersonaje2Antes { get; set; }
        public int? RecursoPersonaje1Antes { get; set; }
        public int? RecursoPersonaje2Antes { get; set; }
        public DateTime Fecha { get; set; }
    }
}
