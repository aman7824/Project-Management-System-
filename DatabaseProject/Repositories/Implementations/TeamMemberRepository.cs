using DatabaseProject.Data;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseProject.Repositories.Implementations;

public class TeamMemberRepository : ITeamMemberRepository
{
    private readonly DatabaseHelper _dbHelper;

    public TeamMemberRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<bool> AddMemberToTeamAsync(int teamId, int workerId)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Use stored procedure sp_Team_AddMember
        using var command = new SqlCommand("sp_Team_AddMember", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@TeamID", teamId);
        command.Parameters.AddWithValue("@WorkerID", workerId);

        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (SqlException)
        {
            // Member already exists or other error
            return false;
        }
    }

    public async Task<IEnumerable<Employee>> GetTeamMembersAsync(int teamId)
    {
        var members = new List<Employee>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT e.WorkerID, e.FirstName, e.LastName, e.Username, e.Email, 
                      e.PhoneNumber, e.Address, e.EmployeeType, e.CreateDate, e.UpdateDate
                      FROM TeamMembers tm
                      INNER JOIN Employee e ON tm.WorkerID = e.WorkerID
                      WHERE tm.TeamID = @TeamID
                      ORDER BY e.FirstName, e.LastName";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TeamID", teamId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            members.Add(new Employee
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
            });
        }

        return members;
    }

    public async Task<bool> RemoveMemberFromTeamAsync(int teamId, int workerId)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = "DELETE FROM TeamMembers WHERE TeamID = @TeamID AND WorkerID = @WorkerID";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TeamID", teamId);
        command.Parameters.AddWithValue("@WorkerID", workerId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
