using DatabaseProject.Data;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseProject.Repositories.Implementations;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly DatabaseHelper _dbHelper;

    public InvoiceRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        var invoices = new List<Invoice>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT i.InvoiceID, i.WorkerID, i.EmployeeWage, i.CreateDate, i.HoursWorked,
                      CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName
                      FROM Invoice i
                      LEFT JOIN HourlyEmployee he ON i.WorkerID = he.WorkerID
                      LEFT JOIN Employee e ON he.WorkerID = e.WorkerID
                      ORDER BY i.CreateDate DESC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            invoices.Add(new Invoice
            {
                InvoiceID = reader.GetInt32(0),
                WorkerID = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                EmployeeWage = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                CreateDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                HoursWorked = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                EmployeeName = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return invoices;
    }

    public async Task<Invoice?> GetByIdAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT i.InvoiceID, i.WorkerID, i.EmployeeWage, i.CreateDate, i.HoursWorked,
                      CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName
                      FROM Invoice i
                      LEFT JOIN HourlyEmployee he ON i.WorkerID = he.WorkerID
                      LEFT JOIN Employee e ON he.WorkerID = e.WorkerID
                      WHERE i.InvoiceID = @InvoiceID";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@InvoiceID", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Invoice
            {
                InvoiceID = reader.GetInt32(0),
                WorkerID = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                EmployeeWage = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                CreateDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                HoursWorked = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                EmployeeName = reader.IsDBNull(5) ? null : reader.GetString(5)
                
            };
        }

        return null;
    }

    public async Task<IEnumerable<Invoice>> GetByEmployeeIdAsync(int employeeId)
    {
        var invoices = new List<Invoice>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT i.InvoiceID, i.WorkerID, i.EmployeeWage, i.CreateDate, i.HoursWorked,
                      CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName
                      FROM Invoice i
                      LEFT JOIN HourlyEmployee he ON i.WorkerID = he.WorkerID
                      LEFT JOIN Employee e ON he.WorkerID = e.WorkerID
                      WHERE i.WorkerID = @WorkerID
                      ORDER BY i.CreateDate DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@WorkerID", employeeId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            invoices.Add(new Invoice
            {
                InvoiceID = reader.GetInt32(0),
                WorkerID = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                EmployeeWage = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                CreateDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                HoursWorked = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                EmployeeName = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return invoices;
    }

    public async Task<int> GenerateInvoiceAsync(int hourlyWorkerId, decimal hoursWorked)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Use stored procedure sp_Accounting_GenerateInvoice
        using var command = new SqlCommand("sp_Accounting_GenerateInvoice", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@HourlyWorkerID", hourlyWorkerId);
        command.Parameters.AddWithValue("@HoursWorked", hoursWorked);

        try
        {
            await command.ExecuteNonQueryAsync();
            
            // Get the newly created invoice ID
            var selectCommand = new SqlCommand("SELECT TOP 1 InvoiceID FROM Invoice WHERE WorkerID = @WorkerID ORDER BY InvoiceID DESC", connection);
            selectCommand.Parameters.AddWithValue("@WorkerID", hourlyWorkerId);

            var result = await selectCommand.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }
}

