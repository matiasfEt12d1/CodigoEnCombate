using Aplicacion.Interfaces;
using Aplicacion.Servicios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistencia;
using Persistencia.Repositorios;

namespace Aplicacion
{
    // Registro de dependencias del simulador (tablas del simulador en combate_api).
    // Lo llama el programa principal (API/Program.cs); los servicios dependen de interfaces
    // y el contenedor decide qué implementación recibe cada uno.
    public static class ConfiguracionServicios
    {
        public static IServiceCollection AgregarCodigoEnCombate(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            // Conexiones Desarrollo y Administrador (variables ConnectionStrings__Desarrollo y ConnectionStrings__Administrador).
            services.TryAddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
            services.AddSingleton<IDapperContext, DapperContext>();

            services.AddScoped<IPersonajeRepository, PersonajeRepository>();
            services.AddScoped<IBatallaRepository, BatallaRepository>();
            services.AddScoped<IEstadisticasRepository, EstadisticasRepository>();

            services.AddSingleton<IResolucionTurnoService, ResolucionTurnoService>();
            services.AddScoped<IPersonajeService, PersonajeService>();
            services.AddScoped<IBatallaService, BatallaService>();
            services.AddScoped<IEstadisticasService, EstadisticasService>();

            return services;
        }
    }
}
