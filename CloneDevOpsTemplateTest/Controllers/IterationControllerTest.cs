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
    public async Task Iterations_ReturnsViewResult_WithNewIteration_WhenModelStateIsInvalid()
    {
        // Arrange
        var mockIterationService = new Mock<IIterationService>();
        var projectId = Guid.NewGuid();

        var controller = new IterationController(mockIterationService.Object);
        controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await controller.Iterations(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<Iteration>(viewResult.Model);
    }

    [Fact]
    public async Task Areas_ReturnsViewResult_WithAreas()
    {
        // Arrange
        var mockIterationService = new Mock<IIterationService>();
        var projectId = Guid.NewGuid();
        var expectedAreas = new Iteration();
        mockIterationService
            .Setup(service => service.GetAreaAsync(projectId))
            .ReturnsAsync(expectedAreas);

        var controller = new IterationController(mockIterationService.Object);

        // Act
        var result = await controller.Areas(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedAreas, viewResult.Model);
    }

    [Fact]
    public async Task Areas_ReturnsViewResult_WithNewIteration_WhenServiceReturnsNull()
    {
        // Arrange
        var mockIterationService = new Mock<IIterationService>();
        var projectId = Guid.NewGuid();
        mockIterationService
            .Setup(service => service.GetAreaAsync(projectId))
            .ReturnsAsync((Iteration?)null);

        var controller = new IterationController(mockIterationService.Object);

        // Act
        var result = await controller.Areas(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<Iteration>(viewResult.Model);
    }

    [Fact]
    public async Task Areas_ReturnsViewResult_WithNewIteration_WhenModelStateIsInvalid()
    {
        // Arrange
        var mockIterationService = new Mock<IIterationService>();
        var projectId = Guid.NewGuid();

        var controller = new IterationController(mockIterationService.Object);
        controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await controller.Areas(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<Iteration>(viewResult.Model);
    }
}
