using System;

namespace BrackeysBot.Core.Data;

internal sealed class Bookmark : IEquatable<Bookmark>, IComparable<Bookmark>
{
    /// <summary>
    ///     Gets the ID of this bookmark.
    /// </summary>
    /// <value>The bookmark ID.</value>
    public Guid Id { get; internal set; } = Guid.Empty;

    /// <summary>
    ///     Gets the date and time at which this bookmark was created.
    /// </summary>
    /// <value>The bookmark creation timestamp.</value>
    public DateTimeOffset CreationTimestamp { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets the ID of the user who created this bookmark.
    /// </summary>
    /// <value>The user ID.</value>
    public ulong UserId { get; internal set; }

    /// <summary>
    ///     Gets the ID of the guild to which this bookmark points.
    /// </summary>
    /// <value>The guild ID.</value>
    public ulong GuildId { get; internal set; }

    /// <summary>
    ///     Gets the ID of the channel to which this bookmark points.
    /// </summary>
    /// <value>The channel ID.</value>
    public ulong ChannelId { get; internal set; }

    /// <summary>
    ///     Gets the ID of the message to which this bookmark points.
    /// </summary>
    /// <value>The message ID.</value>
    public ulong MessageId { get; internal set; }

    /// <summary>
    ///     Gets the ID of the message that was sent to the user's DMs.
    /// </summary>
    /// <value>The bookmark message ID.</value>
    public ulong PrivateMessageId { get; internal set; }

    /// <summary>
    ///     Returns a value indicating whether two <see cref="Bookmark" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true" /> if the instances are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(Bookmark? left, Bookmark? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two <see cref="Bookmark" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true" /> if the instances are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(Bookmark? left, Bookmark? right)
    {
        return !Equals(left, right);
    }

    /// <inheritdoc />
    public int CompareTo(Bookmark? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return CreationTimestamp.CompareTo(other.CreationTimestamp);
    }

    /// <inheritdoc />
    public bool Equals(Bookmark? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is Bookmark other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Id.GetHashCode();
    }
}
