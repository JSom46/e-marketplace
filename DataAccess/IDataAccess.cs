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

    void StartTransaction(string connectionId = "Default");
    void CommitTransaction();
    void RollbackTransaction();

    Task<IEnumerable<U>> QueryInTransaction<T, U>(
        string storedProcedure,
        T parameters);

    Task<U> QuerySingleInTransaction<T, U>(
        string storedProcedure,
        T parameters);

    Task<int> ExecuteInTransaction<T>(
        string storedProcedure,
        T parameters);
}