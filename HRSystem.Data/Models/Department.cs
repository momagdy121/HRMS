using System.ComponentModel.DataAnnotations;

namespace HRSystem.Data.Models;

public class Department
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int ManagerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
