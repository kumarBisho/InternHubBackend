namespace InternMS.Api.DTOs.Projects
{
    using System.Text.Json.Serialization;

    public class CreateProjectUpdateDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }
}