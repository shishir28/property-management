using MediatR;
using PropertyManagement.Application.Common;
using PropertyManagement.Domain.Inspections;
using PropertyManagement.Domain.Workflows;

namespace PropertyManagement.Application.Inspections.Commands;

public record CreateInspectionCommand(Guid PropertyId, Guid? LeaseId, string Type, DateTime ScheduledAt) : IRequest<Guid>;
public record CompleteInspectionCommand(Guid InspectionId, string Notes) : IRequest;
public record CancelInspectionCommand(Guid InspectionId) : IRequest;

public class CreateInspectionHandler(IInspectionRepository repo, IUnitOfWork uow)
    : IRequestHandler<CreateInspectionCommand, Guid>
{
    public async Task<Guid> Handle(CreateInspectionCommand cmd, CancellationToken ct)
    {
        var type = Enum.Parse<InspectionType>(cmd.Type, ignoreCase: true);
        var inspection = Inspection.Create(cmd.PropertyId, cmd.LeaseId, type, cmd.ScheduledAt);
        await repo.AddAsync(inspection, ct);
        await uow.SaveChangesAsync(ct);
        return inspection.Id;
    }
}

public class CompleteInspectionHandler(IInspectionRepository repo, IUnitOfWork uow, IWorkflowClient workflows)
    : IRequestHandler<CompleteInspectionCommand>
{
    public async Task Handle(CompleteInspectionCommand cmd, CancellationToken ct)
    {
        var inspection = await repo.GetByIdAsync(cmd.InspectionId, ct)
            ?? throw new KeyNotFoundException($"Inspection {cmd.InspectionId} not found.");
        inspection.Complete(cmd.Notes);
        await repo.UpdateAsync(inspection, ct);
        await uow.SaveChangesAsync(ct);
        await workflows.TriggerInspectionWorkflowAsync(inspection.Id, ct);
    }
}

public class CancelInspectionHandler(IInspectionRepository repo, IUnitOfWork uow)
    : IRequestHandler<CancelInspectionCommand>
{
    public async Task Handle(CancelInspectionCommand cmd, CancellationToken ct)
    {
        var inspection = await repo.GetByIdAsync(cmd.InspectionId, ct)
            ?? throw new KeyNotFoundException($"Inspection {cmd.InspectionId} not found.");
        inspection.Cancel();
        await repo.UpdateAsync(inspection, ct);
        await uow.SaveChangesAsync(ct);
    }
}
