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
using Microsoft.JSInterop;
using System.Collections.Immutable;
using System.Reflection;

namespace AudioCuesheetEditor.Data.Services
{
    public class MusicBrainzArtist
    {
        public Guid Id { get; init; }
        public String? Name { get; init; }
        public String? Disambiguation { get; init; }
    }
    public class MusicBrainzTrack
    {
        public Guid Id { get; init; }
        public String? Artist { get; init; }
        public String? Title { get; init; }
        public TimeSpan? Length { get; init; }
        public String? Disambiguation { get; init; }
    }
    public class MusicBrainzDataProvider(ILogger<MusicBrainzDataProvider> logger)
    {
        private readonly ILogger<MusicBrainzDataProvider> _logger = logger;

        public const String Application = "AudioCuesheetEditor";
        public const String ProjectUrl = "https://github.com/NeoCoderMatrix86/AudioCuesheetEditor";

        private String? applicationVersion = null;

        public async Task<IReadOnlyCollection<MusicBrainzArtist>> SearchArtistAsync(String searchString)
        {
            List<MusicBrainzArtist> artistSearchResult = [];
            try
            {
                if (String.IsNullOrEmpty(searchString) == false)
                {
                    using var query = new Query(Application, ApplicationVersion, ProjectUrl);
                    var findArtistsResult = await query.FindArtistsAsync(searchString, simple: true);
                    artistSearchResult = findArtistsResult.Results.ToList().ConvertAll(x => new MusicBrainzArtist() { Id = x.Item.Id, Name = x.Item.Name, Disambiguation = x.Item.Disambiguation });
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Error getting response from MusicBrainz");
            }
            return artistSearchResult.AsReadOnly();
        }

        public async Task<IReadOnlyCollection<MusicBrainzTrack>> SearchTitleAsync(String searchString, String? artist = null)
        {
            List<MusicBrainzTrack> titleSearchResult = [];
            try
            {
                if (String.IsNullOrEmpty(searchString) == false)
                {
                    using var query = new Query(Application, ApplicationVersion, ProjectUrl);
                    ISearchResults<ISearchResult<IRecording>> findRecordingsResult;
                    if (String.IsNullOrEmpty(artist))
                    {
                        findRecordingsResult = await query.FindRecordingsAsync(searchString, simple: true);
                    }
                    else
                    {
                        findRecordingsResult = await query.FindRecordingsAsync(String.Format("{0} AND artistname:{1}", searchString, artist));
                    }
                    foreach (var result in findRecordingsResult.Results)
                    {
                        String artistString = String.Empty;
                        if (result.Item.ArtistCredit != null)
                        {
                            foreach (var artistCredit in result.Item.ArtistCredit)
                            {
                                artistString += artistCredit.Name;
                                if (String.IsNullOrEmpty(artistCredit.JoinPhrase) == false)
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
            catch(HttpRequestException hre)
            {
                _logger.LogError(hre, "Error getting response from MusicBrainz");
            }
            return titleSearchResult.AsReadOnly();
        }

        public async Task<MusicBrainzTrack?> GetDetailsAsync(Guid id)
        {
            MusicBrainzTrack? track = null;
            try
            {
                if (id != Guid.Empty)
                {
                    var query = new Query(Application, ApplicationVersion, ProjectUrl);
                    var recording = await query.LookupRecordingAsync(id, Include.Artists);
                    if (recording != null)
                    {
                        String artist = String.Empty;
                        if (recording.ArtistCredit != null)
                        {
                            foreach (var artistCredit in recording.ArtistCredit)
                            {
                                artist += artistCredit.Name;
                                if (String.IsNullOrEmpty(artistCredit.JoinPhrase) == false)
                                {
                                    artist += artistCredit.JoinPhrase;
                                }
                            }
                        }
                        track = new MusicBrainzTrack() { Id = recording.Id, Title = recording.Title, Artist = artist, Length = recording.Length };
                    }
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Error getting response from MusicBrainz");
            }
            return track;
        }

        private String? ApplicationVersion
        {
            get
            {
                if (applicationVersion == null)
                {
                    var versionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                    if (versionAttribute != null)
                    {
                        applicationVersion = versionAttribute.InformationalVersion;
                    }
                }
                return applicationVersion;
            }
        }
    }
}
