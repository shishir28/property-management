using MediatR;
using PropertyManagement.Domain.Properties;

namespace PropertyManagement.Application.Properties.Queries;

public record PropertyDto(Guid Id, string Address, string City, string State, string Zip, int UnitCount);

public record GetPropertyByIdQuery(Guid Id) : IRequest<PropertyDto?>;
public record GetAllPropertiesQuery : IRequest<IReadOnlyList<PropertyDto>>;

file static class Mapper
{
    internal static PropertyDto ToDto(Property p) => new(p.Id, p.Address, p.City, p.State, p.Zip, p.UnitCount);
}

public class GetPropertyByIdHandler(IPropertyRepository repo) : IRequestHandler<GetPropertyByIdQuery, PropertyDto?>
{
    public async Task<PropertyDto?> Handle(GetPropertyByIdQuery q, CancellationToken ct)
    {
        var p = await repo.GetByIdAsync(q.Id, ct);
        return p is null ? null : Mapper.ToDto(p);
    }
}

public class GetAllPropertiesHandler(IPropertyRepository repo) : IRequestHandler<GetAllPropertiesQuery, IReadOnlyList<PropertyDto>>
{
    public async Task<IReadOnlyList<PropertyDto>> Handle(GetAllPropertiesQuery q, CancellationToken ct)
    {
        var props = await repo.GetAllAsync(ct);
        return props.Select(Mapper.ToDto).ToList();
    }
}
