using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class ServiceControllerTest
{
    private readonly Mock<IServiceService> _mockRepositoryService;
    private readonly ServiceController _controller;

    public ServiceControllerTest()
    {
        _mockRepositoryService = new Mock<IServiceService>();
        _controller = new ServiceController(_mockRepositoryService.Object);
    }

    [Fact]
    public async Task ProjectServices_ReturnsViewWithServices()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var servicesModel = new ServicesModel { Value = [] };

        _mockRepositoryService
            .Setup(service => service.GetServicesAsync(projectId))
            .ReturnsAsync(servicesModel);

        // Act
        var result = await _controller.ProjectServices(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Services", viewResult.ViewName);
        Assert.Equal(servicesModel.Value, viewResult.Model);
    }

    [Fact]
    public async Task ProjectServices_ReturnsViewWithEmptyServices_WhenServiceReturnsNull()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        _mockRepositoryService
            .Setup(service => service.GetServicesAsync(projectId))
            .ReturnsAsync((ServicesModel?)null);

        // Act
        var result = await _controller.ProjectServices(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Services", viewResult.ViewName);
        var viewModel = Assert.IsType<ServiceModel[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task Projects_InvalidModelState_ReturnsDefaultView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.ProjectServices(Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Services", viewResult.ViewName);
        Assert.IsType<ServiceModel[]>(viewResult.Model);
    }
}