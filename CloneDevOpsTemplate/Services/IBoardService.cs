using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface IBoardService
{
    Task<Boards?> GetBoardsAsync(Guid projectId, Guid teamId);
    Task<Board?> GetBoardAsync(Guid projectId, Guid teamId, string boardId);

    Task<BoardColumns?> GetBoardColumnsAsync(Guid projectId, Guid teamId, string boardId);
    Task UpdateBoardColumnsAsync(Guid projectId, Guid teamId, string boardId, BoardColumns boardColumns);
    Task MoveBoardColumnsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards);

    Task<BoardRows?> GetBoardRowsAsync(Guid projectId, Guid teamId, string boardId);
    Task UpdateBoardRowsAsync(Guid projectId, Guid teamId, string boardId, BoardRows boardRows);
    Task MoveBoardRowsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards);

    Task<CardSettings?> GetCardSettingsAsync(Guid projectId, Guid teamId, string boardId);
    Task UpdateCardSettingsAsync(Guid projectId, Guid teamId, string boardId, CardSettings cards);
    Task MoveCardSettingsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards);

    Task<CardStyles?> GetCardStylesAsync(Guid projectId, Guid teamId, string boardId);
    Task UpdateCardStylesAsync(Guid projectId, Guid teamId, string boardId, CardStyles cardStyle);
    Task MoveCardStylesAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards);
}
