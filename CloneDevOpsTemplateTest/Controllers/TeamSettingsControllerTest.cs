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
        var teamSettings = new TeamSettings { BugsBehavior = "BugsBehavior", DefaultIterationMacro = "DefaultIterationMacro" };

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
}