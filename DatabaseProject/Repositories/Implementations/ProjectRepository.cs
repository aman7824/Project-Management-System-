using DatabaseProject.Data;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseProject.Repositories.Implementations;

public class ProjectRepository : IProjectRepository
{
    private readonly DatabaseHelper _dbHelper;

    public ProjectRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        var projects = new List<Project>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT ProjectID, ProjectName, ProjectStatus, Deadline, FinishDate, 
                      Budget, ProjectDescription, CreateDate, UpdateDate 
                      FROM Project ORDER BY ProjectID ASC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            projects.Add(MapToProject(reader));
        }

        return projects;
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT ProjectID, ProjectName, ProjectStatus, Deadline, FinishDate, 
                      Budget, ProjectDescription, CreateDate, UpdateDate 
                      FROM Project WHERE ProjectID = @ProjectID";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProjectID", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToProject(reader);
        }

        return null;
    }

    public async Task<int> CreateAsync(Project project)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"INSERT INTO Project (ProjectName, ProjectStatus, Deadline, Budget, ProjectDescription, CreateDate) 
                      VALUES (@ProjectName, @ProjectStatus, @Deadline, @Budget, @ProjectDescription, GETDATE());
                      SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProjectName", project.ProjectName);
        command.Parameters.AddWithValue("@ProjectStatus", project.ProjectStatus ?? "Active");
        command.Parameters.AddWithValue("@Deadline", project.Deadline ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Budget", project.Budget ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ProjectDescription", project.ProjectDescription ?? (object)DBNull.Value);

        var id = await command.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    public async Task<bool> UpdateAsync(Project project)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Note: Trigger will automatically update UpdateDate
        var query = @"UPDATE Project 
                      SET ProjectName = @ProjectName, ProjectStatus = @ProjectStatus, 
                          Deadline = @Deadline, Budget = @Budget, ProjectDescription = @ProjectDescription,
                          FinishDate = @FinishDate
                      WHERE ProjectID = @ProjectID";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProjectID", project.ProjectID);
        command.Parameters.AddWithValue("@ProjectName", project.ProjectName);
        command.Parameters.AddWithValue("@ProjectStatus", project.ProjectStatus ?? "Active");
        command.Parameters.AddWithValue("@Deadline", project.Deadline ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Budget", project.Budget ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ProjectDescription", project.ProjectDescription ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@FinishDate", project.FinishDate ?? (object)DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<int> DeleteAsync(int id)
    {
        int status = 0;
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            // Delete in correct order to handle foreign key constraints

            // 1. Delete Comments related to TaskAssignments for this project
            status = 1;
            var deleteCommentsQuery = @"DELETE FROM Comment 
                                       WHERE AssignmentID IN 
                                       (SELECT AssignmentID FROM TaskAssignment WHERE ProjectID = @ProjectID)";
            using (var deleteCommentsCommand = new SqlCommand(deleteCommentsQuery, connection, transaction))
            {
                deleteCommentsCommand.Parameters.AddWithValue("@ProjectID", id);
                await deleteCommentsCommand.ExecuteNonQueryAsync();
            }

            // 2. Delete TaskAssignments for this project
            status = 2;
            var deleteTasksQuery = "DELETE FROM TaskAssignment WHERE ProjectID = @ProjectID";
            using (var deleteTasksCommand = new SqlCommand(deleteTasksQuery, connection, transaction))
            {
                deleteTasksCommand.Parameters.AddWithValue("@ProjectID", id);
                await deleteTasksCommand.ExecuteNonQueryAsync();
            }

            // 3. Delete Draft records for this project
            var deleteDraftsQuery = "DELETE FROM Draft WHERE ProjectID = @ProjectID";
            using (var deleteDraftsCommand = new SqlCommand(deleteDraftsQuery, connection, transaction))
            {
                deleteDraftsCommand.Parameters.AddWithValue("@ProjectID", id);
                await deleteDraftsCommand.ExecuteNonQueryAsync();
            }

            // 4. Delete Profit records for this project
            var deleteProfitQuery = "DELETE FROM Profit WHERE ProjectID = @ProjectID";
            using (var deleteProfitCommand = new SqlCommand(deleteProfitQuery, connection, transaction))
            {
                deleteProfitCommand.Parameters.AddWithValue("@ProjectID", id);
                await deleteProfitCommand.ExecuteNonQueryAsync();
            }

            // 5. Finally, delete the Project itself
            // Note: Trigger will prevent deletion if project status is 'Completed'
            status = 3;
            var deleteProjectQuery = "DELETE FROM Project WHERE ProjectID = @ProjectID";
            using (var deleteProjectCommand = new SqlCommand(deleteProjectQuery, connection, transaction))
            {
                deleteProjectCommand.Parameters.AddWithValue("@ProjectID", id);
                var rowsAffected = await deleteProjectCommand.ExecuteNonQueryAsync();
                
                transaction.Commit();
                
                return 0;
            }

        }
        catch
        {
            transaction.Rollback();
            return status;
        }
    }

    public async Task<int> GetProjectCountAsync()
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = "SELECT COUNT(*) FROM Project";
        using var command = new SqlCommand(query, connection);
        
        var count = await command.ExecuteScalarAsync();
        return Convert.ToInt32(count);
    }

    public async Task<int> GetActiveProjectCountAsync()
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = "SELECT COUNT(*) FROM Project WHERE ProjectStatus = 'Active'";
        using var command = new SqlCommand(query, connection);
        
        var count = await command.ExecuteScalarAsync();
        return Convert.ToInt32(count);
    }

    public async Task<bool> CompleteProjectAsync(int projectId)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Use stored procedure sp_Project_Complete
        using var command = new SqlCommand("sp_Project_Complete", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@ProjectID", projectId);

        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    public async Task<bool> AddExpensesAsync(int projectId, decimal expenseAmount)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Use stored procedure sp_Project_AddExpenses
        // This SP will create Profit record if it doesn't exist
        // The trg_CheckBudgetOverflow trigger will prevent budget overflow
        using var command = new SqlCommand("sp_Project_AddExpenses", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@ProjectID", projectId);
        command.Parameters.AddWithValue("@ExpenseAmount", expenseAmount);

        // Let the exception bubble up so the UI can show the budget overflow error
        await command.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<bool> AddEarningsAsync(int projectId, decimal earningsAmount)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        // Check if Profit record exists
        var checkQuery = "SELECT COUNT(*) FROM Profit WHERE ProjectID = @ProjectID";
        using var checkCommand = new SqlCommand(checkQuery, connection);
        checkCommand.Parameters.AddWithValue("@ProjectID", projectId);
        var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

        if (!exists)
        {
            // Create new Profit record
            var insertQuery = @"INSERT INTO Profit (ProjectID, TotalEarnings, TotalExpenses, CreateDate) 
                               VALUES (@ProjectID, @Earnings, 0, GETDATE())";
            using var insertCommand = new SqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@ProjectID", projectId);
            insertCommand.Parameters.AddWithValue("@Earnings", earningsAmount);
            await insertCommand.ExecuteNonQueryAsync();
        }
        else
        {
            // Update existing record
            var updateQuery = @"UPDATE Profit 
                               SET TotalEarnings = TotalEarnings + @Earnings,
                                   UpdateDate = GETDATE()
                               WHERE ProjectID = @ProjectID";
            using var updateCommand = new SqlCommand(updateQuery, connection);
            updateCommand.Parameters.AddWithValue("@ProjectID", projectId);
            updateCommand.Parameters.AddWithValue("@Earnings", earningsAmount);
            await updateCommand.ExecuteNonQueryAsync();
        }

        return true;
    }

    private static Project MapToProject(SqlDataReader reader)
    {
        return new Project
        {
            ProjectID = reader.GetInt32(0),
            ProjectName = reader.GetString(1),
            ProjectStatus = reader.IsDBNull(2) ? null : reader.GetString(2),
            Deadline = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
            FinishDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            Budget = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
            ProjectDescription = reader.IsDBNull(6) ? null : reader.GetString(6),
            CreateDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
            UpdateDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
        };
    }
}
