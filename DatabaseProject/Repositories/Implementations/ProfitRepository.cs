using DatabaseProject.Data;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace DatabaseProject.Repositories.Implementations;

public class ProfitRepository : IProfitRepository
{
    private readonly DatabaseHelper _dbHelper;

    public ProfitRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<Profit>> GetAllAsync()
    {
        var profits = new List<Profit>();

        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT 
                        p.ProfitID, 
                        p.ProjectID, 
                        pr.ProjectName,
                        p.TotalEarnings, 
                        p.TotalExpenses, 
                        p.ProfitValue,
                        pr.Budget,
                        (pr.Budget - ISNULL(p.TotalExpenses, 0)) AS BudgetRemaining,
                        CASE 
                            WHEN pr.Budget IS NULL THEN 'No Budget'
                            WHEN p.TotalExpenses > pr.Budget THEN 'Over Budget'
                            WHEN p.TotalExpenses = pr.Budget THEN 'At Limit'
                            ELSE 'Within Budget'
                        END AS BudgetStatus,
                        p.CreateDate, 
                        p.UpdateDate
                      FROM Profit p
                      INNER JOIN Project pr ON p.ProjectID = pr.ProjectID
                      ORDER BY p.ProfitID ASC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            profits.Add(MapToProfit(reader));
        }

        return profits;
    }

    public async Task<Profit?> GetByIdAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT 
                        p.ProfitID, 
                        p.ProjectID, 
                        pr.ProjectName,
                        p.TotalEarnings, 
                        p.TotalExpenses, 
                        p.ProfitValue,
                        pr.Budget,
                        (pr.Budget - ISNULL(p.TotalExpenses, 0)) AS BudgetRemaining,
                        CASE 
                            WHEN pr.Budget IS NULL THEN 'No Budget'
                            WHEN p.TotalExpenses > pr.Budget THEN 'Over Budget'
                            WHEN p.TotalExpenses = pr.Budget THEN 'At Limit'
                            ELSE 'Within Budget'
                        END AS BudgetStatus,
                        p.CreateDate, 
                        p.UpdateDate
                      FROM Profit p
                      INNER JOIN Project pr ON p.ProjectID = pr.ProjectID
                      WHERE p.ProfitID = @ProfitID";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProfitID", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToProfit(reader);
        }

        return null;
    }

    public async Task<Profit?> GetByProjectIdAsync(int projectId)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"SELECT 
                        p.ProfitID, 
                        p.ProjectID, 
                        pr.ProjectName,
                        p.TotalEarnings, 
                        p.TotalExpenses, 
                        p.ProfitValue,
                        pr.Budget,
                        (pr.Budget - ISNULL(p.TotalExpenses, 0)) AS BudgetRemaining,
                        CASE 
                            WHEN pr.Budget IS NULL THEN 'No Budget'
                            WHEN p.TotalExpenses > pr.Budget THEN 'Over Budget'
                            WHEN p.TotalExpenses = pr.Budget THEN 'At Limit'
                            ELSE 'Within Budget'
                        END AS BudgetStatus,
                        p.CreateDate, 
                        p.UpdateDate
                      FROM Profit p
                      INNER JOIN Project pr ON p.ProjectID = pr.ProjectID
                      WHERE p.ProjectID = @ProjectID";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProjectID", projectId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToProfit(reader);
        }

        return null;
    }

    public async Task<int> CreateAsync(Profit profit)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"INSERT INTO Profit (ProjectID, TotalEarnings, TotalExpenses, CreateDate) 
                      VALUES (@ProjectID, @TotalEarnings, @TotalExpenses, GETDATE());
                      SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProjectID", profit.ProjectID);
        command.Parameters.AddWithValue("@TotalEarnings", profit.TotalEarnings ?? 0);
        command.Parameters.AddWithValue("@TotalExpenses", profit.TotalExpenses ?? 0);

        var id = await command.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    public async Task<bool> UpdateAsync(Profit profit)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = @"UPDATE Profit 
                      SET TotalEarnings = @TotalEarnings, 
                          TotalExpenses = @TotalExpenses,
                          UpdateDate = GETDATE()
                      WHERE ProfitID = @ProfitID";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProfitID", profit.ProfitID);
        command.Parameters.AddWithValue("@TotalEarnings", profit.TotalEarnings ?? 0);
        command.Parameters.AddWithValue("@TotalExpenses", profit.TotalExpenses ?? 0);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var query = "DELETE FROM Profit WHERE ProfitID = @ProfitID";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProfitID", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static Profit MapToProfit(SqlDataReader reader)
    {
        return new Profit
        {
            ProfitID = reader.GetInt32(0),
            ProjectID = reader.GetInt32(1),
            ProjectName = reader.IsDBNull(2) ? null : reader.GetString(2),
            TotalEarnings = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
            TotalExpenses = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
            ProfitValue = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
            ProjectBudget = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
            BudgetRemaining = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
            BudgetStatus = reader.IsDBNull(8) ? null : reader.GetString(8),
            CreateDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
            UpdateDate = reader.IsDBNull(10) ? null : reader.GetDateTime(10)
        };
    }
}
