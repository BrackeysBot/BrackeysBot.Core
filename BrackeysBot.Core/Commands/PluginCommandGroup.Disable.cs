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
    [Command("disable")]
    [Description("Disables a plugin.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task DisablePluginCommandAsync(CommandContext context,
        [Description("The name of the plugin to disable.")]
        string name)
    {
        await context.AcknowledgeAsync();

        DiscordEmbedBuilder embed = context.Guild.CreateDefaultEmbed(false);

        try
        {
            IPlugin? plugin = _pluginManager.GetPlugin(name);
            if (plugin is null)
                return;

            _pluginManager.DisablePlugin(plugin);

            embed.WithColor(0x4CAF50);
            embed.WithTitle(EmbedTitles.PluginDisabled);
            embed.WithDescription(string.Format(EmbedMessages.PluginDisabled, plugin.PluginInfo.Name, plugin.PluginInfo.Version));
        }
        catch (Exception exception)
        {
            Logger.Error(exception, string.Format(LoggerMessages.ErrorDisablingPlugin, name));

            embed.WithColor(0xFF0000);
            embed.WithTitle(EmbedTitles.ErrorDisablingPlugin);
            embed.WithDescription(string.Format(EmbedMessages.ErrorDisablingPlugin, exception.GetType(), name));
        }

        await context.RespondAsync(embed);
    }
}
