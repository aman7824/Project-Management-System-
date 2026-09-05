using DatabaseProject.Models;

namespace DatabaseProject.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(int id);
    Task<IEnumerable<Invoice>> GetByEmployeeIdAsync(int employeeId);
    Task<int> GenerateInvoiceAsync(int hourlyWorkerId, decimal hoursWorked); // Uses sp_Accounting_GenerateInvoice
}
