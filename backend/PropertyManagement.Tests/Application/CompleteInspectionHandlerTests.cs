using Moq;
using PropertyManagement.Application.Common;
using PropertyManagement.Application.Inspections.Commands;
using PropertyManagement.Domain.Inspections;
using PropertyManagement.Domain.Workflows;

namespace PropertyManagement.Tests.Application;

public class CompleteInspectionHandlerTests
{
    [Fact]
    public async Task Handle_CompletesInspectionPersistsAndTriggersInspectionWorkflow()
    {
        var inspection = Inspection.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            InspectionType.Routine,
            DateTime.UtcNow.AddDays(1));

        var repo = new Mock<IInspectionRepository>();
        repo.Setup(x => x.GetByIdAsync(inspection.Id, It.IsAny<CancellationToken>())).ReturnsAsync(inspection);

        var uow = new Mock<IUnitOfWork>();
        var workflows = new Mock<IWorkflowClient>();

        var handler = new CompleteInspectionHandler(repo.Object, uow.Object, workflows.Object);

        await handler.Handle(new CompleteInspectionCommand(inspection.Id, "Minor scuffing near window frame."), CancellationToken.None);

        Assert.Equal(InspectionStatus.Completed, inspection.Status);
        Assert.Equal("Minor scuffing near window frame.", inspection.Notes);
        repo.Verify(x => x.UpdateAsync(inspection, It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        workflows.Verify(x => x.TriggerInspectionWorkflowAsync(inspection.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
