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
    private readonly ProjectController _controller;

    public ProjectControllerTest()
    {
        _mockProjectService = new Mock<IProjectService>();
        _mockCloneManager = new Mock<ICloneManager>();

        _controller = new ProjectController(
            _mockProjectService.Object,
            _mockCloneManager.Object
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
        Assert.IsType<ProjectBase[]>(viewResult.Model);
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
            .Setup(manager => manager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility))
            .ReturnsAsync(Tuple.Create(project, templateProject, message));

        _mockCloneManager
            .Setup(manager => manager.CloneClassificationNodes(templateProjectId, project.Id, TreeStructureGroup.Iterations))
            .Returns(Task.CompletedTask);

        _mockCloneManager
            .Setup(manager => manager.CloneClassificationNodes(templateProjectId, project.Id, TreeStructureGroup.Areas))
            .Returns(Task.CompletedTask);

        _mockCloneManager
            .Setup(manager => manager.CloneTeamsAndSettingsAndBoardsAsync(templateProject, project))
            .Returns(Task.CompletedTask);

        _mockCloneManager
            .Setup(manager => manager.CloneRepositoriesAsync(templateProjectId, project.Id))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateProject(templateProjectId, newProjectName, description, visibility);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Project", redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(project.Id, redirectResult.RouteValues["projectId"]);
    }

    [Fact]
    public async Task CreateProject_CloneProjectReturnsMessage_ReturnsCreateProjectViewWithError()
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
            .Setup(manager => manager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility))
            .ReturnsAsync(Tuple.Create(project, templateProject, (string?)message));

        // Act
        var result = await _controller.CreateProject(templateProjectId, newProjectName, description, visibility);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Empty(viewResult.Model as ProjectBase[] ?? []);
        Assert.True(_controller.ModelState.ContainsKey("ErrorMessage"));
        Assert.Equal(message, _controller.ModelState["ErrorMessage"]?.Errors?.FirstOrDefault()?.ErrorMessage);
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
        Assert.Equal(projects.Value, viewResult.Model);
    }

    [Fact]
    public async Task CloneProject_InvalidModelState_ReturnsDefaultView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneProject();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ProjectBase[]>(viewResult.Model);
    }

    [Fact]
    public async Task CloneProject_InvalidModelState_ReturnsProjectsView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneProject(Guid.NewGuid(), "New Project", "Description", Visibility.Private);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(_controller.ModelState.ContainsKey("Error"));
        Assert.IsType<ProjectBase[]>(viewResult.Model);
    }

    [Fact]
    public async Task CloneProject_CloneProjectReturnsMessage_AddsErrorMessageAndReturnsProjectsView()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var newProjectName = "New Project";
        var description = "Description";
        var visibility = Visibility.Private;
        var message = "Error cloning project";

        _mockCloneManager
            .Setup(manager => manager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility))
            .ReturnsAsync(Tuple.Create((Project?)null!, (Project?)null!, (string?)message));

        // Act
        var result = await _controller.CloneProject(templateProjectId, newProjectName, description, visibility);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.True(_controller.ModelState.ContainsKey("ErrorMessage"));
        Assert.Equal(message, _controller.ModelState["ErrorMessage"]?.Errors?.FirstOrDefault()?.ErrorMessage);
    }

    [Fact]
    public async Task CloneProject_CloneProjectSuccess_AddsSuccessMessageAndReturnsProjectsView()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var newProjectName = "New Project";
        var description = "Description";
        var visibility = Visibility.Private;

        _mockCloneManager
            .Setup(manager => manager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility))
            .ReturnsAsync(Tuple.Create((Project?)null!, (Project?)null!, (string?)null));

        // Act
        var result = await _controller.CloneProject(templateProjectId, newProjectName, description, visibility);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Equal("Success", _controller.ViewBag.SuccessMessage);
    }
}