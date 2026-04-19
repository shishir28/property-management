using Moq;
using PropertyManagement.Application.Common;
using PropertyManagement.Application.Leases.Commands;
using PropertyManagement.Domain.Leases;
using PropertyManagement.Domain.Workflows;

namespace PropertyManagement.Tests.Application;

public class ActivateLeaseHandlerTests
{
    [Fact]
    public async Task Handle_ActivatesLeasePersistsAndTriggersOnboardingWorkflow()
    {
        var lease = Lease.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "11D",
            new DateOnly(2026, 2, 1),
            new DateOnly(2027, 1, 31),
            2400m,
            2400m);

        var repo = new Mock<ILeaseRepository>();
        repo.Setup(x => x.GetByIdAsync(lease.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lease);

        var uow = new Mock<IUnitOfWork>();
        var workflows = new Mock<IWorkflowClient>();

        var handler = new ActivateLeaseHandler(repo.Object, uow.Object, workflows.Object);

        await handler.Handle(new ActivateLeaseCommand(lease.Id), CancellationToken.None);

        Assert.Equal(LeaseStatus.Active, lease.Status);
        repo.Verify(x => x.UpdateAsync(lease, It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        workflows.Verify(x => x.TriggerOnboardingWorkflowAsync(lease.TenantId, lease.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLeaseDoesNotExist_ThrowsKeyNotFoundException()
    {
        var repo = new Mock<ILeaseRepository>();
        repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lease?)null);

        var handler = new ActivateLeaseHandler(repo.Object, Mock.Of<IUnitOfWork>(), Mock.Of<IWorkflowClient>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new ActivateLeaseCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
