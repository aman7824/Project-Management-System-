using DatabaseProject.Models;

namespace DatabaseProject.Repositories.Interfaces;

public interface ICommentRepository
{
    Task<IEnumerable<Comment>> GetAllAsync();
    Task<Comment?> GetByIdAsync(int id);
    Task<IEnumerable<Comment>> GetByAssignmentIdAsync(int assignmentId);
    Task<IEnumerable<Comment>> GetByEmployeeIdAsync(int employeeId);
    Task<int> CreateAsync(Comment comment);
    Task<bool> UpdateAsync(Comment comment);
    Task<bool> DeleteAsync(int id);
}
