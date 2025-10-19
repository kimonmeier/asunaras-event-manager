using EventManager.Data.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configurations;

public class DiscordUserConfiguration : IEntityTypeConfiguration<DiscordUser>
{
    public void Configure(EntityTypeBuilder<DiscordUser> builder)
    {
        builder
            .ToTable(nameof(DiscordUser));
        
        builder
            .HasKey(x => x.Id);

        builder
            .HasIndex(x => x.DiscordUserId)
            .IsUnique();
    }
}