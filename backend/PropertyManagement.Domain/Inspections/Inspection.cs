using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Inspections;

public enum InspectionType { MoveIn, MoveOut, Routine, Emergency }
public enum InspectionStatus { Scheduled, Completed, Cancelled }

public class Inspection : Entity
{
    public Guid PropertyId { get; private set; }
    public Guid? LeaseId { get; private set; }
    public InspectionType Type { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }
    public InspectionStatus Status { get; private set; } = InspectionStatus.Scheduled;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Inspection() { }

    public static Inspection Create(Guid propertyId, Guid? leaseId, InspectionType type, DateTime scheduledAt)
    {
        var inspection = new Inspection
        {
            PropertyId = propertyId,
            LeaseId = leaseId,
            Type = type,
            ScheduledAt = scheduledAt,
        };
        inspection.RaiseDomainEvent(new InspectionScheduledEvent(inspection.Id, propertyId, type));
        return inspection;
    }

    public void Complete(string notes)
    {
        Status = InspectionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Notes = notes;
        RaiseDomainEvent(new InspectionCompletedEvent(Id, PropertyId, LeaseId, notes));
    }

    public void Cancel() => Status = InspectionStatus.Cancelled;
}
