using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.API.Extensions;
using BrackeysBot.Core.API;
using BrackeysBot.Core.Data;
using BrackeysBot.Core.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using NLog;

namespace BrackeysBot.Core.Commands;

/// <summary>
///     Represents a class which implements the <c>userinfo</c> command.
/// </summary>
internal sealed class UserInfoCommand : BaseCommandModule
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly UserInfoService _userInfoService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserInfoCommand" /> class.
    /// </summary>
    /// <param name="userInfoService">The user info service.</param>
    public UserInfoCommand(UserInfoService userInfoService)
    {
        _userInfoService = userInfoService;
    }

    [Command("userinfo")]
    [Description("Displays information about a user.")]
    [RequireGuild]
    public async Task UserInfoAsync(CommandContext context,
        [Description("The ID of the user whose information to retrieve.")]
        ulong userId)
    {
        _ = context.AcknowledgeAsync();

        DiscordUser user;
        try
        {
            user = await context.Client.GetUserAsync(userId);
        }
        catch (NotFoundException)
        {
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("⚠️ No such user");
            embed.WithDescription($"No user with the ID {userId} could be found.");
            _ = context.RespondAsync(embed);

            Logger.Info($"{context.Member} attempted to retrieve information about non-existent user {userId}");
            return;
        }

        await UserInfoAsync(context, user);
    }

    [Command("userinfo")]
    [Description("Displays information about a user.")]
    [RequireGuild]
    public async Task UserInfoAsync(CommandContext context,
        [Description("The user whose information to retrieve.")]
        DiscordUser user)
    {
        _ = context.AcknowledgeAsync();

        UserInfoFieldContext fieldContext = await _userInfoService.CreateContextAsync(context, user);
        DiscordEmbed embed = _userInfoService.CreateEmbed(fieldContext);
        await context.RespondAsync(embed);
    }
}
