using System.Net;
using System.Net.Http.Json;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MyTestProject.Service.Tests.Common;

namespace CloneDevOpsTemplateTest.Services;

public class ProjectServiceTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly ProjectService _projectService;

    public ProjectServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var configuration = ConfigurationFactory.GetConfiguration();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(configuration.GetValue<string>("ServiceRootUrl") ?? string.Empty)
        };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _projectService = new ProjectService(httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsProjects()
    {
        // Arrange
        var projectId1 = Guid.NewGuid();
        var projectId2 = Guid.NewGuid();
        var expectedProjects = new Projects
        {
            Count = 2,
            Value =
            [
                new ProjectBase
                {
                    Id = projectId1
                },
                new ProjectBase
                {
                    Id = projectId2
                }
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
                Content = JsonContent.Create(expectedProjects)
            });

        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProjects.Value[0].Id, projectId1);
        Assert.Equal(expectedProjects.Value[1].Id, projectId2);
        Assert.Equal(expectedProjects.Count, result.Count);
    }

    [Fact]
    public async Task GetProjectAsync_ById_ReturnsProject()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var expectedProject = new Project
        {
            Id = projectId
        };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedProject)
            });

        // Act
        var result = await _projectService.GetProjectAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProject.Id, result.Id);
    }

    [Fact]
    public async Task GetProjectAsync_ByName_ReturnsProject()
    {
        // Arrange
        var projectName = "TestProject";
        var expectedProject = new Project
        {
            Name = projectName
        };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedProject)
            });

        // Act
        var result = await _projectService.GetProjectAsync(projectName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProject.Name, result.Name);
    }

    [Fact]
    public async Task CreateProjectAsync_ReturnsCreateProjectResponse()
    {
        // Arrange
        var name = "NewProject";
        var description = "Project Description";
        var processTemplateTypeId = Guid.NewGuid();
        var sourceControlType = "Git";
        var visibility = Visibility.Private;
        var expectedResponse = new CreateProjectResponse
        {
            Id = Guid.NewGuid(),
        };
        _httpMessageHandlerMock.Protected()
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
        var result = await _projectService.CreateProjectAsync(name, description, processTemplateTypeId, sourceControlType, visibility);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
    }

    [Fact]
    public async Task GetProjectPropertiesAsync_ReturnsProjectProperties()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var expectedProperties = new ProjectProperties
        {
            Count = 2,
            Value =
            [
                new ProjectProperty { Name = "Property1", Value = "Value1" },
                new ProjectProperty { Name = "Property2", Value = "Value2" }
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
                Content = JsonContent.Create(expectedProperties)
            });

        // Act
        var result = await _projectService.GetProjectPropertiesAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProperties.Count, result.Count);
        Assert.Equal(expectedProperties.Value[0].Name, result.Value[0].Name);
        Assert.Equal(expectedProperties.Value[0].Value.ToString(), result.Value[0].Value.ToString());
        Assert.Equal(expectedProperties.Value[1].Name, result.Value[1].Name);
        Assert.Equal(expectedProperties.Value[1].Value.ToString(), result.Value[1].Value.ToString());
    }
}