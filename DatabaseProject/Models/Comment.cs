using System.ComponentModel.DataAnnotations;

namespace DatabaseProject.Models;

public class Comment
{
    public int CommentNumber { get; set; }
    
    [Required]
    [Display(Name = "Employee")]
    public int WorkerID { get; set; }
    
    [Display(Name = "Employee Name")]
    public string? EmployeeName { get; set; }
    
    [Display(Name = "Assignment")]
    public int? AssignmentID { get; set; }
    
    [Display(Name = "Assignment Description")]
    public string? AssignmentDescription { get; set; }

    [Required]
    [Display(Name = "Comment Title")]
    [StringLength(100)]
    public string? CommentTitle { get; set; }

    [Required]
    [Display(Name = "Comment")]
    public string? CommentText { get; set; }
    
    [Display(Name = "Created Date")]
    public DateTime? CreateDate { get; set; }
    
    [Display(Name = "Updated Date")]
    public DateTime? UpdateDate { get; set; }
}
