using DatabaseProject.Data;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace DatabaseProject.Repositories.Implementations;

public class CommentRepository : ICommentRepository
{
    private readonly DatabaseHelper _dbHelper;

    public CommentRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<Comment>> GetAllAsync()
    {
        var comments = new List<Comment>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT c.CommentNumber, c.WorkerID, c.AssignmentID, c.CommentTitle, 
                      c.CommentText, c.CreateDate, c.UpdateDate,
                      CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName,
                      t.TaskDescription as AssignmentDescription
                      FROM Comment c
                      INNER JOIN Employee e ON c.WorkerID = e.WorkerID
                      LEFT JOIN TaskAssignment t ON c.AssignmentID = t.AssignmentID
                      ORDER BY c.CreateDate DESC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            comments.Add(MapToComment(reader));
        }

        return comments;
    }

    public async Task<Comment?> GetByIdAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT c.CommentNumber, c.WorkerID, c.AssignmentID, c.CommentTitle, 
                      c.CommentText, c.CreateDate, c.UpdateDate,
                      CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName,
                      t.TaskDescription as AssignmentDescription
                      FROM Comment c
                      INNER JOIN Employee e ON c.WorkerID = e.WorkerID
                      LEFT JOIN TaskAssignment t ON c.AssignmentID = t.AssignmentID
                      WHERE c.CommentNumber = @CommentNumber";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CommentNumber", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToComment(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Comment>> GetByAssignmentIdAsync(int assignmentId)
    {
        var comments = new List<Comment>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT c.CommentNumber, c.WorkerID, c.AssignmentID, c.CommentTitle, 
                      c.CommentText, c.CreateDate, c.UpdateDate,
                      CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName,
                      t.TaskDescription as AssignmentDescription
                      FROM Comment c
                      INNER JOIN Employee e ON c.WorkerID = e.WorkerID
                      LEFT JOIN TaskAssignment t ON c.AssignmentID = t.AssignmentID
                      WHERE c.AssignmentID = @AssignmentID
                      ORDER BY c.CreateDate DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AssignmentID", assignmentId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            comments.Add(MapToComment(reader));
        }

        return comments;
    }

    public async Task<IEnumerable<Comment>> GetByEmployeeIdAsync(int employeeId)
    {
        var comments = new List<Comment>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT c.CommentNumber, c.WorkerID, c.AssignmentID, c.CommentTitle, 
                      c.CommentText, c.CreateDate, c.UpdateDate,
                      CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName,
                      t.TaskDescription as AssignmentDescription
                      FROM Comment c
                      INNER JOIN Employee e ON c.WorkerID = e.WorkerID
                      LEFT JOIN TaskAssignment t ON c.AssignmentID = t.AssignmentID
                      WHERE c.WorkerID = @WorkerID
                      ORDER BY c.CreateDate DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@WorkerID", employeeId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            comments.Add(MapToComment(reader));
        }

        return comments;
    }

    public async Task<int> CreateAsync(Comment comment)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"INSERT INTO Comment (WorkerID, AssignmentID, CommentTitle, CommentText, CreateDate) 
                      VALUES (@WorkerID, @AssignmentID, @CommentTitle, @CommentText, GETDATE());
                      SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@WorkerID", comment.WorkerID);
        command.Parameters.AddWithValue("@AssignmentID", comment.AssignmentID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CommentTitle", comment.CommentTitle ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CommentText", comment.CommentText ?? (object)DBNull.Value);

        var id = await command.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    public async Task<bool> UpdateAsync(Comment comment)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"UPDATE Comment 
                      SET WorkerID = @WorkerID, AssignmentID = @AssignmentID, 
                          CommentTitle = @CommentTitle, CommentText = @CommentText, 
                          UpdateDate = GETDATE()
                      WHERE CommentNumber = @CommentNumber";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CommentNumber", comment.CommentNumber);
        command.Parameters.AddWithValue("@WorkerID", comment.WorkerID);
        command.Parameters.AddWithValue("@AssignmentID", comment.AssignmentID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CommentTitle", comment.CommentTitle ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CommentText", comment.CommentText ?? (object)DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = "DELETE FROM Comment WHERE CommentNumber = @CommentNumber";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CommentNumber", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static Comment MapToComment(SqlDataReader reader)
    {
        return new Comment
        {
            CommentNumber = reader.GetInt32(0),
            WorkerID = reader.GetInt32(1),
            AssignmentID = reader.IsDBNull(2) ? null : reader.GetInt32(2),
            CommentTitle = reader.IsDBNull(3) ? null : reader.GetString(3),
            CommentText = reader.IsDBNull(4) ? null : reader.GetString(4),
            CreateDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
            UpdateDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            EmployeeName = reader.IsDBNull(7) ? null : reader.GetString(7),
            AssignmentDescription = reader.IsDBNull(8) ? null : reader.GetString(8)
        };
    }
}
