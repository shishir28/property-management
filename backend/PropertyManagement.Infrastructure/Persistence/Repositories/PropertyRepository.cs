using Microsoft.EntityFrameworkCore;
using PropertyManagement.Domain.Properties;

namespace PropertyManagement.Infrastructure.Persistence.Repositories;

public class PropertyRepository(AppDbContext db) : IPropertyRepository
{
    public Task<Property?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Properties.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Property>> GetAllAsync(CancellationToken ct = default) =>
        await db.Properties.ToListAsync(ct);

    public async Task AddAsync(Property property, CancellationToken ct = default) =>
        await db.Properties.AddAsync(property, ct);

    public Task UpdateAsync(Property property, CancellationToken ct = default)
    {
        db.Properties.Update(property);
        return Task.CompletedTask;
    }
}
