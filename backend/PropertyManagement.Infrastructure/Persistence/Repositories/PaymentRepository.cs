using Microsoft.EntityFrameworkCore;
using PropertyManagement.Domain.Finance;

namespace PropertyManagement.Infrastructure.Persistence.Repositories;

public class PaymentRepository(AppDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Payment>> GetByLeaseAsync(Guid leaseId, CancellationToken ct = default) =>
        await db.Payments.Where(p => p.LeaseId == leaseId).ToListAsync(ct);

    public async Task<IReadOnlyList<Payment>> GetOverdueAsync(CancellationToken ct = default) =>
        await db.Payments.Where(p => p.Status == PaymentStatus.Overdue).ToListAsync(ct);

    public async Task<IReadOnlyList<Payment>> GetPendingAsync(CancellationToken ct = default) =>
        await db.Payments.Where(p => p.Status == PaymentStatus.Pending).ToListAsync(ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default) =>
        await db.Payments.AddAsync(payment, ct);

    public Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Update(payment);
        return Task.CompletedTask;
    }
}
