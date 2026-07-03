using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class CollaborativeCommentConfiguration : IEntityTypeConfiguration<CollaborativeComment>
    {
        public void Configure(EntityTypeBuilder<CollaborativeComment> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.UserId)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(c => c.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(c => c.UserEmail)
                .HasMaxLength(256);

            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(c => c.ResourceType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.ResourceId)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()");

            builder.Property(c => c.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Relationship with replies
            builder.HasMany(c => c.Replies)
                .WithOne(r => r.Comment)
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(c => new { c.ResourceType, c.ResourceId })
                .HasDatabaseName("ix_comments_resource");

            builder.HasIndex(c => c.UserId)
                .HasDatabaseName("ix_comments_user");

            builder.HasIndex(c => c.CreatedAt)
                .IsDescending()
                .HasDatabaseName("ix_comments_created_desc");

            builder.HasIndex(c => c.IsDeleted)
                .HasDatabaseName("ix_comments_is_deleted");

            builder.ToTable("collaborative_comments");
        }
    }

    public class CommentReplyConfiguration : IEntityTypeConfiguration<CommentReply>
    {
        public void Configure(EntityTypeBuilder<CommentReply> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.CommentId)
                .IsRequired();

            builder.Property(r => r.UserId)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(r => r.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(r => r.UserEmail)
                .HasMaxLength(256);

            builder.Property(r => r.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()");

            builder.Property(r => r.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(r => r.CommentId)
                .HasDatabaseName("ix_replies_comment");

            builder.HasIndex(r => r.UserId)
                .HasDatabaseName("ix_replies_user");

            builder.HasIndex(r => r.CreatedAt)
                .IsDescending()
                .HasDatabaseName("ix_replies_created_desc");

            builder.ToTable("comment_replies");
        }
    }
}
