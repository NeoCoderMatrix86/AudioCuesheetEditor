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
using System.Linq.Expressions;

namespace AudioCuesheetEditor.Services.AudioCuesheet
{
    /// <summary>
    /// Manages track interactions
    /// </summary>
    public interface ITrackManager
    {
        /// <summary>
        /// Set property value for track
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="track"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="value"></param>
        void SetProperty<TProperty>(Track track, Expression<Func<Track, TProperty>> propertyExpression, TProperty value);
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
        void CopyValues(ITrack source, Track target, Boolean setIsLinkedToPreviousTrack = true, Boolean setPosition = true, Boolean setArtist = true, Boolean setTitle = true, Boolean setBegin = true, Boolean setEnd = true, Boolean setLength = true, Boolean setFlags = true, Boolean setPreGap = true, Boolean setPostGap = true);
        /// <summary>
        /// Create a clone of a track
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        Track Clone(ITrack track);
        /// <summary>
        /// Get the previous linked track of a track object
        /// </summary>
        /// <param name="cuesheet">Cuesheet to get the previous linked track from</param>
        /// <param name="track">Track object to get the previous link to</param>
        /// <returns>Previous linked track or null (if not linked)</returns>
        Track? GetPreviousLinkedTrack(Track track);
        /// <summary>
        /// Get next linked track that is linked to the parameter track
        /// </summary>
        /// <param name="cuesheet"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        Track? GetNextLinkedTrack(Track track);
    }
}
