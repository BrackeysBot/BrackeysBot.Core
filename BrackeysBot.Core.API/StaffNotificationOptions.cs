using System;

namespace BrackeysBot.Core.API;

/// <summary>
///     An enumeration of staff notification options.
/// </summary>
[Flags]
public enum StaffNotificationOptions
{
    /// <summary>
    ///     Indicates that no staff member shall be notified.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Indicates that Moderators should be notified.
    /// </summary>
    Moderator = 1,

    /// <summary>
    ///     Indicates that Administrators should be notified.
    /// </summary>
    Administrator = 2,

    /// <summary>
    ///     Indicates that @here should be notified.
    /// </summary>
    Here = 4,

    /// <summary>
    ///     Indicates that @everyone should be notified.
    /// </summary>
    Everyone = 8
}
