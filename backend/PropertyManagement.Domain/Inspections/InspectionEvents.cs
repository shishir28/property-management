using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Inspections;

public record InspectionScheduledEvent(Guid InspectionId, Guid PropertyId, InspectionType Type) : IDomainEvent;
public record InspectionCompletedEvent(Guid InspectionId, Guid PropertyId, Guid? LeaseId, string Notes) : IDomainEvent;
