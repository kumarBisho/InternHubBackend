using System;

namespace InternMS.Api.DTOs.Tasks
{
    public class TaskListDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public DateTime EndDate { get; set; }
    }
}