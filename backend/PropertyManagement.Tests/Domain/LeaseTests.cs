using PropertyManagement.Domain.Leases;

namespace PropertyManagement.Tests.Domain;

public class LeaseTests
{
    [Fact]
    public void Create_WithInvalidDates_ThrowsArgumentException()
    {
        var startDate = new DateOnly(2026, 1, 10);
        var endDate = new DateOnly(2026, 1, 10);

        Assert.Throws<ArgumentException>(() =>
            Lease.Create(Guid.NewGuid(), Guid.NewGuid(), "2A", startDate, endDate, 2100m, 2100m));
    }

    [Fact]
    public void Activate_FromDraft_UpdatesStatusAndRaisesDomainEvent()
    {
        var lease = Lease.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "5B",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            2350m,
            2350m);

        lease.ClearDomainEvents();
        lease.Activate();

        Assert.Equal(LeaseStatus.Active, lease.Status);
        Assert.Contains(lease.DomainEvents, evt => evt is LeaseActivatedEvent);
    }

    [Fact]
    public void Renew_WhenLeaseIsActive_UpdatesEndDateAndRent()
    {
        var lease = Lease.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "8C",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            1800m,
            1800m);

        lease.Activate();
        lease.ClearDomainEvents();

        var newEndDate = new DateOnly(2027, 12, 31);
        lease.Renew(newEndDate, 1950m);

        Assert.Equal(newEndDate, lease.EndDate);
        Assert.Equal(1950m, lease.MonthlyRent);
        Assert.Contains(lease.DomainEvents, evt => evt is LeaseRenewedEvent renewed && renewed.NewMonthlyRent == 1950m);
    }
}
