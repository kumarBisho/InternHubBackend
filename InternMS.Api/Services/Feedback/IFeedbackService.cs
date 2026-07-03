using InternMS.Api.DTOs.Feedback;
using InternMS.Api.DTOs.Common;

namespace InternMS.Api.Services.Feedback
{
    public interface IFeedbackService
    {
        Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackDto dto, Guid mentorId);
        Task<FeedbackDto?> GetFeedbackByIdAsync(Guid feedbackId);
        
        // Get feedback received by intern
        Task<PaginatedResponse<FeedbackListDto>> GetFeedbackReceivedByInternAsync(Guid internId, int pageNumber = 1, int pageSize = 20);
        
        // Get feedback given by mentor
        Task<PaginatedResponse<FeedbackListDto>> GetFeedbackGivenByMentorAsync(Guid mentorId, int pageNumber = 1, int pageSize = 20);
        
        // Get feedback for a specific task
        Task<PaginatedResponse<FeedbackListDto>> GetFeedbackForTaskAsync(Guid taskId, int pageNumber = 1, int pageSize = 20);
        
        // Get feedback for a specific project
        Task<PaginatedResponse<FeedbackListDto>> GetFeedbackForProjectAsync(Guid projectId, int pageNumber = 1, int pageSize = 20);
        
        // Get all feedback for an intern (by type)
        Task<IEnumerable<FeedbackListDto>> GetFeedbackByTypeAsync(Guid internId, string feedbackType);
        
        // Update feedback
        Task UpdateFeedbackAsync(Guid feedbackId, UpdateFeedbackDto dto, Guid mentorId);
        
        // Delete feedback
        Task DeleteFeedbackAsync(Guid feedbackId, Guid mentorId);
    }
}
