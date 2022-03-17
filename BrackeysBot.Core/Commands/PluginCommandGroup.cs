using BrackeysBot.API.Plugins;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using NLog;

namespace BrackeysBot.Core.Commands;

/// <summary>
///     Represents a class which implements the <c>plugin</c> command.
/// </summary>
[Group("plugin")]
internal sealed partial class PluginCommand : BaseCommandModule
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly IPluginManager _pluginManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginCommand" /> class.
    /// </summary>
    /// <param name="pluginManager">The plugin manager.</param>
    public PluginCommand(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }
}
