using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public class PersonajeRepository : IPersonajeRepository
    {
        // Cómo se arma y se guarda cada tipo de personaje. Cada tipo se registra una sola vez:
        // no hay if ni switch por clase.
        private sealed record MapeoTipo(Func<PersonajeDto, Personaje> Crear, Action<Personaje, PersonajeDto> CompletarFila);

        private static readonly Dictionary<string, MapeoTipo> Tipos = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Guerrero"] = new MapeoTipo(
                d => new Guerrero(d.Nombre, d.VidaMax, d.Ataque, d.Defensa, d.Escudo),
                (p, d) => d.Escudo = ((Guerrero)p).Escudo),

            ["Mago"] = new MapeoTipo(
                d => new Mago(d.Nombre, d.VidaMax, d.Ataque, d.Defensa, d.ManaMax) { Mana = d.Mana },
                (p, d) =>
                {
                    var mago = (Mago)p;
                    d.ManaMax = mago.ManaMax;
                    d.Mana = mago.Mana;
                }),

            ["Arquero"] = new MapeoTipo(
                d => new Arquero(d.Nombre, d.VidaMax, d.Ataque, d.Defensa, d.CantidadFlechas),
                (p, d) => d.CantidadFlechas = ((Arquero)p).CantidadFlechas),

            ["Asesino"] = new MapeoTipo(
                d => new Asesino(d.Nombre, d.VidaMax, d.Ataque, d.Defensa, d.ProbabilidadCritico),
                (p, d) => d.ProbabilidadCritico = ((Asesino)p).ProbabilidadCritico)
        };

        private readonly IDapperContext _context;

        public PersonajeRepository(IDapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Personaje? ObtenerPorId(int id)
        {
            const string sqlPersonaje = @"
                SELECT id AS Id, nombre AS Nombre, tipo AS Tipo, vida_max AS VidaMax, 
                       vida AS Vida, ataque AS Ataque, defensa AS Defensa, escudo_actual AS Escudo,
                       mana_max AS ManaMax, mana AS Mana, flechas_actuales AS CantidadFlechas,
                       probabilidad_critico AS ProbabilidadCritico 
                FROM Personajes WHERE id = @Id";

            var dto = _context.Query<PersonajeDto>(sqlPersonaje, new { Id = id }).FirstOrDefault();
            if (dto == null) return null;

            var personaje = MapearEntidad(dto);

            const string sqlHabilidades = @"
                SELECT id AS Id, nombre AS Nombre, costo_recurso AS CostoRecurso, potencia AS Potencia
                FROM Habilidades WHERE personaje_id = @Id";

            var habilidades = _context.Query<HabilidadDto>(sqlHabilidades, new { Id = id });
            foreach (var h in habilidades)
            {
                personaje.AgregarHabilidad(new Habilidad(h.Nombre, h.CostoRecurso, h.Potencia) { Id = h.Id });
            }

            return personaje;
        }

        public Personaje? ObtenerPorNombre(string nombre)
        {
            const string sql = "SELECT id FROM Personajes WHERE nombre = @Nombre";
            var id = _context.Query<int?>(sql, new { Nombre = nombre }).FirstOrDefault();
            return id.HasValue ? ObtenerPorId(id.Value) : null;
        }

        public IEnumerable<Personaje> ObtenerTodos()
        {
            const string sql = "SELECT id FROM Personajes";
            var ids = _context.Query<int>(sql);
            return ids.Select(ObtenerPorId).Where(p => p != null)!;
        }

        public IEnumerable<Personaje> ObtenerPorTipo(string tipo)
        {
            const string sql = "SELECT id FROM Personajes WHERE tipo = @Tipo";
            var ids = _context.Query<int>(sql, new { Tipo = tipo });
            return ids.Select(ObtenerPorId).Where(p => p != null)!;
        }

        public int Agregar(Personaje entidad)
        {
            const string sql = @"
                INSERT INTO Personajes (nombre, tipo, vida_max, vida, ataque, defensa, escudo, escudo_actual, mana_max, mana, cantidad_flechas, flechas_actuales, probabilidad_critico)
                VALUES (@Nombre, @Tipo, @VidaMax, @Vida, @Ataque, @Defensa, @Escudo, @Escudo, @ManaMax, @Mana, @CantidadFlechas, @CantidadFlechas, @ProbabilidadCritico);
                SELECT LAST_INSERT_ID();";

            var pms = ObtenerParametros(entidad);
            int idGenerado = _context.QueryFirst<int>(sql, pms);
            entidad.Id = idGenerado;
            
            foreach (var hab in entidad.Habilidades)
            {
                AgregarHabilidad(idGenerado, hab);
            }

            return idGenerado;
        }

        public bool Actualizar(Personaje entidad)
        {
            if (entidad == null) throw new ArgumentNullException(nameof(entidad));
            if (entidad.Id <= 0) return false;

            // Por ID: el nombre no es único y podría actualizar a otro personaje.
            const string sql = @"
                UPDATE Personajes
                SET vida = @Vida, ataque = @Ataque, defensa = @Defensa, escudo_actual = @Escudo, mana = @Mana, flechas_actuales = @CantidadFlechas
                WHERE id = @Id";

            var pms = ObtenerParametros(entidad);
            return _context.Execute(sql, pms) > 0;
        }

        public bool Eliminar(int id)
        {
            const string sql = "DELETE FROM Personajes WHERE id = @Id";
            return _context.Execute(sql, new { Id = id }) > 0;
        }

        public bool AgregarHabilidad(int personajeId, Habilidad habilidad)
        {
            // Devuelve el ID generado para que la habilidad pueda registrarse en el historial.
            const string sql = @"
                INSERT INTO Habilidades (personaje_id, nombre, costo_recurso, potencia)
                VALUES (@PersonajeId, @Nombre, @CostoRecurso, @Potencia);
                SELECT LAST_INSERT_ID();";

            habilidad.Id = _context.QueryFirst<int>(sql, new
            {
                PersonajeId = personajeId,
                habilidad.Nombre,
                habilidad.CostoRecurso,
                habilidad.Potencia
            });

            return habilidad.Id > 0;
        }

        private static MapeoTipo Mapeo(string tipo) =>
            Tipos.TryGetValue(tipo, out var mapeo)
                ? mapeo
                : throw new InvalidOperationException($"Tipo de personaje desconocido: {tipo}");

        private static Personaje MapearEntidad(PersonajeDto dto)
        {
            var p = Mapeo(dto.Tipo).Crear(dto);
            p.Id = dto.Id;
            p.Vida = dto.Vida;
            return p;
        }

        // Fila con los datos comunes y los propios del tipo; las columnas que no corresponden quedan en 0.
        // BatallaRepository la usa para guardar el estado de los combatientes al registrar un turno.
        internal static PersonajeDto ObtenerParametros(Personaje p)
        {
            string tipo = p.GetType().Name;
            var fila = new PersonajeDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Tipo = tipo,
                VidaMax = p.VidaMax,
                Vida = p.Vida,
                Ataque = p.Ataque,
                Defensa = p.Defensa
            };

            Mapeo(tipo).CompletarFila(p, fila);
            return fila;
        }

        internal class PersonajeDto
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public int VidaMax { get; set; }
            public int Vida { get; set; }
            public int Ataque { get; set; }
            public int Defensa { get; set; }
            public int Escudo { get; set; }
            public int ManaMax { get; set; }
            public int Mana { get; set; }
            public int CantidadFlechas { get; set; }
            public double ProbabilidadCritico { get; set; }
        }

        private class HabilidadDto
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public int CostoRecurso { get; set; }
            public int Potencia { get; set; }
        }
    }
}