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
    /// Manager for cuesheet interactions
    /// </summary>
    public interface ICuesheetManager
    {
        /// <summary>
        /// Event propagated when recording on a cuesheet changes
        /// </summary>
        public event EventHandler? IsRecordingChanged;
        /// <summary>
        /// Set property for cuesheet
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="cuesheet"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="value"></param>
        Task SetPropertyAsync<TProperty>(Expression<Func<Cuesheet, TProperty>> propertyExpression, TProperty value);
        /// <summary>
        /// Checks if recording for the cuesheet is possible
        /// </summary>
        /// <returns></returns>
        Result IsRecordingPossible { get; }
        /// <summary>
        /// Starts recording on cuesheet
        /// </summary>
        /// <returns></returns>
        Result StartRecording();
        /// <summary>
        /// Stop recording on cuesheet
        /// </summary>
        /// <returns></returns>
        void StopRecording();
        /// <summary>
        /// Adds a track to the cuesheet
        /// </summary>
        /// <param name="cuesheet"></param>
        /// <param name="track"></param>
        Task AddTrackAsync(Track track);
        /// <summary>
        /// Remove tracks from cuesheet
        /// </summary>
        /// <param name="cuesheet"></param>
        /// <param name="tracksToRemove"></param>
        Task RemoveTracksAsync(IEnumerable<Track> tracksToRemove);
        /// <summary>
        /// Determines if moving tracks up is possible
        /// </summary>
        /// <param name="selectedTracks"></param>
        /// <returns></returns>
        Boolean IsMoveTracksUpPossible(HashSet<Track> selectedTracks);
        /// <summary>
        /// Determines if moving tracks down is possible
        /// </summary>
        /// <param name="selectedTracks"></param>
        /// <returns></returns>
        Boolean IsMoveTracksDownPossible(HashSet<Track> selectedTracks);
        /// <summary>
        /// Moves selected tracks up
        /// </summary>
        /// <param name="selectedTracks"></param>
        Task<Result> MoveTracksUpAsync(HashSet<Track> selectedTracks);
        /// <summary>
        /// Moves selected tracks down
        /// </summary>
        /// <param name="selectedTracks"></param>
        /// <returns></returns>
        Task<Result> MoveTracksDownAsync(HashSet<Track> selectedTracks);
    }
}
