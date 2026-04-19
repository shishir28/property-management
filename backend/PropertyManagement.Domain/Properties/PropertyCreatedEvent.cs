using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Properties;

public record PropertyCreatedEvent(Guid PropertyId) : IDomainEvent;
