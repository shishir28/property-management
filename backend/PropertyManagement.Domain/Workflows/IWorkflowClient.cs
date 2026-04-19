namespace PropertyManagement.Domain.Workflows;

public interface IWorkflowClient
{
    Task TriggerLeaseRenewalAsync(Guid leaseId, CancellationToken ct = default);
    Task TriggerMaintenanceWorkflowAsync(Guid requestId, CancellationToken ct = default);
    Task TriggerRentCollectionWorkflowAsync(CancellationToken ct = default);
    Task TriggerOnboardingWorkflowAsync(Guid tenantId, Guid leaseId, CancellationToken ct = default);
    Task TriggerInspectionWorkflowAsync(Guid inspectionId, CancellationToken ct = default);
}
