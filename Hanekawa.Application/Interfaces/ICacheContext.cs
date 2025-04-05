using Hanekawa.Interfaces;

namespace Hanekawa.Application.Interfaces;

public interface ICacheContext
{
    /// <summary>
    /// Retrieve a value from cache by its key
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    TEntity? Get<TEntity>(string key) where TEntity : ICached;
    /// <summary>
    /// Retrieve a value from cache by its key and either refresh or adds expiration time
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    TEntity? Get<TEntity>(string key, TimeSpan expiration) where TEntity : ICached;
    /// <summary>
    /// Attempts to add a key-value into cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="TEntity"></typeparam>
    void Add<TEntity>(string key, TEntity value) where TEntity : ICached;
    /// <summary>
    /// Attempts to add a key-value into cache with expiration time
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiration"></param>
    /// <typeparam name="TEntity"></typeparam>
    void Add<TEntity>(string key, TEntity value, TimeSpan expiration) where TEntity : ICached;

    /// <summary>
    /// Removes a key from cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool Remove<TEntity>(string key) where TEntity : ICached;

    /// <summary>
    /// Retrieve a value from cache by its key or create a new one if it doesn't exist
    ///  </summary>
    ValueTask<TEntity> GetOrCreateAsync<TEntity>(string key, Func<Task<TEntity>> factory) where TEntity : ICached;
}