using DatabaseProject.Models;

namespace DatabaseProject.Repositories.Interfaces;

public interface ITeamRepository
{
    Task<IEnumerable<Team>> GetAllAsync();
    Task<Team?> GetByIdAsync(int id);
    Task<int> CreateAsync(Team team);
    Task<int?> UpdateAsync(Team team);
    Task<bool> DeleteAsync(int id);
}
