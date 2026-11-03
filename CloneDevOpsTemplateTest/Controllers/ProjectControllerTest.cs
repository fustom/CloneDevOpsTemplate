using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class ProjectControllerTest
{
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<ICloneManager> _mockCloneManager;
    private readonly MigrationProgressTracker _progressTracker = new();
    private readonly ProjectController _controller;

    public ProjectControllerTest()
    {
        _mockProjectService = new Mock<IProjectService>();
        _mockCloneManager = new Mock<ICloneManager>();

        _controller = new ProjectController(
            _mockProjectService.Object,
            _mockCloneManager.Object,
            _progressTracker
        );
    }

    [Fact]
    public async Task Projects_ReturnsViewWithProjects()
    {
        // Arrange
        var projects = new Projects { Value = [] };
        _mockProjectService.Setup(service => service.GetAllProjectsAsync()).ReturnsAsync(projects);

        // Act
        var result = await _controller.Projects();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(projects.Value, viewResult.Model);
    }

    [Fact]
    public async Task Projects_InvalidModelState_ReturnsDefaultView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.Projects();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ProjectBase[]>(viewResult.Model);
    }

    [Fact]
    public async Task Project_ReturnsViewWithProject()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new Project();
        _mockProjectService.Setup(service => service.GetProjectAsync(projectId)).ReturnsAsync(project);

        // Act
        var result = await _controller.Project(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(project, viewResult.Model);
    }

    [Fact]
    public async Task Project_InvalidModelState_ReturnsDefaultView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.Project(Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<Project>(viewResult.Model);
        Assert.True(viewResult.ViewData.ModelState.ErrorCount > 0);
    }

    [Fact]
    public async Task ProjectProperties_ReturnsViewWithProjectProperties()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectProperties = new ProjectProperties { Value = [] };
        _mockProjectService.Setup(service => service.GetProjectPropertiesAsync(projectId)).ReturnsAsync(projectProperties);

        // Act
        var result = await _controller.ProjectProperties(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(projectProperties.Value, viewResult.Model);
    }

    [Fact]
    public async Task ProjectProperties_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.ProjectProperties(Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ProjectProperty[]>(viewResult.Model);
        Assert.Empty(viewModel);
        Assert.True(viewResult.ViewData.ModelState.ErrorCount > 0);
    }

    [Fact]
    public async Task ProjectProperties_NullProjectProperties_ReturnsViewWithEmptyList()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockProjectService.Setup(service => service.GetProjectPropertiesAsync(projectId)).ReturnsAsync((ProjectProperties?)null);

        // Act
        var result = await _controller.ProjectProperties(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ProjectProperty[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task CreateProject_InvalidModelState_ReturnsCreateProjectView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CreateProject(Guid.NewGuid(), "New Project", "Description", Visibility.Private);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(ProjectController.CreateProject), viewResult.ViewName);
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Equal(nameof(ProjectController.CreateProject), model.Action);
    }

    [Fact]
    public async Task CreateProject_WithFinishedOperation_ShowsFinishedMigration()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Fail(operationId, "Boom");

        // Act
        var result = await _controller.CreateProject(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.NotNull(model.FinishedMigration);
        Assert.Equal(MigrationStatus.Failed, model.FinishedMigration.Status);
        Assert.Equal("Boom", model.FinishedMigration.Message);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CreateProject_ValidInput_RedirectsToProject()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var newProjectName = "New Project";
        var description = "Description";
        var visibility = Visibility.Private;

        var project = new Project { Id = Guid.NewGuid() };
        var templateProject = new Project();
        string? message = null;

        _mockCloneManager
            .Setup(manager => manager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneProjectResult { Project = project, TemplateProject = templateProject, ErrorMessage = message });

        _mockCloneManager
            .Setup(manager => manager.CloneClassificationNodes(templateProjectId, project.Id, TreeStructureGroup.Iterations, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneResult());

        _mockCloneManager
            .Setup(manager => manager.CloneClassificationNodes(templateProjectId, project.Id, TreeStructureGroup.Areas, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneResult());

        _mockCloneManager
            .Setup(manager => manager.CloneTeamsAndSettingsAndBoardsAsync(templateProject, project, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneResult());

        _mockCloneManager
            .Setup(manager => manager.CloneRepositoriesAsync(templateProjectId, project.Id, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneResult());

        _mockCloneManager
            .Setup(manager => manager.CloneGitPullRequestsAsync(templateProjectId, project.Id, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneResult());

        var operationId = Guid.NewGuid();

        // Act
        var result = await _controller.CreateProject(templateProjectId, newProjectName, description, visibility, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Project", redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(project.Id, redirectResult.RouteValues["projectId"]);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CreateProject_CloneProjectReturnsMessage_FailsProgressAndRedirects()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var newProjectName = "New Project";
        var description = "Description";
        var visibility = Visibility.Private;

        var project = new Project();
        var templateProject = new Project();
        string? message = "Error cloning project";

        _mockCloneManager
            .Setup(manager => manager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneProjectResult { Project = project, TemplateProject = templateProject, ErrorMessage = message });

        var operationId = Guid.NewGuid();

        // Act
        var result = await _controller.CreateProject(templateProjectId, newProjectName, description, visibility, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProjectController.CreateProject), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        Assert.False(_controller.ModelState.ContainsKey("ErrorMessage"));
        var progress = _progressTracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(MigrationStatus.Failed, progress.Status);
        Assert.Equal($"Creating project failed: {message}", progress.Message);
        Assert.Equal(1, progress.Step);
        _mockCloneManager.Verify(manager => manager.CloneClassificationNodes(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TreeStructureGroup>(), It.IsAny<IProgress<string>?>()), Times.Never);
        _mockCloneManager.Verify(manager => manager.CloneGitPullRequestsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IProgress<string>?>()), Times.Never);
    }

    [Fact]
    public async Task CreateProject_StepReturnsErrors_FailsAtThatStepAndSkipsRemainingSteps()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid() };

        _mockCloneManager
            .Setup(manager => manager.CloneProjectAsync(templateProjectId, "New Project", "Description", Visibility.Private, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneProjectResult { Project = project, TemplateProject = new Project() });
        _mockCloneManager
            .Setup(manager => manager.CloneClassificationNodes(templateProjectId, project.Id, TreeStructureGroup.Iterations, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(CloneResult.FromError("Iterations: could not read the template project's nodes."));
        _mockCloneManager
            .Setup(manager => manager.CloneClassificationNodes(templateProjectId, project.Id, TreeStructureGroup.Areas, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneResult());

        // Act
        var result = await _controller.CreateProject(templateProjectId, "New Project", "Description", Visibility.Private, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProjectController.CreateProject), redirectResult.ActionName);
        var progress = _progressTracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(MigrationStatus.Failed, progress.Status);
        Assert.Equal(2, progress.Step);
        Assert.Equal("Cloning iterations and areas failed: Iterations: could not read the template project's nodes.", progress.Message);
        _mockCloneManager.Verify(manager => manager.CloneTeamsAndSettingsAndBoardsAsync(It.IsAny<Project>(), It.IsAny<Project>(), It.IsAny<IProgress<string>?>()), Times.Never);
        _mockCloneManager.Verify(manager => manager.CloneGitPullRequestsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IProgress<string>?>()), Times.Never);
    }

    [Fact]
    public async Task CloneProject_ReturnsViewWithProjects()
    {
        // Arrange
        var projects = new Projects { Value = [] };
        _mockProjectService.Setup(service => service.GetAllProjectsAsync()).ReturnsAsync(projects);

        // Act
        var result = await _controller.CloneProject();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Equal(projects.Value, model.Items);
        Assert.Equal("Project", model.Controller);
        Assert.Equal(nameof(ProjectController.CloneProject), model.Action);
        Assert.Null(model.FinishedMigration);
    }

    [Fact]
    public async Task CloneProject_WithCompletedOperation_ShowsFinishedMigrationOnce()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Start(operationId, 1);
        _progressTracker.Complete(operationId, "Done");

        // Act
        var firstResult = await _controller.CloneProject(operationId);
        var firstModel = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(firstResult).Model);
        var firstFinishedMigration = firstModel.FinishedMigration;
        var secondResult = await _controller.CloneProject(operationId);

        // Assert
        Assert.NotNull(firstFinishedMigration);
        Assert.Equal("Done", firstFinishedMigration.Message);
        var secondModel = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(secondResult).Model);
        Assert.Null(secondModel.FinishedMigration);
    }

    [Fact]
    public async Task CloneProject_WithRunningOperation_DoesNotShowOrRemoveIt()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Start(operationId, 1);

        // Act
        var result = await _controller.CloneProject(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Null(model.FinishedMigration);
        Assert.NotNull(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CloneProject_InvalidModelState_ReturnsCloneProjectView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneProject(Guid.NewGuid(), "New Project", "Description", Visibility.Private);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(ProjectController.CloneProject), viewResult.ViewName);
        Assert.True(_controller.ModelState.ContainsKey("Error"));
        Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        _mockCloneManager.Verify(manager => manager.CloneProjectAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Visibility>(), It.IsAny<IProgress<string>?>()), Times.Never);
    }

    [Fact]
    public async Task CloneProject_CloneProjectReturnsMessage_FailsProgressAndRedirects()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var newProjectName = "New Project";
        var description = "Description";
        var visibility = Visibility.Private;
        var message = "Error cloning project";

        _mockCloneManager
            .Setup(manager => manager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneProjectResult { Project = null!, TemplateProject = null!, ErrorMessage = message });

        // Act
        var result = await _controller.CloneProject(templateProjectId, newProjectName, description, visibility, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProjectController.CloneProject), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        Assert.False(_controller.ModelState.ContainsKey("ErrorMessage"));
        var progress = _progressTracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(MigrationStatus.Failed, progress.Status);
        Assert.Equal($"Creating project failed: {message}", progress.Message);
    }

    [Fact]
    public async Task CloneProject_WithoutOperationId_GeneratesOneSoResultIsTracked()
    {
        // Arrange
        _mockCloneManager
            .Setup(manager => manager.CloneProjectAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Visibility>(), It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneProjectResult { Project = null!, TemplateProject = null!, ErrorMessage = null });

        // Act
        var result = await _controller.CloneProject(Guid.NewGuid(), "New Project", "Description", Visibility.Private);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        var operationId = Assert.IsType<Guid>(redirectResult.RouteValues?["operationId"]);
        Assert.NotEqual(Guid.Empty, operationId);
        Assert.Equal(MigrationStatus.Succeeded, _progressTracker.Get(operationId)?.Status);
    }

    [Fact]
    public async Task CloneProject_CloneProjectThrows_RemovesProgress()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _mockCloneManager
            .Setup(manager => manager.CloneProjectAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Visibility>(), It.IsAny<IProgress<string>?>()))
            .ThrowsAsync(new TimeoutException());

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => _controller.CloneProject(Guid.NewGuid(), "New Project", "Description", Visibility.Private, operationId));
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CloneProject_CloneProjectSuccess_CompletesProgressAndRedirects()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var newProjectName = "New Project";
        var description = "Description";
        var visibility = Visibility.Private;

        _mockCloneManager
            .Setup(manager => manager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility, It.IsAny<IProgress<string>?>()))
            .ReturnsAsync(new CloneProjectResult { Project = null!, TemplateProject = null!, ErrorMessage = null });

        // Act
        var result = await _controller.CloneProject(templateProjectId, newProjectName, description, visibility, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProjectController.CloneProject), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);

        var progress = _progressTracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(MigrationStatus.Succeeded, progress.Status);
        Assert.Equal("Project 'New Project' cloned successfully.", progress.Message);
    }
}
