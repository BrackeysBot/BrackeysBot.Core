using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.API.Extensions;
using BrackeysBot.Core.Services;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using X10D.Time;

namespace BrackeysBot.Core.Commands;

/// <summary>
///     Represents a class which implements the <c>clear</c> command.
/// </summary>
internal sealed class ClearCommand : ApplicationCommandModule
{
    private readonly ConfigurationService _configurationService;
    private readonly DiscordLogService _logService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClearCommand" /> class.
    /// </summary>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="logService">The log service.</param>
    public ClearCommand(ConfigurationService configurationService, DiscordLogService logService)
    {
        _configurationService = configurationService;
        _logService = logService;
    }

    [SlashCommand("clear", "Clears all messages which meet a specified criteria.", false)]
    [SlashRequireGuild]
    public async Task ClearSlashCommandAsync(InteractionContext context,
        [Option("count", "The number of messages to clear."), Minimum(2), Maximum(100)] long count,
        [Option("channel", "The channel from which to clear messages.")] DiscordChannel? channel = null,
        [Option("user", "The user whose messages to clear.")] DiscordUser? user = null)
    {
        if (count < 2)
        {
            await context.CreateResponseAsync("You must specify a number greater than 1.", true);
            return;
        }

        await context.DeferAsync(true);
        count = Math.Clamp(count, 2, 100);
        channel ??= context.Channel;

        IEnumerable<DiscordMessage> messages = await channel.GetMessagesAsync().ConfigureAwait(false);
        messages = messages.Where(m => m.Timestamp >= 14.Days().Ago());

        if (user != null)
            messages = messages.Where(m => m.Author == user);

        DiscordMessage[] filteredMessages = messages.Take((int) count).ToArray();
        var builder = new DiscordWebhookBuilder();
        if (filteredMessages.Length == 0)
        {
            builder.WithContent("No messages were found that match the specified criteria.");
            await context.EditResponseAsync(builder).ConfigureAwait(false);
            return;
        }

        await channel.DeleteMessagesAsync(filteredMessages).ConfigureAwait(false);

        builder.WithContent($"Cleared {filteredMessages.Length} messages.");
        await context.EditResponseAsync(builder).ConfigureAwait(false);

        if (user is null)
        {
            string? response = _configurationService.GetGuildConfiguration(context.Guild).ClearMessageResponse;
            if (!string.IsNullOrWhiteSpace(response))
                await channel.SendMessageAsync(response).ConfigureAwait(false);
        }

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(DiscordColor.Orange);
        embed.WithTitle("Cleared Messages");
        embed.WithDescription($"{context.User.Mention} cleared {filteredMessages.Length} messages.");
        embed.AddField("Channel", channel.Mention, true);
        embed.AddField("Count", filteredMessages.Length, true);
        embed.AddField("Moderator", context.User.Mention, true);
        embed.AddFieldIf(user is not null, "Author", () => user!.Mention, true);
        await _logService.LogAsync(context.Guild, embed).ConfigureAwait(false);
    }
}
