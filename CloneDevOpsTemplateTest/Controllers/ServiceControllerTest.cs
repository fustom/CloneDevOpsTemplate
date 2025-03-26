using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class ServiceControllerTest
{
    [Fact]
    public async Task ProjectServices_ReturnsViewWithServices()
    {
        // Arrange
        var mockServiceService = new Mock<IServiceService>();
        var projectId = Guid.NewGuid();
        var servicesModel = new ServicesModel { Value = [] };

        mockServiceService
            .Setup(service => service.GetServicesAsync(projectId))
            .ReturnsAsync(servicesModel);

        var controller = new ServiceController(mockServiceService.Object);

        // Act
        var result = await controller.ProjectServices(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Services", viewResult.ViewName);
        Assert.Equal(servicesModel.Value, viewResult.Model);
    }

    [Fact]
    public async Task ProjectServices_ReturnsViewWithEmptyServices_WhenServiceReturnsNull()
    {
        // Arrange
        var mockServiceService = new Mock<IServiceService>();
        var projectId = Guid.NewGuid();

        mockServiceService
            .Setup(service => service.GetServicesAsync(projectId))
            .ReturnsAsync((ServicesModel?)null);

        var controller = new ServiceController(mockServiceService.Object);

        // Act
        var result = await controller.ProjectServices(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Services", viewResult.ViewName);
        var viewModel = Assert.IsType<ServiceModel[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }
}