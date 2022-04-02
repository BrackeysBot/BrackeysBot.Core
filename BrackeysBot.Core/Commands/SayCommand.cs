using System.Threading.Tasks;
using BrackeysBot.API.Extensions;
using BrackeysBot.Core.API;
using BrackeysBot.Core.API.Attributes;
using BrackeysBot.Core.API.Extensions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace BrackeysBot.Core.Commands;

internal sealed class SayCommand : BaseCommandModule
{
    [Command("say")]
    [Aliases("echo")]
    [Description("Echoes a message back in a specified channel.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task SayCommandAsync(CommandContext context,
        [Description("The channel in which the message should be sent.")]
        DiscordChannel channel,
        [Description("The message to echo.")] [RemainingText]
        string message)
    {
        await context.AcknowledgeAsync();
        await channel.SendMessageAsync(message);
    }

    [Command("sayembed")]
    [Aliases("echoembed")]
    [Description("Echoes a message back in a specified channel, wrapping the content in an embed.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task SayEmbedCommandAsync(CommandContext context,
        [Description("The channel in which the message should be sent.")]
        DiscordChannel channel,
        [Description("The message to echo.")] [RemainingText]
        string message)
    {
        await context.AcknowledgeAsync();
        await channel.SendMessageAsync(context.Guild.CreateDefaultEmbed(false).WithDescription(message));
    }
}
