using MediatR;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Application.Properties.Commands;
using PropertyManagement.Application.Properties.Queries;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await mediator.Send(new GetAllPropertiesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPropertyByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePropertyCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
}
