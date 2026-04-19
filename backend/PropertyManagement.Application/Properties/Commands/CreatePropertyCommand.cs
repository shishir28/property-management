using MediatR;
using PropertyManagement.Application.Common;
using PropertyManagement.Domain.Properties;

namespace PropertyManagement.Application.Properties.Commands;

public record CreatePropertyCommand(string Address, string City, string State, string Zip, int UnitCount) : IRequest<Guid>;

public class CreatePropertyHandler(IPropertyRepository repo, IUnitOfWork uow) : IRequestHandler<CreatePropertyCommand, Guid>
{
    public async Task<Guid> Handle(CreatePropertyCommand cmd, CancellationToken ct)
    {
        var property = Property.Create(cmd.Address, cmd.City, cmd.State, cmd.Zip, cmd.UnitCount);
        await repo.AddAsync(property, ct);
        await uow.SaveChangesAsync(ct);
        return property.Id;
    }
}
