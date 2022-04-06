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

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(user);
        embed.WithTitle($"Information about {user.GetUsernameWithDiscriminator()}");
        embed.WithThumbnail(user.GetAvatarUrl(ImageFormat.Png));

        bool isMember = await user.IsMemberOfAsync(context.Guild);
        DiscordMember? member = null;

        if (isMember)
        {
            embed.WithColor(member!.Color);
            member = await user.GetAsMemberAsync(context.Guild);
        }
        else
        {
            embed.WithColor(DiscordColor.Gray);
            embed.WithFooter("⚠️ This user is not currently in the server!");
        }

        var fieldContext = new UserInfoFieldContext
        {
            Channel = context.Channel,
            Guild = context.Guild,
            User = context.User,
            Member = context.Member,
            TargetMember = member,
            TargetUser = user
        };

        foreach (UserInfoField field in _userInfoService.RegisteredFields.Where(f => f.FilterPredicate(fieldContext)))
            embed.AddField(field.Name, field.ValueEvaluator(fieldContext), true);

        await context.RespondAsync(embed);
    }
}
