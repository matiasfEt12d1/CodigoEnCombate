using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Persistencia;

// Las cadenas se leen de la configuración (variables de entorno ConnectionStrings__<Nombre>),
// así ninguna contraseña queda en el repositorio.
public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public IDbConnection CrearConexion(TipoConexion tipo = TipoConexion.Desarrollo)
    {
        if (!Enum.IsDefined(tipo))
            throw new ArgumentOutOfRangeException(nameof(tipo), $"Tipo de conexión no soportado: {tipo}");

        string nombre = tipo.ToString();
        string? connectionString = _configuration.GetConnectionString(nombre);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"La cadena de conexión '{nombre}' no está configurada. Definir la variable de entorno ConnectionStrings__{nombre}.");

        return new MySqlConnection(connectionString);
    }
}
