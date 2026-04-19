using MediatR;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Application.Leases.Commands;
using PropertyManagement.Application.Leases.Queries;
using PropertyManagement.Application.Workflows.Commands;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeasesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await mediator.Send(new GetAllLeasesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLeaseByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring([FromQuery] int withinDays = 60, CancellationToken ct = default) =>
        Ok(await mediator.Send(new GetExpiringSoonLeasesQuery(withinDays), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaseCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ActivateLeaseCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/renew")]
    public async Task<IActionResult> Renew(Guid id, [FromBody] RenewLeaseRequest req, CancellationToken ct)
    {
        await mediator.Send(new RenewLeaseCommand(id, req.NewEndDate, req.NewMonthlyRent), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/trigger-renewal-workflow")]
    public async Task<IActionResult> TriggerRenewal(Guid id, CancellationToken ct)
    {
        await mediator.Send(new TriggerLeaseRenewalCommand(id), ct);
        return Accepted();
    }
}

public record RenewLeaseRequest(DateOnly NewEndDate, decimal NewMonthlyRent);
