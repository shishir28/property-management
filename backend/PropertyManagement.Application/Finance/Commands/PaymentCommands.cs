using MediatR;
using PropertyManagement.Application.Common;
using PropertyManagement.Domain.Finance;
using PropertyManagement.Domain.Workflows;

namespace PropertyManagement.Application.Finance.Commands;

public record CreatePaymentCommand(Guid LeaseId, decimal Amount, DateOnly DueDate) : IRequest<Guid>;
public record MarkPaymentPaidCommand(Guid PaymentId, string PaymentMethod) : IRequest;
public record RunRentCollectionCommand : IRequest;

public class CreatePaymentHandler(IPaymentRepository repo, IUnitOfWork uow) : IRequestHandler<CreatePaymentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePaymentCommand cmd, CancellationToken ct)
    {
        var payment = Payment.Create(cmd.LeaseId, cmd.Amount, cmd.DueDate);
        await repo.AddAsync(payment, ct);
        await uow.SaveChangesAsync(ct);
        return payment.Id;
    }
}

public class MarkPaymentPaidHandler(IPaymentRepository repo, IUnitOfWork uow) : IRequestHandler<MarkPaymentPaidCommand>
{
    public async Task Handle(MarkPaymentPaidCommand cmd, CancellationToken ct)
    {
        var payment = await repo.GetByIdAsync(cmd.PaymentId, ct)
            ?? throw new KeyNotFoundException($"Payment {cmd.PaymentId} not found.");
        payment.MarkPaid(cmd.PaymentMethod);
        await repo.UpdateAsync(payment, ct);
        await uow.SaveChangesAsync(ct);
    }
}

public class RunRentCollectionHandler(IWorkflowClient workflows) : IRequestHandler<RunRentCollectionCommand>
{
    public async Task Handle(RunRentCollectionCommand cmd, CancellationToken ct)
        => await workflows.TriggerRentCollectionWorkflowAsync(ct);
}
