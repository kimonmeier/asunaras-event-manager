using EventManager.Data.Entities.Events.QOTD;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configurations;

public class QotdQuestionConfiguration : IEntityTypeConfiguration<QotdQuestion>
{
    public void Configure(EntityTypeBuilder<QotdQuestion> builder)
    {
        builder
            .ToTable(nameof(QotdQuestion));

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
    }
}