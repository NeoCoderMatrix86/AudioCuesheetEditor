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
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using MetaBrainz.MusicBrainz.Interfaces.Searches;
using System.Reflection;

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
public partial class MusicBrainzDataProvider(ILogger<MusicBrainzDataProvider> logger)
{
    readonly ILogger<MusicBrainzDataProvider> _logger = logger;

    public const string Application = "AudioCuesheetEditor";
    public const string ProjectUrl = "https://github.com/NeoCoderMatrix86/AudioCuesheetEditor";

    public async Task<IReadOnlyCollection<MusicBrainzArtist>> SearchArtistAsync(string? searchString, CancellationToken token)
    {
        List<MusicBrainzArtist> artistSearchResult = [];
        try
        {
            if (string.IsNullOrEmpty(searchString) == false)
            {
                using var query = new Query(Application, ApplicationVersion, ProjectUrl);
                var findArtistsResult = await query.FindArtistsAsync(searchString, simple: true, cancellationToken: token);
                artistSearchResult = findArtistsResult.Results.ToList().ConvertAll(x => new MusicBrainzArtist() { Id = x.Item.Id, Name = x.Item.Name, Disambiguation = x.Item.Disambiguation });
            }
        }
        catch (HttpRequestException hre)
        {
            LogErrorGettingResponseFromMusicBrainz(hre);
        }
        return artistSearchResult.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<MusicBrainzTrack>> SearchTitleAsync(string? searchString, string? artist, CancellationToken token)
    {
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
                    findRecordingsResult = await query.FindRecordingsAsync(string.Format("{0} AND artistname:{1}", searchString, artist), cancellationToken: token);
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
            LogErrorGettingResponseFromMusicBrainz(hre);
        }
        return titleSearchResult.AsReadOnly();
    }

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

    [LoggerMessage(Level = LogLevel.Error, Message = "Error getting response from MusicBrainz")]
    partial void LogErrorGettingResponseFromMusicBrainz(HttpRequestException hre);
}
