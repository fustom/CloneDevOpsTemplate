using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class IterationControllerTest
{
    private readonly Mock<IIterationService> _mockIterationService;
    private readonly Mock<ICloneManager> _mockCloneManager;
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly IterationController _controller;

    public IterationControllerTest()
    {
        _mockIterationService = new Mock<IIterationService>();
        _mockCloneManager = new Mock<ICloneManager>();
        _mockProjectService = new Mock<IProjectService>();
        _controller = new IterationController(_mockIterationService.Object, _mockCloneManager.Object, _mockProjectService.Object);
    }

    [Fact]
    public async Task Iterations_ReturnsViewResult_WithIterations()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var expectedIterations = new Iteration();
        _mockIterationService
            .Setup(service => service.GetAllAsync(projectId, TreeStructureGroup.Iterations))
            .ReturnsAsync(expectedIterations);

        // Act
        var result = await _controller.Iterations(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedIterations, viewResult.Model);
    }

    [Fact]
    public async Task Iterations_ReturnsViewResult_WithNewIteration_WhenServiceReturnsNull()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockIterationService
            .Setup(service => service.GetAllAsync(projectId, TreeStructureGroup.Iterations))
            .ReturnsAsync((Iteration?)null);

        // Act
        var result = await _controller.Iterations(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<Iteration>(viewResult.Model);
    }

    [Fact]
    public async Task Iterations_ReturnsViewResult_WithNewIteration_WhenModelStateIsInvalid()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.Iterations(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<Iteration>(viewResult.Model);
    }

    [Fact]
    public async Task Areas_ReturnsViewResult_WithAreas()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var expectedAreas = new Iteration();
        _mockIterationService
            .Setup(service => service.GetAllAsync(projectId, TreeStructureGroup.Areas))
            .ReturnsAsync(expectedAreas);

        // Act
        var result = await _controller.Areas(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedAreas, viewResult.Model);
    }

    [Fact]
    public async Task Areas_ReturnsViewResult_WithNewIteration_WhenServiceReturnsNull()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockIterationService
            .Setup(service => service.GetAllAsync(projectId, TreeStructureGroup.Iterations))
            .ReturnsAsync((Iteration?)null);

        // Act
        var result = await _controller.Areas(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<Iteration>(viewResult.Model);
    }

    [Fact]
    public async Task Areas_ReturnsViewResult_WithNewIteration_WhenModelStateIsInvalid()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.Areas(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<Iteration>(viewResult.Model);
    }

    [Fact]
    public async Task CloneIterations_ReturnsViewResult_WithProjects()
    {
        // Arrange
        ProjectBase[] expectedProjects = [];
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync(new Projects { Value = expectedProjects });

        // Act
        var result = await _controller.CloneIterations();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedProjects, viewResult.Model);
    }

    [Fact]
    public async Task CloneIterations_ReturnsViewResult_WithEmptyProjects_WhenServiceReturnsNull()
    {
        // Arrange
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync((Projects?)null);

        // Act
        var result = await _controller.CloneIterations();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        var model = Assert.IsType<ProjectBase[]>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task CloneIterations_Post_ReturnsViewResult_WithSuccessMessage_WhenModelStateIsValid()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        // Act
        var result = await _controller.CloneIterations(templateProjectId, projectId);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Equal("Success", _controller.ViewBag.SuccessMessage);
        _mockCloneManager.Verify(manager => manager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations), Times.Once);
    }

    [Fact]
    public async Task CloneIterations_Post_ReturnsViewResult_WhenModelStateIsInvalid()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneIterations(templateProjectId, projectId);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Null(_controller.ViewBag.SuccessMessage);
        _mockCloneManager.Verify(manager => manager.CloneClassificationNodes(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TreeStructureGroup>()), Times.Never);
    }

    [Fact]
    public async Task CloneAreas_ReturnsViewResult_WithProjects()
    {
        // Arrange
        ProjectBase[] expectedProjects = [];
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync(new Projects { Value = expectedProjects });

        // Act
        var result = await _controller.CloneAreas();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedProjects, viewResult.Model);
    }

    [Fact]
    public async Task CloneAreas_ReturnsViewResult_WithEmptyProjects_WhenServiceReturnsNull()
    {
        // Arrange
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync((Projects?)null);

        // Act
        var result = await _controller.CloneAreas();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        var model = Assert.IsType<ProjectBase[]>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task CloneAreas_Post_ReturnsViewResult_WithSuccessMessage_WhenModelStateIsValid()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        // Act
        var result = await _controller.CloneAreas(templateProjectId, projectId);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Equal("Success", _controller.ViewBag.SuccessMessage);
        _mockCloneManager.Verify(manager => manager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Areas), Times.Once);
    }

    [Fact]
    public async Task CloneAreas_Post_ReturnsViewResult_WhenModelStateIsInvalid()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneAreas(templateProjectId, projectId);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Null(_controller.ViewBag.SuccessMessage);
        _mockCloneManager.Verify(manager => manager.CloneClassificationNodes(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TreeStructureGroup>()), Times.Never);
    }
}