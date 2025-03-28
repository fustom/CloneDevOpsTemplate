using CloneDevOpsTemplate.Services;
using CloneDevOpsTemplate.Models;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using CloneDevOpsTemplate.Constants;

namespace CloneDevOpsTemplateTest.Services;

public class WorkItemServiceTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly WorkItemService _workItemService;

    public WorkItemServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(Const.ServiceRootUrl)
        };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _workItemService = new WorkItemService(httpClientFactoryMock.Object);        
    }

    [Fact]
    public async Task GetWorkItemsAsync_ReturnsWorkItemQueryList_WhenResponseIsSuccessful()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectName = "TestProject";
        var expectedResponse = new WorkItemQueryList() { AsOf = DateTime.Now, WorkItems = [new WorkItemQueryItem() { Id = 666 }] };

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
        var result = await _workItemService.GetWorkItemsAsync(projectId, projectName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.AsOf, result.AsOf);
        Assert.True(result.WorkItems.Length > 0);
        Assert.Equal(expectedResponse.WorkItems[0].Id, result.WorkItems[0].Id);        
    }

    [Fact]
    public async Task GetWorkItemsAsync_ReturnsEmptyWorkItemQueryList_WhenResponseIsUnsuccessful()
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
        var result = await _workItemService.GetWorkItemsAsync(projectId, projectName);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.WorkItems);
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

