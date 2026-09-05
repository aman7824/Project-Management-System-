using DatabaseProject.Models;

namespace DatabaseProject.Repositories.Interfaces;

public interface ILeaveRepository
{
    Task<IEnumerable<Leave>> GetAllAsync();
    Task<Leave?> GetByIdAsync(int id);
    Task<IEnumerable<Leave>> GetByEmployeeIdAsync(int employeeId);
    Task<int> RequestLeaveAsync(Leave leave); // Uses sp_HR_RequestLeave
    Task<bool> DeleteAsync(int id);
}
