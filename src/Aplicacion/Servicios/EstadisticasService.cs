using Aplicacion.Interfaces;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios
{
    public class EstadisticasService : IEstadisticasService
    {
        private readonly IBatallaRepository _batallaRepository;
        private readonly IPersonajeRepository _personajeRepository;

        public EstadisticasService(IBatallaRepository batallaRepository, IPersonajeRepository personajeRepository)
        {
            _batallaRepository = batallaRepository ?? throw new ArgumentNullException(nameof(batallaRepository));
            _personajeRepository = personajeRepository ?? throw new ArgumentNullException(nameof(personajeRepository));
        }

        public ResumenEstadisticas ObtenerResumenEstadisticas()
        {
            var todas = _batallaRepository.ObtenerTodos().ToList();

            return new ResumenEstadisticas
            {
                TotalBatallas = todas.Count,
                BatallasFinalizadas = todas.Count(b => b.EsFinalizada),
                BatallasActivas = todas.Count(b => !b.EsFinalizada)
            };
        }

        public IEnumerable<string> ObtenerRankingPersonajes()
        {
            var personajes = _personajeRepository.ObtenerTodos();
            return personajes
                .OrderByDescending(p => p.Vida)
                .Select(p => $"{p.Nombre} (Vida actual: {p.Vida}/{p.VidaMax})");
        }
    }
}