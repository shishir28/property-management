using MediatR;
using PropertyManagement.Domain.Finance;

namespace PropertyManagement.Application.Finance.Queries;

public record PaymentDto(Guid Id, Guid LeaseId, decimal Amount, DateOnly DueDate, DateOnly? PaidDate, string Status, string? PaymentMethod);

public record GetPaymentsByLeaseQuery(Guid LeaseId) : IRequest<IReadOnlyList<PaymentDto>>;
public record GetOverduePaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;

file static class Mapper
{
    internal static PaymentDto ToDto(Payment p) =>
        new(p.Id, p.LeaseId, p.Amount, p.DueDate, p.PaidDate, p.Status.ToString(), p.PaymentMethod);
}

public class GetPaymentsByLeaseHandler(IPaymentRepository repo) : IRequestHandler<GetPaymentsByLeaseQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPaymentsByLeaseQuery q, CancellationToken ct)
        => (await repo.GetByLeaseAsync(q.LeaseId, ct)).Select(Mapper.ToDto).ToList();
}

public class GetOverduePaymentsHandler(IPaymentRepository repo) : IRequestHandler<GetOverduePaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetOverduePaymentsQuery q, CancellationToken ct)
        => (await repo.GetOverdueAsync(ct)).Select(Mapper.ToDto).ToList();
}
