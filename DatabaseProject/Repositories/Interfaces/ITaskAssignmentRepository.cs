using DatabaseProject.Models;

namespace DatabaseProject.Repositories.Interfaces;

public interface ITaskAssignmentRepository
{
    Task<IEnumerable<TaskAssignment>> GetAllAsync();
    Task<TaskAssignment?> GetByIdAsync(int id);
    Task<IEnumerable<TaskAssignment>> GetByTeamIdAsync(int teamId);
    Task<IEnumerable<TaskAssignment>> GetByProjectIdAsync(int projectId);
    Task<int> CreateAsync(TaskAssignment taskAssignment);
    Task<bool> UpdateAsync(TaskAssignment taskAssignment);
    Task<bool> DeleteAsync(int id);
}
