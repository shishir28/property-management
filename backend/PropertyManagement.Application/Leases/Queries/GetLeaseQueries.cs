using MediatR;
using PropertyManagement.Domain.Leases;

namespace PropertyManagement.Application.Leases.Queries;

public record LeaseDto(Guid Id, Guid PropertyId, Guid TenantId, string UnitNumber,
    DateOnly StartDate, DateOnly EndDate, decimal MonthlyRent, decimal SecurityDeposit, string Status);

public record GetLeaseByIdQuery(Guid Id) : IRequest<LeaseDto?>;
public record GetAllLeasesQuery : IRequest<IReadOnlyList<LeaseDto>>;
public record GetExpiringSoonLeasesQuery(int WithinDays = 60) : IRequest<IReadOnlyList<LeaseDto>>;

file static class Mapper
{
    internal static LeaseDto ToDto(Lease l) => new(l.Id, l.PropertyId, l.TenantId, l.UnitNumber,
        l.StartDate, l.EndDate, l.MonthlyRent, l.SecurityDeposit, l.Status.ToString());
}

public class GetLeaseByIdHandler(ILeaseRepository repo) : IRequestHandler<GetLeaseByIdQuery, LeaseDto?>
{
    public async Task<LeaseDto?> Handle(GetLeaseByIdQuery q, CancellationToken ct)
    {
        var l = await repo.GetByIdAsync(q.Id, ct);
        return l is null ? null : Mapper.ToDto(l);
    }
}

public class GetAllLeasesHandler(ILeaseRepository repo) : IRequestHandler<GetAllLeasesQuery, IReadOnlyList<LeaseDto>>
{
    public async Task<IReadOnlyList<LeaseDto>> Handle(GetAllLeasesQuery q, CancellationToken ct)
        => (await repo.GetAllAsync(ct)).Select(Mapper.ToDto).ToList();
}

public class GetExpiringSoonLeasesHandler(ILeaseRepository repo) : IRequestHandler<GetExpiringSoonLeasesQuery, IReadOnlyList<LeaseDto>>
{
    public async Task<IReadOnlyList<LeaseDto>> Handle(GetExpiringSoonLeasesQuery q, CancellationToken ct)
        => (await repo.GetExpiringSoonAsync(q.WithinDays, ct)).Select(Mapper.ToDto).ToList();
}
