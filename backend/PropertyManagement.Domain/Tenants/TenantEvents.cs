using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Tenants;

public record TenantCreatedEvent(Guid TenantId) : IDomainEvent;
public record TenantActivatedEvent(Guid TenantId) : IDomainEvent;
