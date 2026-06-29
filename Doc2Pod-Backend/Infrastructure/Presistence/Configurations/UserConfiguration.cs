using Domain.Users;
using Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("DomainUsers");

        builder.HasKey(u => u.Id);

        // ========================
        // Email (Value Converter)
        // ========================
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.FromDatabase(value)
            )
            .HasColumnName("Email")
            .IsRequired();

        // ========================
        // Username (Value Converter)
        // ========================
        builder.Property(u => u.Username)
            .HasConversion(
                username => username.Value,
                value => Username.FromDatabase(value)
            )
            .HasColumnName("Username")
            .IsRequired();

        builder.HasIndex(u => u.Username).IsUnique();

        // ========================
        // PhoneNumber (Value Converter)
        // ========================
        builder.Property(u => u.PhoneNumber)
            .HasConversion(
                phone => phone.Value,
                value => PhoneNumber.FromDatabase(value)
            )
            .HasColumnName("PhoneNumber");

        // ========================
        // FullName (Owned Type)
        // ========================
        builder.OwnsOne(u => u.FullName, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .IsRequired()
                .HasMaxLength(100);

            name.Property(n => n.LastName)
                .HasColumnName("LastName")
                .IsRequired()
                .HasMaxLength(100);
        });

        builder.Navigation(u => u.FullName).IsRequired();
        builder.Property(u => u.IdentityId)
                .HasColumnName("IdentityId")
                .IsRequired();

        builder.HasIndex(u => u.IdentityId)
            .IsUnique();


    }
}
