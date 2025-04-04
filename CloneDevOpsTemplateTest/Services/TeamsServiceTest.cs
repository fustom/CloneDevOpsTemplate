using System.Net;
using System.Net.Http.Json;
using CloneDevOpsTemplate.Constants;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MyTestProject.Service.Tests.Common;

namespace CloneDevOpsTemplateTest.Services;

public class TeamsServiceTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly TeamsService _teamsService;

    public TeamsServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var configuration = ConfigurationFactory.GetConfiguration();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(configuration.GetValue<string>("ServiceRootUrl") ?? string.Empty)
        };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _teamsService = new TeamsService(httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task CreateTeamAsync_ShouldReturnCreatedTeam_WhenRequestIsSuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamName = "Test Team";
        var teamDescription = "Test Team Description";
        var expectedTeam = new Team { Name = teamName, Description = teamDescription, Id = Guid.NewGuid() };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedTeam)
            });

        // Act
        var result = await _teamsService.CreateTeamAsync(projectId, teamName, teamDescription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTeam.Name, result.Name);
        Assert.Equal(expectedTeam.Description, result.Description);
        Assert.Equal(expectedTeam.Id, result.Id);
    }

    [Fact]
    public async Task CreateTeamAsync_ShouldReturnEmptyTeam_WhenRequestFails()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamName = "Test Team";
        var teamDescription = "Test Team Description";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var result = await _teamsService.CreateTeamAsync(projectId, teamName, teamDescription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Name);
        Assert.Equal(string.Empty, result.Description);
        Assert.Equal(Guid.Empty, result.Id);
    }
    [Fact]
    public async Task GetTeamsAsync_ShouldReturnTeams_WhenRequestIsSuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var expectedTeams = new Teams
        {
            Value =
            [
                new Team { Id = Guid.NewGuid(), Name = "Team 1", Description = "Description 1" },
                new Team { Id = Guid.NewGuid(), Name = "Team 2", Description = "Description 2" }
            ]
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedTeams)
            });

        // Act
        var result = await _teamsService.GetTeamsAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTeams.Value.Length, result.Value.Length);
        Assert.Equal(expectedTeams.Value[0].Name, result.Value[0].Name);
        Assert.Equal(expectedTeams.Value[1].Name, result.Value[1].Name);
    }

    [Fact]
    public async Task GetTeamsAsync_ShouldReturnNull_WhenRequestFails()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _teamsService.GetTeamsAsync(projectId));
    }
    [Fact]
    public async Task GetTeamAsync_ShouldReturnTeam_WhenRequestIsSuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var expectedTeam = new Team { Id = Guid.NewGuid(), Name = "Test Team", Description = "Test Description" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedTeam)
            });

        // Act
        var result = await _teamsService.GetTeamAsync(projectId, teamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTeam.Id, result.Id);
        Assert.Equal(expectedTeam.Name, result.Name);
        Assert.Equal(expectedTeam.Description, result.Description);
    }

    [Fact]
    public async Task GetTeamAsync_ShouldReturnNull_WhenRequestFails()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _teamsService.GetTeamAsync(projectId, teamId));
    }

    [Fact]
    public async Task CreateTeamFromTemplateAsync_ShouldReturnMappedTeams_WhenAllRequestsAreSuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var defaultTeamId = Guid.NewGuid();
        var newDefaultTeamId = Guid.NewGuid();
        Team[] templateTeams =
        [
            new Team { Id = Guid.NewGuid(), Name = "Team 1", Description = "Description 1" },
            new Team { Id = defaultTeamId, Name = "Default Team", Description = "Default Description" },
            new Team { Id = Guid.NewGuid(), Name = "Team 2", Description = "Description 2" }
        ];

        Team[] createdTeams =
        [
            new Team { Id = Guid.NewGuid(), Name = "Team 1", Description = "Description 1" },
            new Team { Id = Guid.NewGuid(), Name = "Team 2", Description = "Description 2" }
        ];

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(createdTeams[0])
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(createdTeams[1])
            });

        // Act
        var result = await _teamsService.CreateTeamFromTemplateAsync(projectId, templateTeams, defaultTeamId, newDefaultTeamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(templateTeams.Length, result.Count);
        Assert.Equal(createdTeams[0].Id, result[templateTeams[0].Id]);
        Assert.Equal(newDefaultTeamId, result[templateTeams[1].Id]);
        Assert.Equal(createdTeams[1].Id, result[templateTeams[2].Id]);
    }

    [Fact]
    public async Task CreateTeamFromTemplateAsync_ShouldSkipDefaultTeam_WhenDefaultTeamIdMatches()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var defaultTeamId = Guid.NewGuid();
        var newDefaultTeamId = Guid.NewGuid();
        Team[] templateTeams =
        [
            new Team { Id = defaultTeamId, Name = "Default Team", Description = "Default Description" }
        ];

        // Act
        var result = await _teamsService.CreateTeamFromTemplateAsync(projectId, templateTeams, defaultTeamId, newDefaultTeamId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(newDefaultTeamId, result[defaultTeamId]);
    }

    [Fact]
    public async Task CreateTeamFromTemplateAsync_ShouldThrowException_WhenCreateTeamFails()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var defaultTeamId = Guid.NewGuid();
        var newDefaultTeamId = Guid.NewGuid();
        Team[] templateTeams =
        [
            new Team { Id = Guid.NewGuid(), Name = "Team 1", Description = "Description 1" }
        ];

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var result = await _teamsService.CreateTeamFromTemplateAsync(projectId, templateTeams, defaultTeamId, newDefaultTeamId);

        // Assert
        Assert.Equal(Guid.Empty, result.First().Value);
    }

    [Fact]
    public async Task GetAllTeamsAsync_ShouldReturnAllTeams_WhenRequestIsSuccessful()
    {
        // Arrange
        var expectedTeams = new Teams
        {
            Value = 
            [
                new Team { Id = Guid.NewGuid(), Name = "Team 1", Description = "Description 1" },
                new Team { Id = Guid.NewGuid(), Name = "Team 2", Description = "Description 2" }
            ]
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedTeams)
            });

        // Act
        var result = await _teamsService.GetAllTeamsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTeams.Value.Length, result.Value.Length);
        Assert.Equal(expectedTeams.Value[0].Name, result.Value[0].Name);
        Assert.Equal(expectedTeams.Value[1].Name, result.Value[1].Name);
    }

    [Fact]
    public async Task GetAllTeamsAsync_ShouldReturnNull_WhenRequestFails()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(_teamsService.GetAllTeamsAsync);
    }
}