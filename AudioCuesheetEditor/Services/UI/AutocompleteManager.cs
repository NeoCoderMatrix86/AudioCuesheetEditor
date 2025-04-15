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
using AudioCuesheetEditor.Data.Services;

namespace AudioCuesheetEditor.Services.UI
{
    public class AutocompleteManager(MusicBrainzDataProvider musicBrainzDataProvider)
    {
        private readonly MusicBrainzDataProvider _musicBrainzDataProvider = musicBrainzDataProvider;

        public async Task<IEnumerable<MusicBrainzArtist>> SearchArtistsAsync(String? searchString, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<MusicBrainzArtist> artists = await _musicBrainzDataProvider.SearchArtistAsync(searchString, cancellationToken);
                // Add an element that is exactly the entered value, as otherwise the user can not enter a value, that is not available on musicbrainz
                if ((String.IsNullOrEmpty(searchString) == false) && (artists.Any(x => x.Name == searchString) == false))
                {
                    artists = artists.Prepend(new() { Name = searchString });
                }
                return artists;
            }
            catch (TaskCanceledException)
            {
                return [];
            }
            catch (OperationCanceledException)
            {
                return [];
            }
        }

        public async Task<IEnumerable<MusicBrainzTrack>> SearchTitlesAsync(String? searchString, String? artist, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<MusicBrainzTrack> tracks = await _musicBrainzDataProvider.SearchTitleAsync(searchString, artist, cancellationToken);
                //Add an element that is exactly the entered value, as otherwise the user can not enter a value, that is not available on musicbrainz
                if ((String.IsNullOrEmpty(searchString) == false) && (tracks.Any(x => x.Title == searchString) == false))
                {
                    tracks = tracks.Prepend(new() { Artist = artist, Title = searchString });
                }
                return tracks;
            }
            catch (TaskCanceledException)
            {
                return [];
            }
            catch (OperationCanceledException)
            {
                return [];
            }
        }
    }
}
