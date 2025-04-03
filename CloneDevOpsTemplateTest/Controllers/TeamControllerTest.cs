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
    private readonly TeamController _controller;

    public TeamControllerTest()
    {
        _mockTeamsService = new Mock<ITeamsService>();
        _mockCloneManager = new Mock<ICloneManager>();
        _mockProjectService = new Mock<IProjectService>();
        _controller = new TeamController(_mockTeamsService.Object, _mockCloneManager.Object, _mockProjectService.Object);
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
    public async Task CloneTeams_ReturnsViewWithProjects()
    {
        // Arrange
        var mockProjects = new Projects
        {
            Value =
            [
                new Project { Id = Guid.NewGuid(), Name = "Project A" }
            ]
        };
        _mockProjectService.Setup(service => service.GetAllProjectsAsync()).ReturnsAsync(mockProjects);

        // Act
        var result = await _controller.CloneTeams();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockProjects.Value, viewResult.Model);
    }

    [Fact]
    public async Task CloneTeams_NoProjects_ReturnsViewWithEmptyList()
    {
        // Arrange
        _mockProjectService.Setup(service => service.GetAllProjectsAsync()).ReturnsAsync((Projects)null!);

        // Act
        var result = await _controller.CloneTeams();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ProjectBase[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task CloneTeams_InvalidModelState_ReturnsCloneTeamsView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneTeams(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ProjectBase[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task CloneTeams_ValidModelState_ClonesTeamsAndReturnsCloneTeamsView()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        // Act
        var result = await _controller.CloneTeams(templateProjectId, projectId);

        // Assert
        _mockCloneManager.Verify(manager => manager.CloneTeamsAsync(templateProjectId, projectId), Times.Once);
        Assert.IsType<ViewResult>(result);
        Assert.Equal("Success", _controller.ViewBag.SuccessMessage);
    }

    [Fact]
    public async Task CloneTeamFieldValues_ReturnsTeamsView()
    {
        // Arrange
        var mockTeams = new Teams
        {
            Value =
            [
                new Team { Id = Guid.NewGuid(), Name = "Team A" }
            ]
        };
        _mockTeamsService.Setup(service => service.GetAllTeamsAsync()).ReturnsAsync(mockTeams);

        // Act
        var result = await _controller.CloneTeamFieldValues();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockTeams.Value, viewResult.Model);
    }

    [Fact]
    public async Task CloneTeamFieldValues_InvalidModelState_ReturnsDefaultTeamsView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneTeamFieldValues();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<Team[]>(viewResult.Model);
    }

    [Fact]
    public async Task CloneTeamFieldValues_InvalidModelState_ReturnsCloneTeamFieldValuesView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneTeamFieldValues(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<Team[]>(viewResult.Model);
    }

    [Fact]
    public async Task CloneTeamFieldValues_ValidModelState_ClonesTeamFieldValuesAndReturnsCloneTeamFieldValuesView()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();

        // Act
        var result = await _controller.CloneTeamFieldValues(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        _mockCloneManager.Verify(manager => manager.CloneTeamFieldValuesAsync(templateProjectId, projectId, templateTeamId, projectTeamId), Times.Once);
        Assert.IsType<ViewResult>(result);
        Assert.Equal("Success", _controller.ViewBag.SuccessMessage);
    }
}