using MediatR;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Application.Maintenance.Commands;
using PropertyManagement.Application.Maintenance.Queries;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMaintenanceByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-lease/{leaseId:guid}")]
    public async Task<IActionResult> GetByLease(Guid leaseId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetMaintenanceByLeaseQuery(leaseId), ct));

    [HttpGet("open/{priority}")]
    public async Task<IActionResult> GetOpenByPriority(string priority, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOpenMaintenanceByPriorityQuery(priority), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequestCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignRequest req, CancellationToken ct)
    {
        await mediator.Send(new AssignMaintenanceRequestCommand(id, req.Contractor), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ResolveMaintenanceRequestCommand(id), ct);
        return NoContent();
    }
}

public record AssignRequest(string Contractor);
