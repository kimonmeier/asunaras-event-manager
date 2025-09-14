using EventManager.Data.Entities.Birthday;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configurations;

public class UserBirthdayConfiguration : IEntityTypeConfiguration<UserBirthday>
{
    public void Configure(EntityTypeBuilder<UserBirthday> builder)
    {
        builder
            .ToTable(nameof(UserBirthday));

        builder
            .HasKey(x => x.Id);
        
        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.CreationDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder
            .HasIndex([nameof(UserBirthday.DiscordId), nameof(UserBirthday.IsDeleted)]);
    }
}