using System.Data;
using Dapper;

namespace Persistencia;

public class DapperContext : IDapperContext
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperContext(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public int Execute(string sql, object? param = null, IDbTransaction? transaction = null, TipoConexion tipoConexion = TipoConexion.Desarrollo)
    {
        if (transaction != null)
        {
            return transaction.Connection!.Execute(sql, param, transaction);
        }

        using var connection = _connectionFactory.CrearConexion(tipoConexion);
        return connection.Execute(sql, param);
    }

    public IEnumerable<T> Query<T>(string sql, object? param = null, IDbTransaction? transaction = null, TipoConexion tipoConexion = TipoConexion.Desarrollo)
    {
        if (transaction != null)
        {
            return transaction.Connection!.Query<T>(sql, param, transaction);
        }

        using var connection = _connectionFactory.CrearConexion(tipoConexion);
        return connection.Query<T>(sql, param);
    }

    public T QueryFirst<T>(string sql, object? param = null, IDbTransaction? transaction = null, TipoConexion tipoConexion = TipoConexion.Desarrollo)
    {
        if (transaction != null)
        {
            return transaction.Connection!.QueryFirst<T>(sql, param, transaction);
        }

        using var connection = _connectionFactory.CrearConexion(tipoConexion);
        return connection.QueryFirst<T>(sql, param);
    }
}