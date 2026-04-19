using MediatR;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Application.Finance.Commands;
using PropertyManagement.Application.Finance.Queries;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("by-lease/{leaseId:guid}")]
    public async Task<IActionResult> GetByLease(Guid leaseId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetPaymentsByLeaseQuery(leaseId), ct));

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue(CancellationToken ct) =>
        Ok(await mediator.Send(new GetOverduePaymentsQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetByLease), new { leaseId = cmd.LeaseId }, new { id });
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkPaidRequest req, CancellationToken ct)
    {
        await mediator.Send(new MarkPaymentPaidCommand(id, req.PaymentMethod), ct);
        return NoContent();
    }

    [HttpPost("run-collection")]
    public async Task<IActionResult> RunCollection(CancellationToken ct)
    {
        await mediator.Send(new RunRentCollectionCommand(), ct);
        return Accepted();
    }
}

public record MarkPaidRequest(string PaymentMethod);
