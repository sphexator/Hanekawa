namespace Hanekawa.Application.Interfaces;

public interface ICacheContext
{
    /// <summary>
    /// Retrieve a value from cache by its key
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    TEntity? Get<TEntity>(string key);
    /// <summary>
    /// Retrieve a value from cache by its key and either refresh or adds expiration time
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    TEntity? Get<TEntity>(string key, TimeSpan expiration);
    /// <summary>
    /// Attempts to add a key-value into cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="TEntity"></typeparam>
    void Add<TEntity>(string key, TEntity value);
    /// <summary>
    /// Attempts to add a key-value into cache with expiration time
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiration"></param>
    /// <typeparam name="TEntity"></typeparam>
    void Add<TEntity>(string key, TEntity value, TimeSpan expiration);
}