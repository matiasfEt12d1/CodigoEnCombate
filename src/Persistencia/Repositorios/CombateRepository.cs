using Dapper;
using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public class CombateRepository : ICombateRepository
    {
        internal const string Columnas = @"
            id AS Id, personaje1_id AS Personaje1Id, personaje2_id AS Personaje2Id, estado AS Estado,
            turno_personaje1 AS TurnoPersonaje1, numero_turno AS NumeroTurno,
            vida_personaje1 AS VidaPersonaje1, vida_personaje2 AS VidaPersonaje2,
            recurso_personaje1 AS RecursoPersonaje1, recurso_personaje2 AS RecursoPersonaje2,
            ganador_id AS GanadorId, fecha_creacion AS FechaCreacion";

        private readonly IDbConnectionFactory _connectionFactory;

        public CombateRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public IEnumerable<CombateRegistro> ObtenerTodos(TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            using var connection = _connectionFactory.CrearConexion(conexion);
            return connection.Query<CombateRegistro>($"SELECT {Columnas} FROM combates ORDER BY id").ToList();
        }

        public IEnumerable<CombateRegistro> ObtenerPorPersonaje(int personajeId, TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            using var connection = _connectionFactory.CrearConexion(conexion);
            return connection.Query<CombateRegistro>(
                $"SELECT {Columnas} FROM combates WHERE personaje1_id = @Id OR personaje2_id = @Id ORDER BY id",
                new { Id = personajeId }).ToList();
        }

        public CombateRegistro? ObtenerPorId(int id, TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            using var connection = _connectionFactory.CrearConexion(conexion);
            return connection.QueryFirstOrDefault<CombateRegistro>(
                $"SELECT {Columnas} FROM combates WHERE id = @Id", new { Id = id });
        }

        public bool TieneTurnos(int id, TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            using var connection = _connectionFactory.CrearConexion(conexion);
            return connection.ExecuteScalar<bool>(
                "SELECT EXISTS (SELECT 1 FROM turnos WHERE combate_id = @Id)", new { Id = id });
        }

        public int Agregar(CombateRegistro combate)
        {
            const string sql = @"
                INSERT INTO combates (personaje1_id, personaje2_id, estado, turno_personaje1, numero_turno,
                                      vida_personaje1, vida_personaje2, recurso_personaje1, recurso_personaje2, ganador_id)
                VALUES (@Personaje1Id, @Personaje2Id, @Estado, @TurnoPersonaje1, @NumeroTurno,
                        @VidaPersonaje1, @VidaPersonaje2, @RecursoPersonaje1, @RecursoPersonaje2, @GanadorId);
                SELECT LAST_INSERT_ID();";

            using var connection = _connectionFactory.CrearConexion(TipoConexion.ApiAdministrador);
            combate.Id = connection.QuerySingle<int>(sql, combate);
            return combate.Id;
        }

        public bool ActualizarParticipantesSiNoTieneTurnos(CombateRegistro combate)
        {
            const string sql = @"
                UPDATE combates
                SET personaje1_id = @Personaje1Id, personaje2_id = @Personaje2Id,
                    estado = @Estado, turno_personaje1 = @TurnoPersonaje1, numero_turno = @NumeroTurno,
                    vida_personaje1 = @VidaPersonaje1, vida_personaje2 = @VidaPersonaje2,
                    recurso_personaje1 = @RecursoPersonaje1, recurso_personaje2 = @RecursoPersonaje2,
                    ganador_id = @GanadorId
                WHERE id = @Id
                  AND NOT EXISTS (SELECT 1 FROM turnos t WHERE t.combate_id = @Id)";

            using var connection = _connectionFactory.CrearConexion(TipoConexion.ApiAdministrador);
            return connection.Execute(sql, combate) > 0;
        }

        public bool EliminarSiNoTieneTurnos(int id)
        {
            const string sql = @"
                DELETE FROM combates
                WHERE id = @Id
                  AND NOT EXISTS (SELECT 1 FROM turnos t WHERE t.combate_id = @Id)";

            using var connection = _connectionFactory.CrearConexion(TipoConexion.ApiAdministrador);
            return connection.Execute(sql, new { Id = id }) > 0;
        }
    }
}
