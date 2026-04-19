using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Maintenance;

public record MaintenanceRequestCreatedEvent(Guid RequestId, Guid LeaseId, Priority Priority) : IDomainEvent;
public record MaintenanceRequestAssignedEvent(Guid RequestId, string Contractor) : IDomainEvent;
public record MaintenanceRequestResolvedEvent(Guid RequestId, Guid LeaseId) : IDomainEvent;
