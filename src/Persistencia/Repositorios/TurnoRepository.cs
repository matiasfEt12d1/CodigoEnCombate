using System.Data;
using Dapper;
using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public class TurnoRepository : ITurnoRepository
    {
        private const string Columnas = @"
            id AS Id, combate_id AS CombateId, numero AS Numero, atacante_id AS AtacanteId, defensor_id AS DefensorId,
            accion AS Accion, danio AS Danio, nota AS Nota,
            vida_personaje1_antes AS VidaPersonaje1Antes, vida_personaje2_antes AS VidaPersonaje2Antes,
            recurso_personaje1_antes AS RecursoPersonaje1Antes, recurso_personaje2_antes AS RecursoPersonaje2Antes,
            fecha AS Fecha";

        // Solo se actualiza si el combate sigue en el número de turno esperado (evita pisar otro turno concurrente).
        private const string SqlActualizarCombate = @"
            UPDATE combates
            SET estado = @Estado, turno_personaje1 = @TurnoPersonaje1, numero_turno = @NumeroTurno,
                vida_personaje1 = @VidaPersonaje1, vida_personaje2 = @VidaPersonaje2,
                recurso_personaje1 = @RecursoPersonaje1, recurso_personaje2 = @RecursoPersonaje2,
                ganador_id = @GanadorId
            WHERE id = @Id AND numero_turno = @NumeroTurnoEsperado";

        private readonly IDbConnectionFactory _connectionFactory;

        public TurnoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public IEnumerable<TurnoRegistro> ObtenerPorCombate(int combateId, TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            using var connection = _connectionFactory.CrearConexion(conexion);
            return connection.Query<TurnoRegistro>(
                $"SELECT {Columnas} FROM turnos WHERE combate_id = @CombateId ORDER BY numero",
                new { CombateId = combateId }).ToList();
        }

        public TurnoRegistro? ObtenerPorId(int combateId, int turnoId, TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            using var connection = _connectionFactory.CrearConexion(conexion);
            return connection.QueryFirstOrDefault<TurnoRegistro>(
                $"SELECT {Columnas} FROM turnos WHERE id = @TurnoId AND combate_id = @CombateId",
                new { CombateId = combateId, TurnoId = turnoId });
        }

        public TurnoRegistro? ObtenerUltimo(int combateId, TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            using var connection = _connectionFactory.CrearConexion(conexion);
            return connection.QueryFirstOrDefault<TurnoRegistro>(
                $"SELECT {Columnas} FROM turnos WHERE combate_id = @CombateId ORDER BY numero DESC LIMIT 1",
                new { CombateId = combateId });
        }

        public bool Registrar(TurnoRegistro turno, CombateRegistro combateActualizado, int numeroTurnoEsperado)
        {
            const string sqlInsertar = @"
                INSERT INTO turnos (combate_id, numero, atacante_id, defensor_id, accion, danio, nota,
                                    vida_personaje1_antes, vida_personaje2_antes, recurso_personaje1_antes, recurso_personaje2_antes)
                VALUES (@CombateId, @Numero, @AtacanteId, @DefensorId, @Accion, @Danio, @Nota,
                        @VidaPersonaje1Antes, @VidaPersonaje2Antes, @RecursoPersonaje1Antes, @RecursoPersonaje2Antes);
                SELECT LAST_INSERT_ID();";

            return EnTransaccion((connection, transaction) =>
            {
                if (!ActualizarCombate(connection, transaction, combateActualizado, numeroTurnoEsperado))
                    return false;

                turno.Id = connection.QuerySingle<int>(sqlInsertar, turno, transaction);
                return true;
            });
        }

        public bool ActualizarNota(int combateId, int turnoId, string? nota)
        {
            const string sql = "UPDATE turnos SET nota = @Nota WHERE id = @TurnoId AND combate_id = @CombateId";

            using var connection = _connectionFactory.CrearConexion(TipoConexion.ApiAdministrador);
            return connection.Execute(sql, new { CombateId = combateId, TurnoId = turnoId, Nota = nota }) > 0;
        }

        public bool EliminarUltimoYRestaurar(TurnoRegistro turno, CombateRegistro combateRestaurado)
        {
            const string sqlEliminar = "DELETE FROM turnos WHERE id = @Id AND combate_id = @CombateId";

            return EnTransaccion((connection, transaction) =>
            {
                // El combate está en numero + 1 solo si este turno sigue siendo el último.
                if (!ActualizarCombate(connection, transaction, combateRestaurado, turno.Numero + 1))
                    return false;

                return connection.Execute(sqlEliminar, new { turno.Id, turno.CombateId }, transaction) > 0;
            });
        }

        private static bool ActualizarCombate(IDbConnection connection, IDbTransaction transaction, CombateRegistro combate, int numeroTurnoEsperado)
        {
            var parametros = new DynamicParameters(combate);
            parametros.Add("NumeroTurnoEsperado", numeroTurnoEsperado);
            return connection.Execute(SqlActualizarCombate, parametros, transaction) > 0;
        }

        // Ejecuta la operación en una transacción del administrador: confirma si devuelve true, si no la revierte.
        private bool EnTransaccion(Func<IDbConnection, IDbTransaction, bool> operacion)
        {
            using var connection = _connectionFactory.CrearConexion(TipoConexion.ApiAdministrador);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                if (operacion(connection, transaction))
                {
                    transaction.Commit();
                    return true;
                }

                transaction.Rollback();
                return false;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
