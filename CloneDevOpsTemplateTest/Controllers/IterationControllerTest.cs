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
    private readonly MigrationProgressTracker _progressTracker = new();
    private readonly IterationController _controller;

    public IterationControllerTest()
    {
        _mockIterationService = new Mock<IIterationService>();
        _mockCloneManager = new Mock<ICloneManager>();
        _mockProjectService = new Mock<IProjectService>();
        _controller = new IterationController(_mockIterationService.Object, _mockCloneManager.Object, _mockProjectService.Object, _progressTracker);
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
            .Setup(service => service.GetAllAsync(projectId, TreeStructureGroup.Areas))
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
    public async Task CloneIterations_Get_ReturnsCloneFormWithItems()
    {
        // Arrange
        _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync(new Projects { Value = [new ProjectBase { Name = "Source" }] });

        // Act
        var result = await _controller.CloneIterations();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(IterationController.CloneIterations), viewResult.ViewName);
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Equal("Source", Assert.Single(model.Items).Name);
        Assert.Equal("Iteration", model.Controller);
        Assert.Equal(nameof(IterationController.CloneIterations), model.Action);
        Assert.Null(model.FinishedMigration);
    }

    [Fact]
    public async Task CloneIterations_Get_NullItems_ReturnsEmptyCloneForm()
    {
        // Arrange
        _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync((Projects?)null);

        // Act
        var result = await _controller.CloneIterations();

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Items);
    }

    [Fact]
    public async Task CloneIterations_Get_WithFinishedOperation_ShowsItOnce()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Fail(operationId, "Boom");

        // Act
        var result = await _controller.CloneIterations(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(MigrationStatus.Failed, model.FinishedMigration?.Status);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CloneIterations_Post_InvalidModelState_ReturnsCloneFormWithoutCloning()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneIterations(templateProjectId, projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(IterationController.CloneIterations), viewResult.ViewName);
        Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Empty(_mockCloneManager.Invocations);
    }

    [Fact]
    public async Task CloneIterations_Post_Success_CompletesProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        _mockCloneManager.Setup(m => m.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations, It.IsAny<IProgress<string>?>())).ReturnsAsync(new CloneResult());

        // Act
        var result = await _controller.CloneIterations(templateProjectId, projectId, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(IterationController.CloneIterations), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Succeeded, progress?.Status);
        Assert.Equal("Iterations cloned successfully.", progress?.Message);
        _mockCloneManager.Verify(m => m.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations, It.IsAny<IProgress<string>?>()), Times.Once);
    }

    [Fact]
    public async Task CloneIterations_Post_Failure_FailsProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var failure = new CloneResult();
        failure.AddError("Boom");
        _mockCloneManager.Setup(m => m.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations, It.IsAny<IProgress<string>?>())).ReturnsAsync(failure);

        // Act
        var result = await _controller.CloneIterations(templateProjectId, projectId, operationId);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Equal("Cloning iterations failed: Boom", progress?.Message);
        Assert.Equal(0, _controller.ModelState.ErrorCount);
    }

    [Fact]
    public async Task CloneAreas_Get_ReturnsCloneFormWithItems()
    {
        // Arrange
        _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync(new Projects { Value = [new ProjectBase { Name = "Source" }] });

        // Act
        var result = await _controller.CloneAreas();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(IterationController.CloneAreas), viewResult.ViewName);
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Equal("Source", Assert.Single(model.Items).Name);
        Assert.Equal("Iteration", model.Controller);
        Assert.Equal(nameof(IterationController.CloneAreas), model.Action);
        Assert.Null(model.FinishedMigration);
    }

    [Fact]
    public async Task CloneAreas_Get_NullItems_ReturnsEmptyCloneForm()
    {
        // Arrange
        _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync((Projects?)null);

        // Act
        var result = await _controller.CloneAreas();

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Items);
    }

    [Fact]
    public async Task CloneAreas_Get_WithFinishedOperation_ShowsItOnce()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Fail(operationId, "Boom");

        // Act
        var result = await _controller.CloneAreas(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(MigrationStatus.Failed, model.FinishedMigration?.Status);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CloneAreas_Post_InvalidModelState_ReturnsCloneFormWithoutCloning()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneAreas(templateProjectId, projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(IterationController.CloneAreas), viewResult.ViewName);
        Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Empty(_mockCloneManager.Invocations);
    }

    [Fact]
    public async Task CloneAreas_Post_Success_CompletesProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        _mockCloneManager.Setup(m => m.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Areas, It.IsAny<IProgress<string>?>())).ReturnsAsync(new CloneResult());

        // Act
        var result = await _controller.CloneAreas(templateProjectId, projectId, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(IterationController.CloneAreas), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Succeeded, progress?.Status);
        Assert.Equal("Areas cloned successfully.", progress?.Message);
        _mockCloneManager.Verify(m => m.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Areas, It.IsAny<IProgress<string>?>()), Times.Once);
    }

    [Fact]
    public async Task CloneAreas_Post_Failure_FailsProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var failure = new CloneResult();
        failure.AddError("Boom");
        _mockCloneManager.Setup(m => m.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Areas, It.IsAny<IProgress<string>?>())).ReturnsAsync(failure);

        // Act
        var result = await _controller.CloneAreas(templateProjectId, projectId, operationId);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Equal("Cloning areas failed: Boom", progress?.Message);
        Assert.Equal(0, _controller.ModelState.ErrorCount);
    }
}
