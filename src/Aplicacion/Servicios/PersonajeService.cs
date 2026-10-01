using Aplicacion.Interfaces;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios
{
    public class PersonajeService : IPersonajeService
    {
        private readonly IPersonajeRepository _personajeRepository;

        public PersonajeService(IPersonajeRepository personajeRepository)
        {
            _personajeRepository = personajeRepository ?? throw new ArgumentNullException(nameof(personajeRepository));
        }

        public int RegistrarPersonaje(Personaje personaje)
        {
            if (personaje == null)
                throw new ArgumentNullException(nameof(personaje), "El personaje no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(personaje.Nombre))
                throw new ArgumentException("El nombre del personaje es obligatorio.");

            return _personajeRepository.Agregar(personaje);
        }

        public Personaje? ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del personaje debe ser mayor a cero.", nameof(id));

            return _personajeRepository.ObtenerPorId(id);
        }

        public Personaje? ObtenerPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("Debe proporcionar un nombre válido.", nameof(nombre));

            return _personajeRepository.ObtenerPorNombre(nombre);
        }

        public IEnumerable<Personaje> ObtenerTodos()
        {
            return _personajeRepository.ObtenerTodos();
        }

        public IEnumerable<Personaje> ObtenerPorTipo(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException("El tipo de personaje no puede estar vacío.", nameof(tipo));

            return _personajeRepository.ObtenerPorTipo(tipo);
        }

        public bool AgregarHabilidadAPersonaje(int personajeId, Habilidad habilidad)
        {
            if (personajeId <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(personajeId));

            if (habilidad == null)
                throw new ArgumentNullException(nameof(habilidad));

            return _personajeRepository.AgregarHabilidad(personajeId, habilidad);
        }
    }
}