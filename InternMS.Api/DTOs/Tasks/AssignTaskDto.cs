using System;

namespace InternMS.Api.DTOs.Tasks
{
    public class AssignTaskDto
    {
        public Guid TaskId { get; set; }
        public List<Guid> InternIds { get; set; } = new List<Guid>();
    }
}
