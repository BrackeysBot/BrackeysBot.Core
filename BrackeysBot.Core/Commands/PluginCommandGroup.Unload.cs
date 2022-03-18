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
    [Command("unload")]
    [Description("Unloads a plugin.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task UnloadPluginCommandAsync(CommandContext context,
        [Description("The name of the plugin to unload.")]
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
                embed.WithDescription(EmbedMessages.CantUnloadCorePlugin);
            }
            else
            {
                PluginInfo info = plugin.PluginInfo;
                _pluginManager.UnloadPlugin(plugin);

                embed.WithColor(0x4CAF50);
                embed.WithTitle(EmbedTitles.PluginUnloaded);
                embed.WithDescription(string.Format(EmbedMessages.PluginUnloaded, info.Name, info.Version));
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
