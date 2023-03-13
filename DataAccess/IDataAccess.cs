namespace DataAccess;

public interface IDataAccess
{
    /// <summary>
    /// Query a database for a list of items using specified stored procedure.
    /// </summary>
    /// <typeparam name="T">Type of data to be passed to stored procedure.</typeparam>
    /// <typeparam name="U">Type of data to be returned.</typeparam>
    /// <param name="storedProcedure">Name of a stored procedure to run.</param>
    /// <param name="parameters">Parameters to pass to the procedure.</param>
    /// <param name="connectionId">Name of a connection string to use.</param>
    /// <returns>An <seealso cref="IEnumerable{U}"/> of objects returned by database query.</returns>
    Task<IEnumerable<U>> Query<T, U>(
        string storedProcedure,
        T parameters,
        string connectionId = "Default");

    /// <summary>
    /// Query a database for a single item using specified stored procedure.
    /// </summary>
    /// <typeparam name="T">Type of data to be passed to stored procedure.</typeparam>
    /// <typeparam name="U">Type of data to be returned.</typeparam>
    /// <param name="storedProcedure">Name of a stored procedure to run.</param>
    /// <param name="parameters">Parameters to pass to the procedure.</param>
    /// <param name="connectionId">Name of a connection string to use.</param>
    /// <returns>An object returned by database query.</returns>
    Task<U> QuerySingle<T, U>(
        string storedProcedure,
        T parameters,
        string connectionId = "Default");

    /// <summary>
    /// Executes a database non-query procedure
    /// </summary>
    /// <typeparam name="T">Type of data to be passed to stored procedure.</typeparam>
    /// <param name="storedProcedure">Name of a stored procedure to run.</param>
    /// <param name="parameters">Parameters to pass to the procedure.</param>
    /// <param name="connectionId">Name of a connection string to use.</param>
    /// <returns>Number of affected rows returned by stored procedure.</returns>
    Task<int> Execute<T>(
        string storedProcedure,
        T parameters,
        string connectionId = "Default");

    /// <summary>
    /// Starts a new transaction.
    /// </summary>
    /// <param name="connectionId">Name of a connection string to use.</param>
    void StartTransaction(string connectionId = "Default");

    /// <summary>
    /// Commits currently running transaction. If there's no running transaction, does nothing.
    /// </summary>
    void CommitTransaction();

    /// <summary>
    /// Rollbacks currently running transaction. If there's no running transaction, does nothing.
    /// </summary>
    void RollbackTransaction();

    /// <summary>
    /// Query a database for a list of items using specified stored procedure. Uses transaction.
    /// If there's no currently running transaction, <seealso cref="NullReferenceException"/> is thrown.
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    /// <typeparam name="T">Type of data to be passed to stored procedure.</typeparam>
    /// <typeparam name="U">Type of data to be returned.</typeparam>
    /// <param name="storedProcedure">Name of a stored procedure to run.</param>
    /// <param name="parameters">Parameters to pass to the procedure.</param>
    /// <param name="connectionId">Name of a connection string to use.</param>
    /// <returns>A list of objects returned by database query.</returns>
    Task<IEnumerable<U>> QueryInTransaction<T, U>(
        string storedProcedure,
        T parameters);

    /// <summary>
    /// Query a database for a single item using specified stored procedure. Uses transaction.
    /// If there's no currently running transaction, <seealso cref="NullReferenceException"/> is thrown.
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    /// <typeparam name="T">Type of data to be passed to stored procedure.</typeparam>
    /// <typeparam name="U">Type of data to be returned.</typeparam>
    /// <param name="storedProcedure">Name of a stored procedure to run.</param>
    /// <param name="parameters">Parameters to pass to the procedure.</param>
    /// <param name="connectionId">Name of a connection string to use.</param>
    /// <returns>An object returned by database query.</returns>
    Task<U> QuerySingleInTransaction<T, U>(
        string storedProcedure,
        T parameters);

    /// <summary>
    /// Executes a database non-query procedure. Uses transaction.
    /// If there's no currently running transaction, <seealso cref="NullReferenceException"/> is thrown.
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    /// <typeparam name="T">Type of data to be passed to stored procedure.</typeparam>
    /// <param name="storedProcedure">Name of a stored procedure to run.</param>
    /// <param name="parameters">Parameters to pass to the procedure.</param>
    /// <param name="connectionId">Name of a connection string to use.</param>
    /// <returns>Number of affected rows returned by stored procedure.</returns>
    Task<int> ExecuteInTransaction<T>(
        string storedProcedure,
        T parameters);
}