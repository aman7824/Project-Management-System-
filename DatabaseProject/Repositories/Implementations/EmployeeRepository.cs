using DatabaseProject.Data;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseProject.Repositories.Implementations;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly DatabaseHelper _dbHelper;

    public EmployeeRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        var employees = new List<Employee>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Changed from ORDER BY CreateDate DESC to ORDER BY WorkerID ASC
        // This shows employees in ID order (oldest to newest)
        var query = @"SELECT WorkerID, FirstName, LastName, Username, Email, PhoneNumber, 
                      Address, EmployeeType, CreateDate, UpdateDate 
                      FROM Employee ORDER BY WorkerID ASC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            employees.Add(MapToEmployee(reader));
        }

        return employees;
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT WorkerID, FirstName, LastName, Username, Email, PhoneNumber, 
                      Address, EmployeeType, CreateDate, UpdateDate 
                      FROM Employee WHERE WorkerID = @WorkerID";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@WorkerID", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToEmployee(reader);
        }

        return null;
    }

    public async Task<int> CreateAsync(Employee employee)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Use stored procedure for Full-Time employees
        if (employee.EmployeeType == 'F')
        {
            using var command = new SqlCommand("sp_Hiring_AddFullTime", connection);
            command.CommandType = CommandType.StoredProcedure;
            
            command.Parameters.AddWithValue("@FirstName", employee.FirstName);
            command.Parameters.AddWithValue("@LastName", employee.LastName);
            command.Parameters.AddWithValue("@Username", employee.Username);
            command.Parameters.AddWithValue("@Email", employee.Email);
            command.Parameters.AddWithValue("@Phone", employee.PhoneNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Salary", 50000); // Default salary
            command.Parameters.AddWithValue("@EntranceDate", DateTime.Now);

            await command.ExecuteNonQueryAsync();
            
            // Get the newly created ID
            var selectCommand = new SqlCommand("SELECT TOP 1 WorkerID FROM Employee ORDER BY WorkerID DESC", connection);
            var result = await selectCommand.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        // Use stored procedure for Hourly employees
        else if (employee.EmployeeType == 'H')
        {
            using var command = new SqlCommand("sp_Hiring_AddHourly", connection);
            command.CommandType = CommandType.StoredProcedure;
            
            command.Parameters.AddWithValue("@FirstName", employee.FirstName);
            command.Parameters.AddWithValue("@LastName", employee.LastName);
            command.Parameters.AddWithValue("@Username", employee.Username);
            command.Parameters.AddWithValue("@Email", employee.Email);
            command.Parameters.AddWithValue("@HourlyRate", 25.00); // Default hourly rate

            await command.ExecuteNonQueryAsync();
            
            // Get the newly created ID
            var selectCommand = new SqlCommand("SELECT TOP 1 WorkerID FROM Employee ORDER BY WorkerID DESC", connection);
            var result = await selectCommand.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        else
        {
            // Fallback for employees without type
            var query = @"INSERT INTO Employee (FirstName, LastName, Username, Email, PhoneNumber, Address, EmployeeType, CreateDate) 
                          VALUES (@FirstName, @LastName, @Username, @Email, @PhoneNumber, @Address, @EmployeeType, GETDATE());
                          SELECT CAST(SCOPE_IDENTITY() as int)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FirstName", employee.FirstName);
            command.Parameters.AddWithValue("@LastName", employee.LastName);
            command.Parameters.AddWithValue("@Username", employee.Username);
            command.Parameters.AddWithValue("@Email", employee.Email);
            command.Parameters.AddWithValue("@PhoneNumber", employee.PhoneNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EmployeeType", DBNull.Value);

            var id = await command.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }
    }

    public async Task<bool> UpdateAsync(Employee employee)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Note: UPDATE will trigger trg_Employee_AutoUpdateDate automatically
        // EmployeeType is excluded from the UPDATE to prevent any changes
        var query = @"UPDATE Employee 
                      SET FirstName = @FirstName, LastName = @LastName, Username = @Username, 
                          Email = @Email, PhoneNumber = @PhoneNumber, Address = @Address
                      WHERE WorkerID = @WorkerID";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@WorkerID", employee.WorkerID);
        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
        command.Parameters.AddWithValue("@LastName", employee.LastName);
        command.Parameters.AddWithValue("@Username", employee.Username);
        command.Parameters.AddWithValue("@Email", employee.Email);
        command.Parameters.AddWithValue("@PhoneNumber", employee.PhoneNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            // First, delete from child tables based on employee type
            
            // Check if employee is Full-Time and delete from FullTimeEmployee
            var checkFullTimeQuery = "SELECT COUNT(*) FROM FullTimeEmployee WHERE WorkerID = @WorkerID";
            using (var checkCommand = new SqlCommand(checkFullTimeQuery, connection, transaction))
            {
                checkCommand.Parameters.AddWithValue("@WorkerID", id);
                var isFullTime = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                
                if (isFullTime)
                {
                    // Delete related DailyPerformance records first (FK_Perf_FullTimeWorker constraint)
                    var deleteDailyPerfQuery = "DELETE FROM DailyPerformance WHERE WorkerID = @WorkerID";
                    using var deleteDailyPerfCommand = new SqlCommand(deleteDailyPerfQuery, connection, transaction);
                    deleteDailyPerfCommand.Parameters.AddWithValue("@WorkerID", id);
                    await deleteDailyPerfCommand.ExecuteNonQueryAsync();
                    
                    // Delete related Leave records
                    var deleteLeaveQuery = "DELETE FROM Leave WHERE WorkerID = @WorkerID";
                    using var deleteLeaveCommand = new SqlCommand(deleteLeaveQuery, connection, transaction);
                    deleteLeaveCommand.Parameters.AddWithValue("@WorkerID", id);
                    await deleteLeaveCommand.ExecuteNonQueryAsync();
                    
                    // Delete from FullTimeEmployee
                    var deleteFullTimeQuery = "DELETE FROM FullTimeEmployee WHERE WorkerID = @WorkerID";
                    using var deleteFullTimeCommand = new SqlCommand(deleteFullTimeQuery, connection, transaction);
                    deleteFullTimeCommand.Parameters.AddWithValue("@WorkerID", id);
                    await deleteFullTimeCommand.ExecuteNonQueryAsync();
                }
            }
            
            // Check if employee is Hourly and delete from HourlyEmployee
            var checkHourlyQuery = "SELECT COUNT(*) FROM HourlyEmployee WHERE WorkerID = @WorkerID";
            using (var checkCommand = new SqlCommand(checkHourlyQuery, connection, transaction))
            {
                checkCommand.Parameters.AddWithValue("@WorkerID", id);
                var isHourly = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                
                if (isHourly)
                {
                    // Delete related Invoice records first
                    var deleteInvoiceQuery = "DELETE FROM Invoice WHERE WorkerID = @WorkerID";
                    using var deleteInvoiceCommand = new SqlCommand(deleteInvoiceQuery, connection, transaction);
                    deleteInvoiceCommand.Parameters.AddWithValue("@WorkerID", id);
                    await deleteInvoiceCommand.ExecuteNonQueryAsync();
                    
                    // Delete from HourlyEmployee
                    var deleteHourlyQuery = "DELETE FROM HourlyEmployee WHERE WorkerID = @WorkerID";
                    using var deleteHourlyCommand = new SqlCommand(deleteHourlyQuery, connection, transaction);
                    deleteHourlyCommand.Parameters.AddWithValue("@WorkerID", id);
                    await deleteHourlyCommand.ExecuteNonQueryAsync();
                }
            }
            
            // Delete from TeamMembers if employee is in any team
            var deleteTeamMembersQuery = "DELETE FROM TeamMembers WHERE WorkerID = @WorkerID";
            using (var deleteTeamMembersCommand = new SqlCommand(deleteTeamMembersQuery, connection, transaction))
            {
                deleteTeamMembersCommand.Parameters.AddWithValue("@WorkerID", id);
                await deleteTeamMembersCommand.ExecuteNonQueryAsync();
            }
            
            // Delete from Comment if employee has comments
            var deleteCommentsQuery = "DELETE FROM Comment WHERE WorkerID = @WorkerID";
            using (var deleteCommentsCommand = new SqlCommand(deleteCommentsQuery, connection, transaction))
            {
                deleteCommentsCommand.Parameters.AddWithValue("@WorkerID", id);
                await deleteCommentsCommand.ExecuteNonQueryAsync();
            }
            
            // Update Team table - set ManagerID to NULL if this employee is a manager
            var updateTeamQuery = "UPDATE Team SET ManagerID = NULL WHERE ManagerID = @WorkerID";
            using (var updateTeamCommand = new SqlCommand(updateTeamQuery, connection, transaction))
            {
                updateTeamCommand.Parameters.AddWithValue("@WorkerID", id);
                await updateTeamCommand.ExecuteNonQueryAsync();
            }
            
            // Finally, delete from Employee table
            var deleteEmployeeQuery = "DELETE FROM Employee WHERE WorkerID = @WorkerID";
            using (var deleteEmployeeCommand = new SqlCommand(deleteEmployeeQuery, connection, transaction))
            {
                deleteEmployeeCommand.Parameters.AddWithValue("@WorkerID", id);
                var rowsAffected = await deleteEmployeeCommand.ExecuteNonQueryAsync();
                
                transaction.Commit();
                return rowsAffected > 0;
            }
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static Employee MapToEmployee(SqlDataReader reader)
    {
        return new Employee
        {
            WorkerID = reader.GetInt32(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2),
            Username = reader.GetString(3),
            Email = reader.GetString(4),
            PhoneNumber = reader.IsDBNull(5) ? null : reader.GetString(5),
            Address = reader.IsDBNull(6) ? null : reader.GetString(6),
            EmployeeType = reader.IsDBNull(7) ? null : reader.GetString(7)[0],
            CreateDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
            UpdateDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
        };
    }
}
