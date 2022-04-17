using System.IO;
using BrackeysBot.Core.Data.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace BrackeysBot.Core.Data;

/// <summary>
///     Represents a session with the <c>core.db</c> database.
/// </summary>
internal sealed class CoreContext : DbContext
{
    private readonly string _dataSource;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CoreContext" /> class.
    /// </summary>
    /// <param name="plugin">The owning plugin.</param>
    public CoreContext(CorePlugin plugin)
    {
        _dataSource = Path.Combine(plugin.DataDirectory.FullName, "core.db");
    }

    /// <summary>
    ///     Gets the set of bookmarks.
    /// </summary>
    /// <value>The set of bookmarks.</value>
    public DbSet<Bookmark> Bookmarks { get; internal set; } = null!;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite($"Data Source={_dataSource}");
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new BookmarkConfiguration());
    }
}
