using System.ComponentModel.DataAnnotations;

namespace DatabaseProject.Models;

public class Invoice
{
    public int InvoiceID { get; set; }
    
    [Display(Name = "Worker")]
    public int? WorkerID { get; set; }
    
    [Display(Name = "Employee Name")]
    public string? EmployeeName { get; set; }
    
    [Display(Name = "Employee Wage")]
    [DataType(DataType.Currency)]
    public decimal? EmployeeWage { get; set; }
    
    [Display(Name = "Created Date")]
    public DateTime? CreateDate { get; set; }
    
    [Display(Name = "Hours Worked")]
    public decimal? HoursWorked { get; set; }
}
