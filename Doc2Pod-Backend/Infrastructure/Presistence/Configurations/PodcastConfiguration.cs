using Domain.Enums;
using Domain.Podcasts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PodcastConfiguration : IEntityTypeConfiguration<Podcast>
    {
        public void Configure(EntityTypeBuilder<Podcast> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasDefaultValue(PodcastStatus.Pending);

            builder.HasOne(p => p.Document)
                .WithMany(d => d.Podcasts)
                .HasForeignKey(p => p.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.User)
                .WithMany(u => u.Podcasts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
