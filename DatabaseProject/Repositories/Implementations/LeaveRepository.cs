using DatabaseProject.Data;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseProject.Repositories.Implementations;

public class LeaveRepository : ILeaveRepository
{
    private readonly DatabaseHelper _dbHelper;

    public LeaveRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<Leave>> GetAllAsync()
    {
        var leaves = new List<Leave>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT l.LeaveNumber, l.WorkerID, l.LeaveType, l.StartDate, l.EndDate, l.TotalLeaveDay,
                      CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName
                      FROM Leave l
                      INNER JOIN FullTimeEmployee fte ON l.WorkerID = fte.WorkerID
                      INNER JOIN Employee e ON fte.WorkerID = e.WorkerID
                      ORDER BY l.StartDate DESC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            leaves.Add(new Leave
            {
                LeaveNumber = reader.GetInt32(0),
                WorkerID = reader.GetInt32(1),
                LeaveType = reader.GetString(2),
                StartDate = reader.GetDateTime(3),
                EndDate = reader.GetDateTime(4),
                TotalLeaveDay = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                EmployeeName = reader.IsDBNull(6) ? null : reader.GetString(6)
            });
        }

        return leaves;
    }

    public async Task<Leave?> GetByIdAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT l.LeaveNumber, l.WorkerID, l.LeaveType, l.StartDate, l.EndDate, l.TotalLeaveDay,
                      CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName
                      FROM Leave l
                      INNER JOIN FullTimeEmployee fte ON l.WorkerID = fte.WorkerID
                      INNER JOIN Employee e ON fte.WorkerID = e.WorkerID
                      WHERE l.LeaveNumber = @LeaveNumber";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@LeaveNumber", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Leave
            {
                LeaveNumber = reader.GetInt32(0),
                WorkerID = reader.GetInt32(1),
                LeaveType = reader.GetString(2),
                StartDate = reader.GetDateTime(3),
                EndDate = reader.GetDateTime(4),
                TotalLeaveDay = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                EmployeeName = reader.IsDBNull(6) ? null : reader.GetString(6)
            };
        }

        return null;
    }

    public async Task<IEnumerable<Leave>> GetByEmployeeIdAsync(int employeeId)
    {
        var leaves = new List<Leave>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT l.LeaveNumber, l.WorkerID, l.LeaveType, l.StartDate, l.EndDate, l.TotalLeaveDay,
                      CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName
                      FROM Leave l
                      INNER JOIN FullTimeEmployee fte ON l.WorkerID = fte.WorkerID
                      INNER JOIN Employee e ON fte.WorkerID = e.WorkerID
                      WHERE l.WorkerID = @WorkerID
                      ORDER BY l.StartDate DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@WorkerID", employeeId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            leaves.Add(new Leave
            {
                LeaveNumber = reader.GetInt32(0),
                WorkerID = reader.GetInt32(1),
                LeaveType = reader.GetString(2),
                StartDate = reader.GetDateTime(3),
                EndDate = reader.GetDateTime(4),
                TotalLeaveDay = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                EmployeeName = reader.IsDBNull(6) ? null : reader.GetString(6)
            });
        }

        return leaves;
    }

    public async Task<int> RequestLeaveAsync(Leave leave)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Use stored procedure sp_HR_RequestLeave
        using var command = new SqlCommand("sp_HR_RequestLeave", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@WorkerID", leave.WorkerID);
        command.Parameters.AddWithValue("@LeaveType", leave.LeaveType);
        command.Parameters.AddWithValue("@StartDate", leave.StartDate);
        command.Parameters.AddWithValue("@EndDate", leave.EndDate);

        try
        {
            await command.ExecuteNonQueryAsync();
            
            // Get the newly created leave ID
            var selectCommand = new SqlCommand("SELECT TOP 1 LeaveNumber FROM Leave ORDER BY LeaveNumber DESC", connection);
            var result = await selectCommand.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = "DELETE FROM Leave WHERE LeaveNumber = @LeaveNumber";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@LeaveNumber", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
