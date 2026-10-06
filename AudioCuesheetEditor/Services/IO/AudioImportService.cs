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
using AudioCuesheetEditor.Services.UI;
using AudioCuesheetEditor.Shared.Dialogs;
using MudBlazor;

namespace AudioCuesheetEditor.Services.IO
{
    /// <inheritdoc/>
    public class AudioImportService(IDialogService dialogService, ISessionStateContainer sessionStateContainer) : IAudioImportService
    {
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly IDialogService _dialogService = dialogService;

        /// <inheritdoc/>
        public async Task MapAudioImportAsync()
        {
            //TODO: Tests            
            foreach (var audiofile in _sessionStateContainer.ActiveCuesheet!.Audiofiles)
            {
                _sessionStateContainer.ImportAudiofileMapping.Add(audiofile, null);
            }
            //TODO: automatic mapping
            var parameters = new DialogParameters<ImportAudiofilesDialog> 
            {
                { x => x.MappedAudiofiles, _sessionStateContainer.ImportAudiofileMapping },
                { x => x.ImportAudiofiles, _sessionStateContainer.ImportAudiofiles }
            };
            var options = new DialogOptions() { BackdropClick = false, FullWidth = true };
            var dialog = await _dialogService.ShowAsync<ImportAudiofilesDialog>(null, parameters, options);
            var result = await dialog.Result;
            if (result?.Canceled == false)
            {
                if (result.Data is Dictionary<Audiofile, Audiofile?> audiofileMapping)
                {
                    _sessionStateContainer.ImportAudiofileMapping = audiofileMapping;
                    //TODO: Apply mapping to cuesheet
                }
            }
            else
            {
                _sessionStateContainer.ResetImport();
            }
        }
    }
}
