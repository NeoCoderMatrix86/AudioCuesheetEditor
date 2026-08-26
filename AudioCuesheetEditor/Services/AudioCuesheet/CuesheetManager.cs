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
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Services.UI;
using System.Linq.Expressions;
using System.Reflection;

namespace AudioCuesheetEditor.Services.AudioCuesheet
{
    /// <inheritdoc/>
    public class CuesheetManager(ITraceChangeManager traceChangeManager, ISessionStateContainer sessionStateContainer, ITrackManager trackManager, IAudiofileManager audiofileManager) : ICuesheetManager
    {
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly ITrackManager _trackManager = trackManager;
        private readonly IAudiofileManager _audiofileManager = audiofileManager;

        public event EventHandler? IsRecordingChanged;

        /// <inheritdoc/>
        public void SetProperty<TProperty>(Expression<Func<Cuesheet, TProperty>> propertyExpression, TProperty value)
        {
            _traceChangeManager.BulkEdit = true;
            var cuesheet = _sessionStateContainer.GetActiveCuesheet();
            //TODO
            //var audiofile = cuesheet?.Audiofile;
            SetValue(cuesheet!, propertyExpression, value);
            //// If audiofile has been set, we need to calculate last track end
            //if (audiofile != cuesheet?.Audiofile)
            //{
            //    SetLastTrackEnd(cuesheet!);
            //}
            //TODO: Check if an audiofile has been removed, if so, we need to revoke its object url via library.js (revokeAudioObjectURL)
            _traceChangeManager.BulkEdit = false;
        }

