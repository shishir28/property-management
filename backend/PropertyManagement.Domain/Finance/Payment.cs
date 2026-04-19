using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Finance;

public enum PaymentStatus { Pending, Paid, Overdue, Waived }

public class Payment : Entity
{
    public Guid LeaseId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateOnly? PaidDate { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? PaymentMethod { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Payment() { }

    public static Payment Create(Guid leaseId, decimal amount, DateOnly dueDate)
    {
        if (amount <= 0) throw new ArgumentException("Payment amount must be positive.");
        return new Payment { LeaseId = leaseId, Amount = amount, DueDate = dueDate };
    }

    public void MarkPaid(string paymentMethod)
    {
        Status = PaymentStatus.Paid;
        PaidDate = DateOnly.FromDateTime(DateTime.UtcNow);
        PaymentMethod = paymentMethod;
        RaiseDomainEvent(new PaymentReceivedEvent(Id, LeaseId, Amount));
    }

    public void MarkOverdue()
    {
        Status = PaymentStatus.Overdue;
        RaiseDomainEvent(new PaymentOverdueEvent(Id, LeaseId, Amount, DueDate));
    }
}
