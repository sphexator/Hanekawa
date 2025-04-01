using Hanekawa.Interfaces;

namespace Hanekawa.Application.Interfaces;

public interface ICacheContext<T> where T : ICached
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

    /// <summary>
    /// Removes a key from cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool Remove(string key);

    /// <summary>
    /// Retrieve a value from cache by its key or create a new one if it doesn't exist
    ///  </summary>
    ValueTask<TEntity> GetOrCreateAsync<TEntity>(string key, Func<TEntity> factory);
}