namespace PropertyManagement.Domain.Inspections;

public interface IInspectionRepository
{
    Task<Inspection?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Inspection>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Inspection>> GetScheduledAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Inspection>> GetByPropertyAsync(Guid propertyId, CancellationToken ct = default);
    Task AddAsync(Inspection inspection, CancellationToken ct = default);
    Task UpdateAsync(Inspection inspection, CancellationToken ct = default);
}
