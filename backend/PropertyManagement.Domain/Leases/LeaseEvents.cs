using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Leases;

public record LeaseCreatedEvent(Guid LeaseId) : IDomainEvent;
public record LeaseActivatedEvent(Guid LeaseId, Guid TenantId) : IDomainEvent;
public record LeaseExpiredEvent(Guid LeaseId, Guid TenantId) : IDomainEvent;
public record LeaseTerminatedEvent(Guid LeaseId, Guid TenantId) : IDomainEvent;
public record LeaseRenewedEvent(Guid LeaseId, Guid TenantId, DateOnly NewEndDate, decimal NewMonthlyRent) : IDomainEvent;
