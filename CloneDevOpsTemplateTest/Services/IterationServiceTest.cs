using System.Net;
using System.Net.Http.Json;
using CloneDevOpsTemplate.Constants;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Moq;
using Moq.Protected;

namespace CloneDevOpsTemplateTest.Services;

public class IterationServiceTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly IterationService _iterationService;

    public IterationServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(Const.ServiceRootUrl)
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
        var result = await _iterationService.GetIterationAsync(projectId, name);

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
        var result = await _iterationService.GetIterationsAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedIterations.Name, result.Name);
    }

    [Fact]
    public async Task CreateIterationAsync_ReturnsCreatedIteration()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var createIterationRequest = new CreateIterationRequest { Name = "Iteration1" };
        var expectedIteration = new Iteration { Name = createIterationRequest.Name };

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
        var result = await _iterationService.CreateIterationAsync(projectId, createIterationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedIteration.Name, result.Name);
    }

    [Fact]
    public async Task MoveIteration_ReturnsSuccess()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var path = "Iteration1";
        var id = 1;
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        await _iterationService.MoveIterationAsync(projectId, path, id);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    [Fact]
    public async Task CreateIterationAsync_WithIterations_ReturnsCreatedIterations()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var iterations = new Iteration
        {
            Children =
            [
                new Iteration { Name = "Iteration1", Id = 1, Children = [] },
                new Iteration { Name = "Iteration2", Id = 2, Children = [] }
            ]
        };
        var expectedIteration1 = new Iteration { Name = "Iteration1", Id = 1 };
        var expectedIteration2 = new Iteration { Name = "Iteration2", Id = 2 };

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedIteration1)
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedIteration2)
            });

        // Act
        var result = await _iterationService.CreateIterationAsync(projectId, iterations);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Children.Count);
        Assert.Contains(result.Children, i => i.Name == "Iteration1");
        Assert.Contains(result.Children, i => i.Name == "Iteration2");
    }

    [Fact]
    public async Task CreateIterationAsync_WithNestedIterations_ReturnsCreatedIterations()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var iterations = new Iteration
        {
            Children =
            [
                new Iteration
                {
                    Name = "Iteration1",
                    Id = 1,
                    Children =
                    [
                        new Iteration { Name = "SubIteration1", Id = 3, Children = [] }
                    ]
                }
            ]
        };
        var expectedIteration1 = new Iteration { Name = "Iteration1", Id = 1 };
        var expectedSubIteration1 = new Iteration { Name = "SubIteration1", Id = 3 };

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedIteration1)
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedSubIteration1)
            });

        // Act
        var result = await _iterationService.CreateIterationAsync(projectId, iterations);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Children);
        Assert.Equal("Iteration1", result.Children[0].Name);
        Assert.Single(result.Children[0].Children);
        Assert.Equal("SubIteration1", result.Children[0].Children[0].Name);
    }

    [Fact]
    public async Task MoveIteration_WithMultipleIterations_ReturnsSuccess()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var iterations = new List<Iteration>
    {
        new() { Name = "Iteration1", Id = 1, Children = [] },
        new() { Name = "Iteration2", Id = 2, Children = [] }
    };
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        await _iterationService.MoveIterationAsync(projectId, iterations, "");

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task MoveIteration_WithNestedIterations_ReturnsSuccess()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var iterations = new List<Iteration>
        {
            new()
            {
                Name = "Iteration1",
                Id = 1,
                Children =
                [
                    new Iteration { Name = "SubIteration1", Id = 3, Children = [] }
                ]
            }
        };
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        await _iterationService.MoveIterationAsync(projectId, iterations, "");

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
