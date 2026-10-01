using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Persistencia;

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionStringDesarrollo;
    private readonly string _connectionStringAdmin;

    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _connectionStringDesarrollo = configuration.GetConnectionString("Desarrollo") 
            ?? throw new InvalidOperationException("La cadena de conexión 'Desarrollo' no está configurada.");
        _connectionStringAdmin = configuration.GetConnectionString("Administrador") 
            ?? throw new InvalidOperationException("La cadena de conexión 'Administrador' no está configurada.");
    }

    public IDbConnection CrearConexion(TipoConexion tipo = TipoConexion.Desarrollo)
    {
        string connectionString = tipo switch
        {
            TipoConexion.Administrador => _connectionStringAdmin,
            TipoConexion.Desarrollo => _connectionStringDesarrollo,
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), $"Tipo de conexión no soportado: {tipo}")
        };

        return new MySqlConnection(connectionString);
    }
}