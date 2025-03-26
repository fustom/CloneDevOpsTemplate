using System.Net;
using System.Net.Http.Json;
using CloneDevOpsTemplate.Constants;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;

namespace CloneDevOpsTemplateTest.Services;

public class ServiceServiceTest
{
    private readonly ServiceService _serviceService;

    public ServiceServiceTest()
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new ServiceModel())
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new Uri(Const.ServiceRootUrl)
        };
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        DefaultHttpContext httpContext = new()
        {
            Session = new Mock<ISession>().Object
        };
        httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(httpContext);

        _serviceService = new ServiceService(httpClientFactoryMock.Object, httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task GetServicesAsync_ShouldReturnServicesModel()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        // Act
        var result = await _serviceService.GetServicesAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ServicesModel>(result);
    }

    [Fact]
    public async Task CreateServiceAsync_ShouldReturnServiceModel()
    {
        // Arrange
        var serviceName = "TestService";
        var templateRepositoryRemoteUrl = "https://example.com/repo.git";
        var endpointName = "TestEndpoint";
        var projectId = Guid.NewGuid();

        // Act
        var result = await _serviceService.CreateServiceAsync(serviceName, templateRepositoryRemoteUrl, endpointName, projectId);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ServiceModel>(result);
    }
}