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
    [Command("enable")]
    [Description("Enables a plugin.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task EnablePluginCommandAsync(CommandContext context,
        [Description("The name of the plugin to enable.")]
        string name)
    {
        await context.AcknowledgeAsync();

        DiscordEmbedBuilder embed = context.Guild.CreateDefaultEmbed(false);

        try
        {
            IPlugin? plugin = _pluginManager.GetPlugin(name);
            if (plugin is null)
                return;

            _pluginManager.EnablePlugin(plugin);

            embed.WithColor(0x4CAF50);
            embed.WithTitle(EmbedTitles.PluginEnabled);
            embed.WithDescription(string.Format(EmbedMessages.PluginEnabled, plugin.PluginInfo.Name, plugin.PluginInfo.Version));
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
