using DatabaseProject.Data;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace DatabaseProject.Repositories.Implementations;

public class TaskAssignmentRepository : ITaskAssignmentRepository
{
    private readonly DatabaseHelper _dbHelper;

    public TaskAssignmentRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<TaskAssignment>> GetAllAsync()
    {
        var tasks = new List<TaskAssignment>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT ta.AssignmentID, ta.TeamID, ta.ProjectID, ta.TaskDescription, 
                      ta.AssignmentDate, ta.Deadline,
                      t.TeamName, p.ProjectName
                      FROM TaskAssignment ta
                      INNER JOIN Team t ON ta.TeamID = t.TeamID
                      INNER JOIN Project p ON ta.ProjectID = p.ProjectID
                      ORDER BY ta.AssignmentID ASC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tasks.Add(MapToTaskAssignment(reader));
        }

        return tasks;
    }

    public async Task<TaskAssignment?> GetByIdAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT ta.AssignmentID, ta.TeamID, ta.ProjectID, ta.TaskDescription, 
                      ta.AssignmentDate, ta.Deadline,
                      t.TeamName, p.ProjectName
                      FROM TaskAssignment ta
                      INNER JOIN Team t ON ta.TeamID = t.TeamID
                      INNER JOIN Project p ON ta.ProjectID = p.ProjectID
                      WHERE ta.AssignmentID = @AssignmentID";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AssignmentID", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToTaskAssignment(reader);
        }

        return null;
    }

    public async Task<IEnumerable<TaskAssignment>> GetByTeamIdAsync(int teamId)
    {
        var tasks = new List<TaskAssignment>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT ta.AssignmentID, ta.TeamID, ta.ProjectID, ta.TaskDescription, 
                      ta.AssignmentDate, ta.Deadline,
                      t.TeamName, p.ProjectName
                      FROM TaskAssignment ta
                      INNER JOIN Team t ON ta.TeamID = t.TeamID
                      INNER JOIN Project p ON ta.ProjectID = p.ProjectID
                      WHERE ta.TeamID = @TeamID
                      ORDER BY ta.AssignmentDate DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TeamID", teamId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(MapToTaskAssignment(reader));
        }

        return tasks;
    }

    public async Task<IEnumerable<TaskAssignment>> GetByProjectIdAsync(int projectId)
    {
        var tasks = new List<TaskAssignment>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT ta.AssignmentID, ta.TeamID, ta.ProjectID, ta.TaskDescription, 
                      ta.AssignmentDate, ta.Deadline,
                      t.TeamName, p.ProjectName
                      FROM TaskAssignment ta
                      INNER JOIN Team t ON ta.TeamID = t.TeamID
                      INNER JOIN Project p ON ta.ProjectID = p.ProjectID
                      WHERE ta.ProjectID = @ProjectID
                      ORDER BY ta.AssignmentDate DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProjectID", projectId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(MapToTaskAssignment(reader));
        }

        return tasks;
    }

    public async Task<int> CreateAsync(TaskAssignment taskAssignment)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"INSERT INTO TaskAssignment (TeamID, ProjectID, TaskDescription, AssignmentDate, Deadline) 
                      VALUES (@TeamID, @ProjectID, @TaskDescription, GETDATE(), @Deadline);
                      SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TeamID", taskAssignment.TeamID);
        command.Parameters.AddWithValue("@ProjectID", taskAssignment.ProjectID);
        command.Parameters.AddWithValue("@TaskDescription", taskAssignment.TaskDescription ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Deadline", taskAssignment.Deadline ?? (object)DBNull.Value);

        var id = await command.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    public async Task<bool> UpdateAsync(TaskAssignment taskAssignment)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"UPDATE TaskAssignment 
                      SET TeamID = @TeamID, ProjectID = @ProjectID, 
                          TaskDescription = @TaskDescription, Deadline = @Deadline
                      WHERE AssignmentID = @AssignmentID";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AssignmentID", taskAssignment.AssignmentID);
        command.Parameters.AddWithValue("@TeamID", taskAssignment.TeamID);
        command.Parameters.AddWithValue("@ProjectID", taskAssignment.ProjectID);
        command.Parameters.AddWithValue("@TaskDescription", taskAssignment.TaskDescription ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Deadline", taskAssignment.Deadline ?? (object)DBNull.Value);

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
            // Delete in correct order to handle foreign key constraints
            
            // 1. First, delete all Comments related to this TaskAssignment
            var deleteCommentsQuery = "DELETE FROM Comment WHERE AssignmentID = @AssignmentID";
            using (var deleteCommentsCommand = new SqlCommand(deleteCommentsQuery, connection, transaction))
            {
                deleteCommentsCommand.Parameters.AddWithValue("@AssignmentID", id);
                await deleteCommentsCommand.ExecuteNonQueryAsync();
            }
            
            // 2. Finally, delete the TaskAssignment itself
            var deleteTaskQuery = "DELETE FROM TaskAssignment WHERE AssignmentID = @AssignmentID";
            using (var deleteTaskCommand = new SqlCommand(deleteTaskQuery, connection, transaction))
            {
                deleteTaskCommand.Parameters.AddWithValue("@AssignmentID", id);
                var rowsAffected = await deleteTaskCommand.ExecuteNonQueryAsync();
                
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

    private static TaskAssignment MapToTaskAssignment(SqlDataReader reader)
    {
        return new TaskAssignment
        {
            AssignmentID = reader.GetInt32(0),
            TeamID = reader.GetInt32(1),
            ProjectID = reader.GetInt32(2),
            TaskDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
            AssignmentDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            Deadline = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
            TeamName = reader.IsDBNull(6) ? null : reader.GetString(6),
            ProjectName = reader.IsDBNull(7) ? null : reader.GetString(7)
        };
    }
}
