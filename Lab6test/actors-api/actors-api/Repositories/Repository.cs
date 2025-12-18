using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ActorsApi.Repositories;

public class Repository<T>(DbContext db) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = db.Set<T>();
    public Task<List<T>> GetAllAsync() => _set.AsNoTracking().ToListAsync();
    public Task<T?> GetAsync(int id) => _set.FindAsync(id).AsTask();

    public async Task<T> AddAsync(T e) { _set.Add(e); await db.SaveChangesAsync(); return e; }
    public async Task<bool> UpdateAsync(T e) { db.Update(e); return await db.SaveChangesAsync() > 0; }
    public async Task<bool> DeleteAsync(int id)
    {
        var found = await _set.FindAsync(id);
        if (found is null) return false;
        _set.Remove(found);
        return await db.SaveChangesAsync() > 0;
    }
}
