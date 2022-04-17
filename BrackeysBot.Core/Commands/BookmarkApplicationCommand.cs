using System;
using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.Core.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using NLog;

namespace BrackeysBot.Core.Commands;

internal sealed class BookmarkApplicationCommand : ApplicationCommandModule
{
    private readonly ILogger<BookmarkApplicationCommand> _logger;
    private readonly BookmarkService _bookmarkService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BookmarkApplicationCommand" /> class.
    /// </summary>
    public BookmarkApplicationCommand(ILogger<BookmarkApplicationCommand> logger, BookmarkService bookmarkService)
    {
        _logger = logger;
        _bookmarkService = bookmarkService;
    }

    [ContextMenu(ApplicationCommandType.MessageContextMenu, "🔖 Bookmark")]
    public async Task BookmarkMessage(ContextMenuContext context)
    {
        if (context.Member is not { } member)
            return;

        var builder = new DiscordWebhookBuilder();
        await context.DeferAsync(true).ConfigureAwait(false);

        try
        {
            await member.CreateDmChannelAsync();
        }
        catch
        {
            builder.WithContent("Sorry, I could not bookmark this message. Please ensure you have DMs enabled.");
            await context.EditResponseAsync(builder).ConfigureAwait(false);
            _logger.LogWarning("Could not bookmark message for {Member} - they do not have DMs enabled", member);
            return;
        }

        try
        {
            DiscordMessage message = context.Interaction.Data.Resolved.Messages.First().Value;
            await _bookmarkService.CreateBookmarkAsync(member, message).ConfigureAwait(false);

            builder.WithContent("Message bookmarked! Check your DMs for details.");
            await context.EditResponseAsync(builder).ConfigureAwait(false);

            _logger.LogInformation("{Member} bookmarked {Message}", member, message);
        }
        catch (Exception exception)
        {
            builder.WithContent("Sorry, I could not bookmark this message due to an unexpected error. " +
                                "If this persists, contact staff for assistance.");
            await context.EditResponseAsync(builder).ConfigureAwait(false);
            _logger.LogError(exception, "Could not bookmark message: {Message}", exception.Message);
        }
    }
}
