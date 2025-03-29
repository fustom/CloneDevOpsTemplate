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

    [Fact]
    public async Task CreateAreaAsync_ReturnsCreatedArea()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var createIterationRequest = new CreateIterationRequest { Name = "Area1" };
        var expectedArea = new Iteration { Name = createIterationRequest.Name };

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
        var result = await _iterationService.CreateAreaAsync(projectId, createIterationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedArea.Name, result.Name);
    }

    [Fact]
    public async Task CreateAreaAsync_ReturnsExistingAreaOnConflict()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var createIterationRequest = new CreateIterationRequest { Name = "Area1" };
        var existingArea = new Iteration { Name = createIterationRequest.Name };

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(existingArea)
            });

        // Act
        var result = await _iterationService.CreateAreaAsync(projectId, createIterationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingArea.Name, result.Name);
    }

    [Fact]
    public async Task CreateAreaAsync_ReturnsEmptyIterationOnFailure()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var createIterationRequest = new CreateIterationRequest { Name = "Area1" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var result = await _iterationService.CreateAreaAsync(projectId, createIterationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Name);
    }

    [Fact]
    public async Task CreateAreaAsync_WithIterations_ReturnsCreatedAreas()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var iterations = new Iteration
        {
            Children =
            [
                new Iteration { Name = "Area1", Id = 1, Children = [] },
                new Iteration { Name = "Area2", Id = 2, Children = [] }
            ]
        };
        var expectedArea1 = new Iteration { Name = "Area1", Id = 1 };
        var expectedArea2 = new Iteration { Name = "Area2", Id = 2 };

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedArea1)
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedArea2)
            });

        // Act
        var result = await _iterationService.CreateAreaAsync(projectId, iterations);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Children.Count);
        Assert.Contains(result.Children, i => i.Name == "Area1");
        Assert.Contains(result.Children, i => i.Name == "Area2");
    }

    [Fact]
    public async Task CreateAreaAsync_WithNestedAreas_ReturnsCreatedAreas()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var iterations = new Iteration
        {
            Children =
            [
                new Iteration
                {
                    Name = "Area1",
                    Id = 1,
                    Children =
                    [
                        new Iteration { Name = "SubArea1", Id = 3, Children = [] }
                    ]
                }
            ]
        };
        var expectedArea1 = new Iteration { Name = "Area1", Id = 1 };
        var expectedSubArea1 = new Iteration { Name = "SubArea1", Id = 3 };

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedArea1)
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedSubArea1)
            });

        // Act
        var result = await _iterationService.CreateAreaAsync(projectId, iterations);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Children);
        Assert.Equal("Area1", result.Children[0].Name);
        Assert.Single(result.Children[0].Children);
        Assert.Equal("SubArea1", result.Children[0].Children[0].Name);
    }
    [Fact]
    public async Task MoveAreaAsync_ReturnsSuccess()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var path = "Area1";
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
        await _iterationService.MoveAreaAsync(projectId, path, id);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains($"{projectId}/_apis/wit/classificationNodes/areas/{path}?api-version=7.1") &&
                req.Content != null),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task MoveAreaAsync_ReturnsFailure()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var path = "Area1";
        var id = 1;
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await _iterationService.MoveAreaAsync(projectId, path, id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains($"{projectId}/_apis/wit/classificationNodes/areas/{path}?api-version=7.1") &&
                req.Content != null),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    [Fact]
    public async Task MoveAreaAsync_WithMultipleIterations_ReturnsSuccess()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var iterations = new List<Iteration>
        {
            new() { Name = "Area1", Id = 1, Children = [] },
            new() { Name = "Area2", Id = 2, Children = [] }
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
        await _iterationService.MoveAreaAsync(projectId, iterations, "");

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task MoveAreaAsync_WithNestedIterations_ReturnsSuccess()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var iterations = new List<Iteration>
        {
            new()
            {
                Name = "Area1",
                Id = 1,
                Children =
                [
                    new Iteration { Name = "SubArea1", Id = 3, Children = [] }
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
        await _iterationService.MoveAreaAsync(projectId, iterations, "");

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
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
        var result = await _iterationService.GetAreaAsync(projectId);

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
        await Assert.ThrowsAsync<HttpRequestException>(() => _iterationService.GetAreaAsync(projectId));
    }
}