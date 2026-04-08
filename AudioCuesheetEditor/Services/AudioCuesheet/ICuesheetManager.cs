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
        public event EventHandler<Cuesheet>? IsRecordingChanged;
        /// <summary>
        /// Set property for cuesheet
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="cuesheet"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="value"></param>
        void SetProperty<TProperty>(Cuesheet cuesheet, Expression<Func<Cuesheet, TProperty>> propertyExpression, TProperty value);
        /// <summary>
        /// Checks if recording for the cuesheet is possible
        /// </summary>
        /// <param name="cuesheet"></param>
        /// <returns></returns>
        Result IsRecordingPossible(Cuesheet cuesheet);
        /// <summary>
        /// Starts recording on cuesheet
        /// </summary>
        /// <param name="cuesheet"></param>
        /// <returns></returns>
        Result StartRecording(Cuesheet cuesheet);
        /// <summary>
        /// Stop recording on cuesheet
        /// </summary>
        /// <param name="cuesheet"></param>
        /// <returns></returns>
        void StopRecording(Cuesheet cuesheet);

        //TODO: Add all methods from cuesheet here
    }
}
