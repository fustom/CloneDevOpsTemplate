using Microsoft.AspNetCore.Mvc;
using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class HomeControllerTest
{
    [Fact]
    public void Index_ReturnsViewResult_WithCorrectViewBagMessage()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Please login to continue.", result.ViewData["LoginMessage"]);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Privacy() as ViewResult;

        // Assert
        Assert.NotNull(result);
    }
    [Fact]
    public void Login_Get_ReturnsViewResult_WithCorrectViewBagMessage()
    {
        var controller = new HomeController();

        // Act
        var result = controller.Login() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Please login to continue.", result.ViewData["LoginMessage"]);
        Assert.Equal("Index", result.ViewName);
    }


    [Fact]
    public void Login_Post_ValidModel_RedirectsToProjects()
    {
        // Arrange
        var httpContext = new Mock<HttpContext>();
        var session = new Mock<ISession>();
        httpContext.Setup(s => s.Session).Returns(session.Object);

        var controller = new HomeController();
        controller.ControllerContext.HttpContext = httpContext.Object;
        var loginModel = new LoginModel
        {
            OrganizationName = "TestOrg",
            AccessToken = "TestToken"
        };

        controller.ModelState.Clear(); // Simulate valid model state

        // Act
        var result = controller.Login(loginModel) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Projects", result.ActionName);
        Assert.Equal("Project", result.ControllerName);
    }

    [Fact]
    public void Login_Post_InvalidModel_ReturnsIndexView_WithErrorMessage()
    {
        // Arrange
        var controller = new HomeController();
        var loginModel = new LoginModel(); // Invalid model (e.g., missing required fields)

        controller.ModelState.AddModelError("Error", "Invalid model"); // Simulate invalid model state

        // Act
        var result = controller.Login(loginModel) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ViewName);
        Assert.Equal("Login failed. Please try again.", result.ViewData["LoginMessage"]);
    }
    [Fact]
    public void Logout_ClearsSession_AndReturnsIndexView_WithCorrectViewBagMessage()
    {
        // Arrange
        var httpContext = new Mock<HttpContext>();
        var session = new Mock<ISession>();
        httpContext.Setup(s => s.Session).Returns(session.Object);

        var controller = new HomeController();
        controller.ControllerContext.HttpContext = httpContext.Object;

        // Act
        var result = controller.Logout() as ViewResult;

        // Assert
        session.Verify(s => s.Clear(), Times.Once); // Verify session was cleared
        Assert.NotNull(result);
        Assert.Equal("Index", result.ViewName);
        Assert.Equal("You have been logged out.", result.ViewData["LoginMessage"]);
    }
}
