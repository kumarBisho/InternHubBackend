using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class FeedbackConfiguration : IEntityTypeConfiguration<UserFeedback>
    {
        public void Configure(EntityTypeBuilder<UserFeedback> builder)
        {
            builder.ToTable("feedbacks");
            builder.HasKey(f => f.Id);
            
            builder.Property(f => f.Title).IsRequired().HasMaxLength(200);
            builder.Property(f => f.Content).IsRequired().HasMaxLength(2000);
            builder.Property(f => f.Type).HasConversion<string>().IsRequired();
            builder.Property(f => f.Rating).IsRequired(false);
            builder.Property(f => f.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(f => f.UpdatedAt).IsRequired(false);
            builder.Property(f => f.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(f => f.DeletedAt).IsRequired(false);

            // Foreign keys
            builder.HasOne(f => f.Mentor)
                .WithMany(u => u.FeedbackAsmentorGiven)
                .HasForeignKey(f => f.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Intern)
                .WithMany(u => u.FeedbackAsInternReceived)
                .HasForeignKey(f => f.InternId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Task)
                .WithMany(t => t.Feedback)
                .HasForeignKey(f => f.TaskId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasOne(f => f.Project)
                .WithMany(p => p.Feedback)
                .HasForeignKey(f => f.ProjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(f => f.MentorId);
            builder.HasIndex(f => f.InternId);
            builder.HasIndex(f => f.TaskId);
            builder.HasIndex(f => f.ProjectId);
            builder.HasIndex(f => f.CreatedAt);
            builder.HasIndex(f => f.IsDeleted);
        }
    }
}
