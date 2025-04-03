using CloneDevOpsTemplate.Services;
using CloneDevOpsTemplate.Models;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using CloneDevOpsTemplate.Constants;
using MyTestProject.Service.Tests.Common;
using Microsoft.Extensions.Configuration;

namespace CloneDevOpsTemplateTest.Services;

public class WorkItemServiceTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly WorkItemService _workItemService;

    public WorkItemServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var configuration = ConfigurationFactory.GetConfiguration();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(configuration.GetValue<string>("ServiceRootUrl") ?? string.Empty)
        };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _workItemService = new WorkItemService(httpClientFactoryMock.Object);        
    }

    [Fact]
    public async Task GetWorkItemsListAsync_ReturnsWorkItemQueryList_WhenResponseIsSuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectName = "TestProject";
        var expectedResponse = new WorkItemsListQueryResult() { AsOf = DateTime.Now, WorkItems = [new WorkItemsListQueryItem() { Id = 666 }] };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        // Act
        var result = await _workItemService.GetWorkItemsListAsync(projectId, projectName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.AsOf, result.AsOf);
        Assert.True(result.WorkItems.Length > 0);
        Assert.Equal(expectedResponse.WorkItems[0].Id, result.WorkItems[0].Id);        
    }

    [Fact]
    public async Task GetWorkItemsListAsync_ReturnsEmptyWorkItemQueryList_WhenResponseIsUnsuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectName = "TestProject";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var result = await _workItemService.GetWorkItemsListAsync(projectId, projectName);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.WorkItems);
    }

    [Fact]
    public async Task GetWorkItemsAsync_ReturnsWorkItems_WhenResponseIsSuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var workItemIds = new[] { 1, 2, 3 };
        var expectedWorkItems = new WorkItems
        {
            Value =
            [
                new WorkItem { Id = 1, Fields = new Fields { SystemTitle = "Work Item 1" } },
                new WorkItem { Id = 2, Fields = new Fields { SystemTitle = "Work Item 2" } },
                new WorkItem { Id = 3, Fields = new Fields { SystemTitle = "Work Item 3" } }
            ]
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedWorkItems)
            });

        // Act
        var result = await _workItemService.GetWorkItemsAsync(projectId, workItemIds);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedWorkItems.Value.Length, result.Value.Length);
        for (int i = 0; i < expectedWorkItems.Value.Length; i++)
        {
            Assert.Equal(expectedWorkItems.Value[i].Id, result.Value[i].Id);
            Assert.Equal(expectedWorkItems.Value[i].Fields.SystemTitle, result.Value[i].Fields.SystemTitle);
        }
    }

    [Fact]
    public async Task GetWorkItemsAsync_ReturnsEmptyWorkItems_WhenResponseIsUnsuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var workItemIds = new[] { 1, 2, 3 };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var result = await _workItemService.GetWorkItemsAsync(projectId, workItemIds);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetWorkItemAsync_ReturnsWorkItem_WhenResponseIsSuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var workItemId = 123;
        var expectedWorkItem = new WorkItem { Id = workItemId, Fields = new Fields { SystemTitle ="Test Work Item" } };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedWorkItem)
            });

        // Act
        var result = await _workItemService.GetWorkItemAsync(projectId, workItemId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedWorkItem.Id, result?.Id);
        Assert.Equal(expectedWorkItem.Fields.SystemTitle, result?.Fields.SystemTitle);
    }

    [Fact]
    public async Task GetWorkItemAsync_ReturnsNull_WhenResponseIsUnsuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var workItemId = 123;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _workItemService.GetWorkItemAsync(projectId, workItemId));
    }
}

