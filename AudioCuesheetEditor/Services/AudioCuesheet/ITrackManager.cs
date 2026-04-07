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

namespace AudioCuesheetEditor.Services.AudioCuesheet
{
    /// <summary>
    /// Manages track interactions
    /// </summary>
    public interface ITrackManager
    {
        /// <summary>
        /// Set artist for track
        /// </summary>
        /// <param name="track"></param>
        /// <param name="artist"></param>
        void SetArtist(Track track, String? artist);
        /// <summary>
        /// Set title for track
        /// </summary>
        /// <param name="track"></param>
        /// <param name="title"></param>
        void SetTitle(Track track, String? title);
        /// <summary>
        /// Copies values from a track to another
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="setIsLinkedToPreviousTrack"></param>
        /// <param name="setPosition"></param>
        /// <param name="setArtist"></param>
        /// <param name="setTitle"></param>
        /// <param name="setBegin"></param>
        /// <param name="setEnd"></param>
        /// <param name="setLength"></param>
        /// <param name="setFlags"></param>
        /// <param name="setPreGap"></param>
        /// <param name="setPostGap"></param>
        void CopyValues(Track source, Track target, Boolean setIsLinkedToPreviousTrack = true, Boolean setPosition = true, Boolean setArtist = true, Boolean setTitle = true, Boolean setBegin = true, Boolean setEnd = true, Boolean setLength = false, Boolean setFlags = true, Boolean setPreGap = true, Boolean setPostGap = true);
        /// <summary>
        /// Create a clone of a track
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        Track Clone(Track track);
        //TODO: Add all functions for editing a track (IsLinkedToPreviousTrack, Position, Begin, End, Length, Flags, PreGap, PostGap)
    }
}
