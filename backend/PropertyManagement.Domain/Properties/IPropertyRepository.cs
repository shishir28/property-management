namespace PropertyManagement.Domain.Properties;

public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Property>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Property property, CancellationToken ct = default);
    Task UpdateAsync(Property property, CancellationToken ct = default);
}
