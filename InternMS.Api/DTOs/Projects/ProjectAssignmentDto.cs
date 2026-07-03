namespace InternMS.Api.DTOs.Projects;

/// <summary>
/// DTO for project team assignments
/// </summary>
public class ProjectAssignmentDto
{
    public int Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; }
    public Guid InternId { get; set; }
    public string InternName { get; set; }
    public Guid? MentorId { get; set; }
    public string MentorName { get; set; }
    public DateTime AssignedAt { get; set; }
}

