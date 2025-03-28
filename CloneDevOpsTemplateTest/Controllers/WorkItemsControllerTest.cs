using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class WorkItemsControllerTest
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly WorkItemsController _controller;

    public WorkItemsControllerTest()
    {
        _mockWorkItemService = new Mock<IWorkItemService>();
        _controller = new WorkItemsController(_mockWorkItemService.Object);
    }

    [Fact]
    public async Task WorkItems_InvalidModelState_ReturnsEmptyView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.WorkItems(Guid.NewGuid(), "TestProject");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WorkItem[]>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task WorkItems_ValidModelState_ReturnsWorkItems()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectName = "TestProject";

        var workItemQueryList = new WorkItemQueryList
        {
            WorkItems = new List<WorkItemQueryItem>
            {
                new WorkItemQueryItem { Id = 1 },
                new WorkItemQueryItem { Id = 2 }
            }.ToArray()
        };

        var workItem1 = new WorkItem { Id = 1, Fields = new Fields { SystemTitle = "WorkItem1" } };
        var workItem2 = new WorkItem { Id = 2, Fields = new Fields { SystemTitle = "WorkItem2" } };

        _mockWorkItemService
            .Setup(s => s.GetWorkItemsAsync(projectId, projectName))
            .ReturnsAsync(workItemQueryList);

        _mockWorkItemService
            .Setup(s => s.GetWorkItemAsync(projectId, 1))
            .ReturnsAsync(workItem1);

        _mockWorkItemService
            .Setup(s => s.GetWorkItemAsync(projectId, 2))
            .ReturnsAsync(workItem2);

        // Act
        var result = await _controller.WorkItems(projectId, projectName);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WorkItem[]>(viewResult.Model);
        Assert.Equal(2, model.Length);
        Assert.Contains(model, w => w.Id == 1 && w.Fields.SystemTitle == "WorkItem1");
        Assert.Contains(model, w => w.Id == 2 && w.Fields.SystemTitle == "WorkItem2");
    }

    [Fact]
    public async Task WorkItems_ValidModelState_NoWorkItems_ReturnsEmptyView()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectName = "TestProject";

        _mockWorkItemService
            .Setup(s => s.GetWorkItemsAsync(projectId, projectName))
            .ReturnsAsync(new WorkItemQueryList { WorkItems = [] });

        // Act
        var result = await _controller.WorkItems(projectId, projectName);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WorkItem[]>(viewResult.Model);
        Assert.Empty(model);
    }
}