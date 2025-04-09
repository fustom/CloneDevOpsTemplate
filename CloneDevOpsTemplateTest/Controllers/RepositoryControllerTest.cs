using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class RepositoryControllerTest
{
    private readonly Mock<IRepositoryService> _mockRepositoryService;
    private readonly Mock<ICloneManager> _mockCloneManager;
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly RepositoryController _controller;

    public RepositoryControllerTest()
    {
        _mockRepositoryService = new Mock<IRepositoryService>();
        _mockCloneManager = new Mock<ICloneManager>();
        _mockProjectService = new Mock<IProjectService>();
        _controller = new RepositoryController(_mockRepositoryService.Object, _mockCloneManager.Object, _mockProjectService.Object);
    }

    [Fact]
    public async Task Repositories_InvalidModelState_ReturnsDefaultView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.Repositories();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<Repository[]>(viewResult.Model);
    }

    [Fact]
    public async Task Repositories_ReturnsViewWithRepositories()
    {
        // Arrange
        var mockRepositories = new Repositories { Value = [] };
        _mockRepositoryService
            .Setup(service => service.GetAllRepositoriesAsync())
            .ReturnsAsync(mockRepositories);

        // Act
        var result = await _controller.Repositories();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockRepositories.Value, viewResult.Model);
    }

    [Fact]
    public async Task Repositories_ReturnsViewWithEmptyRepositories_WhenServiceReturnsNull()
    {
        // Arrange
        _mockRepositoryService
            .Setup(service => service.GetAllRepositoriesAsync())
            .ReturnsAsync((Repositories?)null);

        // Act
        var result = await _controller.Repositories();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<Repository[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task ProjectRepositories_InvalidModelState_ReturnsDefaultView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.ProjectRepositories(Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Repositories", viewResult.ViewName);
        Assert.IsType<Repository[]>(viewResult.Model);
    }

    [Fact]
    public async Task ProjectRepositories_ReturnsViewWithRepositories()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var mockRepositories = new Repositories { Value = [] };
        _mockRepositoryService
            .Setup(service => service.GetRepositoriesAsync(projectId))
            .ReturnsAsync(mockRepositories);

        // Act
        var result = await _controller.ProjectRepositories(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Repositories", viewResult.ViewName);
        Assert.Equal(mockRepositories.Value, viewResult.Model);
    }

    [Fact]
    public async Task ProjectRepositories_ReturnsViewWithEmptyRepositories_WhenServiceReturnsNull()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockRepositoryService
            .Setup(service => service.GetRepositoriesAsync(projectId))
            .ReturnsAsync((Repositories?)null);

        // Act
        var result = await _controller.ProjectRepositories(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Repositories", viewResult.ViewName);
        var viewModel = Assert.IsType<Repository[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task CloneRepository_ReturnsViewWithProjects()
    {
        // Arrange
        var mockProjects = new Projects { Value = Array.Empty<Project>() };
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync(mockProjects);

        // Act
        var result = await _controller.CloneRepository();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockProjects.Value, viewResult.Model);
    }

    [Fact]
    public async Task CloneRepository_ReturnsViewWithEmptyProjects_WhenServiceReturnsNull()
    {
        // Arrange
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync((Projects?)null);

        // Act
        var result = await _controller.CloneRepository();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ProjectBase[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task CloneRepository_InvalidModelState_ReturnsViewWithProjects()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");
        var mockProjects = new Projects { Value = [] };
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync(mockProjects);

        // Act
        var result = await _controller.CloneRepository(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockProjects.Value, viewResult.Model);
    }

    [Fact]
    public async Task CloneRepository_ValidModelState_ClonesRepositoriesAndReturnsViewWithProjects()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var mockProjects = new Projects { Value = [] };
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync(mockProjects);

        // Act
        var result = await _controller.CloneRepository(templateProjectId, projectId);

        // Assert
        _mockCloneManager.Verify(manager => manager.CloneRepositoriesAsync(templateProjectId, projectId), Times.Once);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockProjects.Value, viewResult.Model);
        Assert.Equal("Success", _controller.ViewBag.SuccessMessage);
    }

    [Fact]
    public async Task PullRequests_InvalidModelState_ReturnsDefaultView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.PullRequests(Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<GitPullRequest[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task PullRequests_ReturnsViewWithPullRequests()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var mockPullRequests = new GitPullRequests { Value = Array.Empty<GitPullRequest>() };
        _mockRepositoryService
            .Setup(service => service.GetGitPullRequestAsync(projectId))
            .ReturnsAsync(mockPullRequests);

        // Act
        var result = await _controller.PullRequests(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockPullRequests.Value, viewResult.Model);
    }

    [Fact]
    public async Task PullRequests_ReturnsViewWithEmptyPullRequests_WhenServiceReturnsNull()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockRepositoryService
            .Setup(service => service.GetGitPullRequestAsync(projectId))
            .ReturnsAsync((GitPullRequests?)null);

        // Act
        var result = await _controller.PullRequests(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<GitPullRequest[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task ClonePullRequests_Get_ReturnsViewWithProjects()
    {
        // Arrange
        var mockProjects = new Projects { Value = Array.Empty<Project>() };
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync(mockProjects);

        // Act
        var result = await _controller.ClonePullRequests();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockProjects.Value, viewResult.Model);
    }

    [Fact]
    public async Task ClonePullRequests_Get_ReturnsViewWithEmptyProjects_WhenServiceReturnsNull()
    {
        // Arrange
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync((Projects?)null);

        // Act
        var result = await _controller.ClonePullRequests();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ProjectBase[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task ClonePullRequests_Post_InvalidModelState_ReturnsViewWithProjects()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");
        var mockProjects = new Projects { Value = Array.Empty<Project>() };
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync(mockProjects);

        // Act
        var result = await _controller.ClonePullRequests(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockProjects.Value, viewResult.Model);
    }

    [Fact]
    public async Task ClonePullRequests_Post_ValidModelState_ClonesPullRequestsAndReturnsViewWithProjects()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var mockProjects = new Projects { Value = Array.Empty<Project>() };
        _mockProjectService
            .Setup(service => service.GetAllProjectsAsync())
            .ReturnsAsync(mockProjects);

        // Act
        var result = await _controller.ClonePullRequests(templateProjectId, projectId);

        // Assert
        _mockCloneManager.Verify(manager => manager.CloneGitPullRequestsAsync(templateProjectId, projectId), Times.Once);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockProjects.Value, viewResult.Model);
        Assert.Equal("Success", _controller.ViewBag.SuccessMessage);
    }
}