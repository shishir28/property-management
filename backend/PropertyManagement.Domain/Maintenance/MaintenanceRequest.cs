using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Maintenance;

public enum Priority { Routine, Urgent, Emergency }
public enum MaintenanceStatus { Open, Assigned, InProgress, Resolved, Closed }

public class MaintenanceRequest : Entity
{
    public Guid LeaseId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public Priority Priority { get; private set; }
    public MaintenanceStatus Status { get; private set; } = MaintenanceStatus.Open;
    public string? AssignedTo { get; private set; }
    public DateTime ReportedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; private set; }

    private MaintenanceRequest() { }

    public static MaintenanceRequest Create(Guid leaseId, string title, string description, Priority priority)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var request = new MaintenanceRequest
        {
            LeaseId = leaseId, Title = title, Description = description, Priority = priority
        };
        request.RaiseDomainEvent(new MaintenanceRequestCreatedEvent(request.Id, leaseId, priority));
        return request;
    }

    public void Assign(string contractor)
    {
        AssignedTo = contractor;
        Status = MaintenanceStatus.Assigned;
        RaiseDomainEvent(new MaintenanceRequestAssignedEvent(Id, contractor));
    }

    public void StartWork() => Status = MaintenanceStatus.InProgress;

    public void Resolve()
    {
        Status = MaintenanceStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        RaiseDomainEvent(new MaintenanceRequestResolvedEvent(Id, LeaseId));
    }
}
