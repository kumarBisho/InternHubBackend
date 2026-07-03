namespace InternMS.Api.DTOs.Projects
{
    using System.Text.Json.Serialization;

    public class PartialUpdateProjectDto
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("progress")]
        public int? Progress { get; set; }

        [JsonPropertyName("repositoryUrl")]
        public string? RepositoryUrl { get; set; }

        [JsonPropertyName("documentationUrl")]
        public string? DocumentationUrl { get; set; }

        [JsonPropertyName("demoUrl")]
        public string? DemoUrl { get; set; }
    }
}