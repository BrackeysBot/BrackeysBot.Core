using System.Text;
using System.Threading.Tasks;
using BrackeysBot.API.Extensions;
using BrackeysBot.API.Plugins;
using BrackeysBot.Core.API.Attributes;
using BrackeysBot.Core.API.Extensions;
using BrackeysBot.Core.Resources;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PermissionLevel = BrackeysBot.Core.API.PermissionLevel;

namespace BrackeysBot.Core.Commands;

internal sealed partial class PluginCommandGroup
{
    [Command("list")]
    [Description("Lists all currently loaded plugins.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task ListPluginsCommandAsync(CommandContext context)
    {
        await context.AcknowledgeAsync();

        DiscordEmbedBuilder embed = context.Guild.CreateDefaultEmbed(false);
        embed.WithTitle(EmbedTitles.LoadedPlugins);

        var builder = new StringBuilder();
        foreach (IPlugin plugin in _pluginManager.LoadedPlugins)
        {
            PluginInfo info = plugin.PluginInfo;
            bool enabled = _pluginManager.IsPluginEnabled(plugin);

            builder.Append(enabled ? "✅" : "❌").Append(' ');
            builder.AppendLine($"{info.Name} ({info.Version})");
        }

        embed.WithDescription(Formatter.BlockCode(builder.ToString()));

        await context.RespondAsync(embed);
    }
}
