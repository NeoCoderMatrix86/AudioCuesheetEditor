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
using AudioCuesheetEditor.Shared.Dialogs;
using MudBlazor;

namespace AudioCuesheetEditor.Services.UI
{
    public class DialogManager(IDialogService dialogService, ITraceChangeManager traceChangeManager)
    {
        private readonly IDialogService _dialogService = dialogService;
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;

        private IDialogReference? loadingDialog;

        public async Task ShowAndHandleModalEditDialogAsync(IEnumerable<Track> tracks)
        {
            if (tracks.Count() == 1)
            {
                var parameters = new DialogParameters<EditTrackModal> { { x => x.EditedTrack, tracks.First().Clone() } };
                var options = new DialogOptions() { CloseOnEscapeKey = true, BackdropClick = false, FullWidth = true };
                var dialog = await _dialogService.ShowAsync<EditTrackModal>(null, parameters, options);
                var result = await dialog.Result;
                if ((result?.Canceled == false) && (result.Data is Track editedTrack))
                {
                    tracks.First().CopyValues(editedTrack, setCuesheet: false);
                }
            }
            if (tracks.Count() > 1)
            {
                var parameters = new DialogParameters<EditMultipleTracksModal> { { x => x.EditedTrack, new() { AutomaticallyCalculateLength = false } } };
                var options = new DialogOptions() { CloseOnEscapeKey = true, BackdropClick = false, FullWidth = true };
                var dialog = await _dialogService.ShowAsync<EditMultipleTracksModal>(null, parameters, options);
                var result = await dialog.Result;
                if ((result?.Canceled == false) && (result.Data is EditMultipleTracksModalResult editMultipleTracksModalResult))
                {
                    _traceChangeManager.BulkEdit = true;
                    foreach (var track in tracks)
                    {
                        var position = editMultipleTracksModalResult.EditedTrack.Position;
                        var begin = editMultipleTracksModalResult.EditedTrack.Begin;
                        var end = editMultipleTracksModalResult.EditedTrack.End;
                        var length = editMultipleTracksModalResult.EditedTrack.Length;
                        var preGap = editMultipleTracksModalResult.EditedTrack.PreGap;
                        var postGap = editMultipleTracksModalResult.EditedTrack.PostGap;
                        Boolean copyTrackPosition = editMultipleTracksModalResult.PositionChanged;
                        Boolean copyTrackBegin = editMultipleTracksModalResult.BeginChanged;
                        Boolean copyTrackEnd = editMultipleTracksModalResult.EndChanged;
                        Boolean copyTrackLength = editMultipleTracksModalResult.LengthChanged;
                        Boolean copyTrackPreGap = editMultipleTracksModalResult.PregapChanged;
                        Boolean copyTrackPostGap = editMultipleTracksModalResult.PostgapChanged;
                        //First process dynamic edit, because we need to increase each value seperately
                        switch (editMultipleTracksModalResult.PositionEditMode)
                        {
                            case DynamicEditValue.EnteredValueEquals:
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                editMultipleTracksModalResult.EditedTrack.Position += track.Position;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: copyTrackPosition, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: false, setFlags: false, setPreGap: false, setPostGap: false, useInternalSetters: Track.AllPropertyNames);
                                copyTrackPosition = false;
                                editMultipleTracksModalResult.EditedTrack.Position = position;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                var newValue = track.Position - editMultipleTracksModalResult.EditedTrack.Position;
                                editMultipleTracksModalResult.EditedTrack.Position = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: copyTrackPosition, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: false, setFlags: false, setPreGap: false, setPostGap: false, useInternalSetters: Track.AllPropertyNames);
                                copyTrackPosition = false;
                                break;
                        }
                        switch (editMultipleTracksModalResult.BeginEditMode)
                        {
                            case DynamicEditValue.EnteredValueEquals:
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                var newValue = editMultipleTracksModalResult.EditedTrack.Begin + track.Begin;
                                editMultipleTracksModalResult.EditedTrack.Begin = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: copyTrackBegin, setEnd: false, setLength: false, setFlags: false, setPreGap: false, setPostGap: false, useInternalSetters: Track.AllPropertyNames);
                                copyTrackBegin = false;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                newValue = track.Begin - editMultipleTracksModalResult.EditedTrack.Begin;
                                editMultipleTracksModalResult.EditedTrack.Begin = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: copyTrackBegin, setEnd: false, setLength: false, setFlags: false, setPreGap: false, setPostGap: false, useInternalSetters: Track.AllPropertyNames);
                                copyTrackBegin = false;
                                break;
                        }
                        switch (editMultipleTracksModalResult.EndEditMode)
                        {
                            case DynamicEditValue.EnteredValueEquals:
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                var newValue = editMultipleTracksModalResult.EditedTrack.End + track.End;
                                editMultipleTracksModalResult.EditedTrack.End = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: copyTrackEnd, setLength: false, setFlags: false, setPreGap: false, setPostGap: false, useInternalSetters: Track.AllPropertyNames);
                                copyTrackEnd = false;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                newValue = track.End - editMultipleTracksModalResult.EditedTrack.End;
                                editMultipleTracksModalResult.EditedTrack.End = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: copyTrackEnd, setLength: false, setFlags: false, setPreGap: false, setPostGap: false, useInternalSetters: Track.AllPropertyNames);
                                copyTrackEnd = false;
                                break;
                        }
                        switch (editMultipleTracksModalResult.LengthEditMode)
                        {
                            case DynamicEditValue.EnteredValueEquals:
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                var newValue = editMultipleTracksModalResult.EditedTrack.Length + track.Length;
                                editMultipleTracksModalResult.EditedTrack.Length = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: copyTrackLength, setFlags: false, setPreGap: false, setPostGap: false, useInternalSetters: Track.AllPropertyNames);
                                copyTrackLength = false;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                newValue = track.Length - editMultipleTracksModalResult.EditedTrack.Length;
                                editMultipleTracksModalResult.EditedTrack.Length = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: copyTrackLength, setFlags: false, setPreGap: false, setPostGap: false, useInternalSetters: Track.AllPropertyNames);
                                copyTrackLength = false;
                                break;
                        }
                        switch (editMultipleTracksModalResult.PregapEditMode)
                        {
                            case DynamicEditValue.EnteredValueEquals:
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                var newValue = editMultipleTracksModalResult.EditedTrack.PreGap + track.PreGap;
                                editMultipleTracksModalResult.EditedTrack.PreGap = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: false, setFlags: false, setPreGap: copyTrackPreGap, setPostGap: false, useInternalSetters: Track.AllPropertyNames);
                                copyTrackPreGap = false;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                newValue = track.PreGap - editMultipleTracksModalResult.EditedTrack.PreGap;
                                editMultipleTracksModalResult.EditedTrack.PreGap = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: false, setFlags: false, setPreGap: copyTrackPreGap, setPostGap: false, useInternalSetters: Track.AllPropertyNames);
                                copyTrackPreGap = false;
                                break;
                        }
                        switch (editMultipleTracksModalResult.PostgapEditMode)
                        {
                            case DynamicEditValue.EnteredValueEquals:
                                break;
                            case DynamicEditValue.EnteredValueAdd:
                                var newValue = editMultipleTracksModalResult.EditedTrack.PostGap + track.PostGap;
                                editMultipleTracksModalResult.EditedTrack.PostGap = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: false, setFlags: false, setPreGap: false, setPostGap: copyTrackPostGap, useInternalSetters: Track.AllPropertyNames);
                                copyTrackPostGap = false;
                                break;
                            case DynamicEditValue.EnteredValueSubstract:
                                newValue = track.PostGap - editMultipleTracksModalResult.EditedTrack.PostGap;
                                editMultipleTracksModalResult.EditedTrack.PostGap = newValue;
                                track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: false, setPosition: false, setArtist: false, setTitle: false, setBegin: false, setEnd: false, setLength: false, setFlags: false, setPreGap: false, setPostGap: copyTrackPostGap, useInternalSetters: Track.AllPropertyNames);
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
                        track.CopyValues(editMultipleTracksModalResult.EditedTrack, setCuesheet: false, setIsLinkedToPreviousTrack: editMultipleTracksModalResult.IsLinkedToPreviousTrackChanged, setPosition: copyTrackPosition, setArtist: editMultipleTracksModalResult.ArtistChanged, setTitle: editMultipleTracksModalResult.TitleChanged, setBegin: copyTrackBegin, setEnd: copyTrackEnd, setLength: copyTrackLength, setFlags: editMultipleTracksModalResult.FlagsChanged, setPreGap: copyTrackPreGap, setPostGap: copyTrackPostGap);
                    }
                    _traceChangeManager.BulkEdit = false;
                }
            }
        }

        public async Task ShowLoadingDialogAsync()
        {
            if (loadingDialog == null)
            { 
                var options = new DialogOptions() { BackdropClick = false, FullWidth = true, MaxWidth = MaxWidth.ExtraSmall, NoHeader = true };
                loadingDialog = await _dialogService.ShowAsync<LoadingDialog>(null, options);
            }
        }

        public void HideLoadingDialog()
        {
            if (loadingDialog != null)
            {
                loadingDialog.Close();
                loadingDialog = null;
            }
        }
    }
}
