using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Managers;

public interface ICloneManager
{
    Task CloneAreasAsync(Guid templateProjectId, Guid projectId);
    Task CloneBoardsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId);
    Task CloneIterationsAsync(Guid templateProjectId, Guid projectId);
    Task<Tuple<Project, Project, string?>> CloneProjectAsync(Guid templateProjectId, string newProjectName, string description, Visibility visibility);
    Task CloneRepositoriesAsync(Guid templateProjectId, Guid projectId);
    Task CloneTeamFieldValuesAsync(Project templateProject, Project project, Guid templateTeamId, Guid projectTeamId);
    Task CloneTeamFieldValuesAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId);
    Task<Dictionary<Guid, Guid>> CloneTeamsAsync(Project templateProject, Project project);
    Task<Dictionary<Guid, Guid>> CloneTeamsAsync(Guid templateProjectId, Guid projectId);
    Task CloneTeamsAndSettingsAndBoardsAsync(Project templateProject, Project project);
    Task CloneTeamSettingsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, Guid? backlogIterationId);
}
