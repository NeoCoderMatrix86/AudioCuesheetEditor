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
public partial class MusicBrainzDataProvider : IDisposable
{
    static string? _applicationVersion;
    readonly record struct TitleSearchKey(string SearchString, string? Artist);

    const string _application = "AudioCuesheetEditor";
    const string _projectUrl = "https://github.com/NeoCoderMatrix86/AudioCuesheetEditor";
    const int _maxRequestAttempts = 3;
    readonly TimeSpan _minimumTimeBetweenRequests = TimeSpan.FromMilliseconds(1100);
    readonly ILogger<MusicBrainzDataProvider> _logger;
    readonly SemaphoreSlim _requestGate = new(1, 1);
    readonly Lock _lastRequestLock = new();
    DateTimeOffset _lastRequestStarted = DateTimeOffset.MinValue;
    readonly ConcurrentDictionary<string, Lazy<Task<IReadOnlyCollection<MusicBrainzArtist>>>> _artistSearchCache = new();
    readonly ConcurrentDictionary<TitleSearchKey, Lazy<Task<IReadOnlyCollection<MusicBrainzTrack>>>> _titleSearchCache = new();
    bool _disposedValue;

    public MusicBrainzDataProvider(ILogger<MusicBrainzDataProvider> logger)
    {
        _logger = logger;
        var versionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (versionAttribute != null)
        {
            _applicationVersion = versionAttribute.InformationalVersion;
        }
    }

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
        if (string.IsNullOrWhiteSpace(searchString))
        {
            return [];
        }

        var normalizedSearchString = searchString.Trim();
        var normalizedArtist = string.IsNullOrWhiteSpace(artist) ? null : artist.Trim();
        var cacheKey = new TitleSearchKey(normalizedSearchString, normalizedArtist);

        var cachedSearch = _titleSearchCache.GetOrAdd(
            cacheKey,
            new Lazy<Task<IReadOnlyCollection<MusicBrainzTrack>>>(() => GetMusicBrainzTracksAsync(normalizedSearchString, normalizedArtist), LazyThreadSafetyMode.ExecutionAndPublication));

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
            _ = _titleSearchCache.TryRemove(cacheKey, out _);
            LogHttpError(he, he.Message);
            return [];
        }
        catch (HttpRequestException hre)
        {
            _ = _titleSearchCache.TryRemove(cacheKey, out _);
            LogHttpRequestException(hre);
            return [];
        }
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
        for (var attempt = 1; attempt <= _maxRequestAttempts; attempt++)
        {
            await WaitForRequestSlotAsync();

            try
            {
                using var query = new Query(_application, _applicationVersion, _projectUrl);
                var result = await query.FindArtistsAsync(searchString, simple: true);

                return [.. result.Results
                        .Select(x => new MusicBrainzArtist
                        {
                            Id = x.Item.Id,
                            Name = x.Item.Name,
                            Disambiguation = x.Item.Disambiguation
                        })];
            }
            catch (HttpError he) when (IsMusicBrainzBusy(he) && attempt < _maxRequestAttempts)
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

    async Task<IReadOnlyCollection<MusicBrainzTrack>> GetMusicBrainzTracksAsync(string searchString, string? artist)
    {
        for (var attempt = 1; attempt <= _maxRequestAttempts; attempt++)
        {
            await WaitForRequestSlotAsync();
            try
            {
                using var query = new Query(_application, _applicationVersion, _projectUrl);
                ISearchResults<ISearchResult<IRecording>> result;

                if (string.IsNullOrEmpty(artist))
                {
                    result = await query.FindRecordingsAsync(searchString, simple: true);
                }
                else
                {
                    result = await query.FindRecordingsAsync($"{searchString} AND artistname:{artist}");
                }

                var tracks = new List<MusicBrainzTrack>();
                foreach (var recording in result.Results)
                {
                    var artistString = string.Empty;
                    if (recording.Item.ArtistCredit != null)
                    {
                        foreach (var artistCredit in recording.Item.ArtistCredit)
                        {
                            artistString += artistCredit.Name;
                            if (!string.IsNullOrEmpty(artistCredit.JoinPhrase))
                            {
                                artistString += artistCredit.JoinPhrase;
                            }
                        }
                    }

                    tracks.Add(new MusicBrainzTrack
                    {
                        Id = recording.Item.Id,
                        Artist = artistString,
                        Title = recording.Item.Title,
                        Length = recording.Item.Length,
                        Disambiguation = recording.Item.Disambiguation
                    });
                }

                return tracks.AsReadOnly();
            }
            catch (HttpError he) when (IsMusicBrainzBusy(he) && attempt < _maxRequestAttempts)
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

    [LoggerMessage(Level = LogLevel.Error, Message = "General error getting response from MusicBrainz")]
    partial void LogHttpRequestException(HttpRequestException hre);

    [LoggerMessage(Level = LogLevel.Error, Message = "HttpError caught: {exceptionMessage}")]
    partial void LogHttpError(HttpError he, string exceptionMessage);
}
