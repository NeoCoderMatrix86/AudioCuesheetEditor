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
    public class MusicBrainzTrack
    {
        public Guid Id { get; init; }
        public String? Artist { get; init; }
        public String? Title { get; init; }
        public TimeSpan? Length { get; init; }
    }
    public class MusicBrainzDataProvider
    {
        public const String Application = "AudioCuesheetEditor";
        public const String ProjectUrl = "https://github.com/NeoCoderMatrix86/AudioCuesheetEditor";

        private String? applicationVersion = null;

        public async Task<Dictionary<Guid, String?>> SearchArtistAsync(String searchString)
        {
            Dictionary<Guid, String?> artistSearchResult = new();
            using var query = new Query(Application, ApplicationVersion, ProjectUrl);
            var result = await query.FindArtistsAsync(searchString, simple: true);
            artistSearchResult = result.Results.ToDictionary(x => x.Item.Id, x => x.Item.Name);
            return artistSearchResult;
        }

        public async Task<Dictionary<Guid, String>> SearchTitleAsync(String searchString, String? artist = null)
        {
            Dictionary<Guid, String> titleSearchResult = new();
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
            //TODO: Disambiguation als einzelnes anzeigefeld nehmen und nicht im title anhängen (sonst wird es automatisch beim track title angesetzt)
            titleSearchResult = findRecordingsResult.Results.ToDictionary(x => x.Item.Id, x => $"{x.Item.Title} ({x.Item.Disambiguation})");
            return titleSearchResult;
        }

        public async Task<MusicBrainzTrack?> GetDetailsAsync(Guid id)
        {
            MusicBrainzTrack? track = null;
            var query = new Query(Application, ApplicationVersion, ProjectUrl);
            var recording = await query.LookupRecordingAsync(id);
            if (recording != null)
            {
                //TODO: Artist
                track = new MusicBrainzTrack() { Id = recording.Id, Title = recording.Title, Length = recording.Length };
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
