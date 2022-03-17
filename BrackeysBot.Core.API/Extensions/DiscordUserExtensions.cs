using DisCatSharp.Entities;

namespace BrackeysBot.Core.API.Extensions;

/// <summary>
///     Extension methods for <see cref="DiscordUser" />.
/// </summary>
public static class DiscordUserExtensions
{
    /// <inheritdoc cref="ICorePlugin.GetPermissionLevel" />
    public static PermissionLevel GetPermissionLevel(this DiscordUser user, DiscordGuild guild)
    {
        return ICorePlugin.Current.GetPermissionLevel(user, guild);
    }

    /// <inheritdoc cref="ICorePlugin.IsHigherLevelThan" />
    public static bool IsHigherLevelThan(this DiscordUser user, DiscordUser other, DiscordGuild guild)
    {
        return ICorePlugin.Current.IsHigherLevelThan(user, other, guild);
    }

    /// <inheritdoc cref="ICorePlugin.IsStaffMember" />
    public static bool IsStaffMember(this DiscordUser user, DiscordGuild guild)
    {
        return ICorePlugin.Current.IsStaffMember(user, guild);
    }
}
