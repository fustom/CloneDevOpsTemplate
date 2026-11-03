using System.Text.Json;
using System.Text.Json.Serialization;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class BoardService(IHttpClientFactory httpClientFactory) : IBoardService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<Boards?> GetBoardsAsync(Guid projectId, Guid teamId)
    {
        return _client.GetFromJsonAsync<Boards>($"{projectId}/{teamId}/_apis/work/boards?api-version=7.1");
    }

    public Task<Board?> GetBoardAsync(Guid projectId, Guid teamId, Guid boardId)
    {
        return _client.GetFromJsonAsync<Board>($"{projectId}/{teamId}/_apis/work/boards/{boardId}?api-version=7.1");
    }

    public Task<BoardColumns?> GetBoardColumnsAsync(Guid projectId, Guid teamId, Guid boardId)
    {
        return _client.GetFromJsonAsync<BoardColumns>($"{projectId}/{teamId}/_apis/work/boards/{boardId}/columns?api-version=7.1");
    }

    public Task<HttpResponseMessage> UpdateBoardColumnsAsync(Guid projectId, Guid teamId, Guid boardId, BoardColumns boardColumns)
    {
        return _client.PutAsJsonAsync($"{projectId}/{teamId}/_apis/work/boards/{boardId}/columns?api-version=7.1", boardColumns.Value, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    public Task<CloneResult> MoveBoardColumnsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards)
    {
        return ForAllMatchingBoards(templateProjectId, templateTeamId, projectBoards,
            async (templateBoard, matchingProjectBoardId, result) =>
            {
                var templateBoardId = templateBoard.Id;
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
                        templateBoardColumn.Id = null;
                    }
                }

                var response = await UpdateBoardColumnsAsync(projectId, projectTeamId, matchingProjectBoardId, templateBoardColumns);
                await result.CheckResponseAsync(response, $"Board '{templateBoard.Name}': updating columns");
            });
    }

    public Task<BoardRows?> GetBoardRowsAsync(Guid projectId, Guid teamId, Guid boardId)
    {
        return _client.GetFromJsonAsync<BoardRows>($"{projectId}/{teamId}/_apis/work/boards/{boardId}/rows?api-version=7.1");
    }

    public Task<HttpResponseMessage> UpdateBoardRowsAsync(Guid projectId, Guid teamId, Guid boardId, BoardRows boardRows)
    {
        return _client.PutAsJsonAsync($"{projectId}/{teamId}/_apis/work/boards/{boardId}/rows?api-version=7.1", boardRows.Value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    public Task<CloneResult> MoveBoardRowsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards)
    {
        return ForAllMatchingBoards(templateProjectId, templateTeamId, projectBoards,
            async (templateBoard, matchingProjectBoardId, result) =>
            {
                BoardRows templateBoardRows = await GetBoardRowsAsync(templateProjectId, templateTeamId, templateBoard.Id) ?? new();
                var response = await UpdateBoardRowsAsync(projectId, projectTeamId, matchingProjectBoardId, templateBoardRows);
                await result.CheckResponseAsync(response, $"Board '{templateBoard.Name}': updating rows");
            });
    }

    public Task<CardSettings?> GetCardSettingsAsync(Guid projectId, Guid teamId, Guid boardId)
    { 
        return _client.GetFromJsonAsync<CardSettings>($"{projectId}/{teamId}/_apis/work/boards/{boardId}/cardsettings?api-version=7.1");
    }

    public Task<HttpResponseMessage> UpdateCardSettingsAsync(Guid projectId, Guid teamId, Guid boardId, CardSettings boardCards)
    {
        return _client.PutAsJsonAsync($"{projectId}/{teamId}/_apis/work/boards/{boardId}/cardsettings?api-version=7.1", boardCards, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    public Task<CloneResult> MoveCardSettingsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards)
    {
        return ForAllMatchingBoards(templateProjectId, templateTeamId, projectBoards,
            async (templateBoard, matchingProjectBoardId, result) =>
            {
                CardSettings templateBoardCards = await GetCardSettingsAsync(templateProjectId, templateTeamId, templateBoard.Id) ?? new();
                var response = await UpdateCardSettingsAsync(projectId, projectTeamId, matchingProjectBoardId, templateBoardCards);
                await result.CheckResponseAsync(response, $"Board '{templateBoard.Name}': updating card settings");
            });
    }

    public Task<CardStyles?> GetCardStylesAsync(Guid projectId, Guid teamId, Guid boardId)
    {
        return _client.GetFromJsonAsync<CardStyles>($"{projectId}/{teamId}/_apis/work/boards/{boardId}/cardrulesettings?api-version=7.1");
    }

    public Task<HttpResponseMessage> UpdateCardStylesAsync(Guid projectId, Guid teamId, Guid boardId, CardStyles cardStyle)
    {
        return _client.PatchAsJsonAsync($"{projectId}/{teamId}/_apis/work/boards/{boardId}/cardrulesettings?api-version=7.1", cardStyle, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    public Task<CloneResult> MoveCardStylesAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards)
    {
        return ForAllMatchingBoards(templateProjectId, templateTeamId, projectBoards,
            async (templateBoard, matchingProjectBoardId, result) =>
            {
                CardStyles templateCardStyles = await GetCardStylesAsync(templateProjectId, templateTeamId, templateBoard.Id) ?? new();
                var response = await UpdateCardStylesAsync(projectId, projectTeamId, matchingProjectBoardId, templateCardStyles);
                await result.CheckResponseAsync(response, $"Board '{templateBoard.Name}': updating card styles");
            });
    }

    public async Task<CloneResult> ForAllMatchingBoards(Guid templateProjectId, Guid templateTeamId, Boards projectBoards, Func<BoardValue, Guid, CloneResult, Task> moveAction)
    {
        var result = new CloneResult();
        Boards? templateBoards = await GetBoardsAsync(templateProjectId, templateTeamId);
        if (templateBoards is null)
        {
            result.AddError("Could not read the template team's boards.");
            return result;
        }

        foreach (BoardValue templateBoard in templateBoards.Value)
        {
            var matchingProjectBoard = projectBoards.Value.SingleOrDefault(b => string.Compare(b.Name, templateBoard.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (matchingProjectBoard != null)
            {
                await moveAction(templateBoard, matchingProjectBoard.Id, result);
            }
        }

        return result;
    }
}
