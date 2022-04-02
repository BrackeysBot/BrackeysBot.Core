using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.API.Plugins;
using BrackeysBot.Core.API;
using BrackeysBot.Core.API.Configuration;
using BrackeysBot.Core.Commands;
using BrackeysBot.Core.Services;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using PermissionLevel = BrackeysBot.Core.API.PermissionLevel;

namespace BrackeysBot.Core;

/// <summary>
///     Represents the core plugin for BrackeysBot.
/// </summary>
[Plugin("BrackeysBot.Core", "1.0.0")]
[PluginDescription("The core plugin for BrackeysBot.")]
internal sealed class CorePlugin : MonoPlugin, ICorePlugin
{
    private ConfigurationService _configurationService = null!;
    private DiscordLogService _discordLogService = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CorePlugin" /> class.
    /// </summary>
    public CorePlugin()
    {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        ICorePlugin.Current ??= this;
    }

    /// <inheritdoc />
    public PermissionLevel GetPermissionLevel(DiscordUser user, DiscordGuild guild)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        if (guild is null) throw new ArgumentNullException(nameof(guild));

        if (!TryGetGuildConfiguration(guild, out GuildConfiguration? configuration))
            return PermissionLevel.Default;

        if (!guild.Members.TryGetValue(user.Id, out DiscordMember? member))
            return PermissionLevel.Default;

        if ((member.Permissions & DSharpPlus.Permissions.Administrator) != 0)
            return PermissionLevel.Administrator;

        RoleConfiguration roleConfiguration = configuration.RoleConfiguration;
        List<ulong> roles = member.Roles.Select(r => r.Id).ToList();

        if (roles.Contains(roleConfiguration.AdministratorRoleId)) return PermissionLevel.Administrator;
        if (roles.Contains(roleConfiguration.ModeratorRoleId)) return PermissionLevel.Moderator;
        if (roles.Contains(roleConfiguration.GuruRoleId)) return PermissionLevel.Guru;

        return PermissionLevel.Default;
    }

    /// <inheritdoc />
    public Task LogAsync(DiscordGuild guild, DiscordEmbed embed,
        StaffNotificationOptions notificationOptions = StaffNotificationOptions.None)
    {
        return _discordLogService.LogAsync(guild, embed, notificationOptions);
    }

    /// <inheritdoc />
    public bool IsHigherLevelThan(DiscordUser user, DiscordUser other, DiscordGuild guild)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        if (other is null) throw new ArgumentNullException(nameof(other));
        if (guild is null) throw new ArgumentNullException(nameof(guild));

        return GetPermissionLevel(user, guild) > GetPermissionLevel(other, guild);
    }

    /// <inheritdoc />
    public bool IsStaffMember(DiscordUser user, DiscordGuild guild)
    {
        return GetPermissionLevel(user, guild) >= PermissionLevel.Moderator;
    }

    /// <inheritdoc />
    public bool TryGetGuildConfiguration(DiscordGuild guild, [NotNullWhen(true)] out GuildConfiguration? configuration)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_configurationService is null)
        {
            configuration = null;
            return false;
        }

        configuration = _configurationService.GetGuildConfiguration(guild);
        return true;
    }

    /// <inheritdoc />
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICorePlugin>(this);
        services.AddSingleton<ConfigurationService>();
        services.AddSingleton<DiscordLogService>();
    }

    /// <inheritdoc />
    protected override Task OnLoad()
    {
        _configurationService = ServiceProvider.GetRequiredService<ConfigurationService>();
        _discordLogService = ServiceProvider.GetRequiredService<DiscordLogService>();

        Logger.Info("Registering command modules");
        CommandsNextExtension commandsNext = DiscordClient.GetCommandsNext();
        commandsNext.RegisterCommands<PluginCommandGroup>();
        commandsNext.RegisterCommands<SayCommand>();

        return base.OnLoad();
    }
}