        /// <inheritdoc/>
        public Result IsRecordingPossible
        {
            get
            {
                if (_sessionStateContainer.Cuesheet.IsRecording == true)
                {
                    return Result.Failure(new Error(ErrorType.NotPossible, "Record is already running!"));
                }
                if (_sessionStateContainer.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).Any())
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
                if (cuesheet.Audiofiles.Count == 0)
                {
                    cuesheet.Audiofiles.Add(new Audiofile());
                }
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
                var lastTrack = GetLastTrack(cuesheet);
                if ((lastTrack != null) && cuesheet.RecordingStart.HasValue)
                {
                    lastTrack.End = DateTime.UtcNow - cuesheet.RecordingStart.Value;
                }
                cuesheet.RecordingStart = null;
                IsRecordingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public bool IsMoveTracksUpPossible(HashSet<Track> selectedTracks) => selectedTracks.Count > 0 && selectedTracks.Min(x => x.Position) >= 2;

        /// <inheritdoc/>
        public bool IsMoveTracksDownPossible(HashSet<Track> selectedTracks) => selectedTracks.Count > 0 &&  selectedTracks.Max(x => x.Position) < _sessionStateContainer.GetActiveCuesheet()?.Audiofiles.SelectMany(x => x.Tracks).Max(x => x.Position);

        /// <inheritdoc/>
        public Result MoveTracksUp(HashSet<Track> selectedTracks)
        {
            if (IsMoveTracksUpPossible(selectedTracks) == false)
            {
                return Result.Failure(new Error(ErrorType.NotPossible, "Moving tracks up is not possible!"));
            }
            _traceChangeManager.BulkEdit = true;
            var cuesheet = _sessionStateContainer.GetActiveCuesheet();
            foreach (var selectedTrack in selectedTracks.OrderBy(x => x.Position))
            {
                var previousTrack = cuesheet?.Audiofiles.SelectMany(x => x.Tracks).FirstOrDefault(x => x.Position == selectedTrack.Position - 1);
                var newBegin = previousTrack?.Begin;
                var newEnd = previousTrack?.End;
                if (previousTrack != null)
                {
                    if (previousTrack.Audiofile != selectedTrack.Audiofile)
                    {
                        SwitchAudiofile(selectedTrack, previousTrack);
                    }
                    _trackManager.SetProperty(previousTrack, x => x.Position, selectedTrack.Position);
                    _trackManager.SetProperty(previousTrack, x => x.Begin, selectedTrack.Begin);
                    _trackManager.SetProperty(previousTrack, x => x.End, selectedTrack.End);
                }
                _trackManager.SetProperty(selectedTrack, x => x.Position, (ushort?)(selectedTrack.Position - 1));
                _trackManager.SetProperty(selectedTrack, x => x.Begin, newBegin);
                _trackManager.SetProperty(selectedTrack, x => x.End, newEnd);
            }
            foreach (var audiofile in cuesheet!.Audiofiles)
            {
                var orderedTracks = audiofile.Tracks.OrderBy(x => x.Position).ToList();
                _audiofileManager.SetProperty(audiofile, x => x.Tracks, orderedTracks);
            }
            _traceChangeManager.BulkEdit = false;
            return Result.Success();
        }

        /// <inheritdoc/>
        public Result MoveTracksDown(HashSet<Track> selectedTracks)
        {
            if (IsMoveTracksDownPossible(selectedTracks) == false)
            {
                return Result.Failure(new Error(ErrorType.NotPossible, "Moving tracks down is not possible!"));
            }
            _traceChangeManager.BulkEdit = true;
            var cuesheet = _sessionStateContainer.GetActiveCuesheet();
            foreach (var selectedTrack in selectedTracks.OrderByDescending(x => x.Position))
            {
                var nextTrack = cuesheet?.Audiofiles.SelectMany(x => x.Tracks).FirstOrDefault(x => x.Position == selectedTrack.Position + 1);
                var newBegin = nextTrack?.Begin;
                var newEnd = nextTrack?.End;
                if (nextTrack != null)
                {
                    if (nextTrack.Audiofile != selectedTrack.Audiofile)
                    {
                        SwitchAudiofile(selectedTrack, nextTrack);
                    }
                    _trackManager.SetProperty(nextTrack, x => x.Position, selectedTrack.Position);
                    _trackManager.SetProperty(nextTrack, x => x.Begin, selectedTrack.Begin);
                    _trackManager.SetProperty(nextTrack, x => x.End, selectedTrack.End);
                }
                _trackManager.SetProperty(selectedTrack, x => x.Position, (ushort?)(selectedTrack.Position + 1));
                _trackManager.SetProperty(selectedTrack, x => x.Begin, newBegin);
                _trackManager.SetProperty(selectedTrack, x => x.End, newEnd);
            }
            foreach (var audiofile in cuesheet!.Audiofiles)
            {
                var orderedTracks = audiofile.Tracks.OrderBy(x => x.Position).ToList();
                _audiofileManager.SetProperty(audiofile, x => x.Tracks, orderedTracks);
            }
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

        void SwitchAudiofile(Track trackToMove, Track currentTrackPosition)
        {
            var currentTrackPositionAudiofile = currentTrackPosition.Audiofile;
            var trackToMoveTrackAudiofile = trackToMove.Audiofile;
            //Switch audiofiles without audiofilemanager since methods there capsulate much logic
            var currentTrackPositionAudiofileTracks = new List<Track>(currentTrackPositionAudiofile!.Tracks);
            currentTrackPositionAudiofileTracks.Remove(currentTrackPosition);
            currentTrackPositionAudiofileTracks.Add(trackToMove);
            _traceChangeManager.AddChange(new(currentTrackPositionAudiofile, new(currentTrackPositionAudiofile.Tracks, nameof(Audiofile.Tracks))));
            currentTrackPositionAudiofile.Tracks = currentTrackPositionAudiofileTracks;
            var trackToMoveAudiofileTracks = new List<Track>(trackToMoveTrackAudiofile!.Tracks);
            trackToMoveAudiofileTracks.Remove(trackToMove);
            trackToMoveAudiofileTracks.Add(currentTrackPosition);
            _traceChangeManager.AddChange(new(trackToMoveTrackAudiofile, new(trackToMoveTrackAudiofile.Tracks, nameof(Audiofile.Tracks))));
            trackToMoveTrackAudiofile.Tracks = trackToMoveAudiofileTracks;
        }

        void SetLastTrackEnd(Cuesheet cuesheet)
        {
            var lastTrack = GetLastTrack(cuesheet);
            //TODO
            //if ((lastTrack?.End.HasValue == false) && (cuesheet.Audiofile?.Duration.HasValue == true))
            //{
            //    _trackManager.SetProperty(lastTrack, x => x.End, cuesheet.Audiofile.Duration);
            //}
        }

        static Track? GetLastTrack(Cuesheet cuesheet)
        {
            return cuesheet.Audiofiles.SelectMany(x => x.Tracks)
                .OrderByDescending(x => x.Position.HasValue).ThenBy(x => x.Position)
                .ThenByDescending(x => x.Begin.HasValue).ThenBy(x => x.Begin)
                .ThenByDescending(x => x.End.HasValue).ThenBy(x => x.End)
                .LastOrDefault();
        }
    }
}
