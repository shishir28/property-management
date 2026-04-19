using MediatR;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Application.Tenants.Commands;
using PropertyManagement.Application.Tenants.Queries;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await mediator.Send(new GetAllTenantsQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTenantByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ActivateTenantCommand(id), ct);
        return NoContent();
    }
}
