using Aplicacion.Dtos;
using Aplicacion.Interfaces;
using Persistencia;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios
{
    public class PersonajeApiService : IPersonajeApiService
    {
        public const int LargoMaximoNombre = 50;

        private readonly IPersonajeApiRepository _personajes;
        private readonly ICombateRepository _combates;

        public PersonajeApiService(IPersonajeApiRepository personajes, ICombateRepository combates)
        {
            _personajes = personajes ?? throw new ArgumentNullException(nameof(personajes));
            _combates = combates ?? throw new ArgumentNullException(nameof(combates));
        }

        public IReadOnlyList<PersonajeRespuesta> ObtenerTodos() =>
            _personajes.ObtenerTodos().Select(MapeoDtos.Personaje).ToList();

        public Resultado<PersonajeRespuesta> ObtenerPorId(int id)
        {
            var personaje = _personajes.ObtenerPorId(id);
            return personaje == null
                ? Resultado<PersonajeRespuesta>.NoEncontrado($"No existe el personaje {id}.")
                : Resultado<PersonajeRespuesta>.Ok(MapeoDtos.Personaje(personaje));
        }

        public Resultado<IReadOnlyList<CombateResumenRespuesta>> ObtenerCombates(int id)
        {
            if (_personajes.ObtenerPorId(id) == null)
                return Resultado<IReadOnlyList<CombateResumenRespuesta>>.NoEncontrado($"No existe el personaje {id}.");

            var personajes = _personajes.ObtenerTodos().ToDictionary(p => p.Id);
            IReadOnlyList<CombateResumenRespuesta> combates = _combates.ObtenerPorPersonaje(id)
                .Select(c => MapeoDtos.CombateResumen(c, personajes[c.Personaje1Id], personajes[c.Personaje2Id]))
                .ToList();

            return Resultado<IReadOnlyList<CombateResumenRespuesta>>.Ok(combates);
        }

        public Resultado<PersonajeRespuesta> Crear(CrearPersonajeSolicitud solicitud)
        {
            var errorNombre = ValidarNombre(solicitud?.Nombre);
            if (errorNombre != null) return Resultado<PersonajeRespuesta>.Invalido(errorNombre);

            if (!CatalogoPersonajes.EsTipoValido(solicitud!.Tipo))
                return Resultado<PersonajeRespuesta>.Invalido(
                    $"El tipo debe ser uno de: {string.Join(", ", CatalogoPersonajes.TiposValidos)}.");

            var registro = CatalogoPersonajes.Nuevo(solicitud.Nombre!.Trim(), solicitud.Tipo!);
            _personajes.Agregar(registro);

            var creado = _personajes.ObtenerPorId(registro.Id, TipoConexion.ApiAdministrador) ?? registro;
            return Resultado<PersonajeRespuesta>.Ok(MapeoDtos.Personaje(creado));
        }

        public Resultado<PersonajeRespuesta> ActualizarNombre(int id, ActualizarPersonajeSolicitud solicitud)
        {
            var errorNombre = ValidarNombre(solicitud?.Nombre);
            if (errorNombre != null) return Resultado<PersonajeRespuesta>.Invalido(errorNombre);

            if (!_personajes.ActualizarNombre(id, solicitud!.Nombre!.Trim()))
                return Resultado<PersonajeRespuesta>.NoEncontrado($"No existe el personaje {id}.");

            var actualizado = _personajes.ObtenerPorId(id, TipoConexion.ApiAdministrador);
            return actualizado == null
                ? Resultado<PersonajeRespuesta>.NoEncontrado($"No existe el personaje {id}.")
                : Resultado<PersonajeRespuesta>.Ok(MapeoDtos.Personaje(actualizado));
        }

        public Resultado Eliminar(int id)
        {
            if (_personajes.ObtenerPorId(id, TipoConexion.ApiAdministrador) == null)
                return Resultado.NoEncontrado($"No existe el personaje {id}.");

            return _personajes.EliminarSiNoParticipo(id)
                ? Resultado.Ok()
                : Resultado.Conflicto($"El personaje {id} participó en un combate y no se puede eliminar.");
        }

        private static string? ValidarNombre(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "El nombre es obligatorio.";
            if (nombre.Trim().Length > LargoMaximoNombre) return $"El nombre no puede superar {LargoMaximoNombre} caracteres.";
            return null;
        }
    }
}
