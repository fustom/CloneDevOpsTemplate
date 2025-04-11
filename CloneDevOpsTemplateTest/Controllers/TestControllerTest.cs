using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class TestControllerTest
{
    private readonly Mock<ITestService> _mockTestService;
    private readonly TestController _controller;

    public TestControllerTest()
    {
        _mockTestService = new Mock<ITestService>();
        _controller = new TestController(_mockTestService.Object);
    }

    [Fact]
    public async Task TestPlans_InvalidModelState_ReturnsViewWithEmptyValue()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");
        var projectId = Guid.NewGuid();

        // Act
        var result = await _controller.TestPlans(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<TestPlan[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task TestPlans_ValidModelState_ReturnsViewWithTestPlansValue()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var testPlans = new TestPlans { Value = [new TestPlan { Name = "Test Plan Value" }] };
        _mockTestService.Setup(service => service.GetTestPlansAsync(projectId))
            .ReturnsAsync(testPlans);

        // Act
        var result = await _controller.TestPlans(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(testPlans.Value, viewResult.Model);
    }

    [Fact]
    public async Task TestPlans_ValidModelState_NullTestPlans_ReturnsViewWithEmptyValue()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockTestService.Setup(service => service.GetTestPlansAsync(projectId))
            .ReturnsAsync((TestPlans)null!);

        // Act
        var result = await _controller.TestPlans(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<TestPlan[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }
}