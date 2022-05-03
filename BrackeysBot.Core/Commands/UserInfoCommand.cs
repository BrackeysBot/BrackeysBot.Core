using System.Threading.Tasks;
using BrackeysBot.Core.API;
using BrackeysBot.Core.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace BrackeysBot.Core.Commands;

/// <summary>
///     Represents a class which implements the <c>userinfo</c> command and <c>User Info</c> context menu.
/// </summary>
internal sealed class UserInfoCommand : ApplicationCommandModule
{
    private readonly UserInfoService _userInfoService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserInfoCommand" /> class.
    /// </summary>
    /// <param name="userInfoService">The user info service.</param>
    public UserInfoCommand(UserInfoService userInfoService)
    {
        _userInfoService = userInfoService;
    }

    [ContextMenu(ApplicationCommandType.UserContextMenu, "User Info")]
    public async Task UserInfoContextMenuAsync(ContextMenuContext context)
    {
        _ = context.DeferAsync(true);

        UserInfoFieldContext fieldContext = await _userInfoService.CreateContextAsync(context);
        DiscordEmbed embed = _userInfoService.CreateEmbed(fieldContext);
        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }

    [SlashCommand("userinfo", "Displays information about a user.")]
    public async Task UserInfoSlashCommandAsync(InteractionContext context,
        [Option("user", "The user whose information to retrieve.")]
        DiscordUser user)
    {
        _ = context.DeferAsync(true);

        UserInfoFieldContext fieldContext = await _userInfoService.CreateContextAsync(context, user);
        DiscordEmbed embed = _userInfoService.CreateEmbed(fieldContext);
        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}
