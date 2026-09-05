using System.ComponentModel.DataAnnotations;

namespace DatabaseProject.Models;

public class Employee
{
    public int WorkerID { get; set; }
    
    [Required]
    [Display(Name = "First Name")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Last Name")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Display(Name = "Phone Number")]
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(255)]
    public string? Address { get; set; }
    
    [Display(Name = "Employee Type")]
    public char? EmployeeType { get; set; } // 'F' for Full-time, 'H' for Hourly
    
    [Display(Name = "Created Date")]
    public DateTime? CreateDate { get; set; }
    
    [Display(Name = "Updated Date")]
    public DateTime? UpdateDate { get; set; }
    
    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";
}
