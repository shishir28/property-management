using MediatR;
using PropertyManagement.Application.Common;
using PropertyManagement.Domain.Maintenance;
using PropertyManagement.Domain.Workflows;

namespace PropertyManagement.Application.Maintenance.Commands;

public record CreateMaintenanceRequestCommand(Guid LeaseId, string Title, string Description, string Priority) : IRequest<Guid>;
public record AssignMaintenanceRequestCommand(Guid RequestId, string Contractor) : IRequest;
public record ResolveMaintenanceRequestCommand(Guid RequestId) : IRequest;

public class CreateMaintenanceRequestHandler(IMaintenanceRepository repo, IUnitOfWork uow, IWorkflowClient workflows)
    : IRequestHandler<CreateMaintenanceRequestCommand, Guid>
{
    public async Task<Guid> Handle(CreateMaintenanceRequestCommand cmd, CancellationToken ct)
    {
        var priority = Enum.Parse<Priority>(cmd.Priority, ignoreCase: true);
        var request = MaintenanceRequest.Create(cmd.LeaseId, cmd.Title, cmd.Description, priority);
        await repo.AddAsync(request, ct);
        await uow.SaveChangesAsync(ct);
        await workflows.TriggerMaintenanceWorkflowAsync(request.Id, ct);
        return request.Id;
    }
}

public class AssignMaintenanceRequestHandler(IMaintenanceRepository repo, IUnitOfWork uow)
    : IRequestHandler<AssignMaintenanceRequestCommand>
{
    public async Task Handle(AssignMaintenanceRequestCommand cmd, CancellationToken ct)
    {
        var request = await repo.GetByIdAsync(cmd.RequestId, ct)
            ?? throw new KeyNotFoundException($"Maintenance request {cmd.RequestId} not found.");
        request.Assign(cmd.Contractor);
        await repo.UpdateAsync(request, ct);
        await uow.SaveChangesAsync(ct);
    }
}

public class ResolveMaintenanceRequestHandler(IMaintenanceRepository repo, IUnitOfWork uow)
    : IRequestHandler<ResolveMaintenanceRequestCommand>
{
    public async Task Handle(ResolveMaintenanceRequestCommand cmd, CancellationToken ct)
    {
        var request = await repo.GetByIdAsync(cmd.RequestId, ct)
            ?? throw new KeyNotFoundException($"Maintenance request {cmd.RequestId} not found.");
        request.Resolve();
        await repo.UpdateAsync(request, ct);
        await uow.SaveChangesAsync(ct);
    }
}
