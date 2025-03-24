using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers
{
    public class BoardsController(IBoardService boardService) : Controller
    {
        private readonly IBoardService _boardService = boardService;

        async public Task<IActionResult> Boards(Guid projectId, Guid teamId)
        {
            Boards boardValues = await _boardService.GetBoardsAsync(projectId, teamId) ?? new Boards();
            List<ViewBoard> boards = [];
            foreach (var board in boardValues.Value)
            {
                var currentBoard = await _boardService.GetBoardAsync(projectId, teamId, board.Id) ?? new();

                // Query rows separately, since row colors are ALWAYS null from the GetBoard call
                var rows = await _boardService.GetBoardRowsAsync(projectId, teamId, currentBoard.Id) ?? new();
                currentBoard.Rows = rows.Value;
                boards.Add(new() { Board = currentBoard, ProjectId = projectId, TeamId = teamId });
            }
            return View(boards.ToArray());
        }

        async public Task<IActionResult> CardSettings(Guid projectId, Guid teamId, string boardId)
        {
            var cardSettings = await _boardService.GetCardSettingsAsync(projectId, teamId, boardId) ?? new();
            return View(cardSettings.Cards);
        }

        async public Task<IActionResult> CardStyles(Guid projectId, Guid teamId, string boardId)
        {
            var cardStyles = await _boardService.GetCardStylesAsync(projectId, teamId, boardId) ?? new();
            return View(cardStyles.Rules);
        }
    }
}
