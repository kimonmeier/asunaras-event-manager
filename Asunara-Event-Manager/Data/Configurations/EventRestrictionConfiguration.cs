using EventManager.Data.Entities.Events;
using EventManager.Data.Entities.Restrictions;
using EventManager.Data.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configurations;

public class EventRestrictionConfiguration : IEntityTypeConfiguration<EventRestriction>
{
    public void Configure(EntityTypeBuilder<EventRestriction> builder)
    {
        builder
            .ToTable(nameof(EventRestriction));

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
        
        builder
            .HasOne(x => x.DiscordEvent)
            .WithMany(x => x.Restrictions)
            .HasForeignKey(x => x.DiscordEventId);

        builder
            .HasDiscriminator(x => x.Type)
            .HasValue<FskRestrictions>(RestrictionType.Fsk);
        
        builder
            .HasIndex([nameof(FskRestrictions.DiscordEventId), nameof(FskRestrictions.Type)])
            .IsUnique();
    }
}