using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MyTestProject.Service.Tests.Common;

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
    public async Task CreateImportRequestAsync_ReturnsGitImportRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var repositoryId = Guid.NewGuid();
        var sourceRepositoryRemoteUrl = "https://example.com/repo.git";
        var serviceEndpointId = Guid.NewGuid();
        var gitImportRequest = new GitImportRequest
        {
            ImportRequestId = 1,
            Status = GitAsyncOperationStatus.Completed
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
                Content = JsonContent.Create(gitImportRequest)
            });

        // Act
        var result = await _repositoryService.CreateImportRequestAsync(projectId, repositoryId, sourceRepositoryRemoteUrl, serviceEndpointId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gitImportRequest.ImportRequestId, result.ImportRequestId);
        Assert.Equal(gitImportRequest.Status, result.Status);
    }

    [Fact]
    public async Task CreateImportRequestAsync_ReturnsNullOnFailure()
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
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(async () => await _repositoryService.CreateImportRequestAsync(projectId, repositoryId, sourceRepositoryRemoteUrl, serviceEndpointId));
    }

    [Fact]
    public async Task GetGitPullRequest_ReturnsGitPullRequests()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var gitPullRequests = new GitPullRequests
        {
            Count = 2,
            Value =
            [
                new GitPullRequest { PullRequestId = 1, Title = "PR 1" },
                new GitPullRequest { PullRequestId = 2, Title = "PR 2" }
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
                Content = JsonContent.Create(gitPullRequests)
            });

        // Act
        var result = await _repositoryService.GetGitPullRequest(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gitPullRequests.Count, result.Count);
        Assert.Equal(gitPullRequests.Value[0].PullRequestId, result.Value[0].PullRequestId);
        Assert.Equal(gitPullRequests.Value[1].PullRequestId, result.Value[1].PullRequestId);
    }

    [Fact]
    public async Task GetGitPullRequest_ReturnsNullOnFailure()
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
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _repositoryService.GetGitPullRequest(projectId));
    }
}