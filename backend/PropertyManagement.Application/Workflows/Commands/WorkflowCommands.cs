using MediatR;
using PropertyManagement.Domain.Workflows;

namespace PropertyManagement.Application.Workflows.Commands;

public record TriggerLeaseRenewalCommand(Guid LeaseId) : IRequest;
public record TriggerInspectionWorkflowCommand(Guid InspectionId) : IRequest;

public class TriggerLeaseRenewalHandler(IWorkflowClient workflows) : IRequestHandler<TriggerLeaseRenewalCommand>
{
    public async Task Handle(TriggerLeaseRenewalCommand cmd, CancellationToken ct)
        => await workflows.TriggerLeaseRenewalAsync(cmd.LeaseId, ct);
}

public class TriggerInspectionWorkflowHandler(IWorkflowClient workflows) : IRequestHandler<TriggerInspectionWorkflowCommand>
{
    public async Task Handle(TriggerInspectionWorkflowCommand cmd, CancellationToken ct)
        => await workflows.TriggerInspectionWorkflowAsync(cmd.InspectionId, ct);
}
