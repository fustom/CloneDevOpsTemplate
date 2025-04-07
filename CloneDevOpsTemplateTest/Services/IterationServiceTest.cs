using System.Net;
using System.Net.Http.Json;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MyTestProject.Service.Tests.Common;

namespace CloneDevOpsTemplateTest.Services;

public class IterationServiceTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly IterationService _iterationService;

    public IterationServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var configuration = ConfigurationFactory.GetConfiguration();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(configuration.GetValue<string>("ServiceRootUrl") ?? string.Empty)
        };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _iterationService = new IterationService(httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetIterationAsync_ReturnsIteration()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var name = "Iteration1";
        var expectedIteration = new Iteration { Name = name };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedIteration)
            });

        // Act
        var result = await _iterationService.GetAsync(projectId, TreeStructureGroup.Iterations, name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedIteration.Name, result.Name);
    }

    [Fact]
    public async Task GetIterationsAsync_ReturnsIterations()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var expectedIterations = new Iteration { Name = "Iteration1" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedIterations)
            });

        // Act
        var result = await _iterationService.GetAllAsync(projectId, TreeStructureGroup.Iterations);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedIterations.Name, result.Name);
    }

    [Fact]
    public async Task GetAreaAsync_ReturnsArea()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var expectedArea = new Iteration { Name = "Area1" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedArea)
            });

        // Act
        var result = await _iterationService.GetAllAsync(projectId, TreeStructureGroup.Areas);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedArea.Name, result.Name);
    }

    [Fact]
    public async Task GetAreaAsync_ThrowsExceptionOnServerError()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _iterationService.GetAllAsync(projectId, TreeStructureGroup.Areas));
    }

    [Fact]
    public async Task CreateAsync_ReturnsSuccessResponse()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var structureGroup = TreeStructureGroup.Iterations;
        var path = "Iteration1";
        var iteration = new ClassificationNodeBase { Name = "Iteration1" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"{projectId}/_apis/wit/classificationNodes/{structureGroup}/{path}?api-version=7.1")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created
            });

        // Act
        var response = await _iterationService.CreateAsync(projectId, structureGroup, iteration, path);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_ThrowsExceptionOnServerError()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var structureGroup = TreeStructureGroup.Iterations;
        var path = "Iteration1";
        var iteration = new ClassificationNodeBase { Name = "Iteration1" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"{projectId}/_apis/wit/classificationNodes/{structureGroup}/{path}?api-version=7.1")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var response = await _iterationService.CreateAsync(projectId, structureGroup, iteration, path);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_CreatesAllIterationsRecursively()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var structureGroup = TreeStructureGroup.Iterations;
        var path = "RootIteration";
        var iterations = new Iteration
        {
            Name = "RootIteration",
            Children =
            [
                new Iteration
                {
                    Name = "ChildIteration1",
                    Children =
                    [
                        new Iteration { Name = "GrandChildIteration1" }
                    ]
                },
                new Iteration { Name = "ChildIteration2" }
            ]
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"{projectId}/_apis/wit/classificationNodes/{structureGroup}")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created
            });

        // Act
        await _iterationService.CreateAsync(projectId, iterations, structureGroup, path);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(3), // ChildIteration1, GrandChildIteration1, ChildIteration2
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task DeleteAsync_DeletesIterationSuccessfully()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var structureGroup = TreeStructureGroup.Iterations;
        var name = "Iteration1";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri!.ToString().Contains($"{projectId}/_apis/wit/classificationNodes/{structureGroup}/{name}?api-version=7.1")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        await _iterationService.DeleteAsync(projectId, structureGroup, name);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Delete &&
                req.RequestUri!.ToString().Contains($"{projectId}/_apis/wit/classificationNodes/{structureGroup}/{name}?api-version=7.1")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task DeleteAsync_ThrowsExceptionOnServerError()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var structureGroup = TreeStructureGroup.Iterations;
        var name = "Iteration1";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri!.ToString().Contains($"{projectId}/_apis/wit/classificationNodes/{structureGroup}/{name}?api-version=7.1")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var response = await _iterationService.DeleteAsync(projectId, structureGroup, name);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_DeletesAllIterationsRecursively()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var structureGroup = TreeStructureGroup.Iterations;
        var iterations = new Iteration
        {
            Name = "RootIteration",
            Children =
            [
                new Iteration
                {
                    Name = "ChildIteration1",
                    Children =
                    [
                        new Iteration { Name = "GrandChildIteration1" }
                    ]
                },
                new Iteration { Name = "ChildIteration2" }
            ]
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri!.ToString().Contains($"{projectId}/_apis/wit/classificationNodes/{structureGroup}")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        await _iterationService.DeleteAsync(projectId, structureGroup, iterations);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2), // ChildIteration1, ChildIteration2
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}