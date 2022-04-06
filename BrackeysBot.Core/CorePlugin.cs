using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.API.Extensions;
using BrackeysBot.API.Plugins;
using BrackeysBot.Core.API;
using BrackeysBot.Core.API.Configuration;
using BrackeysBot.Core.API.Extensions;
using BrackeysBot.Core.Commands;
using BrackeysBot.Core.Services;
using DSharpPlus;
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
    private UserInfoService _userInfoService = null!;

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
    public void RegisterUserInfoField(Action<UserInfoFieldBuilder> builderEvaluator)
    {
        if (builderEvaluator is null) throw new ArgumentNullException(nameof(builderEvaluator));
        var builder = new UserInfoFieldBuilder();
        builderEvaluator(builder);
        RegisterUserInfoField(builder);
    }

    /// <inheritdoc />
    public void RegisterUserInfoField(UserInfoFieldBuilder builder)
    {
        _userInfoService.RegisterField(builder);
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
        services.AddSingleton<UserInfoService>();

        services.AddHostedSingleton<DiscordLogService>();
    }

    /// <inheritdoc />
    protected override Task OnLoad()
    {
        Trace.Assert(DiscordClient is not null);
        Debug.Assert(DiscordClient is not null);

        _configurationService = ServiceProvider.GetRequiredService<ConfigurationService>();
        _discordLogService = ServiceProvider.GetRequiredService<DiscordLogService>();
        _userInfoService = ServiceProvider.GetRequiredService<UserInfoService>();

        RegisterUserInfoFields();

        Logger.Info("Registering command modules");
        CommandsNextExtension commandsNext = DiscordClient.GetCommandsNext();
        commandsNext.RegisterCommands<PluginCommandGroup>();
        commandsNext.RegisterCommands<SayCommand>();
        commandsNext.RegisterCommands<UserInfoCommand>();

        return base.OnLoad();
    }

    private void RegisterUserInfoFields()
    {
        RegisterUserInfoField(builder =>
        {
            builder.WithName("Username");
            builder.WithValue(context =>
            {
                string username = context.TargetUser.GetUsernameWithDiscriminator();
                return context.TargetUser.IsBot ? $"🤖 {username}" : username;
            });
        });

        RegisterUserInfoField(builder =>
        {
            builder.WithName("User ID");
            builder.WithValue(context => context.TargetUser.Id);
        });

        RegisterUserInfoField(builder =>
        {
            builder.WithName("User Created");
            builder.WithValue(context => Formatter.Timestamp(context.TargetUser.CreationTimestamp));
        });

        RegisterUserInfoField(builder =>
        {
            builder.WithName("Nickname");
            builder.WithValue(context => context.TargetMember!.Nickname);
            builder.WithExecutionFilter(context => !string.IsNullOrWhiteSpace(context.TargetMember?.Nickname));
        });

        RegisterUserInfoField(builder =>
        {
            builder.WithName("Join Date");
            builder.WithValue(context => Formatter.Timestamp(context.TargetMember!.JoinedAt));
            builder.WithExecutionFilter(context => context.TargetMember is not null);
        });

        RegisterUserInfoField(builder =>
        {
            builder.WithName("Permission Level");
            builder.WithValue(context => context.TargetMember!.GetPermissionLevel(context.Guild!).ToString("G"));
            builder.WithExecutionFilter(context => context.TargetMember is not null);
        });
    }
}
