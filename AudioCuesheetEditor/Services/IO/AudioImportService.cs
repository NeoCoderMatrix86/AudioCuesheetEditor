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
using AudioCuesheetEditor.Services.AudioCuesheet;
using AudioCuesheetEditor.Services.UI;
using AudioCuesheetEditor.Shared.Dialogs;
using MudBlazor;

namespace AudioCuesheetEditor.Services.IO
{
    /// <inheritdoc/>
    public class AudioImportService(DialogManager dialogManager, ISessionStateContainer sessionStateContainer, ICuesheetManager cuesheetManager) : IAudioImportService
    {
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly DialogManager _dialogManager = dialogManager;
        private readonly ICuesheetManager _cuesheetManager = cuesheetManager;

        /// <inheritdoc/>
        public async Task MapAudioImportAsync()
        {
            //TODO: Tests?
            var mappingRequired = MapImportfiles();
            var cancel = false;
            if (mappingRequired)
            {
                cancel = await _dialogManager.ShowImportAudiofilesDialogAsync();
            }
            if (cancel == false)
            {
                var files = new List<Audiofile>(_sessionStateContainer.ActiveCuesheet!.Audiofiles);
                foreach (var mapping in _sessionStateContainer.ImportAudiofileMapping.Where(x => x.Value != null))
                {
                    var audiofile = files.FirstOrDefault(x => x.Equals(mapping.Key));
                    if (audiofile == null)
                    {
                        audiofile = mapping.Key;
                        files.Add(audiofile);
                    }
                    audiofile.ObjectURL = mapping.Value!.ObjectURL;
                    audiofile.AudioCodec = mapping.Value!.AudioCodec;
                    audiofile.Duration = mapping.Value!.Duration;
                    audiofile.Name = mapping.Value!.Name;
                }
                _cuesheetManager.SetProperty(x => x.Audiofiles, files);
            }
            await _sessionStateContainer.ResetImportAsync(cancel);
        }

        Boolean MapImportfiles()
        {
            _sessionStateContainer.ImportAudiofileMapping.Clear();
            foreach (var audiofile in _sessionStateContainer.ActiveCuesheet!.Audiofiles)
            {
                if (string.IsNullOrEmpty(audiofile.Name))
                {
                    var importAudiofile = _sessionStateContainer.ImportAudiofiles.FirstOrDefault();
                    _sessionStateContainer.ImportAudiofileMapping.Add(audiofile, importAudiofile);
                    if (importAudiofile != null)
                    {
                        _sessionStateContainer.ImportAudiofiles.Remove(importAudiofile);
                    }
                }
                else
                {
                    _sessionStateContainer.ImportAudiofileMapping.Add(audiofile, null);
                }
            }
            return _sessionStateContainer.ImportAudiofiles.Count > 0;
        }
    }
}
