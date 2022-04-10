using System.Threading.Tasks;
using BrackeysBot.Core.API;
using BrackeysBot.Core.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace BrackeysBot.Core.Commands;

internal sealed class UserInfoApplicationCommand : ApplicationCommandModule
{
    private readonly UserInfoService _userInfoService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserInfoApplicationCommand" /> class.
    /// </summary>
    /// <param name="userInfoService">The user info service.</param>
    public UserInfoApplicationCommand(UserInfoService userInfoService)
    {
        _userInfoService = userInfoService;
    }

    [ContextMenu(ApplicationCommandType.UserContextMenu, "User Info")]
    public async Task UserInfoAsync(ContextMenuContext context)
    {
        _ = context.DeferAsync(true);

        UserInfoFieldContext fieldContext = _userInfoService.CreateContext(context);
        DiscordEmbed embed = _userInfoService.CreateEmbed(fieldContext);
        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}
