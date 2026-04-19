using Microsoft.EntityFrameworkCore;
using PropertyManagement.Domain.Inspections;

namespace PropertyManagement.Infrastructure.Persistence.Repositories;

public class InspectionRepository(AppDbContext db) : IInspectionRepository
{
    public Task<Inspection?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Inspections.FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<Inspection>> GetAllAsync(CancellationToken ct = default) =>
        await db.Inspections.OrderByDescending(i => i.ScheduledAt).ToListAsync(ct);

    public async Task<IReadOnlyList<Inspection>> GetScheduledAsync(CancellationToken ct = default) =>
        await db.Inspections.Where(i => i.Status == InspectionStatus.Scheduled).ToListAsync(ct);

    public async Task<IReadOnlyList<Inspection>> GetByPropertyAsync(Guid propertyId, CancellationToken ct = default) =>
        await db.Inspections.Where(i => i.PropertyId == propertyId).ToListAsync(ct);

    public async Task AddAsync(Inspection inspection, CancellationToken ct = default) =>
        await db.Inspections.AddAsync(inspection, ct);

    public Task UpdateAsync(Inspection inspection, CancellationToken ct = default)
    {
        db.Inspections.Update(inspection);
        return Task.CompletedTask;
    }
}
