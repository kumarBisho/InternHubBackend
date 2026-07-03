using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd();

            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(a => a.UserEmail)
                .HasMaxLength(256);

            builder.Property(a => a.ActionType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.ResourceType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.ResourceId)
                .IsRequired(false);

            builder.Property(a => a.ResourceName)
                .HasMaxLength(500);

            builder.Property(a => a.Description)
                .HasMaxLength(1000);

            builder.Property(a => a.ChangeDetails)
                .HasColumnType("jsonb");

            builder.Property(a => a.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("now()");

            // Indexes for performance
            builder.HasIndex(a => a.Timestamp)
                .IsDescending()
                .HasDatabaseName("ix_activity_logs_timestamp_desc");

            builder.HasIndex(a => a.UserId)
                .HasDatabaseName("ix_activity_logs_user_id");

            builder.HasIndex(a => new { a.ResourceType, a.ResourceId })
                .HasDatabaseName("ix_activity_logs_resource");

            builder.HasIndex(a => a.ActionType)
                .HasDatabaseName("ix_activity_logs_action_type");

            builder.ToTable("activity_logs");
        }
    }
}
