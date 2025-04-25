using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
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

    [Fact]
    public async Task TestSuites_InvalidModelState_ReturnsViewWithEmptyValue()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");
        var projectId = Guid.NewGuid();
        var testPlanId = 1;

        // Act
        var result = await _controller.TestSuites(projectId, testPlanId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<TestSuite[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task TestSuites_ValidModelState_ReturnsViewWithTestSuitesValue()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var testPlanId = 1;
        var testSuites = new TestSuites { Value = [new TestSuite { Name = "Test Suite Value" }] };
        _mockTestService.Setup(service => service.GetTestSuitesAsync(projectId, testPlanId))
            .ReturnsAsync(testSuites);

        // Act
        var result = await _controller.TestSuites(projectId, testPlanId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(testSuites.Value, viewResult.Model);
    }

    [Fact]
    public async Task TestSuites_ValidModelState_NullTestSuites_ReturnsViewWithEmptyValue()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var testPlanId = 1;
        _mockTestService.Setup(service => service.GetTestSuitesAsync(projectId, testPlanId))
            .ReturnsAsync((TestSuites)null!);

        // Act
        var result = await _controller.TestSuites(projectId, testPlanId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<TestSuite[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }
}
