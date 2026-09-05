using System.ComponentModel.DataAnnotations;

namespace DatabaseProject.Models;

public class TaskAssignment
{
    public int AssignmentID { get; set; }
    
    [Required]
    [Display(Name = "Team")]
    public int TeamID { get; set; }
    
    [Display(Name = "Team Name")]
    public string? TeamName { get; set; }
    
    [Required]
    [Display(Name = "Project")]
    public int ProjectID { get; set; }
    
    [Display(Name = "Project Name")]
    public string? ProjectName { get; set; }

    [Required]
    [Display(Name = "Task Description")]
    public string? TaskDescription { get; set; }
    
    [Display(Name = "Assignment Date")]
    public DateTime? AssignmentDate { get; set; }
    
    public DateTime? Deadline { get; set; }

    public static implicit operator Task<object>(TaskAssignment? v)
    {
        throw new NotImplementedException();
    }
}
