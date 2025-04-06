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
    private readonly TeamSettingsController _controller;

    public TeamSettingsControllerTest()
    {
        _mockTeamSettingsService = new Mock<ITeamSettingsService>();
        _mockCloneManager = new Mock<ICloneManager>();
        _mockTeamsService = new Mock<ITeamsService>();
        _controller = new TeamSettingsController(_mockTeamSettingsService.Object, _mockCloneManager.Object, _mockTeamsService.Object);
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
    public async Task CloneTeamSettings_ReturnsViewResult_WithTeams()
    {
        // Arrange
        Team[] teams =
        [
            new() { Id = Guid.NewGuid(), Name = "Team1" },
            new() { Id = Guid.NewGuid(), Name = "Team2" }
        ];
        var teamsResponse = new Teams { Value = teams };

        _mockTeamsService
            .Setup(service => service.GetAllTeamsAsync())
            .ReturnsAsync(teamsResponse);

        // Act
        var result = await _controller.CloneTeamSettings();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Team>>(viewResult.Model);
        Assert.Equal(teams, model);
    }

    [Fact]
    public async Task CloneTeamSettings_ReturnsViewResult_WithEmptyTeams_WhenServiceReturnsNull()
    {
        // Arrange
        _mockTeamsService
            .Setup(service => service.GetAllTeamsAsync())
            .ReturnsAsync((Teams?)null);

        // Act
        var result = await _controller.CloneTeamSettings();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Team>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task CloneTeamSettings_Post_ReturnsViewResult_WithSuccessMessage_WhenModelStateIsValid()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();

        _mockCloneManager
            .Setup(manager => manager.CloneTeamSettingsAsync(templateProjectId, projectId, templateTeamId, projectTeamId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CloneTeamSettings(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Equal("Success", _controller.ViewBag.SuccessMessage);
    }

    [Fact]
    public async Task CloneTeamSettings_Post_ReturnsViewResult_WhenModelStateIsInvalid()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneTeamSettings(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Null(_controller.ViewBag.SuccessMessage);
    }
}