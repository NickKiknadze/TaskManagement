using System.Text.Json;
using StackExchange.Redis;
using Tasky.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Tasky.Infrastructure.Services;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        if (expiry.HasValue)
            await _db.StringSetAsync(key, json, expiry.Value);
        else
            await _db.StringSetAsync(key, json);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task AddToSetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.SetAddAsync(key, json);
    }

    public async Task<IEnumerable<T>> GetSetAsync<T>(string key)
    {
        var members = await _db.SetMembersAsync(key);
        return members.Select(m => JsonSerializer.Deserialize<T>(m.ToString())!).ToList();
    }

    public async Task RemoveFromSetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.SetRemoveAsync(key, json);
    }

    public async Task SetHashAsync<T>(string key, string field, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.HashSetAsync(key, field, json);
    }

    public async Task<T?> GetHashAsync<T>(string key, string field)
    {
        var value = await _db.HashGetAsync(key, field);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(value.ToString());
    }
    
    public async Task<Dictionary<string, T>> GetAllHashAsync<T>(string key)
    {
        var entries = await _db.HashGetAllAsync(key);
        return entries.ToDictionary(
            x => x.Name.ToString(),
            x => JsonSerializer.Deserialize<T>(x.Value.ToString())!
        );
    }
}
