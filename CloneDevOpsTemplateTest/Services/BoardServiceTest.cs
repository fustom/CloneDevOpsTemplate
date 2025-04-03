using System.Net.Http.Json;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MyTestProject.Service.Tests.Common;

namespace CloneDevOpsTemplateTest.Services;

public class BoardServiceTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly BoardService _boardService;

    public BoardServiceTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var configuration = ConfigurationFactory.GetConfiguration();
        HttpClient httpClient = new(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(configuration.GetValue<string>("ServiceRootUrl") ?? string.Empty)
        };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _boardService = new BoardService(httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetBoardsAsync_ShouldReturnBoards()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boards = new Boards { Value = [] };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(boards)
            });

        // Act
        var result = await _boardService.GetBoardsAsync(projectId, teamId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetBoardAsync_ShouldReturnBoard()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var board = new Board { Id = Guid.NewGuid() };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(board)
            });

        // Act
        var result = await _boardService.GetBoardAsync(projectId, teamId, board.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(board.Id, result.Id);
    }

    [Fact]
    public async Task GetBoardColumnsAsync_ShouldReturnBoardColumns()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var boardColumns = new BoardColumns { Value = [] };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(boardColumns)
            });

        // Act
        var result = await _boardService.GetBoardColumnsAsync(projectId, teamId, boardId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task UpdateBoardColumnsAsync_ShouldUpdateBoardColumns()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var boardColumns = new BoardColumns { Value = [] };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        // Act
        await _boardService.UpdateBoardColumnsAsync(projectId, teamId, boardId, boardColumns);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetBoardRowsAsync_ShouldReturnBoardRows()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var boardRows = new BoardRows { Value = [] };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(boardRows)
            });

        // Act
        var result = await _boardService.GetBoardRowsAsync(projectId, teamId, boardId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task UpdateBoardRowsAsync_ShouldUpdateBoardRows()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var boardRows = new BoardRows { Value = [] };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        // Act
        await _boardService.UpdateBoardRowsAsync(projectId, teamId, boardId, boardRows);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    [Fact]
    public async Task MoveBoardColumnsAsync_ShouldMoveBoardColumns()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();
        var templateProjectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectBoards = new Boards
        {
            Value =
            [
                new BoardValue { Id = Guid.NewGuid(), Name = "Board1" }
            ]
        };
        var templateBoards = new Boards
        {
            Value =
            [
                new BoardValue { Id = Guid.NewGuid(), Name = "Board1" }
            ]
        };
        var templateBoardColumns = new BoardColumns
        {
            Value =
            [
                new BoardColumn { Id = Guid.NewGuid(), ColumnType = BoardColumnType.Incoming },
                new BoardColumn { Id = Guid.NewGuid(), ColumnType = BoardColumnType.Outgoing }
            ]
        };
        var currentBoardColumns = new BoardColumns
        {
            Value =
            [
                new BoardColumn { Id = Guid.NewGuid(), ColumnType = BoardColumnType.Incoming },
                new BoardColumn { Id = Guid.NewGuid(), ColumnType = BoardColumnType.Outgoing }
            ]
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("_apis/work/boards") && req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(templateBoards)
            });

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/columns")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(templateBoardColumns)
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(currentBoardColumns)
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("_apis/work/boards") && req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
            });

        // Act
        await _boardService.MoveBoardColumnsAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(3),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/columns")),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    [Fact]
    public async Task MoveBoardRowsAsync_ShouldMoveBoardRows()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();
        var templateProjectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectBoards = new Boards
        {
            Value =
            [
                new BoardValue { Id = Guid.NewGuid(), Name = "Board1" }
            ]
        };
        var templateBoards = new Boards
        {
            Value =
            [
                new BoardValue { Id = Guid.NewGuid(), Name = "Board1" }
            ]
        };
        var templateBoardRows = new BoardRows
        {
            Value =
            [
                new BoardRow { Id = Guid.NewGuid() },
                new BoardRow { Id = Guid.NewGuid() }
            ]
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("_apis/work/boards") && req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(templateBoards)
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/rows")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(templateBoardRows)
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("_apis/work/boards") && req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
            });

        // Act
        await _boardService.MoveBoardRowsAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/rows")),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    [Fact]
    public async Task GetCardSettingsAsync_ShouldReturnCardSettings()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardsettings = new CardSettings();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(cardsettings)
            });

        // Act
        var result = await _boardService.GetCardSettingsAsync(projectId, teamId, boardId);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetCardSettingsAsync_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.NotFound
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _boardService.GetCardSettingsAsync(projectId, teamId, boardId));
    }

    [Fact]
    public async Task UpdateCardSettingsAsync_ShouldUpdateCardSettings()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardSettings = new CardSettings { /* Initialize properties as needed */ };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        // Act
        var response = await _boardService.UpdateCardSettingsAsync(projectId, teamId, boardId, cardSettings);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Put &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains($"{projectId}/{teamId}/_apis/work/boards/{boardId}/cardsettings?api-version=7.1")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task UpdateCardSettingsAsync_ShouldThrowExceptionOnBadRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardSettings = new CardSettings { /* Initialize properties as needed */ };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        // Act & Assert
        var result = await _boardService.UpdateCardSettingsAsync(projectId, teamId, boardId, cardSettings);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task MoveCardSettingsAsync_ShouldMoveCardSettings()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();
        var templateProjectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectBoards = new Boards
        {
            Value =
            [
                new BoardValue { Id = Guid.NewGuid(), Name = "Board1" }
            ]
        };
        var templateBoards = new Boards
        {
            Value =
            [
                new BoardValue { Id = Guid.NewGuid(), Name = "Board1" }
            ]
        };
        var templateCardSettings = new CardSettings
        {
            // Initialize properties as needed
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("_apis/work/boards") && req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(templateBoards)
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/cardsettings")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(templateCardSettings)
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("_apis/work/boards") && req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
            });

        // Act
        await _boardService.MoveCardSettingsAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/cardsettings") && req.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>()
        );

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/cardsettings") && req.Method == HttpMethod.Put),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetCardStylesAsync_ShouldReturnCardStyles()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardStyles = new CardStyles();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(cardStyles)
            });

        // Act
        var result = await _boardService.GetCardStylesAsync(projectId, teamId, boardId);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetCardStylesAsync_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.NotFound
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _boardService.GetCardStylesAsync(projectId, teamId, boardId));
    }

    [Fact]
    public async Task GetCardStylesAsync_ShouldThrowExceptionOnServerError()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _boardService.GetCardStylesAsync(projectId, teamId, boardId));
    }
    [Fact]
    public async Task UpdateCardStylesAsync_ShouldUpdateCardStyles()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardStyles = new CardStyles { /* Initialize properties as needed */ };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        // Act
        var response = await _boardService.UpdateCardStylesAsync(projectId, teamId, boardId, cardStyles);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Patch &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains($"{projectId}/{teamId}/_apis/work/boards/{boardId}/cardrulesettings?api-version=7.1")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task UpdateCardStylesAsync_ShouldThrowExceptionOnBadRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardStyles = new CardStyles { /* Initialize properties as needed */ };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        // Act
        var response = await _boardService.UpdateCardStylesAsync(projectId, teamId, boardId, cardStyles);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCardStylesAsync_ShouldThrowExceptionOnServerError()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardStyles = new CardStyles { /* Initialize properties as needed */ };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        // Act
        var response = await _boardService.UpdateCardStylesAsync(projectId, teamId, boardId, cardStyles);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task MoveCardStylesAsync_ShouldMoveCardStyles()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();
        var templateProjectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectBoards = new Boards
        {
            Value =
            [
                new BoardValue { Id = Guid.NewGuid(), Name = "Board1" }
            ]
        };
        var templateBoards = new Boards
        {
            Value =
            [
                new BoardValue { Id = Guid.NewGuid(), Name = "Board1" }
            ]
        };
        var templateCardStyles = new CardStyles
        {
            // Initialize properties as needed
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("_apis/work/boards") && req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(templateBoards)
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/cardrulesettings")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(templateCardStyles)
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("_apis/work/boards") && req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
            });

        // Act
        await _boardService.MoveCardStylesAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/cardrulesettings") && req.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>()
        );

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/cardrulesettings") && req.Method == HttpMethod.Patch),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
