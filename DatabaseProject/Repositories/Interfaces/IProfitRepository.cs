using DatabaseProject.Models;

namespace DatabaseProject.Repositories.Interfaces;

public interface IProfitRepository
{
    Task<IEnumerable<Profit>> GetAllAsync();
    Task<Profit?> GetByIdAsync(int id);
    Task<Profit?> GetByProjectIdAsync(int projectId);
    Task<int> CreateAsync(Profit profit);
    Task<bool> UpdateAsync(Profit profit);
    Task<bool> DeleteAsync(int id);
}
