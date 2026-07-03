using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternMS.Infrastructure.Configurations;

namespace InternMS.Infrastructure.Data
{
    // AppDbContext
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User> ();
        public DbSet<BlacklistedToken> BlacklistedTokens => Set<BlacklistedToken>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<UserProfile> Profiles => Set<UserProfile>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
        public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();
        public DbSet<ProjectTaskAssignment> ProjectTaskAssignments => Set<ProjectTaskAssignment>();
        public DbSet<ProjectUpdate> ProjectUpdates => Set<ProjectUpdate>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
        public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        public DbSet<CollaborativeComment> CollaborativeComments => Set<CollaborativeComment>();
        public DbSet<CommentReply> CommentReplies => Set<CommentReply>();
        public DbSet<UserFeedback> Feedbacks => Set<UserFeedback>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new ProfileConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectTaskConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectTaskAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectUpdateConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationPreferenceConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new ActivityLogConfiguration());
            modelBuilder.ApplyConfiguration(new CollaborativeCommentConfiguration());
            modelBuilder.ApplyConfiguration(new CommentReplyConfiguration());
            modelBuilder.ApplyConfiguration(new FeedbackConfiguration());
        }
    }

}