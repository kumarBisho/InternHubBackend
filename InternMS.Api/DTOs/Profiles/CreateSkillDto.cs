namespace InternMS.Api.DTOs.Profiles
{
    public class CreateSkillDto
    {
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced, Expert
    }
}
