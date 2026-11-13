using System.Globalization;
using Microsoft.JSInterop;

namespace AudioCuesheetEditor.Services.Options;

public class LocalStorageMigrationService(IJSRuntime jsRuntime, IEnumerable<ILocalStorageMigration> migrations)
{
    readonly IJSRuntime _jsRuntime = jsRuntime;
    readonly IEnumerable<ILocalStorageMigration> _migrations = migrations;
    const string _versionKey = "OptionsMigrationVersion";

    public async Task ApplyMigrationsAsync()
    {
        var storedVersion = await _jsRuntime.InvokeAsync<string?>("AppSettings.get", _versionKey);
        var currentVersion = int.TryParse(storedVersion, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedVersion) ? parsedVersion : 0;
        foreach (var migration in _migrations.OrderBy(x => x.Version))
        {
            if (migration.Version <= currentVersion)
            {
                continue;
            }

            await migration.ApplyAsync();

            await _jsRuntime.InvokeVoidAsync("AppSettings.set", _versionKey, migration.Version.ToString(CultureInfo.InvariantCulture));

            currentVersion = migration.Version;
        }
    }
}
