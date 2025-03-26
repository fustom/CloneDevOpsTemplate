using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class TeamControllerTest
{
    private readonly Mock<ITeamsService> _mockTeamsService;
    private readonly TeamController _controller;

    public TeamControllerTest()
    {
        _mockTeamsService = new Mock<ITeamsService>();
        _controller = new TeamController(_mockTeamsService.Object);
    }

    [Fact]
    public async Task Teams_ReturnsViewWithTeams()
    {
        // Arrange
        var mockTeams = new Teams { Value = [ new() { Id = Guid.NewGuid(), Name = "Team A" } ] };
        _mockTeamsService.Setup(service => service.GetAllTeamsAsync()).ReturnsAsync(mockTeams);

        // Act
        var result = await _controller.Teams();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockTeams.Value, viewResult.Model);
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
    public async Task ProjectTeams_ReturnsViewWithTeams()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var mockTeams = new Teams { Value = [ new Team { Id = Guid.NewGuid(), Name = "Team A" } ] };
        _mockTeamsService.Setup(service => service.GetTeamsAsync(projectId)).ReturnsAsync(mockTeams);

        // Act
        var result = await _controller.ProjectTeams(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockTeams.Value, viewResult.Model);
    }
}