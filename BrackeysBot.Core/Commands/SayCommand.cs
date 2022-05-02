using System.Threading.Tasks;
using BrackeysBot.API.Extensions;
using BrackeysBot.Core.API;
using BrackeysBot.Core.API.Attributes;
using BrackeysBot.Core.API.Extensions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace BrackeysBot.Core.Commands;

internal sealed class SayCommand : ApplicationCommandModule
{
    [SlashCommand("say", "Echoes a message to a specified channel.", false)]
    [SlashRequireGuild]
    public async Task SaySlashCommandAsync(InteractionContext context,
        [Option("channel", "The channel to send the message to.")] DiscordChannel channel,
        [Option("message", "The message to send.")] string message,
        [Option("replyTo", "The message to which to reply, if any.")] string? replyTo = null)
    {
        var builder = new DiscordMessageBuilder();
        builder.WithContent(message);
        if (replyTo is not null && ulong.TryParse(replyTo, out ulong messageId))
            builder.WithReply(messageId);

        await channel.SendMessageAsync(builder).ConfigureAwait(false);
        await context.CreateResponseAsync("Message sent", true).ConfigureAwait(false);
    }
}
