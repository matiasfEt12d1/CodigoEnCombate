using System.Data;

namespace Persistencia;

public interface IDapperContext
{
    int Execute(string sql, object? param = null, IDbTransaction? transaction = null, TipoConexion tipoConexion = TipoConexion.Desarrollo);
    IEnumerable<T> Query<T>(string sql, object? param = null, IDbTransaction? transaction = null, TipoConexion tipoConexion = TipoConexion.Desarrollo);
    T QueryFirst<T>(string sql, object? param = null, IDbTransaction? transaction = null, TipoConexion tipoConexion = TipoConexion.Desarrollo);
}