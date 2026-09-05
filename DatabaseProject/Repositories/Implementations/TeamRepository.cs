using DatabaseProject.Data;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseProject.Repositories.Implementations;

public class TeamRepository : ITeamRepository
{
    private readonly DatabaseHelper _dbHelper;

    public TeamRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<Team>> GetAllAsync()
    {
        var teams = new List<Team>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT t.TeamID, t.TeamName, t.ManagerID, 
                      CONCAT(e.FirstName, ' ', e.LastName) as ManagerName,
                      t.CreateDate, t.UpdateDate 
                      FROM Team t
                      LEFT JOIN Employee e ON t.ManagerID = e.WorkerID
                      ORDER BY t.TeamID ASC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            teams.Add(new Team
            {
                TeamID = reader.GetInt32(0),
                TeamName = reader.GetString(1),
                ManagerID = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                ManagerName = reader.IsDBNull(3) ? null : reader.GetString(3),
                CreateDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                UpdateDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            });
        }

        return teams;
    }

    public async Task<Team?> GetByIdAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT t.TeamID, t.TeamName, t.ManagerID, 
                      CONCAT(e.FirstName, ' ', e.LastName) as ManagerName,
                      t.CreateDate, t.UpdateDate 
                      FROM Team t
                      LEFT JOIN Employee e ON t.ManagerID = e.WorkerID
                      WHERE t.TeamID = @TeamID";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TeamID", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Team
            {
                TeamID = reader.GetInt32(0),
                TeamName = reader.GetString(1),
                ManagerID = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                ManagerName = reader.IsDBNull(3) ? null : reader.GetString(3),
                CreateDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                UpdateDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            };
        }

        return null;
    }

    public async Task<int> CreateAsync(Team team)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Use stored procedure sp_Team_CreateNew
        using var command = new SqlCommand("sp_Team_CreateNew", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@TeamName", team.TeamName);
        command.Parameters.AddWithValue("@ManagerID", team.ManagerID ?? (object)DBNull.Value);

        try
        {
            await command.ExecuteNonQueryAsync();
            
            // Get the newly created team ID
            var selectCommand = new SqlCommand("SELECT TOP 1 TeamID FROM Team ORDER BY TeamID DESC", connection);
            var result = await selectCommand.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (SqlException ex)
        {
            // Handle the error from stored procedure
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public async Task<int?> UpdateAsync(Team team)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"UPDATE Team 
                      SET TeamName = @TeamName, ManagerID = @ManagerID
                      WHERE TeamID = @TeamID";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TeamID", team.TeamID);
        command.Parameters.AddWithValue("@TeamName", team.TeamName);
        command.Parameters.AddWithValue("@ManagerID", team.ManagerID ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
        var selectCommand = new SqlCommand("SELECT TOP 1 ManagerID FROM Team WHERE TeamID = @TeamID", connection);
        selectCommand.Parameters.AddWithValue("@TeamID", team.TeamID);
        var result = await selectCommand.ExecuteScalarAsync();


        if (result is DBNull)
            return null;

        return Convert.ToInt32(result);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            // Delete in correct order to handle foreign key constraints
            
            // 1. Delete Comments related to TaskAssignments for this team
            var deleteCommentsQuery = @"DELETE FROM Comment 
                                       WHERE AssignmentID IN 
                                       (SELECT AssignmentID FROM TaskAssignment WHERE TeamID = @TeamID)";
            using (var deleteCommentsCommand = new SqlCommand(deleteCommentsQuery, connection, transaction))
            {
                deleteCommentsCommand.Parameters.AddWithValue("@TeamID", id);
                await deleteCommentsCommand.ExecuteNonQueryAsync();
            }
            
            // 2. Delete TaskAssignments for this team
            var deleteTasksQuery = "DELETE FROM TaskAssignment WHERE TeamID = @TeamID";
            using (var deleteTasksCommand = new SqlCommand(deleteTasksQuery, connection, transaction))
            {
                deleteTasksCommand.Parameters.AddWithValue("@TeamID", id);
                await deleteTasksCommand.ExecuteNonQueryAsync();
            }
            
            // 3. Delete TeamMembers for this team
            var deleteTeamMembersQuery = "DELETE FROM TeamMembers WHERE TeamID = @TeamID";
            using (var deleteTeamMembersCommand = new SqlCommand(deleteTeamMembersQuery, connection, transaction))
            {
                deleteTeamMembersCommand.Parameters.AddWithValue("@TeamID", id);
                await deleteTeamMembersCommand.ExecuteNonQueryAsync();
            }
            
            // 4. Finally, delete the Team itself
            var deleteTeamQuery = "DELETE FROM Team WHERE TeamID = @TeamID";
            using (var deleteTeamCommand = new SqlCommand(deleteTeamQuery, connection, transaction))
            {
                deleteTeamCommand.Parameters.AddWithValue("@TeamID", id);
                var rowsAffected = await deleteTeamCommand.ExecuteNonQueryAsync();
                
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
}
