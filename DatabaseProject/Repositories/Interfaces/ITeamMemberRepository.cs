using DatabaseProject.Models;

namespace DatabaseProject.Repositories.Interfaces;

public interface ITeamMemberRepository
{
    Task<bool> AddMemberToTeamAsync(int teamId, int workerId);
    Task<IEnumerable<Employee>> GetTeamMembersAsync(int teamId);
    Task<bool> RemoveMemberFromTeamAsync(int teamId, int workerId);
}
