using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Finance;

public record PaymentReceivedEvent(Guid PaymentId, Guid LeaseId, decimal Amount) : IDomainEvent;
public record PaymentOverdueEvent(Guid PaymentId, Guid LeaseId, decimal Amount, DateOnly DueDate) : IDomainEvent;
