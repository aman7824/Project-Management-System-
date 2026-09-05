using System.ComponentModel.DataAnnotations;

namespace DatabaseProject.Models;

public class Leave
{
    public int LeaveNumber { get; set; }
    
    [Required]
    [Display(Name = "Employee")]
    public int WorkerID { get; set; }
    
    [Display(Name = "Employee Name")]
    public string? EmployeeName { get; set; }
    
    [Required]
    [Display(Name = "Leave Type")]
    public string LeaveType { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
    
    [Required]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }
    
    [Display(Name = "Total Days")]
    public int? TotalLeaveDay { get; set; }
}
