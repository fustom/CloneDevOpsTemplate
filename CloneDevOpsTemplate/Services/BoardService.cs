using System.Text.Json;
using System.Text.Json.Serialization;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class BoardService(IHttpClientFactory httpClientFactory) : IBoardService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<Boards?> GetBoardsAsync(Guid projectId, Guid teamId)
    {
        return _client.GetFromJsonAsync<Boards>($"{projectId}/{teamId}/_apis/work/boards?api-version=7.1");
    }

    public Task<Board?> GetBoardAsync(Guid projectId, Guid teamId, string boardId)
    {
        return _client.GetFromJsonAsync<Board>($"{projectId}/{teamId}/_apis/work/boards/{boardId}?api-version=7.1");
    }

    public Task<BoardColumns?> GetBoardColumnsAsync(Guid projectId, Guid teamId, string boardId)
    {
        return _client.GetFromJsonAsync<BoardColumns>($"{projectId}/{teamId}/_apis/work/boards/{boardId}/columns?api-version=7.1");
    }

    public Task UpdateBoardColumnsAsync(Guid projectId, Guid teamId, string boardId, BoardColumns boardColumns)
    {
        return _client.PutAsJsonAsync($"{projectId}/{teamId}/_apis/work/boards/{boardId}/columns?api-version=7.1", boardColumns.Value, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    public Task MoveBoardColumnsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards)
    {
        return ForAllMatchingBoards(templateProjectId, templateTeamId, projectBoards,
            async (templateBoardId, matchingProjectBoardId) =>
            {
                BoardColumns currentBoardColumns = await GetBoardColumnsAsync(projectId, projectTeamId, matchingProjectBoardId) ?? new();
                var incomingColumnId = currentBoardColumns.Value.FirstOrDefault(c => c.ColumnType == BoardColumnType.Incoming)?.Id;
                var outgoingColumnId = currentBoardColumns.Value.FirstOrDefault(c => c.ColumnType == BoardColumnType.Outgoing)?.Id;

                BoardColumns templateBoardColumns = await GetBoardColumnsAsync(templateProjectId, templateTeamId, templateBoardId) ?? new();
                foreach (var templateBoardColumn in templateBoardColumns.Value)
                {
                    if (templateBoardColumn.ColumnType == BoardColumnType.Incoming && incomingColumnId is not null)
                    {
                        templateBoardColumn.Id = incomingColumnId;
                    }
                    else if (templateBoardColumn.ColumnType == BoardColumnType.Outgoing && outgoingColumnId is not null)
                    {
                        templateBoardColumn.Id = outgoingColumnId;
                    }
                    else
                    {
                        templateBoardColumn.Id = string.Empty;
                    }
                }

                await UpdateBoardColumnsAsync(projectId, projectTeamId, matchingProjectBoardId, templateBoardColumns);
            });
    }

    public Task<BoardRows?> GetBoardRowsAsync(Guid projectId, Guid teamId, string boardId)
    {
        return _client.GetFromJsonAsync<BoardRows>($"{projectId}/{teamId}/_apis/work/boards/{boardId}/rows?api-version=7.1");
    }

    public Task UpdateBoardRowsAsync(Guid projectId, Guid teamId, string boardId, BoardRows boardRows)
    {
        return _client.PutAsJsonAsync($"{projectId}/{teamId}/_apis/work/boards/{boardId}/rows?api-version=7.1", boardRows.Value, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    public Task MoveBoardRowsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards)
    {
        return ForAllMatchingBoards(templateProjectId, templateTeamId, projectBoards,
            async (templateBoardId, matchingProjectBoardId) =>
            {
                BoardRows templateBoardRows = await GetBoardRowsAsync(templateProjectId, templateTeamId, templateBoardId) ?? new();
                await UpdateBoardRowsAsync(projectId, projectTeamId, matchingProjectBoardId, templateBoardRows);
            });
    }

    public Task<CardSettings?> GetCardSettingsAsync(Guid projectId, Guid teamId, string boardId)
    { 
        return _client.GetFromJsonAsync<CardSettings>($"{projectId}/{teamId}/_apis/work/boards/{boardId}/cardsettings?api-version=7.1");
    }

    public Task UpdateCardSettingsAsync(Guid projectId, Guid teamId, string boardId, CardSettings boardCards)
    {
        return _client.PutAsJsonAsync($"{projectId}/{teamId}/_apis/work/boards/{boardId}/cardsettings?api-version=7.1", boardCards, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    public Task MoveCardSettingsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards)
    {
        return ForAllMatchingBoards(templateProjectId, templateTeamId, projectBoards,
            async (templateBoardId, matchingProjectBoardId) =>
            {
                CardSettings templateBoardCards = await GetCardSettingsAsync(templateProjectId, templateTeamId, templateBoardId) ?? new();
                await UpdateCardSettingsAsync(projectId, projectTeamId, matchingProjectBoardId, templateBoardCards);
            });
    }

    public Task<CardStyles?> GetCardStylesAsync(Guid projectId, Guid teamId, string boardId)
    {
        return _client.GetFromJsonAsync<CardStyles>($"{projectId}/{teamId}/_apis/work/boards/{boardId}/cardrulesettings?api-version=7.1");
    }

    public Task UpdateCardStylesAsync(Guid projectId, Guid teamId, string boardId, CardStyles cardStyle)
    {
        return _client.PutAsJsonAsync($"{projectId}/{teamId}/_apis/work/boards/{boardId}/cardrulesettings?api-version=7.1", cardStyle, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    public Task MoveCardStylesAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards)
    {
        return ForAllMatchingBoards(templateProjectId, templateTeamId, projectBoards,
            async (templateBoardId, matchingProjectBoardId) =>
            {
                CardStyles templateCardStyles = await GetCardStylesAsync(templateProjectId, templateTeamId, templateBoardId) ?? new();
                await UpdateCardStylesAsync(projectId, projectTeamId, matchingProjectBoardId, templateCardStyles);
            });
    }

    public async Task ForAllMatchingBoards(Guid templateProjectId, Guid templateTeamId, Boards projectBoards, Action<string, string> moveAction)
    {
        Boards templateBoards = await GetBoardsAsync(templateProjectId, templateTeamId) ?? new();
        foreach (BoardValue templateBoard in templateBoards.Value)
        {
            var matchingProjectBoard = projectBoards.Value.SingleOrDefault(b => string.Compare(b.Name, templateBoard.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (matchingProjectBoard != null)
            {
                moveAction(templateBoard.Id, matchingProjectBoard.Id);
            }
        }
    }
}
