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
    private readonly MigrationProgressTracker _progressTracker = new();
    private readonly RepositoryController _controller;

    public RepositoryControllerTest()
    {
        _mockRepositoryService = new Mock<IRepositoryService>();
        _mockCloneManager = new Mock<ICloneManager>();
        _mockProjectService = new Mock<IProjectService>();
        _controller = new RepositoryController(_mockRepositoryService.Object, _mockCloneManager.Object, _mockProjectService.Object, _progressTracker);
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
    public async Task CloneRepository_Get_ReturnsCloneFormWithItems()
    {
        // Arrange
        _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync(new Projects { Value = [new ProjectBase { Name = "Source" }] });

        // Act
        var result = await _controller.CloneRepository();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(RepositoryController.CloneRepository), viewResult.ViewName);
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Equal("Source", Assert.Single(model.Items).Name);
        Assert.Equal("Repository", model.Controller);
        Assert.Equal(nameof(RepositoryController.CloneRepository), model.Action);
        Assert.Null(model.FinishedMigration);
    }

    [Fact]
    public async Task CloneRepository_Get_NullItems_ReturnsEmptyCloneForm()
    {
        // Arrange
        _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync((Projects?)null);

        // Act
        var result = await _controller.CloneRepository();

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Items);
    }

    [Fact]
    public async Task CloneRepository_Get_WithFinishedOperation_ShowsItOnce()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Fail(operationId, "Boom");

        // Act
        var result = await _controller.CloneRepository(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(MigrationStatus.Failed, model.FinishedMigration?.Status);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CloneRepository_Post_InvalidModelState_ReturnsCloneFormWithoutCloning()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneRepository(templateProjectId, projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(RepositoryController.CloneRepository), viewResult.ViewName);
        Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Empty(_mockCloneManager.Invocations);
    }

    [Fact]
    public async Task CloneRepository_Post_Success_CompletesProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        _mockCloneManager.Setup(m => m.CloneRepositoriesAsync(templateProjectId, projectId, It.IsAny<IProgress<string>?>())).ReturnsAsync(new CloneResult());

        // Act
        var result = await _controller.CloneRepository(templateProjectId, projectId, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RepositoryController.CloneRepository), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Succeeded, progress?.Status);
        Assert.Equal("Repositories cloned successfully.", progress?.Message);
        _mockCloneManager.Verify(m => m.CloneRepositoriesAsync(templateProjectId, projectId, It.IsAny<IProgress<string>?>()), Times.Once);
    }

    [Fact]
    public async Task CloneRepository_Post_Failure_FailsProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var failure = new CloneResult();
        failure.AddError("Boom");
        _mockCloneManager.Setup(m => m.CloneRepositoriesAsync(templateProjectId, projectId, It.IsAny<IProgress<string>?>())).ReturnsAsync(failure);

        // Act
        var result = await _controller.CloneRepository(templateProjectId, projectId, operationId);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Equal("Cloning repositories failed: Boom", progress?.Message);
        Assert.Equal(0, _controller.ModelState.ErrorCount);
    }

    [Fact]
    public async Task ClonePullRequests_Get_ReturnsCloneFormWithItems()
    {
        // Arrange
        _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync(new Projects { Value = [new ProjectBase { Name = "Source" }] });

        // Act
        var result = await _controller.ClonePullRequests();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(RepositoryController.ClonePullRequests), viewResult.ViewName);
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Equal("Source", Assert.Single(model.Items).Name);
        Assert.Equal("Repository", model.Controller);
        Assert.Equal(nameof(RepositoryController.ClonePullRequests), model.Action);
        Assert.Null(model.FinishedMigration);
    }

    [Fact]
    public async Task ClonePullRequests_Get_NullItems_ReturnsEmptyCloneForm()
    {
        // Arrange
        _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync((Projects?)null);

        // Act
        var result = await _controller.ClonePullRequests();

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Items);
    }

    [Fact]
    public async Task ClonePullRequests_Get_WithFinishedOperation_ShowsItOnce()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Fail(operationId, "Boom");

        // Act
        var result = await _controller.ClonePullRequests(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(MigrationStatus.Failed, model.FinishedMigration?.Status);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task ClonePullRequests_Post_InvalidModelState_ReturnsCloneFormWithoutCloning()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.ClonePullRequests(templateProjectId, projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(RepositoryController.ClonePullRequests), viewResult.ViewName);
        Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Empty(_mockCloneManager.Invocations);
    }

    [Fact]
    public async Task ClonePullRequests_Post_Success_CompletesProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        _mockCloneManager.Setup(m => m.CloneGitPullRequestsAsync(templateProjectId, projectId, It.IsAny<IProgress<string>?>())).ReturnsAsync(new CloneResult());

        // Act
        var result = await _controller.ClonePullRequests(templateProjectId, projectId, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RepositoryController.ClonePullRequests), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Succeeded, progress?.Status);
        Assert.Equal("Pull requests cloned successfully.", progress?.Message);
        _mockCloneManager.Verify(m => m.CloneGitPullRequestsAsync(templateProjectId, projectId, It.IsAny<IProgress<string>?>()), Times.Once);
    }

    [Fact]
    public async Task ClonePullRequests_Post_Failure_FailsProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var failure = new CloneResult();
        failure.AddError("Boom");
        _mockCloneManager.Setup(m => m.CloneGitPullRequestsAsync(templateProjectId, projectId, It.IsAny<IProgress<string>?>())).ReturnsAsync(failure);

        // Act
        var result = await _controller.ClonePullRequests(templateProjectId, projectId, operationId);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Equal("Cloning pull requests failed: Boom", progress?.Message);
        Assert.Equal(0, _controller.ModelState.ErrorCount);
    }
}
