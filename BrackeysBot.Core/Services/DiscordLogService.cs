using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using BrackeysBot.Core.API;
using BrackeysBot.Core.API.Configuration;
using BrackeysBot.Core.Resources;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using NLog;

namespace BrackeysBot.Core.Services;

/// <summary>
///     Represents a service which manages staff log channels, and allows the posting of embeds and messages in a guild's staff
///     log channel.
/// </summary>
internal sealed class DiscordLogService : BackgroundService
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly DiscordClient _discordClient;
    private readonly ConfigurationService _configurationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DiscordLogService" /> class.
    /// </summary>
    public DiscordLogService(DiscordClient discordClient, ConfigurationService configurationService)
    {
        _discordClient = discordClient;
        _configurationService = configurationService;
    }

    /// <summary>
    ///     Gets the log channel for a specified guild.
    /// </summary>
    /// <param name="guild">The guild whose log channel to retrieve.</param>
    /// <param name="channel">
    ///     When this method returns, contains the log channel; or <see langword="null" /> if no such channel is found.
    /// </param>
    /// <returns><see langword="true" /> if the log channel was successfully found; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="guild" /> is <see langword="null" />.</exception>
    public bool TryGetLogChannel(DiscordGuild guild, [NotNullWhen(true)] out DiscordChannel? channel)
    {
        if (guild is null)
            throw new ArgumentNullException(nameof(guild));

        ChannelConfiguration channelConfiguration = _configurationService.GetGuildConfiguration(guild).ChannelConfiguration;
        channel = guild.GetChannel(channelConfiguration.LogChannelId);
        return channel is not null;
    }

    /// <inheritdoc cref="ICorePlugin.LogAsync" />
    public async Task LogAsync(DiscordGuild guild, DiscordEmbed embed,
        StaffNotificationOptions notificationOptions = StaffNotificationOptions.None)
    {
        if (!TryGetLogChannel(guild, out DiscordChannel? logChannel)) return;
        await logChannel.SendMessageAsync(BuildMentionString(guild, notificationOptions), embed);
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordClient.GuildAvailable += DiscordClientOnGuildAvailable;
        return Task.CompletedTask;
    }

    private string? BuildMentionString(DiscordGuild guild, StaffNotificationOptions notificationOptions)
    {
        if (!TryGetLogChannel(guild, out DiscordChannel? logChannel)) return null;
        if (notificationOptions == StaffNotificationOptions.None) return null;

        RoleConfiguration roleConfiguration = _configurationService.GetGuildConfiguration(logChannel.Guild).RoleConfiguration;
        DiscordRole? administratorRole = logChannel.Guild.GetRole(roleConfiguration.AdministratorRoleId);
        DiscordRole? moderatorRole = logChannel.Guild.GetRole(roleConfiguration.ModeratorRoleId);

        var mentions = new List<string>();

        if ((notificationOptions & StaffNotificationOptions.Administrator) != 0) mentions.Add(administratorRole.Mention);
        if ((notificationOptions & StaffNotificationOptions.Moderator) != 0) mentions.Add(moderatorRole.Mention);
        if ((notificationOptions & StaffNotificationOptions.Here) != 0) mentions.Add("@here");
        if ((notificationOptions & StaffNotificationOptions.Everyone) != 0) mentions.Add("@everyone");

        return string.Join(' ', mentions);
    }

    private Task DiscordClientOnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        GuildConfiguration guildConfiguration = _configurationService.GetGuildConfiguration(e.Guild);
        ulong logChannelId = guildConfiguration.ChannelConfiguration.LogChannelId;

        if (logChannelId != 0)
        {
            if (e.Guild.GetChannel(logChannelId) is { } channel)
                Logger.Info(string.Format(LoggerMessages.LogChannelFound, channel, e.Guild));
            else
                Logger.Warn(string.Format(LoggerMessages.LogChannelNotFound, e.Guild));
        }
        else
            Logger.Warn(string.Format(LoggerMessages.LogChannelNotDefined, e.Guild));

        return Task.CompletedTask;
    }
}
