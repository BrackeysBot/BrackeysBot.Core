using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrackeysBot.API;
using BrackeysBot.API.Extensions;
using BrackeysBot.Core.API.Extensions;
using BrackeysBot.Core.Data;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BrackeysBot.Core.Services;

/// <summary>
///     Represents a service that handles user bookmarks.
/// </summary>
internal sealed class BookmarkService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DiscordClient _discordClient;
    private readonly Dictionary<DiscordUser, List<Bookmark>> _bookmarks = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="BookmarkService" /> class.
    /// </summary>
    public BookmarkService(IServiceScopeFactory scopeFactory, DiscordClient discordClient)
    {
        _scopeFactory = scopeFactory;
        _discordClient = discordClient;
    }

    /// <summary>
    ///     Creates a bookmark message to send to the user who bookmarked it.
    /// </summary>
    /// <param name="bookmark">The bookmark whose message to create.</param>
    /// <returns>The builder necessary to construct the message.</returns>
    public async Task<DiscordMessageBuilder> CreateBookmarkMessageAsync(Bookmark bookmark)
    {
        DiscordGuild guild = await _discordClient.GetGuildAsync(bookmark.GuildId).ConfigureAwait(false);
        DiscordChannel channel = guild.GetChannel(bookmark.ChannelId);
        DiscordMessage message = await channel.GetMessageAsync(bookmark.MessageId).ConfigureAwait(false);

        DiscordEmbedBuilder embed = guild.CreateDefaultEmbed();
        embed.WithColor(0x007EC6);
        embed.WithAuthor(message.Author);
        embed.WithTitle("🔖 Message bookmarked");
        embed.WithDescription($"{MentionUtility.MentionUser(bookmark.UserId)}, you have bookmarked a message in " +
                              $"{message.Channel.Mention}. To remove this bookmark, hit the Delete button.");
        embed.AddField("Author", message.Author.Mention, true);
        embed.AddField("Channel", message.Channel.Mention, true);
        embed.AddField("Message Created", Formatter.Timestamp(message.Timestamp), true);

        var linkEmoji = new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔗"));
        var deleteEmoji = new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑️"));

        var buttons = new List<DiscordComponent>
        {
            new DiscordLinkButtonComponent(message.JumpLink.ToString(), "View", emoji: linkEmoji),
            new DiscordButtonComponent(ButtonStyle.Danger, $"delete-bookmark-{bookmark.Id}", "Delete", emoji: deleteEmoji)
        };

        var builder = new DiscordMessageBuilder();
        builder.AddEmbed(embed);
        builder.AddComponents(buttons);
        return builder;
    }

    /// <summary>
    ///     Creates a new bookmark.
    /// </summary>
    /// <param name="user">The user who bookmarked the message.</param>
    /// <param name="message">The message to bookmark.</param>
    /// <returns>The newly-created bookmark.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="user" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="message" /> is <see langword="null" />.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException"><paramref name="message" /> is in a DM channel.</exception>
    public async Task<Bookmark> CreateBookmarkAsync(DiscordUser user, DiscordMessage message)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        if (message.Channel.Guild is not { } guild)
            throw new InvalidOperationException("Cannot create a bookmark in a DM channel.");

        Bookmark? bookmark;

        await using (AsyncServiceScope scope = _scopeFactory.CreateAsyncScope())
        await using (var context = scope.ServiceProvider.GetRequiredService<CoreContext>())
        {
            bookmark = await context.Bookmarks.FirstOrDefaultAsync(b => b.UserId == user.Id && b.MessageId == message.Id)
                .ConfigureAwait(false);

            if (bookmark is not null)
                return bookmark;

            bookmark = new Bookmark
            {
                UserId = user.Id,
                GuildId = guild.Id,
                ChannelId = message.Channel.Id,
                MessageId = message.Id
            };

            EntityEntry<Bookmark> entry = await context.AddAsync(bookmark);
            bookmark = entry.Entity;

            await context.SaveChangesAsync();
        }

        DiscordMember member = await guild.GetMemberAsync(user.Id).ConfigureAwait(false);
        DiscordMessageBuilder builder = await CreateBookmarkMessageAsync(bookmark).ConfigureAwait(false);
        DiscordMessage privateMessage = await member.SendMessageAsync(builder).ConfigureAwait(false);

        await using (AsyncServiceScope scope = _scopeFactory.CreateAsyncScope())
        await using (var context = scope.ServiceProvider.GetRequiredService<CoreContext>())
        {
            bookmark.PrivateMessageId = privateMessage.Id;
            context.Update(bookmark);
            await context.SaveChangesAsync();
        }

        return bookmark;
    }

    /// <summary>
    ///     Retrieves a bookmark by its ID.
    /// </summary>
    /// <param name="bookmarkId">The ID of the bookmark to retrieve.</param>
    /// <returns>
    ///     The bookmark whose ID is equal to <paramref name="bookmarkId" />, or <see langword="null" /> if no matching bookmark
    ///     was found.
    /// </returns>
    public async Task<Bookmark?> GetBookmarkByIdAsync(Guid bookmarkId)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

        return await context.Bookmarks.FindAsync(bookmarkId);
    }

    /// <summary>
    ///     Removes a bookmark, found by its ID, from the database. If no bookmark with the specified ID is found, nothing
    ///     happens.
    /// </summary>
    /// <param name="bookmarkId">The ID of the bookmark to remove.</param>
    public async Task RemoveBookmarkAsync(Guid bookmarkId)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();
        var bookmark = context.Find<Bookmark>(bookmarkId);
        if (bookmark is null)
            return;

        context.Remove(bookmark);
        await context.SaveChangesAsync().ConfigureAwait(false);

        try
        {
            DiscordGuild guild = await _discordClient.GetGuildAsync(bookmark.GuildId).ConfigureAwait(false);
            DiscordMember member = await guild.GetMemberAsync(bookmark.UserId).ConfigureAwait(false);
            DiscordDmChannel dmChannel = await member.CreateDmChannelAsync().ConfigureAwait(false);
            DiscordMessage message = await dmChannel.GetMessageAsync(bookmark.PrivateMessageId).ConfigureAwait(false);
            await message.DeleteAsync("Bookmark removed").ConfigureAwait(false);
        }
        catch (NotFoundException)
        {
            // can't get DM channel of non-member. ignore
        }
    }

    /// <summary>
    ///     Removes a bookmark from the database.
    /// </summary>
    /// <param name="bookmark">The bookmark to remove.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bookmark" /> is <see langword="null" />.</exception>
    public async Task RemoveBookmarkAsync(Bookmark bookmark)
    {
        ArgumentNullException.ThrowIfNull(bookmark, nameof(bookmark));

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();
        context.Remove(context.Entry(bookmark).Entity);
        await context.SaveChangesAsync().ConfigureAwait(false);

        try
        {
            DiscordGuild guild = await _discordClient.GetGuildAsync(bookmark.GuildId).ConfigureAwait(false);
            DiscordMember member = await guild.GetMemberAsync(bookmark.UserId).ConfigureAwait(false);
            DiscordDmChannel dmChannel = await member.CreateDmChannelAsync().ConfigureAwait(false);
            DiscordMessage message = await dmChannel.GetMessageAsync(bookmark.PrivateMessageId).ConfigureAwait(false);
            await message.DeleteAsync("Bookmark removed").ConfigureAwait(false);
        }
        catch (NotFoundException)
        {
            // can't get DM channel of non-member. ignore
        }
    }

    /// <summary>
    ///     Removes all bookmarks held by a user in every guild.
    /// </summary>
    /// <param name="userId">The ID of the user whose bookmarks to remove.</param>
    public async Task RemoveUserBookmarksAsync(ulong userId)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

        context.RemoveRange(context.Bookmarks.Where(b => b.UserId == userId));
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Removes all bookmarks held by a user in every guild.
    /// </summary>
    /// <param name="user">The user whose bookmarks to remove.</param>
    /// <exception cref="ArgumentNullException"><paramref name="user" /> is <see langword="null" />.</exception>
    public async Task RemoveUserBookmarksAsync(DiscordUser user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        if (user is null) throw new ArgumentNullException(nameof(user));
        await RemoveUserBookmarksAsync(user.Id).ConfigureAwait(false);
    }

    /// <summary>
    ///     Removes all bookmarks held by a user in a specified guild.
    /// </summary>
    /// <param name="userId">The ID of the user whose bookmarks to remove.</param>
    /// <param name="guildId">The ID of the guild whose bookmarks to search.</param>
    public async Task RemoveUserBookmarksAsync(ulong userId, ulong guildId)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

        context.RemoveRange(context.Bookmarks.Where(b => b.UserId == userId && b.GuildId == guildId));
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Removes all bookmarks held by a user in a specified guild.
    /// </summary>
    /// <param name="user">The user whose bookmarks to remove.</param>
    /// <param name="guild">The guild whose bookmarks to search.</param>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="user" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="guild" /> is <see langword="null" />.</para>
    /// </exception>
    public async Task RemoveUserBookmarksAsync(DiscordUser user, DiscordGuild guild)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(guild, nameof(guild));

        await RemoveUserBookmarksAsync(user.Id, guild.Id).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();
        await context.Database.EnsureCreatedAsync(stoppingToken).ConfigureAwait(false);

        foreach (IGrouping<ulong, Bookmark> group in context.Bookmarks.GroupBy(b => b.UserId))
        {
            ulong userId = group.Key;
            DiscordUser bookmarker;

            try
            {
                bookmarker = await _discordClient.GetUserAsync(userId).ConfigureAwait(false);
            }
            catch (NotFoundException)
            {
                await RemoveUserBookmarksAsync(userId).ConfigureAwait(false);
                continue;
            }

            if (!_bookmarks.TryGetValue(bookmarker, out List<Bookmark>? bookmarks))
            {
                bookmarks = new List<Bookmark>();
                _bookmarks.Add(bookmarker, bookmarks);
            }

            bookmarks.AddRange(group);
        }
    }
}
