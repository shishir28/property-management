using MediatR;
using PropertyManagement.Domain.Tenants;

namespace PropertyManagement.Application.Tenants.Queries;

public record TenantDto(Guid Id, string FullName, string Email, string? Phone, string Status);

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto?>;
public record GetAllTenantsQuery : IRequest<IReadOnlyList<TenantDto>>;

file static class Mapper
{
    internal static TenantDto ToDto(Tenant t) => new(t.Id, t.FullName, t.Email, t.Phone, t.Status.ToString());
}

public class GetTenantByIdHandler(ITenantRepository repo) : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    public async Task<TenantDto?> Handle(GetTenantByIdQuery q, CancellationToken ct)
    {
        var t = await repo.GetByIdAsync(q.Id, ct);
        return t is null ? null : Mapper.ToDto(t);
    }
}

public class GetAllTenantsHandler(ITenantRepository repo) : IRequestHandler<GetAllTenantsQuery, IReadOnlyList<TenantDto>>
{
    public async Task<IReadOnlyList<TenantDto>> Handle(GetAllTenantsQuery q, CancellationToken ct)
    {
        var tenants = await repo.GetAllAsync(ct);
        return tenants.Select(Mapper.ToDto).ToList();
    }
}
