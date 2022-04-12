using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace BrackeysBot.Core.API.Attributes;

/// <summary>
///     Defines that usage of this command must require a specified minimum <see cref="PermissionLevel" />.
/// </summary>
public sealed class SlashRequirePermissionLevelAttribute : SlashCheckBaseAttribute
{
    private readonly PermissionLevel _permissionLevel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequirePermissionLevelAttribute" /> class.
    /// </summary>
    /// <param name="permissionLevel">The minimum permission level.</param>
    public SlashRequirePermissionLevelAttribute(PermissionLevel permissionLevel)
    {
        _permissionLevel = permissionLevel;
    }

    /// <inheritdoc />
    public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        if (ctx.Guild is null) return Task.FromResult(false);

        var corePlugin = ctx.Services.GetRequiredService<ICorePlugin>();
        return Task.FromResult(corePlugin.GetPermissionLevel(ctx.User, ctx.Guild) >= _permissionLevel);
    }
}
