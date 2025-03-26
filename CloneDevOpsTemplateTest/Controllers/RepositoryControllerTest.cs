using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class RepositoryControllerTest
{
    private readonly Mock<IRepositoryService> _mockRepositoryService;
    private readonly RepositoryController _controller;

    public RepositoryControllerTest()
    {
        _mockRepositoryService = new Mock<IRepositoryService>();
        _controller = new RepositoryController(_mockRepositoryService.Object);
    }

    [Fact]
    public async Task Repositories_ReturnsViewWithRepositories()
    {
        // Arrange
        var mockRepositories = new Repositories { Value = [] };
        _mockRepositoryService
            .Setup(service => service.GetAllRepositoriesAsync())
            .ReturnsAsync(mockRepositories);

        // Act
        var result = await _controller.Repositories();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockRepositories.Value, viewResult.Model);
    }

    [Fact]
    public async Task Repositories_ReturnsViewWithEmptyRepositories_WhenServiceReturnsNull()
    {
        // Arrange
        _mockRepositoryService
            .Setup(service => service.GetAllRepositoriesAsync())
            .ReturnsAsync((Repositories?)null);

        // Act
        var result = await _controller.Repositories();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<Repository[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }

    [Fact]
    public async Task ProjectRepositories_ReturnsViewWithRepositories()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var mockRepositories = new Repositories { Value = [] };
        _mockRepositoryService
            .Setup(service => service.GetRepositoriesAsync(projectId))
            .ReturnsAsync(mockRepositories);

        // Act
        var result = await _controller.ProjectRepositories(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(mockRepositories.Value, viewResult.Model);
    }

    [Fact]
    public async Task ProjectRepositories_ReturnsViewWithEmptyRepositories_WhenServiceReturnsNull()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockRepositoryService
            .Setup(service => service.GetRepositoriesAsync(projectId))
            .ReturnsAsync((Repositories?)null);

        // Act
        var result = await _controller.ProjectRepositories(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<Repository[]>(viewResult.Model);
        Assert.Empty(viewModel);
    }
}