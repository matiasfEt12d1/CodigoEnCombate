using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistencia;
using Persistencia.Entidades;
using Persistencia.Repositorios;
using Aplicacion.Servicios;

namespace Aplicacion
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1. Cargar configuración desde appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // 2. Configurar el contenedor de Inyección de Dependencias
            var services = new ServiceCollection();
            services.AddSingleton(configuration);
            services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
            services.AddSingleton<IDapperContext, DapperContext>();

            // Repositorios de Persistencia
            services.AddScoped<IPersonajeRepository, PersonajeRepository>();
            services.AddScoped<IBatallaRepository, BatallaRepository>();

            // Servicios de la Capa de Aplicación
            services.AddScoped<PersonajeService>();
            services.AddScoped<BatallaService>();
            services.AddScoped<EstadisticasService>();

            var serviceProvider = services.BuildServiceProvider();

            // 3. Obtener los servicios para ejecutar la simulación
            var personajeService = serviceProvider.GetRequiredService<PersonajeService>();
            var batallaService = serviceProvider.GetRequiredService<BatallaService>();
            var estadisticasService = serviceProvider.GetRequiredService<EstadisticasService>();

            Console.WriteLine("=== SIMULADOR DE BATALLAS - CÓDIGO EN COMBATE ===\n");

            // 4. Registro e inserción de Personajes con Habilidades en MySQL
            var guerrero = new Guerrero("Thor", vidaMax: 100, ataque: 22, defensa: 8, escudo: 15);
            guerrero.AgregarHabilidad(new Habilidad("Golpe de Escudo", costoRecurso: 0, potencia: 10));

            var mago = new Mago("Gandalf", vidaMax: 80, ataque: 25, defensa: 4, manaMax: 30);
            mago.AgregarHabilidad(new Habilidad("Bola de Fuego", costoRecurso: 10, potencia: 25));

            var arquero = new Arquero("Robin", vidaMax: 85, ataque: 20, defensa: 5, cantidadFlechas: 5);
            var asesino = new Asesino("Sombra", vidaMax: 75, ataque: 28, defensa: 3, probabilidadCritico: 0.35);

            personajeService.RegistrarPersonaje(guerrero);
            personajeService.RegistrarPersonaje(mago);
            personajeService.RegistrarPersonaje(arquero);
            personajeService.RegistrarPersonaje(asesino);

            Console.WriteLine("Personajes guardados correctamente en la base de datos.");

            // 5. Simulación de Combate
            Console.WriteLine("\n--- Iniciando Combate Completo: Thor vs Gandalf ---");
            var batalla = batallaService.CrearBatalla(guerrero, mago);
            var ganador = batallaService.EjecutarCombateCompleto(batalla);

            Console.WriteLine($"\n¡Combate Finalizado!");
            Console.WriteLine($"Ganador: {ganador.Nombre} ({ganador.GetType().Name})");

            // 6. Reporte de Estadísticas finales
            Console.WriteLine("\n=== RESUMEN DE ESTADÍSTICAS ===");
            var resumen = estadisticasService.ObtenerResumenEstadisticas();
            Console.WriteLine($"Total de Batallas: {resumen.TotalBatallas}");
            Console.WriteLine($"Batallas Finalizadas: {resumen.BatallasFinalizadas}");
            Console.WriteLine($"Batallas Activas: {resumen.BatallasActivas}");

            Console.WriteLine("\n--- Ranking de Personajes ---");
            foreach (var item in estadisticasService.ObtenerRankingPersonajes())
            {
                Console.WriteLine($"- {item}");
            }
        }
    }
}