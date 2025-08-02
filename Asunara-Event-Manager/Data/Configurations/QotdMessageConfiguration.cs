using EventManager.Data.Entities.Events.QOTD;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configurations;

public class QotdMessageConfiguration : IEntityTypeConfiguration<QotdMessage>
{
    public void Configure(EntityTypeBuilder<QotdMessage> builder)
    {
        builder
            .ToTable(nameof(QotdMessage));

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
        
        builder
            .HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId);
        
        builder
            .HasIndex(x => x.MessageId)
            .IsUnique();
    }
}