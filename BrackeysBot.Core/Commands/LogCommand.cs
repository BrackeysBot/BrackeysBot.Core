using System.IO;
using System.Threading.Tasks;
using BrackeysBot.API.Extensions;
using BrackeysBot.Core.API;
using BrackeysBot.Core.API.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace BrackeysBot.Core.Commands;

/// <summary>
///     Represents a class which implements the <c>log</c> command.
/// </summary>
internal sealed class LogCommand : BaseCommandModule
{
    private const string LogsDirectory = "logs";
    private const string LatestLogFile = "latest.log";

    [Command("log")]
    [Description("Attaches the latest logfile.")]
    [RequirePermissionLevel(PermissionLevel.Administrator)]
    public async Task LogCommandAsync(CommandContext context)
    {
        _ = context.AcknowledgeAsync();

        FileStream stream = File.OpenRead(Path.Combine(LogsDirectory, LatestLogFile));
        await context.RespondAsync(builder => builder.WithFile(stream));
        await stream.DisposeAsync();
    }
}
