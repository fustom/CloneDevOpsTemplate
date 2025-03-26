using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class IterationControllerTest
{
    [Fact]
    public async Task Iterations_ReturnsViewResult_WithIterations()
    {
        // Arrange
        var mockIterationService = new Mock<IIterationService>();
        var projectId = Guid.NewGuid();
        var expectedIterations = new Iteration();
        mockIterationService
            .Setup(service => service.GetIterationsAsync(projectId))
            .ReturnsAsync(expectedIterations);

        var controller = new IterationController(mockIterationService.Object);

        // Act
        var result = await controller.Iterations(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedIterations, viewResult.Model);
    }

    [Fact]
    public async Task Iterations_ReturnsViewResult_WithNewIteration_WhenServiceReturnsNull()
    {
        // Arrange
        var mockIterationService = new Mock<IIterationService>();
        var projectId = Guid.NewGuid();
        mockIterationService
            .Setup(service => service.GetIterationsAsync(projectId))
            .ReturnsAsync((Iteration?)null);

        var controller = new IterationController(mockIterationService.Object);

        // Act
        var result = await controller.Iterations(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<Iteration>(viewResult.Model);
    }

    [Fact]
    public async Task CreateIteration_RedirectsToIterations_AfterSuccessfulCreation()
    {
        // Arrange
        var mockIterationService = new Mock<IIterationService>();
        var projectId = Guid.NewGuid();
        var iterationRequest = new CreateIterationRequest();

        mockIterationService
            .Setup(service => service.CreateIterationAsync(projectId, iterationRequest))
            .Returns(Task.FromResult(new Iteration()));

        var controller = new IterationController(mockIterationService.Object);

        // Act
        var result = await controller.CreateIteration(projectId, iterationRequest);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Iterations", redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(projectId, redirectResult.RouteValues["projectId"]);
        mockIterationService.Verify(service => service.CreateIterationAsync(projectId, iterationRequest), Times.Once);
    }

    [Fact]
    public async Task CreateIteration_ThrowsException_WhenServiceFails()
    {
        // Arrange
        var mockIterationService = new Mock<IIterationService>();
        var projectId = Guid.NewGuid();
        var iterationRequest = new CreateIterationRequest();

        mockIterationService
            .Setup(service => service.CreateIterationAsync(projectId, iterationRequest))
            .ThrowsAsync(new Exception("Service failure"));

        var controller = new IterationController(mockIterationService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => controller.CreateIteration(projectId, iterationRequest));
        mockIterationService.Verify(service => service.CreateIterationAsync(projectId, iterationRequest), Times.Once);
    }
}