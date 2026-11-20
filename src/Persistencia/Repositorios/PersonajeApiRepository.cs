using Dapper;
using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public class PersonajeApiRepository : IPersonajeApiRepository
    {
        private const string Columnas = @"
            id AS Id, nombre AS Nombre, tipo AS Tipo, vida_max AS VidaMax, ataque AS Ataque, defensa AS Defensa,
            escudo AS Escudo, mana_max AS ManaMax, cantidad_flechas AS CantidadFlechas,
            probabilidad_critico AS ProbabilidadCritico, fecha_creacion AS FechaCreacion";

        private readonly IDbConnectionFactory _connectionFactory;

        public PersonajeApiRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public IEnumerable<PersonajeRegistro> ObtenerTodos(TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            using var connection = _connectionFactory.CrearConexion(conexion);
            return connection.Query<PersonajeRegistro>($"SELECT {Columnas} FROM personajes ORDER BY id").ToList();
        }

        public PersonajeRegistro? ObtenerPorId(int id, TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            using var connection = _connectionFactory.CrearConexion(conexion);
            return connection.QueryFirstOrDefault<PersonajeRegistro>(
                $"SELECT {Columnas} FROM personajes WHERE id = @Id", new { Id = id });
        }

        public int Agregar(PersonajeRegistro personaje)
        {
            const string sql = @"
                INSERT INTO personajes (nombre, tipo, vida_max, ataque, defensa, escudo, mana_max, cantidad_flechas, probabilidad_critico)
                VALUES (@Nombre, @Tipo, @VidaMax, @Ataque, @Defensa, @Escudo, @ManaMax, @CantidadFlechas, @ProbabilidadCritico);
                SELECT LAST_INSERT_ID();";

            using var connection = _connectionFactory.CrearConexion(TipoConexion.ApiAdministrador);
            personaje.Id = connection.QuerySingle<int>(sql, personaje);
            return personaje.Id;
        }

        public bool ActualizarNombre(int id, string nombre)
        {
            const string sql = "UPDATE personajes SET nombre = @Nombre WHERE id = @Id";

            using var connection = _connectionFactory.CrearConexion(TipoConexion.ApiAdministrador);
            return connection.Execute(sql, new { Id = id, Nombre = nombre }) > 0;
        }

        public bool EliminarSiNoParticipo(int id)
        {
            const string sql = @"
                DELETE FROM personajes
                WHERE id = @Id
                  AND NOT EXISTS (SELECT 1 FROM combates c WHERE c.personaje1_id = @Id OR c.personaje2_id = @Id)
                  AND NOT EXISTS (SELECT 1 FROM Batallas b WHERE b.combatiente1_id = @Id OR b.combatiente2_id = @Id)";

            using var connection = _connectionFactory.CrearConexion(TipoConexion.ApiAdministrador);
            return connection.Execute(sql, new { Id = id }) > 0;
        }
    }
}
