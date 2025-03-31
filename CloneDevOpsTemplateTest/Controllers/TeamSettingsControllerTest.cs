using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class TeamSettingsControllerTest
{
    private readonly Mock<ITeamSettingsService> _mockTeamSettingsService;
    private readonly TeamSettingsController _controller;

    public TeamSettingsControllerTest()
    {
        _mockTeamSettingsService = new Mock<ITeamSettingsService>();
        _controller = new TeamSettingsController(_mockTeamSettingsService.Object);
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
}