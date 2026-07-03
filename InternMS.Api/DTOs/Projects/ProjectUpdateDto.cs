namespace InternMS.Api.DTOs.Projects
{
    public class ProjectUpdateDto
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid AuthorId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}