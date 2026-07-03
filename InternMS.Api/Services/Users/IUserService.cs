using InternMS.Api.DTOs.Users;
using InternMS.Api.DTOs.Profiles;
using InternMS.Api.DTOs.Common;
using InternMS.Api.DTOs.Projects;
using InternMS.Domain.Entities;


namespace InternMS.Api.Services.Users   
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<PaginatedResponse<UserDto>> GetAllUsersPaginatedAsync(int pageNumber = 1, int pageSize = 20);
        
        Task<User?> GetUserByIdAsync(Guid id);
        
        Task UpdateUserAsync(Guid id, UserDto updatedUser);
        Task DeleteUserAsync(Guid id);
        
        Task<UserProfile?> GetProfileAsync(Guid userId);
        Task<UserProfileDetailDto?> GetUserProfileDetailAsync(Guid userId);
        Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        
        Task<IEnumerable<User>> GetInternsByMentorAsync(Guid mentorId);
        Task<PaginatedResponse<UserDto>> GetInternsByMentorPaginatedAsync(Guid mentorId, int pageNumber = 1, int pageSize = 20);
        
        Task<IEnumerable<User>> GetInternsByMentorAndProjectAsync(Guid mentorId, Guid projectId);
        
        Task<User?> GetAssignedMentorAsync(Guid internId);
        
        Task<IEnumerable<ProjectAssignment>> GetAllProjectAssignmentsAsync();
        Task<PaginatedResponse<ProjectAssignmentDto>> GetAllProjectAssignmentsPaginatedAsync(int pageNumber = 1, int pageSize = 20);
        
        Task<IEnumerable<User>> GetAllMentorsAndInternsAsync();
        Task<PaginatedResponse<UserDto>> GetAllMentorsAndInternsPaginatedAsync(int pageNumber = 1, int pageSize = 20);
    }
}