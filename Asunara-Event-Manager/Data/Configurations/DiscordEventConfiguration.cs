using EventManager.Data.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configurations;

public class DiscordEventConfiguration : IEntityTypeConfiguration<DiscordEvent>
{
    public void Configure(EntityTypeBuilder<DiscordEvent> builder)
    {
        builder
            .ToTable(nameof(DiscordEvent));
        
        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder
            .HasIndex(x => x.DiscordId);

        builder
            .HasIndex(x => x.IsCompleted);
    }
}