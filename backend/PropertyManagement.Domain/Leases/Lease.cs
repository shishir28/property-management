using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Leases;

public enum LeaseStatus { Draft, Active, Expired, Terminated }

public class Lease : Entity
{
    public Guid PropertyId { get; private set; }
    public Guid TenantId { get; private set; }
    public string UnitNumber { get; private set; } = default!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal MonthlyRent { get; private set; }
    public decimal SecurityDeposit { get; private set; }
    public LeaseStatus Status { get; private set; } = LeaseStatus.Draft;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public bool IsExpiringSoon(int withinDays = 60) =>
        Status == LeaseStatus.Active && EndDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(withinDays));

    private Lease() { }

    public static Lease Create(Guid propertyId, Guid tenantId, string unitNumber,
        DateOnly startDate, DateOnly endDate, decimal monthlyRent, decimal securityDeposit)
    {
        if (endDate <= startDate) throw new ArgumentException("End date must be after start date.");
        if (monthlyRent <= 0) throw new ArgumentException("Monthly rent must be positive.");

        var lease = new Lease
        {
            PropertyId = propertyId,
            TenantId = tenantId,
            UnitNumber = unitNumber,
            StartDate = startDate,
            EndDate = endDate,
            MonthlyRent = monthlyRent,
            SecurityDeposit = securityDeposit
        };
        lease.RaiseDomainEvent(new LeaseCreatedEvent(lease.Id));
        return lease;
    }

    public void Activate()
    {
        if (Status != LeaseStatus.Draft) throw new InvalidOperationException("Only draft leases can be activated.");
        Status = LeaseStatus.Active;
        RaiseDomainEvent(new LeaseActivatedEvent(Id, TenantId));
    }

    public void Expire()
    {
        Status = LeaseStatus.Expired;
        RaiseDomainEvent(new LeaseExpiredEvent(Id, TenantId));
    }

    public void Terminate()
    {
        Status = LeaseStatus.Terminated;
        RaiseDomainEvent(new LeaseTerminatedEvent(Id, TenantId));
    }

    public void Renew(DateOnly newEndDate, decimal newMonthlyRent)
    {
        if (Status != LeaseStatus.Active) throw new InvalidOperationException("Only active leases can be renewed.");
        EndDate = newEndDate;
        MonthlyRent = newMonthlyRent;
        RaiseDomainEvent(new LeaseRenewedEvent(Id, TenantId, newEndDate, newMonthlyRent));
    }
}
