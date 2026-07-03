using InternMS.Api.DTOs.Projects;
using InternMS.Api.DTOs.Common;
using InternMS.Domain.Entities;

namespace InternMS.Api.Services.Projects
{
    public interface IProjectService
    {
        Task<Project> CreateProjectAsync(Guid creatorId, CreateProjectDto dto);
        Task<Project?> GetProjectByIdAsync(Guid id);
        
        Task<IEnumerable<Project>> GetProjectsForUserAsync(Guid userId, string role);
        Task<PaginatedResponse<ProjectDto>> GetProjectsForUserPaginatedAsync(Guid userId, string role, int pageNumber = 1, int pageSize = 20);
        
        Task<Project> UpdateProjectAsync(Guid projectId, CreateProjectDto dto);
        Task<Project> PartialUpdateProjectAsync(Guid projectId, PartialUpdateProjectDto dto);
        Task AssignProjectAsync(Guid projectId, AssignProjectDto dto);
        Task<ProjectUpdateDto> AddProjectUpdateAsync(Guid projectId, Guid authorId, CreateProjectUpdateDto dto);
        Task DeleteProjectAsync(Guid id);
    }
}