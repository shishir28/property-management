using MediatR;
using PropertyManagement.Application.Common;
using PropertyManagement.Domain.Tenants;

namespace PropertyManagement.Application.Tenants.Commands;

public record CreateTenantCommand(string FirstName, string LastName, string Email, string? Phone) : IRequest<Guid>;
public record ActivateTenantCommand(Guid TenantId) : IRequest;

public class CreateTenantHandler(ITenantRepository repo, IUnitOfWork uow) : IRequestHandler<CreateTenantCommand, Guid>
{
    public async Task<Guid> Handle(CreateTenantCommand cmd, CancellationToken ct)
    {
        var tenant = Tenant.Create(cmd.FirstName, cmd.LastName, cmd.Email, cmd.Phone);
        await repo.AddAsync(tenant, ct);
        await uow.SaveChangesAsync(ct);
        return tenant.Id;
    }
}

public class ActivateTenantHandler(ITenantRepository repo, IUnitOfWork uow) : IRequestHandler<ActivateTenantCommand>
{
    public async Task Handle(ActivateTenantCommand cmd, CancellationToken ct)
    {
        var tenant = await repo.GetByIdAsync(cmd.TenantId, ct)
            ?? throw new KeyNotFoundException($"Tenant {cmd.TenantId} not found.");
        tenant.Activate();
        await repo.UpdateAsync(tenant, ct);
        await uow.SaveChangesAsync(ct);
    }
}
