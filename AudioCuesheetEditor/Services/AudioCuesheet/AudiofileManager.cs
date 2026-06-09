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
            //TODO: Tests
            _traceChangeManager.BulkEdit = true;
            if (browserFile == null)
            {
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
            //TODO: Tests
            SetValue(audiofile, propertyExpression, value);
        }

        /// <inheritdoc/>
        public void AddTrack(Audiofile audiofile, Track track)
        {
            //TODO: Tests
            //Calculate track properties
            _traceChangeManager.BulkEdit = true;
            var cuesheet = _sessionStateContainer.GetActiveCuesheet();
            if (cuesheet?.IsRecording == true)
            {
                _trackManager.SetProperty(track, x => x.Begin, DateTime.UtcNow - cuesheet.RecordingStart);
            }
            if (cuesheet?.Audiofiles.SelectMany(x => x.Tracks).Any() == false)
            {
                _trackManager.SetProperty(track, x => x.Position, (ushort)(1));
                if ((track.Begin.HasValue == false) || cuesheet?.IsRecording == true)
                {
                    _trackManager.SetProperty(track, x => x.Begin, TimeSpan.Zero);
                }
            }
            else
            {
                //TODO: calculate position and begin/end upgoing
                //TODO: adapt to multiple audio files
                var lastTrack = GetLastTrack(cuesheet!);
                //if ((cuesheet?.Audiofile?.Duration.HasValue == true) && (lastTrack?.End.HasValue == true) && (lastTrack.End == cuesheet.Audiofile.Duration))
                //{
                //    _trackManager.SetProperty(lastTrack, x => x.End, null);
                //}
                if (track.Position.HasValue == false)
                {
                    _trackManager.SetProperty(track, x => x.Position, (ushort?)(lastTrack?.Position + 1));
                }
                if (track.Begin.HasValue == false)
                {
                    _trackManager.SetProperty(track, x => x.Begin, lastTrack?.End);
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
            var newValue = new List<Track>(audiofile.Tracks)
            {
                track
            };
            SetValue(audiofile, x => x.Tracks, newValue);
            SetLastTrackEnd(cuesheet!);
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
