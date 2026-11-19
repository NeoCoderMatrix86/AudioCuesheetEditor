//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
using System.Collections.Concurrent;
using System.Reflection;
using MetaBrainz.Common;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using MetaBrainz.MusicBrainz.Interfaces.Searches;

namespace AudioCuesheetEditor.Services.Provider;

public class MusicBrainzArtist
{
    public Guid? Id { get; init; }
    public string? Name { get; init; }
    public string? Disambiguation { get; init; }
}
public class MusicBrainzTrack
{
    public Guid? Id { get; init; }
    public string? Artist { get; init; }
    public string? Title { get; init; }
    public TimeSpan? Length { get; init; }
    public string? Disambiguation { get; init; }
}
public partial class MusicBrainzDataProvider(ILogger<MusicBrainzDataProvider> logger) : IDisposable
{
    readonly ILogger<MusicBrainzDataProvider> _logger = logger;
    readonly SemaphoreSlim _requestGate = new(1, 1);
    readonly TimeSpan _minimumTimeBetweenRequests = TimeSpan.FromMilliseconds(1100);
    readonly Lock _lastRequestLock = new();
    DateTimeOffset _lastRequestStarted = DateTimeOffset.MinValue;
    readonly ConcurrentDictionary<string, Lazy<Task<IReadOnlyCollection<MusicBrainzArtist>>>> _artistSearchCache = new();
    bool _disposedValue;
    public const string Application = "AudioCuesheetEditor";
    public const string ProjectUrl = "https://github.com/NeoCoderMatrix86/AudioCuesheetEditor";

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async Task<IReadOnlyCollection<MusicBrainzArtist>> SearchArtistAsync(string? searchString, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(searchString))
        {
            return [];
        }
        var cacheKey = searchString.Trim();

        var cachedSearch = _artistSearchCache.GetOrAdd(
            cacheKey,
            new Lazy<Task<IReadOnlyCollection<MusicBrainzArtist>>>(() => GetMusicBrainzArtistsAsync(cacheKey), LazyThreadSafetyMode.ExecutionAndPublication));
        try
        {
            return await cachedSearch.Value.WaitAsync(token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpError he)
        {
            _ = _artistSearchCache.TryRemove(cacheKey, out _);
           LogHttpError(he, he.Message);
            return [];
        }
        catch (HttpRequestException hre)
        {
            _ = _artistSearchCache.TryRemove(cacheKey, out _);
            LogHttpRequestException(hre);
            return [];
        }
    }

    public async Task<IReadOnlyCollection<MusicBrainzTrack>> SearchTitleAsync(string? searchString, string? artist, CancellationToken token)
    {
        //TODO: Caching an retry like SearchArtistAsync
        List<MusicBrainzTrack> titleSearchResult = [];
        try
        {
            if (string.IsNullOrEmpty(searchString) == false)
            {
                using var query = new Query(Application, ApplicationVersion, ProjectUrl);
                ISearchResults<ISearchResult<IRecording>> findRecordingsResult;
                if (string.IsNullOrEmpty(artist))
                {
                    findRecordingsResult = await query.FindRecordingsAsync(searchString, simple: true, cancellationToken: token);
                }
                else
                {
                    findRecordingsResult = await query.FindRecordingsAsync($"{searchString} AND artistname:{artist}", cancellationToken: token);
                }
                foreach (var result in findRecordingsResult.Results)
                {
                    var artistString = string.Empty;
                    if (result.Item.ArtistCredit != null)
                    {
                        foreach (var artistCredit in result.Item.ArtistCredit)
                        {
                            artistString += artistCredit.Name;
                            if (string.IsNullOrEmpty(artistCredit.JoinPhrase) == false)
                            {
                                artistString += artistCredit.JoinPhrase;
                            }
                        }
                    }
                    titleSearchResult.Add(new MusicBrainzTrack()
                    {
                        Id = result.Item.Id,
                        Artist = artistString,
                        Title = result.Item.Title,
                        Length = result.Item.Length,
                        Disambiguation = result.Item.Disambiguation
                    });
                }
            }
        }
        catch (HttpRequestException hre)
        {
            LogHttpRequestException(hre);
        }
        return titleSearchResult.AsReadOnly();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _requestGate.Dispose();
            }

            _disposedValue = true;
        }
    }

    async Task WaitForRequestSlotAsync()
    {
        await _requestGate.WaitAsync();

        TimeSpan delay;

        lock (_lastRequestLock)
        {
            delay = _lastRequestStarted + _minimumTimeBetweenRequests - DateTimeOffset.UtcNow;

            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }
        }

        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay);
        }

        lock (_lastRequestLock)
        {
            _lastRequestStarted = DateTimeOffset.UtcNow;
        }
    }

    async Task<IReadOnlyCollection<MusicBrainzArtist>> GetMusicBrainzArtistsAsync(string searchString)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            await WaitForRequestSlotAsync();

            try
            {
                using var query = new Query(Application, ApplicationVersion, ProjectUrl);
                var result = await query.FindArtistsAsync(searchString, simple: true);

                return [.. result.Results
                        .Select(x => new MusicBrainzArtist
                        {
                            Id = x.Item.Id,
                            Name = x.Item.Name,
                            Disambiguation = x.Item.Disambiguation
                        })];
            }
            catch (HttpError he) when (IsMusicBrainzBusy(he) && attempt < maxAttempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                await Task.Delay(delay);
            }
            finally
            {
                _ = _requestGate.Release();
            }
        }
        return [];
    }

    static bool IsMusicBrainzBusy(HttpError error) => error.Message.StartsWith("The MusicBrainz web server is currently busy. Please try again later.", StringComparison.InvariantCultureIgnoreCase);

    string? ApplicationVersion
    {
        get
        {
            if (field == null)
            {
                var versionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (versionAttribute != null)
                {
                    field = versionAttribute.InformationalVersion;
                }
            }
            return field;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "General error getting response from MusicBrainz")]
    partial void LogHttpRequestException(HttpRequestException hre);

    [LoggerMessage(Level = LogLevel.Error, Message = "HttpError caught: {exceptionMessage}")]
    partial void LogHttpError(HttpError he, string exceptionMessage);
}
