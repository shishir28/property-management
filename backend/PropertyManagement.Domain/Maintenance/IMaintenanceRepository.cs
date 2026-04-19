namespace PropertyManagement.Domain.Maintenance;

public interface IMaintenanceRepository
{
    Task<MaintenanceRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MaintenanceRequest>> GetByLeaseAsync(Guid leaseId, CancellationToken ct = default);
    Task<IReadOnlyList<MaintenanceRequest>> GetOpenByPriorityAsync(Priority priority, CancellationToken ct = default);
    Task AddAsync(MaintenanceRequest request, CancellationToken ct = default);
    Task UpdateAsync(MaintenanceRequest request, CancellationToken ct = default);
}
