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
    [Command("load")]
    [Description("Loads a plugin.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task LoadPluginCommandAsync(CommandContext context,
        [Description("The name of the plugin to load.")]
        string name)
    {
        await context.AcknowledgeAsync();

        DiscordEmbedBuilder embed = context.Guild.CreateDefaultEmbed(false);
        
        IPlugin? plugin;

        try
        {
            plugin = _pluginManager.LoadPlugin(name);
        }
        catch (Exception exception)
        {
            Logger.Error(exception, string.Format(LoggerMessages.ErrorLoadingPlugin, name));

            embed.WithColor(0xFF0000);
            embed.WithTitle(EmbedTitles.ErrorLoadingPlugin);
            embed.WithDescription(string.Format(EmbedMessages.ErrorLoadingPlugin, exception.GetType(), name));

            plugin = null;
        }

        if (plugin is not null)
        {
            try
            {
                _pluginManager.EnablePlugin(plugin);
                PluginInfo info = plugin.PluginInfo;

                embed.WithColor(0x4CAF50);
                embed.WithTitle(EmbedTitles.PluginLoaded);
                embed.WithDescription(string.Format(EmbedMessages.PluginLoaded, info.Name, info.Version));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, string.Format(LoggerMessages.ErrorEnablingPlugin, name));

                embed.WithColor(0xFF0000);
                embed.WithTitle(EmbedTitles.ErrorEnablingPlugin);
                embed.WithDescription(string.Format(EmbedMessages.ErrorEnablingPlugin, exception.GetType(), name));
            }
        }

        await context.RespondAsync(embed);
    }
}
