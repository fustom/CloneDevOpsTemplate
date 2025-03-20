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
            List<Board> boards = [];
            foreach (var board in boardValues.Value)
            {
                var currentBoard = await _boardService.GetBoardAsync(projectId, teamId, board.Id);
                if (currentBoard != null)
                {
                    // Query rows separately, since row colors are ALWAYS null from the GetBoard call
                    var rows = await _boardService.GetBoardRowsAsync(projectId, teamId, currentBoard.Id) ?? new();
                    currentBoard.Rows = rows.Value;
                    boards.Add(currentBoard);
                }
            }
            return View(boards.ToArray());
        }
    }
}
