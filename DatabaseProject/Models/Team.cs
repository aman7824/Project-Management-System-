using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace DatabaseProject.Models;

public class Team
{
    public int TeamID { get; set; }
    
    [Required]
    [Display(Name = "Team Name")]
    [StringLength(100)]
    public string TeamName { get; set; } = string.Empty;

    
    [Display(Name = "Manager")]
    public int? ManagerID { get; set; }

    [Display(Name = "Manager Name")]
    public string? ManagerName { get; set; }

    [Display(Name = "Created Date")]
    public DateTime? CreateDate { get; set; }

    [Display(Name = "Updated Date")]
    public DateTime? UpdateDate { get; set; }
}
