using System;

namespace InternMS.Api.DTOs.Tasks
{
    public class TaskAssignmentDto
    {
        public int Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid InternId { get; set; }
        public string InternName { get; set; } = string.Empty;
        public string InternEmail { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }
}
