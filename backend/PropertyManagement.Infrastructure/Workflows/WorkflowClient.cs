using System.Text;
using System.Text.Json;
using PropertyManagement.Domain.Workflows;

namespace PropertyManagement.Infrastructure.Workflows;

public class WorkflowClient(HttpClient http) : IWorkflowClient
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    private Task Post(string path, object payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, JsonOpts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return http.PostAsync(path, content, ct);
    }

    public Task TriggerLeaseRenewalAsync(Guid leaseId, CancellationToken ct = default) =>
        Post("/workflows/lease-renewal", new { lease_id = leaseId }, ct);

    public Task TriggerMaintenanceWorkflowAsync(Guid requestId, CancellationToken ct = default) =>
        Post("/workflows/maintenance", new { request_id = requestId }, ct);

    public Task TriggerRentCollectionWorkflowAsync(CancellationToken ct = default) =>
        Post("/workflows/rent-collection", new { triggered_at = DateTime.UtcNow }, ct);

    public Task TriggerOnboardingWorkflowAsync(Guid tenantId, Guid leaseId, CancellationToken ct = default) =>
        Post("/workflows/onboarding", new { tenant_id = tenantId, lease_id = leaseId }, ct);

    public Task TriggerInspectionWorkflowAsync(Guid inspectionId, CancellationToken ct = default) =>
        Post("/workflows/inspection", new { inspection_id = inspectionId }, ct);
}
