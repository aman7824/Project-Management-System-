using System.ComponentModel.DataAnnotations;

namespace DatabaseProject.Models;

public class Profit
{
    public int ProfitID { get; set; }
    
    [Required]
    [Display(Name = "Project")]
    public int ProjectID { get; set; }
    
    [Display(Name = "Project Name")]
    public string? ProjectName { get; set; }
    
    [Display(Name = "Total Earnings")]
    [DataType(DataType.Currency)]
    public decimal? TotalEarnings { get; set; }
    
    [Display(Name = "Total Expenses")]
    [DataType(DataType.Currency)]
    public decimal? TotalExpenses { get; set; }
    
    [Display(Name = "Profit/Loss")]
    [DataType(DataType.Currency)]
    public decimal? ProfitValue { get; set; }
    
    [Display(Name = "Project Budget")]
    [DataType(DataType.Currency)]
    public decimal? ProjectBudget { get; set; }
    
    [Display(Name = "Budget Remaining")]
    [DataType(DataType.Currency)]
    public decimal? BudgetRemaining { get; set; }
    
    [Display(Name = "Budget Status")]
    public string? BudgetStatus { get; set; }
    
    [Display(Name = "Created Date")]
    public DateTime? CreateDate { get; set; }
    
    [Display(Name = "Updated Date")]
    public DateTime? UpdateDate { get; set; }
}
