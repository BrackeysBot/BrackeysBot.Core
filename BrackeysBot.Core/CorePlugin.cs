using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.API.Plugins;
using BrackeysBot.Core.API;
using BrackeysBot.Core.API.Configuration;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
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
    private readonly Dictionary<DiscordGuild, GuildConfiguration> _guildConfigurations = new();

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

        if ((member.Permissions & Permissions.Administrator) != 0) return PermissionLevel.Administrator;

        RoleConfiguration roleConfiguration = configuration.RoleConfiguration;
        List<ulong> roles = member.Roles.Select(r => r.Id).ToList();

        if (roles.Contains(roleConfiguration.AdministratorRoleId)) return PermissionLevel.Administrator;
        if (roles.Contains(roleConfiguration.ModeratorRoleId)) return PermissionLevel.Moderator;
        if (roles.Contains(roleConfiguration.GuruRoleId)) return PermissionLevel.Guru;

        return PermissionLevel.Default;
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
        return _guildConfigurations.TryGetValue(guild, out configuration);
    }

    /// <inheritdoc />
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICorePlugin>(this);
    }

    /// <inheritdoc />
    protected override Task OnLoad()
    {
        if (DiscordClient is not null)
            DiscordClient.GuildAvailable += DiscordClientOnGuildAvailable;

        return Task.CompletedTask;
    }

    private Task DiscordClientOnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        var guildConfiguration = Configuration.Get<GuildConfiguration>($"guilds.{e.Guild.Id}");
        guildConfiguration ??= new GuildConfiguration();
        _guildConfigurations[e.Guild] = guildConfiguration;
        return Task.CompletedTask;
    }
}
