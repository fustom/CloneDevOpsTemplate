using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class TeamSettingsControllerTest
{
    private readonly Mock<ITeamSettingsService> _mockTeamSettingsService;
    private readonly Mock<ICloneManager> _mockCloneManager;
    private readonly Mock<ITeamsService> _mockTeamsService;
    private readonly MigrationProgressTracker _progressTracker = new();
    private readonly TeamSettingsController _controller;

    public TeamSettingsControllerTest()
    {
        _mockTeamSettingsService = new Mock<ITeamSettingsService>();
        _mockCloneManager = new Mock<ICloneManager>();
        _mockTeamsService = new Mock<ITeamsService>();
        _controller = new TeamSettingsController(_mockTeamSettingsService.Object, _mockCloneManager.Object, _mockTeamsService.Object, _progressTracker);
    }

    [Fact]
    public async Task TeamSettings_ReturnsViewResult_WithTeamSettings()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var teamSettings = new TeamSettings { BugsBehavior = BugsBehavior.AsRequirements, DefaultIterationMacro = "DefaultIterationMacro" };

        _mockTeamSettingsService
            .Setup(service => service.GetTeamSettings(projectId, teamId))
            .ReturnsAsync(teamSettings);

        // Act
        var result = await _controller.TeamSettings(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TeamSettings>(viewResult.Model);
        Assert.Equal(teamSettings, model);
    }

    [Fact]
    public async Task TeamSettings_ReturnsViewResult_WithNewTeamSettings_WhenServiceReturnsNull()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        _mockTeamSettingsService
            .Setup(service => service.GetTeamSettings(projectId, teamId))
            .ReturnsAsync((TeamSettings?)null);

        // Act
        var result = await _controller.TeamSettings(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TeamSettings>(viewResult.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task TeamSettings_ReturnsViewResult_WithNewTeamFieldValues_WhenModelStateIsInvalid()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.TeamSettings(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TeamSettings>(viewResult.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task TeamFieldValues_ReturnsViewResult_WithTeamFieldValues()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var teamFieldValues = new TeamFieldValues { DefaultValue = "Field1", Values = [new Values { Value = "Value1" }, new Values { Value = "Value2" }] };

        _mockTeamSettingsService
            .Setup(service => service.GetTeamFieldValues(projectId, teamId))
            .ReturnsAsync(teamFieldValues);

        // Act
        var result = await _controller.TeamFieldValues(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TeamFieldValues>(viewResult.Model);
        Assert.Equal(teamFieldValues, model);
    }

    [Fact]
    public async Task TeamFieldValues_ReturnsViewResult_WithNewTeamFieldValues_WhenServiceReturnsNull()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        _mockTeamSettingsService
            .Setup(service => service.GetTeamFieldValues(projectId, teamId))
            .ReturnsAsync((TeamFieldValues?)null);

        // Act
        var result = await _controller.TeamFieldValues(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TeamFieldValues>(viewResult.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task TeamFieldValues_ReturnsViewResult_WithNewTeamFieldValues_WhenModelStateIsInvalid()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.TeamFieldValues(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TeamFieldValues>(viewResult.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task Iterations_ReturnsViewResult_WithIterations()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var iterations = new TeamIterations
        {
            Value =
            [
                new TeamIterationSettings { Name = "Iteration1" },
                new TeamIterationSettings { Name = "Iteration2" }
            ]
        };

        _mockTeamSettingsService
            .Setup(service => service.GetIterations(projectId, teamId))
            .ReturnsAsync(iterations);

        // Act
        var result = await _controller.Iterations(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<TeamIterationSettings>>(viewResult.Model);
        Assert.Equal(iterations.Value, model);
    }

    [Fact]
    public async Task Iterations_ReturnsViewResult_WithEmptyIterations_WhenServiceReturnsNull()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        _mockTeamSettingsService
            .Setup(service => service.GetIterations(projectId, teamId))
            .ReturnsAsync((TeamIterations?)null);

        // Act
        var result = await _controller.Iterations(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<TeamIterationSettings>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Iterations_ReturnsViewResult_WithEmptyIterations_WhenModelStateIsInvalid()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.Iterations(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<TeamIterationSettings>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task CloneTeamSettings_Get_ReturnsCloneFormWithItems()
    {
        // Arrange
        _mockTeamsService.Setup(s => s.GetAllTeamsAsync()).ReturnsAsync(new Teams { Value = [new Team { Name = "Source" }] });

        // Act
        var result = await _controller.CloneTeamSettings();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(TeamSettingsController.CloneTeamSettings), viewResult.ViewName);
        var model = Assert.IsType<CloneFormViewModel<Team>>(viewResult.Model);
        Assert.Equal("Source", Assert.Single(model.Items).Name);
        Assert.Equal("TeamSettings", model.Controller);
        Assert.Equal(nameof(TeamSettingsController.CloneTeamSettings), model.Action);
        Assert.Null(model.FinishedMigration);
    }

    [Fact]
    public async Task CloneTeamSettings_Get_NullItems_ReturnsEmptyCloneForm()
    {
        // Arrange
        _mockTeamsService.Setup(s => s.GetAllTeamsAsync()).ReturnsAsync((Teams?)null);

        // Act
        var result = await _controller.CloneTeamSettings();

        // Assert
        var model = Assert.IsType<CloneFormViewModel<Team>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Items);
    }

    [Fact]
    public async Task CloneTeamSettings_Get_WithFinishedOperation_ShowsItOnce()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Fail(operationId, "Boom");

        // Act
        var result = await _controller.CloneTeamSettings(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<Team>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(MigrationStatus.Failed, model.FinishedMigration?.Status);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CloneTeamSettings_Post_InvalidModelState_ReturnsCloneFormWithoutCloning()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneTeamSettings(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(TeamSettingsController.CloneTeamSettings), viewResult.ViewName);
        Assert.IsType<CloneFormViewModel<Team>>(viewResult.Model);
        Assert.Empty(_mockCloneManager.Invocations);
    }

    [Fact]
    public async Task CloneTeamSettings_Post_Success_CompletesProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        _mockCloneManager.Setup(m => m.CloneTeamSettingsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>())).ReturnsAsync(new CloneResult());

        // Act
        var result = await _controller.CloneTeamSettings(templateProjectId, projectId, templateTeamId, projectTeamId, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TeamSettingsController.CloneTeamSettings), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Succeeded, progress?.Status);
        Assert.Equal("Team settings cloned successfully.", progress?.Message);
        _mockCloneManager.Verify(m => m.CloneTeamSettingsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>()), Times.Once);
    }

    [Fact]
    public async Task CloneTeamSettings_Post_Failure_FailsProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var failure = new CloneResult();
        failure.AddError("Boom");
        _mockCloneManager.Setup(m => m.CloneTeamSettingsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>())).ReturnsAsync(failure);

        // Act
        var result = await _controller.CloneTeamSettings(templateProjectId, projectId, templateTeamId, projectTeamId, operationId);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Equal("Cloning team settings failed: Boom", progress?.Message);
        Assert.Equal(0, _controller.ModelState.ErrorCount);
    }

    [Fact]
    public async Task CloneTeamIterations_Get_ReturnsCloneFormWithItems()
    {
        // Arrange
        _mockTeamsService.Setup(s => s.GetAllTeamsAsync()).ReturnsAsync(new Teams { Value = [new Team { Name = "Source" }] });

        // Act
        var result = await _controller.CloneTeamIterations();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(TeamSettingsController.CloneTeamIterations), viewResult.ViewName);
        var model = Assert.IsType<CloneFormViewModel<Team>>(viewResult.Model);
        Assert.Equal("Source", Assert.Single(model.Items).Name);
        Assert.Equal("TeamSettings", model.Controller);
        Assert.Equal(nameof(TeamSettingsController.CloneTeamIterations), model.Action);
        Assert.Null(model.FinishedMigration);
    }

    [Fact]
    public async Task CloneTeamIterations_Get_NullItems_ReturnsEmptyCloneForm()
    {
        // Arrange
        _mockTeamsService.Setup(s => s.GetAllTeamsAsync()).ReturnsAsync((Teams?)null);

        // Act
        var result = await _controller.CloneTeamIterations();

        // Assert
        var model = Assert.IsType<CloneFormViewModel<Team>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Items);
    }

    [Fact]
    public async Task CloneTeamIterations_Get_WithFinishedOperation_ShowsItOnce()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Fail(operationId, "Boom");

        // Act
        var result = await _controller.CloneTeamIterations(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<Team>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(MigrationStatus.Failed, model.FinishedMigration?.Status);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CloneTeamIterations_Post_InvalidModelState_ReturnsCloneFormWithoutCloning()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneTeamIterations(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(TeamSettingsController.CloneTeamIterations), viewResult.ViewName);
        Assert.IsType<CloneFormViewModel<Team>>(viewResult.Model);
        Assert.Empty(_mockCloneManager.Invocations);
    }

    [Fact]
    public async Task CloneTeamIterations_Post_Success_CompletesProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        _mockCloneManager.Setup(m => m.CloneTeamIterationsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>())).ReturnsAsync(new CloneResult());

        // Act
        var result = await _controller.CloneTeamIterations(templateProjectId, projectId, templateTeamId, projectTeamId, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TeamSettingsController.CloneTeamIterations), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Succeeded, progress?.Status);
        Assert.Equal("Team iterations cloned successfully.", progress?.Message);
        _mockCloneManager.Verify(m => m.CloneTeamIterationsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>()), Times.Once);
    }

    [Fact]
    public async Task CloneTeamIterations_Post_Failure_FailsProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var failure = new CloneResult();
        failure.AddError("Boom");
        _mockCloneManager.Setup(m => m.CloneTeamIterationsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>())).ReturnsAsync(failure);

        // Act
        var result = await _controller.CloneTeamIterations(templateProjectId, projectId, templateTeamId, projectTeamId, operationId);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Equal("Cloning team iterations failed: Boom", progress?.Message);
        Assert.Equal(0, _controller.ModelState.ErrorCount);
    }
}
