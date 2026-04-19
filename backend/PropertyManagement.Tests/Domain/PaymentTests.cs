using PropertyManagement.Domain.Finance;

namespace PropertyManagement.Tests.Domain;

public class PaymentTests
{
    [Fact]
    public void MarkPaid_SetsPaidFieldsAndRaisesDomainEvent()
    {
        var payment = Payment.Create(Guid.NewGuid(), 875m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)));
        payment.ClearDomainEvents();

        payment.MarkPaid("manual");

        Assert.Equal(PaymentStatus.Paid, payment.Status);
        Assert.Equal("manual", payment.PaymentMethod);
        Assert.NotNull(payment.PaidDate);
        Assert.Contains(payment.DomainEvents, evt => evt is PaymentReceivedEvent);
    }
}
