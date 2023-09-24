namespace Hanekawa.Application.Interfaces;

public interface ICacheContext
{
    Task<TEntity?> GetAsync<TEntity>(string key);
    Task<TEntity?> GetAsync<TEntity>(string key, TimeSpan expiration);
    Task AddAsync<TEntity>(string key, TEntity value);
    Task AddAsync<TEntity>(string key, TEntity value, TimeSpan expiration);
}