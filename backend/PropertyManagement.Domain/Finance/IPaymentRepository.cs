namespace PropertyManagement.Domain.Finance;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetByLeaseAsync(Guid leaseId, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetOverdueAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetPendingAsync(CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task UpdateAsync(Payment payment, CancellationToken ct = default);
}
