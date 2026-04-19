using Moq;
using PropertyManagement.Application.Common;
using PropertyManagement.Application.Maintenance.Commands;
using PropertyManagement.Domain.Maintenance;
using PropertyManagement.Domain.Workflows;

namespace PropertyManagement.Tests.Application;

public class CreateMaintenanceRequestHandlerTests
{
    [Fact]
    public async Task Handle_CreatesRequestPersistsAndTriggersWorkflow()
    {
        var repo = new Mock<IMaintenanceRepository>();
        MaintenanceRequest? createdRequest = null;
        repo.Setup(x => x.AddAsync(It.IsAny<MaintenanceRequest>(), It.IsAny<CancellationToken>()))
            .Callback<MaintenanceRequest, CancellationToken>((request, _) => createdRequest = request)
            .Returns(Task.CompletedTask);

        var uow = new Mock<IUnitOfWork>();
        var workflows = new Mock<IWorkflowClient>();

        var handler = new CreateMaintenanceRequestHandler(repo.Object, uow.Object, workflows.Object);

        var requestId = await handler.Handle(
            new CreateMaintenanceRequestCommand(Guid.NewGuid(), "Water leak", "Ceiling leak in bathroom", "Emergency"),
            CancellationToken.None);

        Assert.NotNull(createdRequest);
        Assert.Equal(requestId, createdRequest!.Id);
        Assert.Equal(Priority.Emergency, createdRequest.Priority);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        workflows.Verify(x => x.TriggerMaintenanceWorkflowAsync(requestId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
