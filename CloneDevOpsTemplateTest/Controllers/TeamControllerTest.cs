using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class TeamControllerTest
{
    private readonly Mock<ITeamsService> _mockTeamsService;
    private readonly Mock<ICloneManager> _mockCloneManager;
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly MigrationProgressTracker _progressTracker = new();
    private readonly TeamController _controller;

    public TeamControllerTest()
    {
        _mockTeamsService = new Mock<ITeamsService>();
        _mockCloneManager = new Mock<ICloneManager>();
        _mockProjectService = new Mock<IProjectService>();
        _controller = new TeamController(_mockTeamsService.Object, _mockCloneManager.Object, _mockProjectService.Object, _progressTracker);
    }

    [Fact]
    public async Task Teams_InvalidModelState_ReturnsDefaultView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.Teams();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<Team[]>(viewResult.Model);
    }

    [Fact]
    public async Task Teams_ReturnsViewWithTeams()
    {
        // Arrange
        var mockTeams = new Teams { Value = [new() { Id = Guid.NewGuid(), Name = "Team A" }] };
        _mockTeamsService.Setup(service => service.GetAllTeamsAsync()).ReturnsAsync(mockTeams);

        // Act
        var result = await _controller.Teams();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockTeams.Value, viewResult.Model);
    }

    [Fact]
    public async Task Team_InvalidModelState_ReturnsDefaultView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.Team(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<Team>(viewResult.Model);
    }

    [Fact]
    public async Task Team_ReturnsViewWithTeam()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var mockTeam = new Team { Id = teamId, Name = "Team A" };
        _mockTeamsService.Setup(service => service.GetTeamAsync(projectId, teamId)).ReturnsAsync(mockTeam);

        // Act
        var result = await _controller.Team(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockTeam, viewResult.Model);
    }

    [Fact]
    public async Task ProjectTeams_InvalidModelState_ReturnsDefaultView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.ProjectTeams(Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Teams", viewResult.ViewName);
        Assert.IsType<Team[]>(viewResult.Model);
    }

    [Fact]
    public async Task ProjectTeams_ReturnsViewWithTeams()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var mockTeams = new Teams { Value = [new Team { Id = Guid.NewGuid(), Name = "Team A" }] };
        _mockTeamsService.Setup(service => service.GetTeamsAsync(projectId)).ReturnsAsync(mockTeams);

        // Act
        var result = await _controller.ProjectTeams(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockTeams.Value, viewResult.Model);
    }

    [Fact]
    public async Task CloneTeams_Get_ReturnsCloneFormWithItems()
    {
        // Arrange
        _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync(new Projects { Value = [new ProjectBase { Name = "Source" }] });

        // Act
        var result = await _controller.CloneTeams();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(TeamController.CloneTeams), viewResult.ViewName);
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Equal("Source", Assert.Single(model.Items).Name);
        Assert.Equal("Team", model.Controller);
        Assert.Equal(nameof(TeamController.CloneTeams), model.Action);
        Assert.Null(model.FinishedMigration);
    }

    [Fact]
    public async Task CloneTeams_Get_NullItems_ReturnsEmptyCloneForm()
    {
        // Arrange
        _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync((Projects?)null);

        // Act
        var result = await _controller.CloneTeams();

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Items);
    }

    [Fact]
    public async Task CloneTeams_Get_WithFinishedOperation_ShowsItOnce()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Fail(operationId, "Boom");

        // Act
        var result = await _controller.CloneTeams(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<ProjectBase>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(MigrationStatus.Failed, model.FinishedMigration?.Status);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CloneTeams_Post_InvalidModelState_ReturnsCloneFormWithoutCloning()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneTeams(templateProjectId, projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(TeamController.CloneTeams), viewResult.ViewName);
        Assert.IsType<CloneFormViewModel<ProjectBase>>(viewResult.Model);
        Assert.Empty(_mockCloneManager.Invocations);
    }

    [Fact]
    public async Task CloneTeams_Post_Success_CompletesProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        _mockCloneManager.Setup(m => m.CloneTeamsAsync(templateProjectId, projectId, It.IsAny<IProgress<string>?>())).ReturnsAsync(new CloneTeamsResult());

        // Act
        var result = await _controller.CloneTeams(templateProjectId, projectId, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TeamController.CloneTeams), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Succeeded, progress?.Status);
        Assert.Equal("Teams cloned successfully.", progress?.Message);
        _mockCloneManager.Verify(m => m.CloneTeamsAsync(templateProjectId, projectId, It.IsAny<IProgress<string>?>()), Times.Once);
    }

    [Fact]
    public async Task CloneTeams_Post_Failure_FailsProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var failure = new CloneTeamsResult();
        failure.AddError("Boom");
        _mockCloneManager.Setup(m => m.CloneTeamsAsync(templateProjectId, projectId, It.IsAny<IProgress<string>?>())).ReturnsAsync(failure);

        // Act
        var result = await _controller.CloneTeams(templateProjectId, projectId, operationId);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Equal("Cloning teams failed: Boom", progress?.Message);
        Assert.Equal(0, _controller.ModelState.ErrorCount);
    }

    [Fact]
    public async Task CloneTeamFieldValues_Get_ReturnsCloneFormWithItems()
    {
        // Arrange
        _mockTeamsService.Setup(s => s.GetAllTeamsAsync()).ReturnsAsync(new Teams { Value = [new Team { Name = "Source" }] });

        // Act
        var result = await _controller.CloneTeamFieldValues();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(TeamController.CloneTeamFieldValues), viewResult.ViewName);
        var model = Assert.IsType<CloneFormViewModel<Team>>(viewResult.Model);
        Assert.Equal("Source", Assert.Single(model.Items).Name);
        Assert.Equal("Team", model.Controller);
        Assert.Equal(nameof(TeamController.CloneTeamFieldValues), model.Action);
        Assert.Null(model.FinishedMigration);
    }

    [Fact]
    public async Task CloneTeamFieldValues_Get_NullItems_ReturnsEmptyCloneForm()
    {
        // Arrange
        _mockTeamsService.Setup(s => s.GetAllTeamsAsync()).ReturnsAsync((Teams?)null);

        // Act
        var result = await _controller.CloneTeamFieldValues();

        // Assert
        var model = Assert.IsType<CloneFormViewModel<Team>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Items);
    }

    [Fact]
    public async Task CloneTeamFieldValues_Get_WithFinishedOperation_ShowsItOnce()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Fail(operationId, "Boom");

        // Act
        var result = await _controller.CloneTeamFieldValues(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<Team>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(MigrationStatus.Failed, model.FinishedMigration?.Status);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CloneTeamFieldValues_Post_InvalidModelState_ReturnsCloneFormWithoutCloning()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneTeamFieldValues(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(TeamController.CloneTeamFieldValues), viewResult.ViewName);
        Assert.IsType<CloneFormViewModel<Team>>(viewResult.Model);
        Assert.Empty(_mockCloneManager.Invocations);
    }

    [Fact]
    public async Task CloneTeamFieldValues_Post_Success_CompletesProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        _mockCloneManager.Setup(m => m.CloneTeamFieldValuesAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>())).ReturnsAsync(new CloneResult());

        // Act
        var result = await _controller.CloneTeamFieldValues(templateProjectId, projectId, templateTeamId, projectTeamId, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TeamController.CloneTeamFieldValues), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Succeeded, progress?.Status);
        Assert.Equal("Team field values cloned successfully.", progress?.Message);
        _mockCloneManager.Verify(m => m.CloneTeamFieldValuesAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>()), Times.Once);
    }

    [Fact]
    public async Task CloneTeamFieldValues_Post_Failure_FailsProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var failure = new CloneResult();
        failure.AddError("Boom");
        _mockCloneManager.Setup(m => m.CloneTeamFieldValuesAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>())).ReturnsAsync(failure);

        // Act
        var result = await _controller.CloneTeamFieldValues(templateProjectId, projectId, templateTeamId, projectTeamId, operationId);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Equal("Cloning team field values failed: Boom", progress?.Message);
        Assert.Equal(0, _controller.ModelState.ErrorCount);
    }
}
