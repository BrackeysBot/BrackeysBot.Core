using DSharpPlus.Entities;

namespace BrackeysBot.Core.API;

/// <summary>
///     Represents a context for field value and execution check delegates when used with
///     <see cref="ICorePlugin.RegisterUserInfoField(UserInfoFieldBuilder)" /> and
///     <see cref="ICorePlugin.RegisterUserInfoField(System.Action{UserInfoFieldBuilder})" />.
/// </summary>
public sealed class UserInfoFieldContext
{
    /// <summary>
    ///     Gets the channel in which this field was created.
    /// </summary>
    /// <value>The channel.</value>
    public DiscordChannel Channel { get; internal set; } = null!;

    /// <summary>
    ///     Gets the guild in which this field was created.
    /// </summary>
    /// <value>The guild.</value>
    public DiscordGuild? Guild { get; internal set; }

    /// <summary>
    ///     Gets the member which requested this field, if this field was requested in a guild.
    /// </summary>
    /// <value>The member.</value>
    public DiscordMember? Member { get; internal set; }

    /// <summary>
    ///     Gets the target member.
    /// </summary>
    /// <value>The target member.</value>
    public DiscordMember? TargetMember { get; internal set; }

    /// <summary>
    ///     Gets the target user.
    /// </summary>
    /// <value>The target user.</value>
    public DiscordUser TargetUser { get; internal set; } = null!;

    /// <summary>
    ///     Gets the user which requested this field.
    /// </summary>
    /// <value>The user.</value>
    public DiscordUser User { get; internal set; } = null!;
}
