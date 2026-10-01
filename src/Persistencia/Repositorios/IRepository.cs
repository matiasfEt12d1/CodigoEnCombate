using System.Collections.Generic;

namespace Persistencia.Repositorios
{
    public interface IRepository<T> where T : class
    {
        T? ObtenerPorId(int id);
        IEnumerable<T> ObtenerTodos();
        int Agregar(T entidad);
        bool Actualizar(T entidad);
        bool Eliminar(int id);
    }
}