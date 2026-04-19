using MediatR;
using PropertyManagement.Domain.Inspections;

namespace PropertyManagement.Application.Inspections.Queries;

public record InspectionDto(Guid Id, Guid PropertyId, Guid? LeaseId, string Type,
    string Status, DateTime ScheduledAt, DateTime? CompletedAt, string? Notes);

public record GetInspectionByIdQuery(Guid Id) : IRequest<InspectionDto?>;
public record GetAllInspectionsQuery : IRequest<IReadOnlyList<InspectionDto>>;
public record GetScheduledInspectionsQuery : IRequest<IReadOnlyList<InspectionDto>>;
public record GetInspectionsByPropertyQuery(Guid PropertyId) : IRequest<IReadOnlyList<InspectionDto>>;

file static class Mapper
{
    internal static InspectionDto ToDto(Inspection i) =>
        new(i.Id, i.PropertyId, i.LeaseId, i.Type.ToString(),
            i.Status.ToString(), i.ScheduledAt, i.CompletedAt, i.Notes);
}

public class GetInspectionByIdHandler(IInspectionRepository repo) : IRequestHandler<GetInspectionByIdQuery, InspectionDto?>
{
    public async Task<InspectionDto?> Handle(GetInspectionByIdQuery q, CancellationToken ct)
    {
        var i = await repo.GetByIdAsync(q.Id, ct);
        return i is null ? null : Mapper.ToDto(i);
    }
}

public class GetAllInspectionsHandler(IInspectionRepository repo) : IRequestHandler<GetAllInspectionsQuery, IReadOnlyList<InspectionDto>>
{
    public async Task<IReadOnlyList<InspectionDto>> Handle(GetAllInspectionsQuery q, CancellationToken ct)
        => (await repo.GetAllAsync(ct)).Select(Mapper.ToDto).ToList();
}

public class GetScheduledInspectionsHandler(IInspectionRepository repo) : IRequestHandler<GetScheduledInspectionsQuery, IReadOnlyList<InspectionDto>>
{
    public async Task<IReadOnlyList<InspectionDto>> Handle(GetScheduledInspectionsQuery q, CancellationToken ct)
        => (await repo.GetScheduledAsync(ct)).Select(Mapper.ToDto).ToList();
}

public class GetInspectionsByPropertyHandler(IInspectionRepository repo) : IRequestHandler<GetInspectionsByPropertyQuery, IReadOnlyList<InspectionDto>>
{
    public async Task<IReadOnlyList<InspectionDto>> Handle(GetInspectionsByPropertyQuery q, CancellationToken ct)
        => (await repo.GetByPropertyAsync(q.PropertyId, ct)).Select(Mapper.ToDto).ToList();
}
