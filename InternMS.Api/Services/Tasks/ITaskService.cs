using InternMS.Api.DTOs.Tasks;
using InternMS.Api.DTOs.Common;

namespace InternMS.Api.Services.Tasks
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, Guid createdById);
        
        Task<IEnumerable<TaskListDto>> GetTasksByProjectAsync(Guid projectId);
        Task<PaginatedResponse<TaskListDto>> GetTasksByProjectPaginatedAsync(Guid projectId, int pageNumber = 1, int pageSize = 20);
        
        Task<TaskDto?> GetTaskByIdAsync(Guid taskId);
        Task UpdateTaskAsync(Guid taskId, UpdateTaskDto dto);
        Task DeleteTaskAsync(Guid taskId);
        
        // Task assignment methods
        Task<IEnumerable<TaskAssignmentDto>> AssignTaskToInternsAsync(AssignTaskDto dto);
        Task<IEnumerable<AssignedTaskDto>> GetTasksAssignedToInternAsync(Guid internId);
        Task<PaginatedResponse<AssignedTaskDto>> GetTasksAssignedToInternPaginatedAsync(Guid internId, int pageNumber = 1, int pageSize = 20);
        
        Task<IEnumerable<AssignedTaskDto>> GetCompletedTasksForInternAsync(Guid internId);
        Task<PaginatedResponse<AssignedTaskDto>> GetCompletedTasksForInternPaginatedAsync(Guid internId, int pageNumber = 1, int pageSize = 20);
        
        Task<AssignedTaskDto?> GetTaskWithAssignmentsAsync(Guid taskId);
        
        // Diagnostic methods
        Task<object> GetRawDiagnosticDataAsync(Guid internId);
    }
}
