namespace PropertyManagement.Domain.Leases;

public interface ILeaseRepository
{
    Task<Lease?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Lease>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Lease>> GetExpiringSoonAsync(int withinDays = 60, CancellationToken ct = default);
    Task<IReadOnlyList<Lease>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Lease lease, CancellationToken ct = default);
    Task UpdateAsync(Lease lease, CancellationToken ct = default);
}
