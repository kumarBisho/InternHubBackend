using FluentValidation;
using InternMS.Api.DTOs.Notifications;

namespace InternMS.Api.Validators
{
    /// <summary>
    /// Validators for notification DTOs - ensures data quality and requirements
    /// </summary>
    
    public class CreateNotificationDtoValidator : AbstractValidator<CreateNotificationDto>
    {
        public CreateNotificationDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Title must be at least 3 characters");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required")
                .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters")
                .MinimumLength(5).WithMessage("Message must be at least 5 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ActionUrl)
                .Must(url => url == null || Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _))
                .WithMessage("ActionUrl must be a valid URI")
                .When(x => !string.IsNullOrEmpty(x.ActionUrl));

            RuleFor(x => x.PriorityLevel)
                .InclusiveBetween(1, 5).WithMessage("PriorityLevel must be between 1 and 5")
                .When(x => x.PriorityLevel.HasValue);
        }
    }

    public class NotificationPreferenceDtoValidator : AbstractValidator<NotificationPreferenceDto>
    {
        public NotificationPreferenceDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.QuietHourStartTime)
                .LessThan(x => x.QuietHourEndTime)
                .WithMessage("Quiet hour start time must be before end time")
                .When(x => x.QuietHourStartTime.HasValue && x.QuietHourEndTime.HasValue);

            RuleFor(x => x.QuietHourStartTime)
                .Must(time => time == null || time.Value.TotalHours >= 0 && time.Value.TotalHours < 24)
                .WithMessage("Quiet hour must be between 00:00 and 23:59")
                .When(x => x.QuietHourStartTime.HasValue);

            RuleFor(x => x.QuietHourEndTime)
                .Must(time => time == null || time.Value.TotalHours >= 0 && time.Value.TotalHours < 24)
                .WithMessage("Quiet hour must be between 00:00 and 23:59")
                .When(x => x.QuietHourEndTime.HasValue);
        }
    }
}
