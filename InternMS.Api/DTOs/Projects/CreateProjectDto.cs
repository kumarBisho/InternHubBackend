namespace InternMS.Api.DTOs.Projects
{
    using System.Text.Json.Serialization;

    public class CreateProjectDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("techStack")]
        public string? TechStack { get; set; } // JSON array as string

        [JsonPropertyName("progress")]
        public int Progress { get; set; } = 0;

        [JsonPropertyName("repositoryUrl")]
        public string? RepositoryUrl { get; set; }

        [JsonPropertyName("documentationUrl")]
        public string? DocumentationUrl { get; set; }

        [JsonPropertyName("demoUrl")]
        public string? DemoUrl { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}