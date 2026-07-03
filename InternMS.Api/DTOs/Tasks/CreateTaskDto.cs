using System;
using System.ComponentModel.DataAnnotations;

namespace InternMS.Api.DTOs.Tasks
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public DateTime EndDate { get; set; }
        public string Priority { get; set; } = "High";
    }
}