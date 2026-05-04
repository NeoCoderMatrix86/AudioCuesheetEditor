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
using AudioCuesheetEditor.Model.UI;
using AudioCuesheetEditor.Services.AudioCuesheet;
using AudioCuesheetEditor.Shared.Dialogs;
using MudBlazor;

namespace AudioCuesheetEditor.Services.UI
{
    public class DialogManager(IDialogService dialogService, ITraceChangeManager traceChangeManager, ITrackManager trackManager)
    {
        private readonly IDialogService _dialogService = dialogService;
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
        private readonly ITrackManager _trackManager = trackManager;

        private IDialogReference? _loadingDialog;

        public async Task ShowAndHandleModalEditDialogAsync(IEnumerable<Track> tracks)
        {
            _traceChangeManager.BulkEdit = true;
            if (tracks.Count() == 1)
            {
                var parameters = new DialogParameters<EditTrackModal> { { x => x.EditedTrack, _trackManager.Clone(tracks.First()) } };
                var options = new DialogOptions() { CloseOnEscapeKey = true, BackdropClick = false, FullWidth = true, CloseButton = true };
                var dialog = await _dialogService.ShowAsync<EditTrackModal>(null, parameters, options);
                var result = await dialog.Result;
                if ((result?.Canceled == false) && (result.Data is Track editedTrack))
                {
                    _trackManager.CopyValues(editedTrack, tracks.First());
                    _trackManager.RecalculateLinkedTracksProperties(tracks.First());
                }
            }
            if (tracks.Count() > 1)
            {
                var parameters = new DialogParameters<EditMultipleTracksModal> { { x => x.EditedTrack, new() } };
                var options = new DialogOptions() { CloseOnEscapeKey = true, BackdropClick = false, FullWidth = true, CloseButton = true };
                var dialog = await _dialogService.ShowAsync<EditMultipleTracksModal>(null, parameters, options);
                var result = await dialog.Result;
                if ((result?.Canceled == false) && (result.Data is EditMultipleTracksModalResult editMultipleTracksModalResult))
                {
                    foreach (var track in tracks)
                    {
                        var position = editMultipleTracksModalResult.EditedTrack.Position;
                        var begin = editMultipleTracksModalResult.EditedTrack.Begin;
                        var end = editMultipleTracksModalResult.EditedTrack.End;
                        var length = editMultipleTracksModalResult.EditedTrack.Length;
                        var preGap = editMultipleTracksModalResult.EditedTrack.PreGap;
                        var postGap = editMultipleTracksModalResult.EditedTrack.PostGap;
                        Boolean copyIsLinkedToPreviousTrack = editMultipleTracksModalResult.IsLinkedToPreviousTrackEditMode == DynamicEditValue.EnteredValueEquals;
                        Boolean copyTrackArtist = editMultipleTracksModalResult.ArtistEditMode == DynamicEditValue.EnteredValueEquals;
                        Boolean copyTrackTitle = editMultipleTracksModalResult.TitleEditMode == DynamicEditValue.EnteredValueEquals;
                        Boolean copyTrackBegin = true;
                        Boolean copyTrackEnd = true;
                        Boolean copyTrackLength = true;
                        Boolean copyTrackFlags = editMultipleTracksModalResult.FlagsEditMode == DynamicEditValue.EnteredValueEquals;
                        Boolean copyTrackPreGap = true;
                        Boolean copyTrackPostGap = true;
                        //First process dynamic edit, because we need to increase each value separately
                        switch (editMultipleTracksModalResult.BeginEditMode)
                        {
                            case DynamicEditValue.DoNotChange:
                                copyTrackBegin = false;
                                break;
                            case DynamicEditValue.EnteredValueEquals:
                                copyTrackBegin = true;
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                var newValue = editMultipleTracksModalResult.EditedTrack.Begin + track.Begin;
                                editMultipleTracksModalResult.EditedTrack.Begin = newValue;
                                _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: copyTrackBegin, setEnd: false, setLength: false, setFlags: false, setPreGap: false, setPostGap: false);
                                copyTrackBegin = false;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                newValue = track.Begin - editMultipleTracksModalResult.EditedTrack.Begin;
                                editMultipleTracksModalResult.EditedTrack.Begin = newValue;
                                _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: copyTrackBegin, setEnd: false, setLength: false, setFlags: false, setPreGap: false, setPostGap: false);
                                copyTrackBegin = false;
                                break;
                        }
                        switch (editMultipleTracksModalResult.EndEditMode)
                        {
                            case DynamicEditValue.DoNotChange:
                                copyTrackEnd = false;
                                break;
                            case DynamicEditValue.EnteredValueEquals:
                                copyTrackEnd = true;
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                var newValue = editMultipleTracksModalResult.EditedTrack.End + track.End;
                                editMultipleTracksModalResult.EditedTrack.End = newValue;
                                _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: copyTrackEnd, setLength: false, setFlags: false, setPreGap: false, setPostGap: false);
                                copyTrackEnd = false;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                newValue = track.End - editMultipleTracksModalResult.EditedTrack.End;
                                editMultipleTracksModalResult.EditedTrack.End = newValue;
                                _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: copyTrackEnd, setLength: false, setFlags: false, setPreGap: false, setPostGap: false);
                                copyTrackEnd = false;
                                break;
                        }
                        switch (editMultipleTracksModalResult.LengthEditMode)
                        {
                            case DynamicEditValue.DoNotChange:
                                copyTrackLength = false;
                                break;
                            case DynamicEditValue.EnteredValueEquals:
                                copyTrackLength = true;
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                var newValue = editMultipleTracksModalResult.EditedTrack.Length + track.Length;
                                editMultipleTracksModalResult.EditedTrack.Length = newValue;
                                _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: copyTrackLength, setFlags: false, setPreGap: false, setPostGap: false);
                                copyTrackLength = false;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                newValue = track.Length - editMultipleTracksModalResult.EditedTrack.Length;
                                editMultipleTracksModalResult.EditedTrack.Length = newValue;
                                _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: copyTrackLength, setFlags: false, setPreGap: false, setPostGap: false);
                                copyTrackLength = false;
                                break;
                        }
                        switch (editMultipleTracksModalResult.PregapEditMode)
                        {
                            case DynamicEditValue.DoNotChange:
                                copyTrackPreGap = false;
                                break;
                            case DynamicEditValue.EnteredValueEquals:
                                copyTrackPreGap = true;
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                var newValue = editMultipleTracksModalResult.EditedTrack.PreGap + track.PreGap;
                                editMultipleTracksModalResult.EditedTrack.PreGap = newValue;
                                _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: false, setFlags: false, setPreGap: copyTrackPreGap, setPostGap: false);
                                copyTrackPreGap = false;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                newValue = track.PreGap - editMultipleTracksModalResult.EditedTrack.PreGap;
                                editMultipleTracksModalResult.EditedTrack.PreGap = newValue;
                                _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: false, setFlags: false, setPreGap: copyTrackPreGap, setPostGap: false);
                                copyTrackPreGap = false;
                                break;
                        }
                        switch (editMultipleTracksModalResult.PostgapEditMode)
                        {
                            case DynamicEditValue.DoNotChange:
                                copyTrackPostGap = false;
                                break;
                            case DynamicEditValue.EnteredValueEquals:
                                copyTrackPostGap = true;
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                var newValue = editMultipleTracksModalResult.EditedTrack.PostGap + track.PostGap;
                                editMultipleTracksModalResult.EditedTrack.PostGap = newValue;
                                _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: false, setFlags: false, setPreGap: false, setPostGap: copyTrackPostGap);
                                copyTrackPostGap = false;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                newValue = track.PostGap - editMultipleTracksModalResult.EditedTrack.PostGap;
                                editMultipleTracksModalResult.EditedTrack.PostGap = newValue;
                                _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: false, setFlags: false, setPreGap: false, setPostGap: copyTrackPostGap);
                                copyTrackPostGap = false;
                                break;
                        }
                        editMultipleTracksModalResult.EditedTrack.Position = position;
                        editMultipleTracksModalResult.EditedTrack.Begin = begin;
                        editMultipleTracksModalResult.EditedTrack.End = end;
                        editMultipleTracksModalResult.EditedTrack.Length = length;
                        editMultipleTracksModalResult.EditedTrack.PreGap = preGap;
                        editMultipleTracksModalResult.EditedTrack.PostGap = postGap;
                        //Now copy all values
                        _trackManager.CopyValues(editMultipleTracksModalResult.EditedTrack, track, setIsLinkedToPreviousTrack: copyIsLinkedToPreviousTrack, setPosition: false, setArtist: copyTrackArtist, setTitle: copyTrackTitle, setBegin: copyTrackBegin, setEnd: copyTrackEnd, setLength: copyTrackLength, setFlags: copyTrackFlags, setPreGap: copyTrackPreGap, setPostGap: copyTrackPostGap);
                    }
                }
            }
            _traceChangeManager.BulkEdit = false;
        }

        public async Task ShowLoadingDialogAsync()
        {
            if (_loadingDialog == null)
            { 
                var options = new DialogOptions() { BackdropClick = false, FullWidth = true, MaxWidth = MaxWidth.ExtraSmall, NoHeader = true };
                _loadingDialog = await _dialogService.ShowAsync<LoadingDialog>(null, options);
                await Task.Delay(1);
            }
        }

        public void HideLoadingDialog()
        {
            _loadingDialog?.Close();
            _loadingDialog = null;
        }
    }
}
