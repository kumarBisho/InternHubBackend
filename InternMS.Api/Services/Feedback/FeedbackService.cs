using AutoMapper;
using AutoMapper.QueryableExtensions;
using InternMS.Api.DTOs.Feedback;
using InternMS.Api.DTOs.Common;
using InternMS.Api.Utils;
using InternMS.Domain.Entities;
using InternMS.Domain.Enums;
using InternMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternMS.Api.Services.Feedback
{
    public class FeedbackService : IFeedbackService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(
            AppDbContext db,
            IMapper mapper,
            INotificationService notificationService,
            ILogger<FeedbackService> logger)
        {
            _db = db;
            _mapper = mapper;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackDto dto, Guid mentorId)
        {
            // Validate intern exists and is active
            var intern = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.InternId && u.IsActive);
            if (intern == null)
                throw new KeyNotFoundException("Intern not found or is inactive.");

            // Validate mentor exists and is active
            var mentor = await _db.Users.FirstOrDefaultAsync(u => u.Id == mentorId && u.IsActive);
            if (mentor == null)
                throw new KeyNotFoundException("Mentor not found or is inactive.");

            // Validate feedback type
            if (!Enum.TryParse<FeedbackType>(dto.Type, true, out var feedbackType))
                throw new InvalidOperationException("Invalid feedback type.");

            // Validate task if task feedback
            if (feedbackType == FeedbackType.TaskFeedback && dto.TaskId.HasValue)
            {
                var task = await _db.ProjectTasks.FirstOrDefaultAsync(t => t.Id == dto.TaskId);
                if (task == null)
                    throw new KeyNotFoundException("Task not found.");
            }

            // Validate project if project feedback
            if (feedbackType == FeedbackType.ProjectFeedback && dto.ProjectId.HasValue)
            {
                var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
                if (project == null)
                    throw new KeyNotFoundException("Project not found.");
            }

            var feedback = _mapper.Map<Domain.Entities.UserFeedback>(dto);
            feedback.MentorId = mentorId;
            feedback.CreatedAt = DateTime.UtcNow;

            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync();

            // Send notification to intern
            try
            {
                var feedbackTitle = feedbackType == FeedbackType.TaskFeedback
                    ? $"New feedback on task '{_db.ProjectTasks.FirstOrDefault(t => t.Id == dto.TaskId)?.Title}'"
                    : feedbackType == FeedbackType.ProjectFeedback
                        ? $"New feedback on project '{_db.Projects.FirstOrDefault(p => p.Id == dto.ProjectId)?.Title}'"
                        : "New general feedback";

                await _notificationService.CreateNotificationAsync(
                    dto.InternId,
                    "New Feedback",
                    $"You have received new feedback from {mentor.FirstName} {mentor.LastName}",
                    NotificationType.GeneralNotification,
                    triggeredByUserId: mentorId,
                    relatedEntityId: feedback.Id,
                    relatedEntityType: "Feedback",
                    actionUrl: $"/feedback/{feedback.Id}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send feedback notification: {ex.Message}");
            }

            // Reload with relationships to return full DTO
            var savedFeedback = await _db.Feedbacks
                .Include(f => f.Mentor)
                .Include(f => f.Intern)
                .Include(f => f.Task)
                .Include(f => f.Project)
                .FirstOrDefaultAsync(f => f.Id == feedback.Id);

            return _mapper.Map<FeedbackDto>(savedFeedback);
        }

        public async Task<FeedbackDto?> GetFeedbackByIdAsync(Guid feedbackId)
        {
            var feedback = await _db.Feedbacks
                .Include(f => f.Mentor)
                .Include(f => f.Intern)
                .Include(f => f.Task)
                .Include(f => f.Project)
                .FirstOrDefaultAsync(f => f.Id == feedbackId && !f.IsDeleted);

            return feedback != null ? _mapper.Map<FeedbackDto>(feedback) : null;
        }

        public async Task<PaginatedResponse<FeedbackListDto>> GetFeedbackReceivedByInternAsync(
            Guid internId, int pageNumber = 1, int pageSize = 20)
        {
            var query = _db.Feedbacks
                .Include(f => f.Mentor)
                .Where(f => f.InternId == internId && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<FeedbackListDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginatedResponse<FeedbackListDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedResponse<FeedbackListDto>> GetFeedbackGivenByMentorAsync(
            Guid mentorId, int pageNumber = 1, int pageSize = 20)
        {
            var query = _db.Feedbacks
                .Include(f => f.Mentor)
                .Where(f => f.MentorId == mentorId && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<FeedbackListDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginatedResponse<FeedbackListDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedResponse<FeedbackListDto>> GetFeedbackForTaskAsync(
            Guid taskId, int pageNumber = 1, int pageSize = 20)
        {
            var query = _db.Feedbacks
                .Include(f => f.Mentor)
                .Where(f => f.TaskId == taskId && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<FeedbackListDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginatedResponse<FeedbackListDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedResponse<FeedbackListDto>> GetFeedbackForProjectAsync(
            Guid projectId, int pageNumber = 1, int pageSize = 20)
        {
            var query = _db.Feedbacks
                .Include(f => f.Mentor)
                .Where(f => f.ProjectId == projectId && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<FeedbackListDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginatedResponse<FeedbackListDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<FeedbackListDto>> GetFeedbackByTypeAsync(Guid internId, string feedbackType)
        {
            if (!Enum.TryParse<FeedbackType>(feedbackType, true, out var type))
                throw new InvalidOperationException("Invalid feedback type.");

            var feedbacks = await _db.Feedbacks
                .Include(f => f.Mentor)
                .Where(f => f.InternId == internId && f.Type == type && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .ProjectTo<FeedbackListDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return feedbacks;
        }

        public async Task UpdateFeedbackAsync(Guid feedbackId, UpdateFeedbackDto dto, Guid mentorId)
        {
            var feedback = await _db.Feedbacks.FirstOrDefaultAsync(f => f.Id == feedbackId && !f.IsDeleted);
            if (feedback == null)
                throw new KeyNotFoundException("Feedback not found.");

            if (feedback.MentorId != mentorId)
                throw new UnauthorizedAccessException("You can only update your own feedback.");

            feedback.Title = dto.Title;
            feedback.Content = dto.Content;
            feedback.Rating = dto.Rating;
            feedback.UpdatedAt = DateTime.UtcNow;

            _db.Feedbacks.Update(feedback);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Feedback {feedbackId} updated by mentor {mentorId}");
        }

        public async Task DeleteFeedbackAsync(Guid feedbackId, Guid mentorId)
        {
            var feedback = await _db.Feedbacks.FirstOrDefaultAsync(f => f.Id == feedbackId && !f.IsDeleted);
            if (feedback == null)
                throw new KeyNotFoundException("Feedback not found.");

            if (feedback.MentorId != mentorId)
                throw new UnauthorizedAccessException("You can only delete your own feedback.");

            feedback.IsDeleted = true;
            feedback.DeletedAt = DateTime.UtcNow;

            _db.Feedbacks.Update(feedback);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Feedback {feedbackId} deleted by mentor {mentorId}");
        }
    }
}
