using System;
using System.Threading.Tasks;
using BrackeysBot.API.Extensions;
using BrackeysBot.API.Plugins;
using BrackeysBot.Core.API;
using BrackeysBot.Core.API.Attributes;
using BrackeysBot.Core.API.Extensions;
using BrackeysBot.Core.Resources;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace BrackeysBot.Core.Commands;

internal sealed partial class PluginCommandGroup
{
    [Command("disable")]
    [Description("Disables a plugin.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task DisablePluginCommandAsync(CommandContext context,
        [Description("The name of the plugin to disable.")]
        string name)
    {
        await context.AcknowledgeAsync();

        DiscordEmbedBuilder embed = context.Guild.CreateDefaultEmbed(false);


        if (_pluginManager.TryGetPlugin(name, out IPlugin? plugin))
        {
            if (plugin is CorePlugin)
            {
                embed.WithColor(0xFF0000);
                embed.WithTitle(EmbedTitles.InvalidPlugin);
                embed.WithDescription(EmbedMessages.CantDisableCorePlugin);
            }
            else
            {
                PluginInfo info = plugin.PluginInfo;
                try
                {
                    _pluginManager.DisablePlugin(plugin);
                    embed.WithColor(0x4CAF50);
                    embed.WithTitle(EmbedTitles.PluginDisabled);
                    embed.WithDescription(string.Format(EmbedMessages.PluginDisabled, info.Name, info.Version));
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, string.Format(LoggerMessages.ErrorDisablingPlugin, name));

                    embed.WithColor(0xFF0000);
                    embed.WithTitle(EmbedTitles.ErrorDisablingPlugin);
                    embed.WithDescription(string.Format(EmbedMessages.ErrorDisablingPlugin, exception.GetType(), name));
                }
            }
        }
        else
        {
            embed.WithColor(0xFF0000);
            embed.WithTitle(EmbedTitles.InvalidPlugin);
            embed.WithDescription(string.Format(EmbedMessages.PluginNotLoaded, name));
        }

        await context.RespondAsync(embed);
    }
}
