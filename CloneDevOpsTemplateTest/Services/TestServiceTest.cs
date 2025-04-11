using System.Net;
using System.Net.Http.Json;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MyTestProject.Service.Tests.Common;

namespace CloneDevOpsTemplateTest.Services;

public class TestServiceTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly TestService _testService;

    public TestServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var configuration = ConfigurationFactory.GetConfiguration();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(configuration.GetValue<string>("ServiceRootUrl") ?? string.Empty)
        };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _testService = new TestService(httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetTestPlansAsync_ReturnsTestPlans_WhenApiResponseIsSuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var expectedTestPlans = new TestPlans { Count = 1, Value = [new TestPlan { Id = 1, Name = "test" }] };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedTestPlans)
            });

        // Act
        var result = await _testService.GetTestPlansAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(expectedTestPlans, result);
    }

    [Fact]
    public async Task GetTestPlansAsync_ReturnsNull_WhenApiResponseIsNotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _testService.GetTestPlansAsync(projectId));
    }
}