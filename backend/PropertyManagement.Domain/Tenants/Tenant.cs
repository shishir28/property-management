using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Tenants;

public enum TenantStatus { Applicant, Active, Former }

public class Tenant : Entity
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Phone { get; private set; }
    public TenantStatus Status { get; private set; } = TenantStatus.Applicant;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}";

    private Tenant() { }

    public static Tenant Create(string firstName, string lastName, string email, string? phone = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var tenant = new Tenant { FirstName = firstName, LastName = lastName, Email = email, Phone = phone };
        tenant.RaiseDomainEvent(new TenantCreatedEvent(tenant.Id));
        return tenant;
    }

    public void Activate() { Status = TenantStatus.Active; RaiseDomainEvent(new TenantActivatedEvent(Id)); }
    public void Deactivate() { Status = TenantStatus.Former; }
}
