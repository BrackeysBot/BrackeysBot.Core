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

internal sealed partial class PluginCommand
{
    [Command("load")]
    [Description("Loads a plugin.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task LoadPluginCommandAsync(CommandContext context,
        [Description("The name of the plugin to load.")]
        string name)
    {
        await context.AcknowledgeAsync();

        DiscordEmbedBuilder embed = context.Guild.CreateDefaultEmbed();

        try
        {
            IPlugin plugin = _pluginManager.LoadPlugin(name);

            embed.WithColor(0xFF0000);
            embed.WithTitle(EmbedTitles.PluginLoaded);
            embed.WithDescription(string.Format(EmbedMessages.PluginLoaded, plugin.PluginInfo.Name, plugin.PluginInfo.Version));
        }
        catch (Exception exception)
        {
            Logger.Error(exception, string.Format(LoggerMessages.ErrorLoadingPlugin, name));

            embed.WithColor(0xFF0000);
            embed.WithTitle(EmbedTitles.ErrorLoadingPlugin);
            embed.WithDescription(string.Format(EmbedMessages.ErrorLoadingPlugin, exception.GetType(), name));
        }

        await context.RespondAsync(embed);
    }
}
