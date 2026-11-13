namespace AudioCuesheetEditor.Services.Options;

/// <summary>
/// Interface for local storage migrations
/// </summary>
public interface ILocalStorageMigration
{
    /// <summary>
    /// Version of this migration (used for ordering updates)
    /// </summary>
    int Version { get; }
    /// <summary>
    /// Apply updates from this migration
    /// </summary>
    /// <returns></returns>
    Task ApplyAsync();
}
