using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BrackeysBot.Core.Data.EntityConfigurations;

internal sealed class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.ToTable(nameof(Bookmark));
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasConversion<GuidToBytesConverter>();
        builder.Property(e => e.CreationTimestamp).HasConversion<DateTimeOffsetToBytesConverter>();
        builder.Property(e => e.UserId);
        builder.Property(e => e.GuildId);
        builder.Property(e => e.ChannelId);
        builder.Property(e => e.MessageId);
        builder.Property(e => e.PrivateMessageId);
    }
}
