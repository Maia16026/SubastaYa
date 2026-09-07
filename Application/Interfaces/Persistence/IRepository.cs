namespace Application.Interfaces.Persistence;

public interface IRepository<T> where T : class
{
    Task<T?> ObtenerAsync(int id);
    Task<List<T>> ListarAsync();
    Task AgregarAsync(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
}