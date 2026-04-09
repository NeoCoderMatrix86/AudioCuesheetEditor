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
using System.Linq.Expressions;
using System.Reflection;

namespace AudioCuesheetEditor.Services.AudioCuesheet
{
    /// <inheritdoc/>
    public class CuesheetManager(ITraceChangeManager traceChangeManager, ISessionStateContainer sessionStateContainer) : ICuesheetManager
    {
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;

        public event EventHandler? IsRecordingChanged;

        //TODO: Tests

        /// <inheritdoc/>
        public void SetProperty<TProperty>(Expression<Func<Cuesheet, TProperty>> propertyExpression, TProperty value)
        {
            SetValue(_sessionStateContainer.Cuesheet, propertyExpression, value);
        }

        /// <inheritdoc/>
        public Result IsRecordingPossible
        {
            get
            {
                if (_sessionStateContainer.Cuesheet.Tracks.Count != 0)
                {
                    return Result.Failure(new Error(ErrorType.NotPossible, "Cuesheet already contains tracks!"));
                }
                return Result.Success();
            }
        }

        /// <inheritdoc/>
        public Result StartRecording()
        {
            var cuesheet = _sessionStateContainer.Cuesheet;
            if (cuesheet.IsRecording == true)
            {
                return Result.Failure(new Error(ErrorType.NotPossible, "Record is already running!"));
            }
            cuesheet.RecordingStart = DateTime.UtcNow;
            IsRecordingChanged?.Invoke(this, EventArgs.Empty);
            return Result.Success();
        }

        /// <inheritdoc/>
        public void StopRecording()
        {
            var cuesheet = _sessionStateContainer.Cuesheet;
            var lastTrack = cuesheet.Tracks.LastOrDefault();
            if ((lastTrack != null) && cuesheet.RecordingStart.HasValue)
            {
                lastTrack.End = DateTime.UtcNow - cuesheet.RecordingStart.Value;
            }
            cuesheet.RecordingStart = null;
            IsRecordingChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public Track? GetPreviousLinkedTrack(Track track)
        {
            Track? previousLinkedTrack = null;
            if (track.IsLinkedToPreviousTrack && track.Position.HasValue)
            {
                if (track.Position.Value > 1)
                {
                    previousLinkedTrack = track.Cuesheet?.Tracks.SingleOrDefault(x => x.Position == track.Position.Value - 1);
                }
            }
            return previousLinkedTrack;
        }

        /// <inheritdoc/>
        public Track? GetNextLinkedTrack(Track track)
        {
            Track? nextLinkedTrack = null;
            if (track.Position.HasValue)
            {
                nextLinkedTrack = track.Cuesheet?.Tracks.SingleOrDefault(x => x.Position == track.Position.Value + 1 && x.IsLinkedToPreviousTrack == true);
            }
            return nextLinkedTrack;
        }

        /// <inheritdoc/>
        public void AddTrack(Track track)
        {
            var cuesheet = _sessionStateContainer.Cuesheet;
            var newValue = new List<Track>(cuesheet.Tracks)
            {
                track
            };
            SetValue(cuesheet, x => x.Tracks, newValue);
            //TODO: calculate track begin when cuesheet is recording
            //TODO: recalculate track properties like Cuesheet.RecalculateTrackProperties did
        }

        /// <inheritdoc/>
        public void RemoveTracks(IEnumerable<Track> tracksToRemove)
        {
            var cuesheet = _sessionStateContainer.Cuesheet;
            var intersection = cuesheet.Tracks.Intersect(tracksToRemove);
            ICollection<Track> newValue = [.. cuesheet.Tracks.Except(intersection)];
            SetValue(cuesheet, x => x.Tracks, newValue);
            //TODO: calculate position and begin of all tracks
            //TODO: calculate last track end Cuesheet.RecalculateTrackProperties did
        }

        void SetValue<TProperty>(Cuesheet cuesheet, Expression<Func<Cuesheet, TProperty>> propertyExpression, TProperty value)
        {
            if (propertyExpression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("Expression must be a property");
            }

            if (memberExpression.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException("Member is not a property");
            }

            var previousValue = (TProperty?)propertyInfo.GetValue(cuesheet);
            if (Equals(previousValue, value))
            {
                return;
            }

            propertyInfo.SetValue(cuesheet, value);

            _traceChangeManager.AddChange(new(cuesheet, new(previousValue, propertyInfo.Name)));
        }

    }
}
