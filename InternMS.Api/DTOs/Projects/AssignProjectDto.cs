namespace InternMS.Api.DTOs.Projects
{
    using System.Text.Json.Serialization;

    public class AssignProjectDto
    {
        [JsonPropertyName("internId")]
        public Guid InternId { get; set; }

        [JsonPropertyName("mentorId")]
        public Guid? MentorId { get; set; }
    }
}