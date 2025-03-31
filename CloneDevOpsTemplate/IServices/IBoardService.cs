using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface IBoardService
{
    Task<Boards?> GetBoardsAsync(Guid projectId, Guid teamId);
    Task<Board?> GetBoardAsync(Guid projectId, Guid teamId, Guid boardId);

    Task<BoardColumns?> GetBoardColumnsAsync(Guid projectId, Guid teamId, Guid boardId);
    Task<HttpResponseMessage> UpdateBoardColumnsAsync(Guid projectId, Guid teamId, Guid boardId, BoardColumns boardColumns);
    Task MoveBoardColumnsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards);

    Task<BoardRows?> GetBoardRowsAsync(Guid projectId, Guid teamId, Guid boardId);
    Task<HttpResponseMessage> UpdateBoardRowsAsync(Guid projectId, Guid teamId, Guid boardId, BoardRows boardRows);
    Task MoveBoardRowsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards);

    Task<CardSettings?> GetCardSettingsAsync(Guid projectId, Guid teamId, Guid boardId);
    Task<HttpResponseMessage> UpdateCardSettingsAsync(Guid projectId, Guid teamId, Guid boardId, CardSettings boardCards);
    Task MoveCardSettingsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards);

    Task<CardStyles?> GetCardStylesAsync(Guid projectId, Guid teamId, Guid boardId);
    Task<HttpResponseMessage> UpdateCardStylesAsync(Guid projectId, Guid teamId, Guid boardId, CardStyles cardStyle);
    Task MoveCardStylesAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards);
}
