using BrackeysBot.API.Plugins;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using NLog;

namespace BrackeysBot.Core.Commands;

/// <summary>
///     Represents a class which implements the <c>plugin</c> command.
/// </summary>
[Group("plugin")]
internal sealed partial class PluginCommandGroup : BaseCommandModule
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly IPluginManager _pluginManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginCommandGroup" /> class.
    /// </summary>
    /// <param name="pluginManager">The plugin manager.</param>
    public PluginCommandGroup(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }
}
