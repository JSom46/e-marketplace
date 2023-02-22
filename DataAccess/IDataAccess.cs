namespace DataAccess;

public interface IDataAccess
{
    Task<IEnumerable<U>> Query<T, U>(
        string storedProcedure,
        T parameters,
        string connectionId = "Default");

    Task<U> QuerySingle<T, U>(
        string storedProcedure,
        T parameters,
        string connectionId = "Default");

    Task<int> Execute<T>(
        string storedProcedure,
        T parameters,
        string connectionId = "Default");
}