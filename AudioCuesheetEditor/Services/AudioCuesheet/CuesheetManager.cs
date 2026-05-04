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
using AudioCuesheetEditor.Data.Options;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Services.UI;
using System.Linq.Expressions;
using System.Reflection;

namespace AudioCuesheetEditor.Services.AudioCuesheet
{
    /// <inheritdoc/>
    public class CuesheetManager(ITraceChangeManager traceChangeManager, ISessionStateContainer sessionStateContainer, ITrackManager trackManager, ILocalStorageOptionsProvider localStorageOptionsProvider) : ICuesheetManager
    {
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly ITrackManager _trackManager = trackManager;
        private readonly ILocalStorageOptionsProvider _localStorageOptionsProvider = localStorageOptionsProvider;

        public event EventHandler? IsRecordingChanged;

        /// <inheritdoc/>
        public async Task SetPropertyAsync<TProperty>(Expression<Func<Cuesheet, TProperty>> propertyExpression, TProperty value)
        {
            _traceChangeManager.BulkEdit = true;
            var cuesheet = await GetCurrentCuesheetAsync();
            var audiofile = cuesheet?.Audiofile;
            SetValue(cuesheet!, propertyExpression, value);
            // If audiofile has been set, we need to calculate last track end
            if (audiofile != cuesheet?.Audiofile)
            {
                SetLastTrackEnd(cuesheet!);
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
        public async Task AddTrackAsync(Track track)
        {
            var cuesheet = await GetCurrentCuesheetAsync();
            track.Cuesheet = cuesheet;
            // Calculate track properties
            _traceChangeManager.BulkEdit = true;
            if (cuesheet?.IsRecording == true)
            {
                _trackManager.SetProperty(track, x => x.Begin, DateTime.UtcNow - cuesheet.RecordingStart);
            }
            if (cuesheet?.Tracks.Any() == false)
            {
                _trackManager.SetProperty(track, x => x.Position, (ushort)(1));
                if ((track.Begin.HasValue == false) || cuesheet.IsRecording)
                {
                    _trackManager.SetProperty(track, x => x.Begin, TimeSpan.Zero);
                }
            }
            else
            {
                var lastTrack = GetLastTrack(cuesheet!);
                if ((cuesheet?.Audiofile?.Duration.HasValue == true) && (lastTrack?.End.HasValue == true) && (lastTrack.End == cuesheet.Audiofile.Duration))
                {
                    _trackManager.SetProperty(lastTrack, x => x.End, null);
                }
                if (track.Position.HasValue == false)
                {
                    _trackManager.SetProperty(track, x => x.Position, (ushort?)(lastTrack?.Position + 1));
                }
                if (track.Begin.HasValue == false)
                {
                    track.Begin = lastTrack?.End;
                }
                else
                {
                    if (lastTrack?.End.HasValue == false)
                    {
                        _trackManager.SetProperty(lastTrack, x => x.End, track.Begin);
                    }
                }
                if (cuesheet?.IsRecording == true && lastTrack != null)
                {
                    _trackManager.SetProperty(lastTrack, x => x.End, track.Begin);
                }
            }
            var newValue = new List<Track>(cuesheet!.Tracks)
            {
                track
            };
            SetValue(cuesheet, x => x.Tracks, newValue);
            SetLastTrackEnd(cuesheet);
            _traceChangeManager.BulkEdit = false;
        }

        /// <inheritdoc/>
        public async Task RemoveTracksAsync(IEnumerable<Track> tracksToRemove)
        {
            var cuesheet = await GetCurrentCuesheetAsync();
            var intersection = cuesheet!.Tracks.Intersect(tracksToRemove);
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
            SetLastTrackEnd(cuesheet);
            _traceChangeManager.BulkEdit = false;
        }

        /// <inheritdoc/>
        public bool IsMoveTracksUpPossible(HashSet<Track> selectedTracks) => selectedTracks.Count > 0 && selectedTracks.Min(x => x.Position) >= 2;

        /// <inheritdoc/>
        //TODO: Tests
        public bool IsMoveTracksDownPossible(Cuesheet cuesheet, HashSet<Track> selectedTracks) => selectedTracks.Count > 0 && selectedTracks.Max(x => x.Position) < cuesheet?.Tracks.Max(x => x.Position);

        /// <inheritdoc/>
        //TODO: Tests
        public async Task<Result> MoveTracksUpAsync(HashSet<Track> selectedTracks)
        {
            if (IsMoveTracksUpPossible(selectedTracks) == false)
            {
                return Result.Failure(new Error(ErrorType.NotPossible, "Moving tracks up is not possible!"));
            }
            _traceChangeManager.BulkEdit = true;
            var cuesheet = await GetCurrentCuesheetAsync();
            foreach (var selectedTrack in selectedTracks.OrderBy(x => x.Position))
            {
                var previousTrack = cuesheet?.Tracks.FirstOrDefault(x => x.Position == selectedTrack.Position - 1);
                var newBegin = previousTrack?.Begin;
                var newEnd = previousTrack?.End;
                if (previousTrack != null)
                {
                    _trackManager.SetProperty(previousTrack, x => x.Position, selectedTrack.Position);
                    _trackManager.SetProperty(previousTrack, x => x.Begin, selectedTrack.Begin);
                    _trackManager.SetProperty(previousTrack, x => x.End, selectedTrack.End);
                }
                _trackManager.SetProperty(selectedTrack, x => x.Position, (ushort?)(selectedTrack.Position - 1));
                _trackManager.SetProperty(selectedTrack, x => x.Begin, newBegin);
                _trackManager.SetProperty(selectedTrack, x => x.End, newEnd);
            }
            await SetPropertyAsync(x => x.Tracks, cuesheet?.Tracks.OrderBy(x => x.Position));
            _traceChangeManager.BulkEdit = false;
            return Result.Success();
        }

        /// <inheritdoc/>
        //TODO: Tests
        public async Task<Result> MoveTracksDownAsync(HashSet<Track> selectedTracks)
        {
            var cuesheet = await GetCurrentCuesheetAsync();
            if (IsMoveTracksDownPossible(cuesheet!, selectedTracks) == false)
            {
                return Result.Failure(new Error(ErrorType.NotPossible, "Moving tracks down is not possible!"));
            }
            _traceChangeManager.BulkEdit = true;
            foreach (var selectedTrack in selectedTracks.OrderByDescending(x => x.Position))
            {
                var nextTrack = cuesheet?.Tracks.FirstOrDefault(x => x.Position == selectedTrack.Position + 1);
                var newBegin = nextTrack?.Begin;
                var newEnd = nextTrack?.End;
                if (nextTrack != null)
                {
                    _trackManager.SetProperty(nextTrack, x => x.Position, selectedTrack.Position);
                    _trackManager.SetProperty(nextTrack, x => x.Begin, selectedTrack.Begin);
                    _trackManager.SetProperty(nextTrack, x => x.End, selectedTrack.End);
                }
                var newPosition = (ushort?)(selectedTrack.Position + 1);
                _trackManager.SetProperty(selectedTrack, x => x.Position, newPosition);
                _trackManager.SetProperty(selectedTrack, x => x.Begin, newBegin);
                _trackManager.SetProperty(selectedTrack, x => x.End, newEnd);
            }
            await SetPropertyAsync(x => x.Tracks, cuesheet?.Tracks.OrderBy(x => x.Position));
            _traceChangeManager.BulkEdit = false;
            return Result.Success();
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

        void SetLastTrackEnd(Cuesheet cuesheet)
        {
            var lastTrack = GetLastTrack(cuesheet);
            if ((lastTrack?.End.HasValue == false) && (cuesheet.Audiofile?.Duration.HasValue == true))
            {
                _trackManager.SetProperty(lastTrack, x => x.End, cuesheet.Audiofile.Duration);
            }
        }

        async Task<Cuesheet?> GetCurrentCuesheetAsync()
        {
            var viewOptions = await _localStorageOptionsProvider.GetOptionsAsync<ViewOptions>();
            if (viewOptions.ActiveTab == ViewMode.ImportView)
            {
                return _sessionStateContainer.ImportCuesheet;
            }
            return _sessionStateContainer.Cuesheet;
        }

        static Track? GetLastTrack(Cuesheet cuesheet)
        {
            return cuesheet.Tracks
                .OrderByDescending(x => x.Position.HasValue).ThenBy(x => x.Position)
                .ThenByDescending(x => x.Begin.HasValue).ThenBy(x => x.Begin)
                .ThenByDescending(x => x.End.HasValue).ThenBy(x => x.End)
                .LastOrDefault();
        }
    }
}
