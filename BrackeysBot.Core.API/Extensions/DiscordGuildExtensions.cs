using BrackeysBot.API.Extensions;
using BrackeysBot.Core.API.Configuration;
using DisCatSharp.Entities;

namespace BrackeysBot.Core.API.Extensions;

/// <summary>
///     Extension methods for <see cref="DiscordGuild" />.
/// </summary>
public static class DiscordGuildExtensions
{
    /// <summary>
    ///     Constructs an embed by populating the footer and thumbnail with the guild's branding.
    /// </summary>
    /// <param name="guild">The guild whose branding to apply.</param>
    /// <param name="addThumbnail">
    ///     <see langword="true" /> to include the guild icon as a thumbnail; otherwise, <see langword="false" />.
    /// </param>
    /// <returns>A new <see cref="DiscordEmbedBuilder" /> with the footer and thumbnail assigned the guild's branding.</returns>
    public static DiscordEmbedBuilder CreateDefaultEmbed(this DiscordGuild guild, bool addThumbnail = true)
    {
        ICorePlugin.Current.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration);
        guildConfiguration ??= new GuildConfiguration();
        return guild.CreateDefaultEmbed(guildConfiguration, addThumbnail);
    }

    /// <summary>
    ///     Constructs an embed by populating the footer and thumbnail with the guild's branding.
    /// </summary>
    /// <param name="guild">The guild whose branding to apply.</param>
    /// <param name="guildConfiguration">The configuration from which to build.</param>
    /// <param name="addThumbnail">
    ///     <see langword="true" /> to include the guild icon as a thumbnail; otherwise, <see langword="false" />.
    /// </param>
    /// <returns>A new <see cref="DiscordEmbedBuilder" /> with the footer and thumbnail assigned the guild's branding.</returns>
    public static DiscordEmbedBuilder CreateDefaultEmbed(this DiscordGuild guild, GuildConfiguration guildConfiguration,
        bool addThumbnail = true)
    {
        return new DiscordEmbedBuilder().WithColor(guildConfiguration.PrimaryColor).WithGuildInfo(guild, addThumbnail);
    }
}
