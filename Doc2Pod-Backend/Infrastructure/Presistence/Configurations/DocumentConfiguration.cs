using Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.FilePath)
                .IsRequired();

            builder.HasOne(p => p.User)
               .WithMany(u => u.Documents)
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}