using System.ComponentModel.DataAnnotations;

namespace DatabaseProject.Models;

public class Project
{
    public int ProjectID { get; set; }
    
    [Required]
    [Display(Name = "Project Name")]
    [StringLength(100)]
    public string ProjectName { get; set; } = string.Empty;
    
    [Display(Name = "Status")]
    [StringLength(20)]
    public string? ProjectStatus { get; set; } = "Active";
    
    public DateTime? Deadline { get; set; }
    
    [Display(Name = "Finish Date")]
    public DateTime? FinishDate { get; set; }
    
    [DataType(DataType.Currency)]
    public decimal? Budget { get; set; }
    
    [Display(Name = "Description")]
    public string? ProjectDescription { get; set; }
    
    [Display(Name = "Created Date")]
    public DateTime? CreateDate { get; set; }
    
    [Display(Name = "Updated Date")]
    public DateTime? UpdateDate { get; set; }
}
