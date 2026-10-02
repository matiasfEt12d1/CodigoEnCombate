using System.Collections.Generic;
using System.Linq;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Tests
{
    public class FakePersonajeRepository : IPersonajeRepository
    {
        private readonly List<Personaje> _personajes = new();

        public Personaje? ObtenerPorId(int id) => _personajes.FirstOrDefault();
        public IEnumerable<Personaje> ObtenerTodos() => _personajes;
        public int Agregar(Personaje entidad)
        {
            _personajes.Add(entidad);
            return _personajes.Count;
        }
        public bool Actualizar(Personaje entidad) => true;
        public bool Eliminar(int id) => true;
        public Personaje? ObtenerPorNombre(string nombre) => _personajes.FirstOrDefault(p => p.Nombre == nombre);
        public IEnumerable<Personaje> ObtenerPorTipo(string tipo) => _personajes;
        public bool AgregarHabilidad(int personajeId, Habilidad habilidad) => true;
    }

    public class FakeBatallaRepository : IBatallaRepository
    {
        private readonly List<Batalla> _batallas = new();

        public Batalla? ObtenerPorId(int id) => _batallas.FirstOrDefault();
        public IEnumerable<Batalla> ObtenerTodos() => _batallas;
        public int Agregar(Batalla entidad)
        {
            _batallas.Add(entidad);
            return _batallas.Count;
        }
        public bool Actualizar(Batalla entidad) => true;
        public bool Eliminar(int id) => true;
        public IEnumerable<Batalla> ObtenerBatallasActivas() => _batallas.Where(b => !b.EsFinalizada);
        public List<(int BatallaId, int Turno, int AtacanteId, int DefensorId, int Dano)> HistorialRegistrado { get; } = new();
        public void RegistrarHistorialTurno(int batallaId, int numeroTurno, int atacanteId, int defensorId, string? habilidadUsada, int danoCausado)
        {
            HistorialRegistrado.Add((batallaId, numeroTurno, atacanteId, defensorId, danoCausado));
        }
    }
}