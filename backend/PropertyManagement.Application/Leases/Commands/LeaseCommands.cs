using MediatR;
using PropertyManagement.Application.Common;
using PropertyManagement.Domain.Leases;
using PropertyManagement.Domain.Workflows;

namespace PropertyManagement.Application.Leases.Commands;

public record CreateLeaseCommand(Guid PropertyId, Guid TenantId, string UnitNumber,
    DateOnly StartDate, DateOnly EndDate, decimal MonthlyRent, decimal SecurityDeposit) : IRequest<Guid>;

public record ActivateLeaseCommand(Guid LeaseId) : IRequest;

public record RenewLeaseCommand(Guid LeaseId, DateOnly NewEndDate, decimal NewMonthlyRent) : IRequest;

public class CreateLeaseHandler(ILeaseRepository repo, IUnitOfWork uow) : IRequestHandler<CreateLeaseCommand, Guid>
{
    public async Task<Guid> Handle(CreateLeaseCommand cmd, CancellationToken ct)
    {
        var lease = Lease.Create(cmd.PropertyId, cmd.TenantId, cmd.UnitNumber,
            cmd.StartDate, cmd.EndDate, cmd.MonthlyRent, cmd.SecurityDeposit);
        await repo.AddAsync(lease, ct);
        await uow.SaveChangesAsync(ct);
        return lease.Id;
    }
}

public class ActivateLeaseHandler(ILeaseRepository repo, IUnitOfWork uow, IWorkflowClient workflows)
    : IRequestHandler<ActivateLeaseCommand>
{
    public async Task Handle(ActivateLeaseCommand cmd, CancellationToken ct)
    {
        var lease = await repo.GetByIdAsync(cmd.LeaseId, ct)
            ?? throw new KeyNotFoundException($"Lease {cmd.LeaseId} not found.");
        lease.Activate();
        await repo.UpdateAsync(lease, ct);
        await uow.SaveChangesAsync(ct);
        await workflows.TriggerOnboardingWorkflowAsync(lease.TenantId, lease.Id, ct);
    }
}

public class RenewLeaseHandler(ILeaseRepository repo, IUnitOfWork uow)
    : IRequestHandler<RenewLeaseCommand>
{
    public async Task Handle(RenewLeaseCommand cmd, CancellationToken ct)
    {
        var lease = await repo.GetByIdAsync(cmd.LeaseId, ct)
            ?? throw new KeyNotFoundException($"Lease {cmd.LeaseId} not found.");
        lease.Renew(cmd.NewEndDate, cmd.NewMonthlyRent);
        await repo.UpdateAsync(lease, ct);
        await uow.SaveChangesAsync(ct);
    }
}
