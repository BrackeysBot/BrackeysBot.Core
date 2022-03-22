using System;
using System.Threading.Tasks;
using BrackeysBot.API.Extensions;
using BrackeysBot.API.Plugins;
using BrackeysBot.Core.API;
using BrackeysBot.Core.API.Attributes;
using BrackeysBot.Core.API.Extensions;
using BrackeysBot.Core.Resources;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;

namespace BrackeysBot.Core.Commands;

internal sealed partial class PluginCommandGroup
{
    [Command("reload")]
    [Description("Reloads a plugin.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task ReloadPluginCommandAsync(CommandContext context,
        [Description("The name of the plugin to reload.")]
        string name)
    {
        await context.AcknowledgeAsync();

        DiscordEmbedBuilder embed = context.Guild.CreateDefaultEmbed(false);

        IPlugin? plugin;
        try
        {
            plugin = _pluginManager.GetPlugin(name);
            if (plugin is null)
                return;

            PluginInfo info = plugin.PluginInfo;

            _pluginManager.DisablePlugin(plugin);
            _pluginManager.UnloadPlugin(plugin);

            embed.WithColor(0x4CAF50);
            embed.WithTitle(EmbedTitles.PluginEnabled);
            embed.WithDescription(string.Format(EmbedMessages.PluginUnloaded, info.Name, info.Version));
        }
        catch (Exception exception)
        {
            Logger.Error(exception, string.Format(LoggerMessages.ErrorDisablingPlugin, name));

            embed.WithColor(0xFF0000);
            embed.WithTitle(EmbedTitles.ErrorEnablingPlugin);
            embed.WithDescription(string.Format(EmbedMessages.ErrorDisablingPlugin, exception.GetType(), name));

            await context.RespondAsync(embed);
            return;
        }

        try
        {
            _pluginManager.UnloadPlugin(plugin);
            plugin = _pluginManager.LoadPlugin(name);
            _pluginManager.EnablePlugin(plugin);

            PluginInfo info = plugin.PluginInfo;

            embed.WithColor(0x4CAF50);
            embed.WithTitle(EmbedTitles.PluginEnabled);
            embed.WithDescription(string.Format(EmbedMessages.PluginReloaded, info.Name, info.Version));
        }
        catch (Exception exception)
        {
            Logger.Error(exception, string.Format(LoggerMessages.ErrorEnablingPlugin, name));

            embed.WithColor(0xFF0000);
            embed.WithTitle(EmbedTitles.ErrorEnablingPlugin);
            embed.WithDescription(string.Format(EmbedMessages.ErrorEnablingPlugin, exception.GetType(), name));
        }

        await context.RespondAsync(embed);
    }
}
