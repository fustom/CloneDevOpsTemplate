using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class ProjectControllerTest
{
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<IIterationService> _mockIterationService;
    private readonly Mock<ITeamsService> _mockTeamsService;
    private readonly Mock<ITeamSettingsService> _mockTeamSettingsService;
    private readonly Mock<IBoardService> _mockBoardService;
    private readonly Mock<IRepositoryService> _mockRepositoryService;
    private readonly Mock<IServiceService> _mockServiceService;
    private readonly ProjectController _controller;

    public ProjectControllerTest()
    {
        _mockProjectService = new Mock<IProjectService>();
        _mockIterationService = new Mock<IIterationService>();
        _mockTeamsService = new Mock<ITeamsService>();
        _mockTeamSettingsService = new Mock<ITeamSettingsService>();
        _mockBoardService = new Mock<IBoardService>();
        _mockRepositoryService = new Mock<IRepositoryService>();
        _mockServiceService = new Mock<IServiceService>();

        _controller = new ProjectController(
            _mockProjectService.Object,
            _mockIterationService.Object,
            _mockTeamsService.Object,
            _mockTeamSettingsService.Object,
            _mockBoardService.Object,
            _mockRepositoryService.Object,
            _mockServiceService.Object
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
    public async Task Project_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.Project(Guid.NewGuid());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
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
    public async Task CreateProject_InvalidModelState_ReturnsCreateProjectView()
    {
        // Arrange
        var projects = new Projects
        {
            Value =
            [
                new Project()
            ]
        };
        _mockProjectService.Setup(service => service.GetAllProjectsAsync()).ReturnsAsync(projects);

        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CreateProject(Guid.NewGuid(), "New Project", "Description", "Private");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(projects.Value, viewResult.Model);
    }

    [Fact]
    public async Task CreateProject_ValidData_RedirectsToProject()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var newProjectName = "New Project";
        var description = "Description";
        var visibility = "Private";
        var templateProject = new Project
        {
            Capabilities = new Capabilities
            {
                ProcessTemplate = new ProcessTemplate { TemplateTypeId = "TemplateId" },
                Versioncontrol = new VersionControl { SourceControlType = "Git" }
            }
        };
        var createProjectResponse = new CreateProjectResponse();
        var newProject = new Project { Id = Guid.NewGuid(), State = "wellFormed" };
        var iterations = new Iteration();
        var createdIterations = new Iteration();

        _mockProjectService.Setup(service => service.GetProjectAsync(templateProjectId)).ReturnsAsync(templateProject);
        _mockProjectService.Setup(service => service.CreateProjectAsync(newProjectName, description, templateProject.Capabilities.ProcessTemplate.TemplateTypeId, templateProject.Capabilities.Versioncontrol.SourceControlType, visibility)).ReturnsAsync(createProjectResponse);
        _mockProjectService.SetupSequence(service => service.GetProjectAsync(newProjectName))
            .ReturnsAsync(new Project { State = "notStarted" })
            .ReturnsAsync(newProject);

        _mockIterationService.Setup(service => service.GetIterationsAsync(templateProjectId)).ReturnsAsync(iterations);
        _mockIterationService.Setup(service => service.CreateIterationAsync(newProject.Id, iterations)).ReturnsAsync(createdIterations);

        // Act
        var result = await _controller.CreateProject(templateProjectId, newProjectName, description, visibility);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Project", redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(newProject.Id, redirectResult.RouteValues["projectId"]);
    }

    [Fact]
    public async Task ProjectProperties_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.ProjectProperties(Guid.NewGuid());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
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
    public async Task CreateProject_CreateProjectResponseWithError_ReturnsCreateProjectView()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var newProjectName = "New Project";
        var description = "Description";
        var visibility = "Private";
        var templateProject = new Project
        {
            Capabilities = new Capabilities
            {
                ProcessTemplate = new ProcessTemplate { TemplateTypeId = "TemplateId" },
                Versioncontrol = new VersionControl { SourceControlType = "Git" }
            }
        };
        var createProjectResponse = new CreateProjectResponse { Message = "Error creating project" };

        _mockProjectService.Setup(service => service.GetProjectAsync(templateProjectId)).ReturnsAsync(templateProject);
        _mockProjectService.Setup(service => service.CreateProjectAsync(newProjectName, description, templateProject.Capabilities.ProcessTemplate.TemplateTypeId, templateProject.Capabilities.Versioncontrol.SourceControlType, visibility)).ReturnsAsync(createProjectResponse);

        // Act
        await _controller.CreateProject(templateProjectId, newProjectName, description, visibility);

        // Assert
        Assert.True(_controller.ModelState.ContainsKey("ErrorMessage"));
    }

    [Fact]
    public async Task CreateProject_ClonesIterationsAndRepositories()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var newProjectName = "New Project";
        var description = "Description";
        var visibility = "Private";
        var templateProject = new Project
        {
            Capabilities = new Capabilities
            {
                ProcessTemplate = new ProcessTemplate { TemplateTypeId = "TemplateId" },
                Versioncontrol = new VersionControl { SourceControlType = "Git" }
            }
        };
        var createProjectResponse = new CreateProjectResponse();
        var newProject = new Project { Id = Guid.NewGuid(), State = "wellFormed" };
        var iterations = new Iteration();
        var createdIterations = new Iteration();
        var repositories = new Repositories { Value = [new Repository { Id = Guid.NewGuid() }] };
        var templateRepositories = new Repositories { Value = [new Repository { Name = "TemplateRepo", RemoteUrl = "http://example.com" }] };

        _mockProjectService.Setup(service => service.GetProjectAsync(templateProjectId)).ReturnsAsync(templateProject);
        _mockProjectService.Setup(service => service.CreateProjectAsync(newProjectName, description, templateProject.Capabilities.ProcessTemplate.TemplateTypeId, templateProject.Capabilities.Versioncontrol.SourceControlType, visibility)).ReturnsAsync(createProjectResponse);
        _mockProjectService.SetupSequence(service => service.GetProjectAsync(newProjectName))
            .ReturnsAsync(new Project { State = "notStarted" })
            .ReturnsAsync(newProject);

        _mockIterationService.Setup(service => service.GetIterationsAsync(templateProjectId)).ReturnsAsync(iterations);
        _mockIterationService.Setup(service => service.CreateIterationAsync(newProject.Id, iterations)).ReturnsAsync(createdIterations);

        _mockRepositoryService.Setup(service => service.GetRepositoriesAsync(newProject.Id)).ReturnsAsync(repositories);
        _mockRepositoryService.Setup(service => service.GetRepositoriesAsync(templateProjectId)).ReturnsAsync(templateRepositories);

        // Act
        var result = await _controller.CreateProject(templateProjectId, newProjectName, description, visibility);

        // Assert
        _mockIterationService.Verify(service => service.CreateIterationAsync(newProject.Id, iterations), Times.Once);
        _mockRepositoryService.Verify(service => service.CreateRepositoryAsync(newProject.Id, "TemplateRepo"), Times.Once);
        _mockRepositoryService.Verify(service => service.DeleteRepositoryAsync(newProject.Id, repositories.Value[0].Id), Times.Once);
        Assert.IsType<RedirectToActionResult>(result);
    }
}