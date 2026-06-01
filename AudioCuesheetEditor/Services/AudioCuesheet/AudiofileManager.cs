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
using System.Linq.Expressions;
using System.Reflection;

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

        public void SetProperty<TProperty>(Audiofile audiofile, Expression<Func<Audiofile, TProperty>> propertyExpression, TProperty value)
        {
            //TODO: Tests
            SetValue(audiofile, propertyExpression, value);
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
    }
}
