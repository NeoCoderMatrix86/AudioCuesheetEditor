using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.JSInterop;

namespace AudioCuesheetEditor.Services.Options;

/// <inheritdoc/>
public class LocalStorageMigrationV1(IJSRuntime jsRuntime) : ILocalStorageMigration
{
    readonly IJSRuntime _jsRuntime = jsRuntime;
    /// <inheritdoc/>
    public int Version => 1;

    /// <inheritdoc/>
    public async Task ApplyAsync()
    {
        await MigrateKeyAsync("ExportOptions", value => value.Replace("%Track.End%", "%Track.Finish%", StringComparison.Ordinal));
        await MigrateKeyAsync("ImportOptions", value => Regex.Replace(value, @"\bEnd\b", "Finish"));
    }

    async Task MigrateKeyAsync(string key, Func<string, string> replace)
    {
        var storedJson = await _jsRuntime.InvokeAsync<string>("AppSettings.get", key);
        if (string.IsNullOrWhiteSpace(storedJson))
        {
            return;
        }
        var root = JsonNode.Parse(storedJson);
        if (root is null)
        {
            return;
        }

        if (ReplaceSchemeTracks(root, replace))
        {
            await _jsRuntime.InvokeVoidAsync("AppSettings.set", key, root.ToJsonString());
        }
    }

    static bool ReplaceSchemeTracks(JsonNode node, Func<string, string> replace)
    {
        var changed = false;

        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (property.Key == "SchemeTracks"
                    && property.Value is JsonValue jsonValue
                    && jsonValue.TryGetValue<string>(out var oldValue)
                    && oldValue is not null)
                {
                    var newValue = replace(oldValue);

                    if (newValue != oldValue)
                    {
                        jsonObject[property.Key] = newValue;
                        changed = true;
                    }
                }
                else
                {
                    changed |= ReplaceSchemeTracks(property.Value, replace);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                changed |= ReplaceSchemeTracks(item, replace);
            }
        }

        return changed;
    }
}
