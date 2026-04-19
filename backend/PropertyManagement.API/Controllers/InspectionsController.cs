using MediatR;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.API.Pagination;
using PropertyManagement.Application.Inspections.Commands;
using PropertyManagement.Application.Inspections.Queries;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InspectionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await mediator.Send(new GetAllInspectionsQuery(), ct));

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default) =>
        Ok((await mediator.Send(new GetAllInspectionsQuery(), ct)).ToPagedResponse(page, pageSize));

    [HttpGet("scheduled")]
    public async Task<IActionResult> GetScheduled(CancellationToken ct) =>
        Ok(await mediator.Send(new GetScheduledInspectionsQuery(), ct));

    [HttpGet("scheduled/paged")]
    public async Task<IActionResult> GetScheduledPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default) =>
        Ok((await mediator.Send(new GetScheduledInspectionsQuery(), ct)).ToPagedResponse(page, pageSize));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetInspectionByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-property/{propertyId:guid}")]
    public async Task<IActionResult> GetByProperty(Guid propertyId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetInspectionsByPropertyQuery(propertyId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInspectionCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteRequest req, CancellationToken ct)
    {
        await mediator.Send(new CompleteInspectionCommand(id, req.Notes), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await mediator.Send(new CancelInspectionCommand(id), ct);
        return NoContent();
    }
}

public record CompleteRequest(string Notes);
