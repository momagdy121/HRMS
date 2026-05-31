namespace HRSystem.Business.DTOs.Tasks;

public class AssignTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssignedToId { get; set; }
    public DateOnly? DueDate { get; set; }
}
