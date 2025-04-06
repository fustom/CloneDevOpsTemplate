using System.Net;
using System.Net.Http.Json;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MyTestProject.Service.Tests.Common;

namespace CloneDevOpsTemplateTest.Services;

public class TeamSettingsServiceTest
{
    private readonly TeamSettingsService _teamSettingsService;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public TeamSettingsServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var configuration = ConfigurationFactory.GetConfiguration();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(configuration.GetValue<string>("ServiceRootUrl") ?? string.Empty)
        };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _teamSettingsService = new TeamSettingsService(httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetTeamSettings_ReturnsTeamSettings()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var expectedTeamSettings = new TeamSettings
        {
            BugsBehavior = BugsBehavior.AsRequirements
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedTeamSettings)
            });

        // Act
        var result = await _teamSettingsService.GetTeamSettings(projectId, teamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTeamSettings.BugsBehavior, result.BugsBehavior);
    }

    [Fact]
    public async Task GetTeamSettings_ReturnsNull_WhenNotFound()
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
        await Assert.ThrowsAsync<HttpRequestException>(() => _teamSettingsService.GetTeamSettings(projectId, teamId));
    }

    [Fact]
    public async Task UpdateTeamSettings_SendsCorrectRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var teamSettings = new PatchTeamSettings
        {
            BugsBehavior = BugsBehavior.AsRequirements
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        await _teamSettingsService.UpdateTeamSettings(projectId, teamId, teamSettings);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Patch &&
                req.Content != null && req.Content.ReadAsStringAsync().Result.Contains("AsRequirements")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTeamSettings_ThrowsException_OnErrorResponse()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var teamSettings = new PatchTeamSettings
        {
            BugsBehavior = BugsBehavior.AsRequirements
        };

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
        var result = await _teamSettingsService.UpdateTeamSettings(projectId, teamId, teamSettings);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task GetTeamFieldValues_ReturnsTeamFieldValues()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var expectedTeamFieldValues = new TeamFieldValues
        {
            Values =
            [
                new Values { Value = "Area1", IncludeChildren = true },
                new Values { Value = "Area2", IncludeChildren = false }
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
                Content = JsonContent.Create(expectedTeamFieldValues)
            });

        // Act
        var result = await _teamSettingsService.GetTeamFieldValues(projectId, teamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTeamFieldValues.Values.Length, result.Values.Length);
        Assert.Equal(expectedTeamFieldValues.Values[0].Value, result.Values[0].Value);
        Assert.Equal(expectedTeamFieldValues.Values[0].IncludeChildren, result.Values[0].IncludeChildren);
    }

    [Fact]
    public async Task GetTeamFieldValues_ReturnsNull_WhenNotFound()
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
        await Assert.ThrowsAsync<HttpRequestException>(() => _teamSettingsService.GetTeamFieldValues(projectId, teamId));
    }

    [Fact]
    public async Task GetTeamFieldValues_ThrowsException_OnErrorResponse()
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
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _teamSettingsService.GetTeamFieldValues(projectId, teamId));
    }

    [Fact]
    public async Task UpdateTeamFieldValues_SendsCorrectRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var teamFieldValues = new TeamFieldValues
        {
            Values =
            [
                new Values { Value = "Area1", IncludeChildren = true },
                new Values { Value = "Area2", IncludeChildren = false }
            ]
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        await _teamSettingsService.UpdateTeamFieldValues(projectId, teamId, teamFieldValues);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Patch &&
                req.Content != null && req.Content.ReadAsStringAsync().Result.Contains("Area1") &&
                req.Content.ReadAsStringAsync().Result.Contains("Area2")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTeamFieldValues_ThrowsException_OnErrorResponse()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var teamFieldValues = new TeamFieldValues
        {
            Values =
            [
                new Values { Value = "Area1", IncludeChildren = true }
            ]
        };

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
        var result = await _teamSettingsService.UpdateTeamFieldValues(projectId, teamId, teamFieldValues);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task GetIterations_ReturnsTeamIterations()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var expectedIterations = new TeamIterations
        {
            Value =
            [
                new TeamIterationSettings { Id = Guid.NewGuid(), Name = "Iteration 1" },
                new TeamIterationSettings { Id = Guid.NewGuid(), Name = "Iteration 2" }
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
                Content = JsonContent.Create(expectedIterations)
            });

        // Act
        var result = await _teamSettingsService.GetIterations(projectId, teamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedIterations.Value.Length, result.Value.Length);
        Assert.Equal(expectedIterations.Value[0].Name, result.Value[0].Name);
    }

    [Fact]
    public async Task GetIterations_ReturnsNull_WhenNotFound()
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
        await Assert.ThrowsAsync<HttpRequestException>(() => _teamSettingsService.GetIterations(projectId, teamId));
    }

    [Fact]
    public async Task GetIterations_ThrowsException_OnErrorResponse()
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
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _teamSettingsService.GetIterations(projectId, teamId));
    }

    [Fact]
    public async Task CreateIteration_SendsCorrectRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var iterationId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created
            });

        // Act
        var response = await _teamSettingsService.CreateIteration(projectId, teamId, iterationId);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.Content != null && req.Content.ReadAsStringAsync().Result.Contains(iterationId.ToString())),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CreateIteration_ThrowsException_OnErrorResponse()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var iterationId = Guid.NewGuid();

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
        var response = await _teamSettingsService.CreateIteration(projectId, teamId, iterationId);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteIteration_SendsCorrectRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var iterationId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var response = await _teamSettingsService.DeleteIteration(projectId, teamId, iterationId);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Delete &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains($"{projectId}/{teamId}/_apis/work/teamsettings/iterations/{iterationId}?api-version=7.1")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task DeleteIteration_ThrowsException_OnErrorResponse()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var iterationId = Guid.NewGuid();

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
        var response = await _teamSettingsService.DeleteIteration(projectId, teamId, iterationId);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}