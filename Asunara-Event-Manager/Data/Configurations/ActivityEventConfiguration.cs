using EventManager.Data.Entities.Activity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configurations;

public class ActivityEventConfiguration : IEntityTypeConfiguration<ActivityEvent>
{
    public void Configure(EntityTypeBuilder<ActivityEvent> builder)
    {
        builder
            .ToTable(nameof(ActivityEvent));
        
        builder
            .HasKey(x => x.Id);
        
        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder
            .HasIndex(x => x.DiscordUserId);
        
        builder
            .HasIndex(nameof(ActivityEvent.Type), nameof(ActivityEvent.DiscordUserId), nameof(ActivityEvent.Date));
    }
}