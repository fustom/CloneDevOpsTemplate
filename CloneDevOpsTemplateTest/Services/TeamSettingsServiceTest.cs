using CloneDevOpsTemplate.Services;
using CloneDevOpsTemplate.Models;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using CloneDevOpsTemplate.Constants;

namespace CloneDevOpsTemplateTest.Services;

public class TeamSettingsServiceTest
{
    private readonly TeamSettingsService _teamSettingsService;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public TeamSettingsServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(Const.ServiceRootUrl)
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
            BugsBehavior = "BugsBehavior"
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
        var teamSettings = new TeamSettings
        {
            BugsBehavior = "BugsBehavior"
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
                req.Method == HttpMethod.Put &&
                req.Content != null && req.Content.ReadAsStringAsync().Result.Contains("BugsBehavior")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTeamSettings_ThrowsException_OnErrorResponse()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var teamSettings = new TeamSettings
        {
            BugsBehavior = "BugsBehavior"
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
}