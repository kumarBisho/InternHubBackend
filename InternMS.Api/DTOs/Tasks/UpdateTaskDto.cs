using System;

namespace InternMS.Api.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        public string? Title { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Priority { get; set; }

        public string? Status { get; set; }
    }
}