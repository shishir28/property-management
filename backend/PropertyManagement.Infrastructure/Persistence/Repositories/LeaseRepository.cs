using Microsoft.EntityFrameworkCore;
using PropertyManagement.Domain.Leases;

namespace PropertyManagement.Infrastructure.Persistence.Repositories;

public class LeaseRepository(AppDbContext db) : ILeaseRepository
{
    public Task<Lease?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Leases.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<Lease>> GetAllAsync(CancellationToken ct = default) =>
        await db.Leases.ToListAsync(ct);

    public async Task<IReadOnlyList<Lease>> GetExpiringSoonAsync(int withinDays = 60, CancellationToken ct = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(withinDays));
        return await db.Leases
            .Where(l => l.Status == LeaseStatus.Active && l.EndDate <= cutoff)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Lease>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await db.Leases.Where(l => l.TenantId == tenantId).ToListAsync(ct);

    public async Task AddAsync(Lease lease, CancellationToken ct = default) =>
        await db.Leases.AddAsync(lease, ct);

    public Task UpdateAsync(Lease lease, CancellationToken ct = default)
    {
        db.Leases.Update(lease);
        return Task.CompletedTask;
    }
}
