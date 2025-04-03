using CloneDevOpsTemplate.Services;
using CloneDevOpsTemplate.Models;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using MyTestProject.Service.Tests.Common;
using Microsoft.Extensions.Configuration;

namespace CloneDevOpsTemplateTest.Services;

public class RepositoryServiceTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly RepositoryService _repositoryService;

    public RepositoryServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var configuration = ConfigurationFactory.GetConfiguration();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(configuration.GetValue<string>("ServiceRootUrl") ?? string.Empty)
        };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _repositoryService = new RepositoryService(httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetAllRepositoriesAsync_ReturnsRepositories()
    {
        // Arrange
        var repositories = new Repositories
        {
            Count = 2,
            Value =
            [
                new Repository
                {
                    Id = Guid.NewGuid()
                },
                new Repository
                {
                    Id = Guid.NewGuid()
                }
            ]
        };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(repositories)
            });

        // Act
        var result = await _repositoryService.GetAllRepositoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(repositories.Count, result.Count);
        Assert.Equal(repositories.Value[0].Id, result.Value[0].Id);
        Assert.Equal(repositories.Value[1].Id, result.Value[1].Id);
    }

    [Fact]
    public async Task GetRepositoriesAsync_ReturnsRepositories()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var repositories = new Repositories
        {
            Value =
            [
                new Repository
                {
                    Id = projectId
                }
            ]
        };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(repositories)
            });

        // Act
        var result = await _repositoryService.GetRepositoriesAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(repositories.Value[0].Id, result.Value[0].Id);
    }

    [Fact]
    public async Task CreateRepositoryAsync_ReturnsRepository()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var repository = new Repository
        {
            Id = projectId
        };
        var name = "TestRepo";
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(repository)
            });

        // Act
        var result = await _repositoryService.CreateRepositoryAsync(projectId, name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(repository.Id, result.Id);
    }

    [Fact]
    public async Task DeleteRepositoryAsync_DeletesRepository()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var repositoryId = Guid.NewGuid();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        await _repositoryService.DeleteRepositoryAsync(projectId, repositoryId);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task CreateImportRequest_CreatesImportRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var repositoryId = Guid.NewGuid();
        var sourceRepositoryRemoteUrl = "https://example.com/repo.git";
        var serviceEndpointId = Guid.NewGuid();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        await _repositoryService.CreateImportRequest(projectId, repositoryId, sourceRepositoryRemoteUrl, serviceEndpointId);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}