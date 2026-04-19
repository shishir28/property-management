using Microsoft.EntityFrameworkCore;
using PropertyManagement.Domain.Maintenance;

namespace PropertyManagement.Infrastructure.Persistence.Repositories;

public class MaintenanceRepository(AppDbContext db) : IMaintenanceRepository
{
    public Task<MaintenanceRequest?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.MaintenanceRequests.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<MaintenanceRequest>> GetByLeaseAsync(Guid leaseId, CancellationToken ct = default) =>
        await db.MaintenanceRequests.Where(r => r.LeaseId == leaseId).ToListAsync(ct);

    public async Task<IReadOnlyList<MaintenanceRequest>> GetOpenByPriorityAsync(Priority priority, CancellationToken ct = default) =>
        await db.MaintenanceRequests
            .Where(r => r.Priority == priority && r.Status == MaintenanceStatus.Open)
            .ToListAsync(ct);

    public async Task AddAsync(MaintenanceRequest request, CancellationToken ct = default) =>
        await db.MaintenanceRequests.AddAsync(request, ct);

    public Task UpdateAsync(MaintenanceRequest request, CancellationToken ct = default)
    {
        db.MaintenanceRequests.Update(request);
        return Task.CompletedTask;
    }
}
