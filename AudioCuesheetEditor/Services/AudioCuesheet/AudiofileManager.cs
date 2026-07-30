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
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Linq.Expressions;
using System.Reflection;

namespace AudioCuesheetEditor.Services.AudioCuesheet
{
    public class AudiofileManager(IFileInputManager fileInputManager, ITraceChangeManager traceChangeManager, IJSRuntime jsRuntime, ITrackManager trackManager, ISessionStateContainer sessionStateContainer) : IAudiofileManager
    {
        private readonly IFileInputManager _fileInputManager = fileInputManager;
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
        private readonly IJSRuntime _jsRuntime = jsRuntime;
        private readonly ITrackManager _trackManager = trackManager;
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;

        /// <inheritdoc/>
        public async Task SetPropertiesAsync(Audiofile audiofile, IBrowserFile? browserFile, string fileInputId)
        {
            _traceChangeManager.BulkEdit = true;
            if (browserFile == null)
            {
                if (string.IsNullOrEmpty(audiofile.ObjectURL) == false)
                {
                    await _jsRuntime.InvokeVoidAsync("revokeAudioObjectURL", audiofile.ObjectURL);
                }
                SetValue(audiofile, x => x.AudioCodec, null);
                SetValue(audiofile, x => x.Name, null);
                SetValue(audiofile, x => x.ObjectURL, null);
                SetValue(audiofile, x => x.Duration, null);
            }
            else
            {
                var codec = _fileInputManager.GetAudioCodec(browserFile.ContentType, browserFile.Name);
                var objectUrl = await _fileInputManager.GetObjectUrlAsync(fileInputId);
                TimeSpan? duration = null;
                if (String.IsNullOrEmpty(objectUrl) == false)
                {
                    var durationSeconds = await _jsRuntime.InvokeAsync<double>("getAudioDurationFromFile", objectUrl);
                    duration = TimeSpan.FromSeconds(durationSeconds);
                }
                SetValue(audiofile, x => x.AudioCodec, codec);
                SetValue(audiofile, x => x.Name, browserFile.Name);
                SetValue(audiofile, x => x.ObjectURL, objectUrl);
                SetValue(audiofile, x => x.Duration, duration);
            }
            _traceChangeManager.BulkEdit = false;
        }

        /// <inheritdoc/>
        public void SetProperty<TProperty>(Audiofile audiofile, Expression<Func<Audiofile, TProperty>> propertyExpression, TProperty value)
        {
            SetValue(audiofile, propertyExpression, value);
        }

        /// <inheritdoc/>
        public void AddTrack(Audiofile audiofile, Track track)
        {
            _traceChangeManager.BulkEdit = true;
            var cuesheet = _sessionStateContainer.GetActiveCuesheet();
            track.Cuesheet = cuesheet;
            var newValue = new List<Track>(audiofile.Tracks)
            {
                track
            };
            SetValue(audiofile, x => x.Tracks, newValue);
            RecalculateTrackProperties(cuesheet!);
            _traceChangeManager.BulkEdit = false;
        }

        /// <inheritdoc/>
        public void RemoveTracks(Audiofile audiofile, IEnumerable<Track> tracksToRemove)
        {
            var cuesheet = _sessionStateContainer.GetActiveCuesheet();
            var intersection = audiofile.Tracks.Intersect(tracksToRemove);
            ICollection<Track> newValue = [.. audiofile.Tracks.Except(intersection)];
            _traceChangeManager.BulkEdit = true;
            SetValue(audiofile, x => x.Tracks, newValue);
            RecalculateTrackProperties(cuesheet!);
            _traceChangeManager.BulkEdit = false;
        }

        void SetValue<TProperty>(Audiofile audiofile, Expression<Func<Audiofile, TProperty>> propertyExpression, TProperty value)
        {
            if (propertyExpression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("Expression must be a property");
            }

            if (memberExpression.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException("Member is not a property");
            }

            var previousValue = (TProperty?)propertyInfo.GetValue(audiofile);
            if (Equals(previousValue, value))
            {
                return;
            }

            propertyInfo.SetValue(audiofile, value);
            _traceChangeManager.AddChange(new(audiofile, new(previousValue, propertyInfo.Name)));
        }

        void RecalculateTrackProperties(Cuesheet cuesheet)
        {
            // Unset first track begin
            var firstTrack = GetFirstTrack(cuesheet);
            if (firstTrack?.Begin == TimeSpan.Zero)
            {
                _trackManager.SetProperty(firstTrack, x => x.Begin, null);
            }
            // Recalculate position, begin and end ascending
            ushort position = 1;
            foreach (var audiofile in cuesheet.Audiofiles)
            {
                foreach (var track in audiofile.Tracks)
                {
                    if (track.Position != position)
                    {
                        _trackManager.SetProperty(track, x => x.Position, position);
                    }
                    var previousTrack = _trackManager.GetPreviousLinkedTrack(track);
                    if (previousTrack?.End.HasValue == true)
                    {
                        _trackManager.SetProperty(track, x => x.Begin, previousTrack.End);
                    }
                    position++;
                }
            }
            // Set first track begin
            firstTrack = GetFirstTrack(cuesheet);
            if (firstTrack?.Begin.HasValue == false)
            {
                _trackManager.SetProperty(firstTrack, x => x.Begin, TimeSpan.Zero);
            }
            // Set track ends based on audiofile duration
            foreach (var audiofile in cuesheet.Audiofiles)
            {
                var lastTrack = audiofile.Tracks.OrderByDescending(x => x.Position.HasValue).ThenBy(x => x.Position)
                    .ThenByDescending(x => x.Begin.HasValue).ThenBy(x => x.Begin)
                    .ThenByDescending(x => x.End.HasValue).ThenBy(x => x.End)
                    .LastOrDefault();
                if ((lastTrack?.End.HasValue == false) && (audiofile.Duration.HasValue == true))
                {
                    _trackManager.SetProperty(lastTrack, x => x.End, audiofile.Duration);
                }
            }
        }

        static Track? GetFirstTrack(Cuesheet cuesheet)
        {
            return cuesheet.Audiofiles.SelectMany(x => x.Tracks)
                .OrderByDescending(x => x.Position.HasValue).ThenBy(x => x.Position)
                .ThenByDescending(x => x.Begin.HasValue).ThenBy(x => x.Begin)
                .ThenByDescending(x => x.End.HasValue).ThenBy(x => x.End)
                .FirstOrDefault();
        }

    }
}
