using Microsoft.EntityFrameworkCore;
using PropertyManagement.Domain.Tenants;

namespace PropertyManagement.Infrastructure.Persistence.Repositories;

public class TenantRepository(AppDbContext db) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default) =>
        await db.Tenants.ToListAsync(ct);

    public async Task<IReadOnlyList<Tenant>> GetByStatusAsync(TenantStatus status, CancellationToken ct = default) =>
        await db.Tenants.Where(t => t.Status == status).ToListAsync(ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default) =>
        await db.Tenants.AddAsync(tenant, ct);

    public Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        db.Tenants.Update(tenant);
        return Task.CompletedTask;
    }
}
