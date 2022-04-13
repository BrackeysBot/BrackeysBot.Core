using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrackeysBot.API.Logging;
using BrackeysBot.API.Plugins;
using BrackeysBot.Core.API.Configuration;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using NLog;

namespace BrackeysBot.Core.Services;

/// <summary>
///     Represents a service which writes buffered log events to Discord.
/// </summary>
internal sealed class BufferedLogService : BackgroundService
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly List<DiscordChannel> _bufferedConsoleLogChannels = new();
    private readonly IPluginManager _pluginManager;
    private readonly DiscordClient _discordClient;
    private readonly ConfigurationService _configurationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BufferedLogService" /> class.
    /// </summary>
    public BufferedLogService(IPluginManager pluginManager, DiscordClient discordClient,
        ConfigurationService configurationService)
    {
        _pluginManager = pluginManager;
        _discordClient = discordClient;
        _configurationService = configurationService;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _pluginManager.BotApplication.BufferedLog += OnBufferedLog;
        _discordClient.GuildAvailable += OnGuildAvailable;
        return Task.CompletedTask;
    }

    private async void OnBufferedLog(object? sender, BufferedLogEventArgs e)
    {
        if (_bufferedConsoleLogChannels.Count == 0) return;

        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        foreach (string logEvent in e.LogEvents)
            await writer.WriteLineAsync(logEvent);

        string fileName = $"log-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.log";
        foreach (DiscordChannel logChannel in _bufferedConsoleLogChannels)
        {
            stream.Position = 0;

            var builder = new DiscordMessageBuilder();
            builder.WithFile(fileName, stream);
            await logChannel.SendMessageAsync(builder);
        }
    }

    private Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        GuildConfiguration guildConfiguration = _configurationService.GetGuildConfiguration(e.Guild);
        ulong logChannelId = guildConfiguration.ChannelConfiguration.BufferedConsoleLogChannelId;

        if (logChannelId == 0)
        {
            Logger.Warn($"Buffered log channel not specified in config for {e.Guild} - buffered log will not be sent");
        }
        else
        {
            DiscordChannel? logChannel = e.Guild.GetChannel(logChannelId);
            if (logChannel is null)
            {
                Logger.Warn($"Buffered log channel {logChannelId} was specified for {e.Guild}, " +
                            "but that channel could not be found! Buffered log will not be sent");
            }
            else
            {
                _bufferedConsoleLogChannels.Add(logChannel);
                Logger.Info($"Buffered log channel found for {e.Guild}: {logChannel}");
            }
        }

        return Task.CompletedTask;
    }
}
