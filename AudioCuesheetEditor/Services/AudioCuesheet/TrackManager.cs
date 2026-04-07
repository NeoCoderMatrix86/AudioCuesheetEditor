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

        /// <inheritdoc/>
        public void SetFlags(Track track, IEnumerable<Flag> flags)
        {
            var previousValue = track.Flags;
            if (Equals(flags, previousValue) == false)
            {
                track.Flags = flags;
                _traceChangeManager.AddChange(new(track, new(previousValue, nameof(Track.Flags))));
            }
        }

        /// <inheritdoc/>
        public Track Clone(Track track)
        {
            var clone = new Track();
            CopyValues(track, clone);
            return clone;
        }

        /// <inheritdoc/>
        public void CopyValues(Track source, Track target, bool setIsLinkedToPreviousTrack = true, bool setPosition = true, bool setArtist = true, bool setTitle = true, bool setBegin = true, bool setEnd = true, bool setLength = false, bool setFlags = true, bool setPreGap = true, bool setPostGap = true)
        {
            //TODO: Create change for ITraceChangeManager
            if (setIsLinkedToPreviousTrack)
            {
                target.IsLinkedToPreviousTrack = source.IsLinkedToPreviousTrack;
            }
            if (setPosition)
            {
                target.Position = source.Position;
            }
            if (setArtist)
            {
                target.Artist = source.Artist;
            }
            if (setTitle)
            {
                target.Title = source.Title;
            }
            if (setBegin)
            {
                target.Begin = source.Begin;
            }
            if (setEnd)
            {
                target.End = source.End;
            }
            if (setLength)
            {
                target.Length = source.Length;
            }
            if (setFlags)
            {
                target.Flags = source.Flags;
            }
            if (setPreGap)
            {
                target.PreGap = source.PreGap;
            }
            if (setPostGap)
            {
                target.PostGap = source.PostGap;
            }
        }
    }
}
