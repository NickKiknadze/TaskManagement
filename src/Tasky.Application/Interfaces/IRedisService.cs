namespace Tasky.Application.Interfaces;

public interface IRedisService
{
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
    
    Task AddToSetAsync<T>(string key, T value);
    Task<IEnumerable<T>> GetSetAsync<T>(string key);
    Task RemoveFromSetAsync<T>(string key, T value);
    
    Task SetHashAsync<T>(string key, string field, T value);
    Task<T?> GetHashAsync<T>(string key, string field);
    Task<Dictionary<string, T>> GetAllHashAsync<T>(string key);
}
