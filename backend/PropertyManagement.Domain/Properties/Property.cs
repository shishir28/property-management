using PropertyManagement.Domain.Common;

namespace PropertyManagement.Domain.Properties;

public class Property : Entity
{
    public string Address { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string State { get; private set; } = default!;
    public string Zip { get; private set; } = default!;
    public int UnitCount { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Property() { }

    public static Property Create(string address, string city, string state, string zip, int unitCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        if (unitCount < 1) throw new ArgumentException("Unit count must be at least 1.");

        var property = new Property
        {
            Address = address,
            City = city,
            State = state,
            Zip = zip,
            UnitCount = unitCount
        };
        property.RaiseDomainEvent(new PropertyCreatedEvent(property.Id));
        return property;
    }
}
