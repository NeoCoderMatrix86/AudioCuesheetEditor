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
    public class CuesheetManager(ITraceChangeManager traceChangeManager, ISessionStateContainer sessionStateContainer, ITrackManager trackManager) : ICuesheetManager
    {
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly ITrackManager _trackManager = trackManager;

        public event EventHandler? IsRecordingChanged;

        /// <inheritdoc/>
        public void SetProperty<TProperty>(Expression<Func<Cuesheet, TProperty>> propertyExpression, TProperty value)
        {
            _traceChangeManager.BulkEdit = true;
            var audiofile = _sessionStateContainer.Cuesheet.Audiofile;
            SetValue(_sessionStateContainer.Cuesheet, propertyExpression, value);
            // If audiofile has been set, we need to calculate last track end
            if (audiofile != _sessionStateContainer.Cuesheet.Audiofile)
            {
                SetLastTrackEnd();
            }
            _traceChangeManager.BulkEdit = false;
        }

        /// <inheritdoc/>
        public Result IsRecordingPossible
        {
            get
            {
                if (_sessionStateContainer.Cuesheet.Tracks.Any())
                {
                    return Result.Failure(new Error(ErrorType.NotPossible, "Cuesheet already contains tracks!"));
                }
                return Result.Success();
            }
        }

        /// <inheritdoc/>
        public Result StartRecording()
        {
            var isRecordingPossibleResult = IsRecordingPossible;
            if (isRecordingPossibleResult.IsSuccess)
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
            return isRecordingPossibleResult;
        }

        /// <inheritdoc/>
        public void StopRecording()
        {
            var cuesheet = _sessionStateContainer.Cuesheet;
            if (cuesheet.IsRecording == true)
            {
                var lastTrack = cuesheet.Tracks.LastOrDefault();
                if ((lastTrack != null) && cuesheet.RecordingStart.HasValue)
                {
                    lastTrack.End = DateTime.UtcNow - cuesheet.RecordingStart.Value;
                }
                cuesheet.RecordingStart = null;
                IsRecordingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public void AddTrack(Track track)
        {
            var cuesheet = _sessionStateContainer.Cuesheet;
            track.Cuesheet = cuesheet;
            // Calculate track properties
            if (cuesheet.IsRecording)
            {
                track.Begin = DateTime.UtcNow - cuesheet.RecordingStart;
            }
            if (cuesheet.Tracks.Any() == false)
            {
                track.Position = 1;
                if ((track.Begin.HasValue == false) || cuesheet.IsRecording)
                {
                    track.Begin = TimeSpan.Zero;
                }
            }
            else
            {
                //TODO: All this changes needs to be done with previousvalue for tracechangemanager, because otherwise undo will not work as expected
                var trackBeforeNewTrack = cuesheet.Tracks.Last();
                if ((cuesheet.Audiofile?.Duration.HasValue == true) && (trackBeforeNewTrack.End.HasValue) && (trackBeforeNewTrack.End == cuesheet.Audiofile.Duration))
                {
                    trackBeforeNewTrack.End = null;
                }
                if (track.Position.HasValue == false)
                {
                    track.Position = (ushort?)(trackBeforeNewTrack.Position + 1);
                }
                if (track.Begin.HasValue == false)
                {
                    track.Begin = trackBeforeNewTrack.End;
                }
                else
                {
                    if (trackBeforeNewTrack.End.HasValue == false)
                    {
                        trackBeforeNewTrack.End = track.Begin;
                    }
                }
                if (cuesheet.IsRecording)
                {
                    trackBeforeNewTrack.End = track.Begin;
                }
            }
            var newValue = new List<Track>(cuesheet.Tracks)
            {
                track
            };
            _traceChangeManager.BulkEdit = true;
            SetValue(cuesheet, x => x.Tracks, newValue);
            SetLastTrackEnd();
            _traceChangeManager.BulkEdit = false;
        }

        /// <inheritdoc/>
        public void RemoveTracks(IEnumerable<Track> tracksToRemove)
        {
            var cuesheet = _sessionStateContainer.Cuesheet;
            var intersection = cuesheet.Tracks.Intersect(tracksToRemove);
            ICollection<Track> newValue = [.. cuesheet.Tracks.Except(intersection)];
            //Calculate position and begin of new tracks
            ushort position = 1;
            foreach (var track in newValue.OrderBy(x => x.Position))
            {
                track.Position = position;
                position++;
                var previousTrack = _trackManager.GetPreviousLinkedTrack(track);
                if (previousTrack?.End.HasValue == true)
                {
                    track.Begin = previousTrack.End;
                }
            }
            _traceChangeManager.BulkEdit = true;
            SetValue(cuesheet, x => x.Tracks, newValue);
            SetLastTrackEnd();
            _traceChangeManager.BulkEdit = false;
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

        void SetLastTrackEnd()
        {
            var lastTrack = _sessionStateContainer.Cuesheet.Tracks
                .OrderByDescending(x => x.Position.HasValue).ThenBy(x => x.Position)
                .ThenByDescending(x => x.Begin.HasValue).ThenBy(x => x.Begin)
                .ThenByDescending(x => x.End.HasValue).ThenBy(x => x.End)
                .LastOrDefault();
            if ((lastTrack?.End.HasValue == false) && (_sessionStateContainer.Cuesheet.Audiofile?.Duration.HasValue == true))
            {
                _trackManager.SetProperty(lastTrack, x => x.End, _sessionStateContainer.Cuesheet.Audiofile.Duration);
            }
        }
    }
}
