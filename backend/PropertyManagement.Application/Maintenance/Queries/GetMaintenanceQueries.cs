using MediatR;
using PropertyManagement.Domain.Maintenance;

namespace PropertyManagement.Application.Maintenance.Queries;

public record MaintenanceRequestDto(Guid Id, Guid LeaseId, string Title, string Description,
    string Priority, string Status, string? AssignedTo, DateTime ReportedAt, DateTime? ResolvedAt);

public record GetMaintenanceByIdQuery(Guid Id) : IRequest<MaintenanceRequestDto?>;
public record GetMaintenanceByLeaseQuery(Guid LeaseId) : IRequest<IReadOnlyList<MaintenanceRequestDto>>;
public record GetOpenMaintenanceByPriorityQuery(string Priority) : IRequest<IReadOnlyList<MaintenanceRequestDto>>;

file static class Mapper
{
    internal static MaintenanceRequestDto ToDto(MaintenanceRequest r) =>
        new(r.Id, r.LeaseId, r.Title, r.Description, r.Priority.ToString(),
            r.Status.ToString(), r.AssignedTo, r.ReportedAt, r.ResolvedAt);
}

public class GetMaintenanceByIdHandler(IMaintenanceRepository repo) : IRequestHandler<GetMaintenanceByIdQuery, MaintenanceRequestDto?>
{
    public async Task<MaintenanceRequestDto?> Handle(GetMaintenanceByIdQuery q, CancellationToken ct)
    {
        var r = await repo.GetByIdAsync(q.Id, ct);
        return r is null ? null : Mapper.ToDto(r);
    }
}

public class GetMaintenanceByLeaseHandler(IMaintenanceRepository repo) : IRequestHandler<GetMaintenanceByLeaseQuery, IReadOnlyList<MaintenanceRequestDto>>
{
    public async Task<IReadOnlyList<MaintenanceRequestDto>> Handle(GetMaintenanceByLeaseQuery q, CancellationToken ct)
        => (await repo.GetByLeaseAsync(q.LeaseId, ct)).Select(Mapper.ToDto).ToList();
}

public class GetOpenMaintenanceByPriorityHandler(IMaintenanceRepository repo) : IRequestHandler<GetOpenMaintenanceByPriorityQuery, IReadOnlyList<MaintenanceRequestDto>>
{
    public async Task<IReadOnlyList<MaintenanceRequestDto>> Handle(GetOpenMaintenanceByPriorityQuery q, CancellationToken ct)
    {
        var priority = Enum.Parse<Priority>(q.Priority, ignoreCase: true);
        return (await repo.GetOpenByPriorityAsync(priority, ct)).Select(Mapper.ToDto).ToList();
    }
}
