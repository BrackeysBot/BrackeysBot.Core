using System.Text.Json.Serialization;

namespace BrackeysBot.Core.API.Configuration;

/// <summary>
///     Represents a guild configuration.
/// </summary>
public sealed class GuildConfiguration
{
    /// <summary>
    ///     Gets or sets the emoji for which to listen when users react to bookmark a message.
    /// </summary>
    /// <value>The bookmark reaction for which to listen.</value>
    [JsonPropertyName("bookmarkEmoji")]
    public string BookmarkEmoji { get; set; } = ":bookmark:";

    /// <summary>
    ///     Gets or sets the channel configuration.
    /// </summary>
    /// <value>The channel configuration.</value>
    [JsonPropertyName("channels")]
    public ChannelConfiguration ChannelConfiguration { get; set; } = new();

    /// <summary>
    ///     Gets or sets the response to give when the <c>clear</c> command has been used.
    /// </summary>
    /// <value>The <c>clear</c> response.</value>
    [JsonPropertyName("clearMessageResponse")]
    public string? ClearMessageResponse { get; set; } = "Please no spam! :slight_smile:";

    /// <summary>
    ///     Gets or sets the role configuration.
    /// </summary>
    /// <value>The role configuration.</value>
    [JsonPropertyName("roles")]
    public RoleConfiguration RoleConfiguration { get; set; } = new();

    /// <summary>
    ///     Gets or sets the guild's primary color.
    /// </summary>
    /// <value>The guild's primary color, in 24-bit RGB format.</value>
    [JsonPropertyName("primaryColor")]
    public int PrimaryColor { get; set; } = 0x7837FF;

    /// <summary>
    ///     Gets or sets the guild's secondary color.
    /// </summary>
    /// <value>The guild's secondary color, in 24-bit RGB format.</value>
    [JsonPropertyName("secondaryColor")]
    public int SecondaryColor { get; set; } = 0xE33C6C;

    /// <summary>
    ///     Gets or sets the guild's tertiary color.
    /// </summary>
    /// <value>The guild's tertiary color, in 24-bit RGB format.</value>
    [JsonPropertyName("tertiaryColor")]
    public int TertiaryColor { get; set; } = 0xFFE056;
}
