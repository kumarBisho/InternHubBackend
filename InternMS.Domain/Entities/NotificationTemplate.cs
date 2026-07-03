using System;

namespace InternMS.Domain.Entities
{
    /// <summary>
    /// Stores notification message templates with variable placeholders.
    /// Allows for consistent messaging across the application and easy multi-language support.
    /// </summary>
    public class NotificationTemplate
    {
        public int Id { get; set; }
        public NotificationType NotificationType { get; set; }
        
        // Template fields with placeholders like {UserName}, {ProjectName}, {TaskTitle}
        public string TitleTemplate { get; set; } = string.Empty;   // e.g., "Task {TaskTitle} assigned to you"
        public string MessageTemplate { get; set; } = string.Empty;  // e.g., "{UserName} assigned {TaskTitle} on {ProjectName}"
        public string? DescriptionTemplate { get; set; }              // e.g., "Priority: {Priority}, Due: {DueDate}"
        
        // Parameters that this template expects (comma-separated)
        // e.g., "UserName,TaskTitle,ProjectName,Priority,DueDate"
        public string? Parameters { get; set; }
        
        // Delivery preferences
        public bool SendInRealTime { get; set; } = true;
        public bool IncludeInEmailDigest { get; set; } = true;
        public bool IncludeInBrowserNotifications { get; set; } = true;
        
        // Priority level for notifications (1=low, 5=critical)
        public int PriorityLevel { get; set; } = 3;
        
        // Action URL template (e.g., "/projects/{ProjectId}/tasks/{TaskId}")
        public string? ActionUrlTemplate { get; set; }
        
        // Language/locale support (default is 'en')
        public string Language { get; set; } = "en";
        
        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        /// <summary>
        /// Render the template with provided values
        /// </summary>
        public string RenderTitle(Dictionary<string, string> parameters)
        {
            var result = TitleTemplate;
            foreach (var param in parameters)
            {
                result = result.Replace($"{{{param.Key}}}", param.Value);
            }
            return result;
        }

        /// <summary>
        /// Render the message template with provided values
        /// </summary>
        public string RenderMessage(Dictionary<string, string> parameters)
        {
            var result = MessageTemplate;
            foreach (var param in parameters)
            {
                result = result.Replace($"{{{param.Key}}}", param.Value);
            }
            return result;
        }

        /// <summary>
        /// Render the description template with provided values
        /// </summary>
        public string? RenderDescription(Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(DescriptionTemplate))
                return null;

            var result = DescriptionTemplate;
            foreach (var param in parameters)
            {
                result = result.Replace($"{{{param.Key}}}", param.Value);
            }
            return result;
        }

        /// <summary>
        /// Render the action URL with provided values
        /// </summary>
        public string? RenderActionUrl(Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(ActionUrlTemplate))
                return null;

            var result = ActionUrlTemplate;
            foreach (var param in parameters)
            {
                result = result.Replace($"{{{param.Key}}}", param.Value);
            }
            return result;
        }
    }
}
