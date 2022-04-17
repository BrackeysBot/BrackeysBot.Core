using System;
using System.Threading;
using System.Threading.Tasks;
using BrackeysBot.Core.Data;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BrackeysBot.Core.Services;

/// <summary>
///     Represents a service that listens for emoji reactions and component interactions.
/// </summary>
internal sealed class UserReactionService : BackgroundService
{
    private readonly ILogger<UserReactionService> _logger;
    private readonly DiscordClient _discordClient;
    private readonly BookmarkService _bookmarkService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserReactionService" /> class.
    /// </summary>
    public UserReactionService(ILogger<UserReactionService> logger, DiscordClient discordClient, BookmarkService bookmarkService)
    {
        _logger = logger;
        _discordClient = discordClient;
        _bookmarkService = bookmarkService;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordClient.MessageReactionAdded += OnMessageReactionAdded;
        _discordClient.ComponentInteractionCreated += OnInteractionCreated;
        return Task.CompletedTask;
    }

    private async Task OnInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        DiscordInteractionData data = e.Interaction.Data;
        DiscordMessage? message = e.Message;

        if (message?.Author != _discordClient.CurrentUser) return;
        if (!data.CustomId.StartsWith("delete-bookmark-")) return;

        string id = data.CustomId[16..]; // "delete-bookmark-".Length
        if (!Guid.TryParse(id, out Guid bookmarkId)) return;

        Bookmark? bookmark = await _bookmarkService.GetBookmarkByIdAsync(bookmarkId);
        if (bookmark?.UserId != e.Interaction.User.Id) return;

        try
        {
            await _bookmarkService.RemoveBookmarkAsync(bookmark);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not remove bookmark: {Message}", exception.Message);

            var builder = new DiscordInteractionResponseBuilder();
            builder.WithContent("An error occurred while deleting the bookmark. " +
                                "If this problem persists, please contact the server staff.");

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder)
                .ConfigureAwait(false);
        }
    }

    private async Task OnMessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        if (e.Guild is null)
            return;

        if (e.Emoji.GetDiscordName() != ":bookmark:")
            return;

        await e.Message.DeleteReactionAsync(e.Emoji, e.User).ConfigureAwait(false);
        await _bookmarkService.CreateBookmarkAsync(e.User, e.Message).ConfigureAwait(false);
    }
}
