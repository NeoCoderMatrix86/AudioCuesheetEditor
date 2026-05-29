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
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace AudioCuesheetEditor.Services.AudioCuesheet
{
    public class AudiofileManager(IFileInputManager fileInputManager, ITraceChangeManager traceChangeManager, IJSRuntime jsRuntime) : IAudiofileManager
    {
        private readonly IFileInputManager _fileInputManager = fileInputManager;
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        /// <inheritdoc/>
        public async Task SetPropertiesAsync(Audiofile audiofile, IBrowserFile? browserFile, string fileInputId)
        {
            if (browserFile == null)
            {
                SetProperties(audiofile, null, null, null, null);
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
                SetProperties(audiofile, codec, browserFile.Name, objectUrl, duration);
            }
        }

        void SetProperties(Audiofile audiofile, AudioCodec? audioCodec, string? name, string? objectUrl, TimeSpan? duration)
        {
            _traceChangeManager.BulkEdit = true;
            if (audiofile.AudioCodec != audioCodec)
            {
                _traceChangeManager.AddChange(new(audiofile, new(audiofile.AudioCodec, nameof(Audiofile.AudioCodec))));
                audiofile.AudioCodec = audioCodec;
            }
            if (audiofile.Name != name)
            {
                _traceChangeManager.AddChange(new(audiofile, new(audiofile.Name, nameof(Audiofile.Name))));
                audiofile.Name = name;
            }
            if (audiofile.ObjectURL != objectUrl)
            {
                _traceChangeManager.AddChange(new(audiofile, new(audiofile.ObjectURL, nameof(Audiofile.ObjectURL))));
                audiofile.ObjectURL = objectUrl;
            }
            if (audiofile.Duration != duration)
            {
                _traceChangeManager.AddChange(new(audiofile, new(audiofile.Duration, nameof(Audiofile.Duration))));
                audiofile.Duration = duration;
            }
            _traceChangeManager.BulkEdit = false;
        }
    }
}
