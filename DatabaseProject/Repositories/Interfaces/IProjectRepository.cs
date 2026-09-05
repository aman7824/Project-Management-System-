using DatabaseProject.Models;

namespace DatabaseProject.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(int id);
    Task<int> CreateAsync(Project project);
    Task<bool> UpdateAsync(Project project);
    Task<int> DeleteAsync(int id);
    Task<int> GetProjectCountAsync();
    Task<int> GetActiveProjectCountAsync();
    Task<bool> CompleteProjectAsync(int projectId); // Uses sp_Project_Complete
    Task<bool> AddExpensesAsync(int projectId, decimal expenseAmount); // Uses sp_Project_AddExpenses
    Task<bool> AddEarningsAsync(int projectId, decimal earningsAmount); // Add earnings to project
}
