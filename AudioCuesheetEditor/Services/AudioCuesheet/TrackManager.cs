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
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Services.UI;

namespace AudioCuesheetEditor.Services.AudioCuesheet
{
    /// <inheritdoc/>
    public class TrackManager(ITraceChangeManager traceChangeManager) : ITrackManager
    {
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
        //TODO: Tests
        /// <inheritdoc/>
        public void SetArtist(Track track, string? artist)
        {
            var previousValue = track.Artist;
            if (Equals(artist, previousValue) == false)
            {
                track.Artist = artist;
                _traceChangeManager.AddChange(new(track, new(previousValue, nameof(Track.Artist))));
            }
        }

        /// <inheritdoc/>
        public void SetTitle(Track track, string? title)
        {
            var previousValue = track.Title;
            if (Equals(title, previousValue) == false)
            {
                track.Title = title;
                _traceChangeManager.AddChange(new(track, new(previousValue, nameof(Track.Title))));
            }
        }
    }
}
