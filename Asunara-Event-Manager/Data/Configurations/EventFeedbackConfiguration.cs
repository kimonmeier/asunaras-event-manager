using EventManager.Data.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configurations;

public class EventFeedbackConfiguration : IEntityTypeConfiguration<EventFeedback>
{
    public void Configure(EntityTypeBuilder<EventFeedback> builder)
    {
        builder
            .ToTable(nameof(EventFeedback));
        
        builder
            .HasKey(x => x.Id);
        
        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
        
        builder
            .HasOne(x => x.DiscordEvent)
            .WithMany()
            .HasForeignKey(x => x.DiscordEventId);

        builder
            .HasIndex([nameof(EventFeedback.DiscordEventId), nameof(EventFeedback.UserId)])
            .IsUnique();
    }
}