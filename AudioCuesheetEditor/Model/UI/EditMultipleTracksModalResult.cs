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

namespace AudioCuesheetEditor.Model.UI
{
    public enum DynamicEditValue
    {
        DoNotChange = 0,
        EnteredValueEquals = 1,
        EnteredValueAdd = 2,
        EnteredValueSubstract = 3
    }
    public class EditMultipleTracksModalResult(Track editedTrack, DynamicEditValue isLinkedToPreviousTrackChanged, DynamicEditValue positionEditMode, DynamicEditValue artistEditMode, DynamicEditValue titleEditMode, DynamicEditValue beginEditMode, DynamicEditValue endEditMode, DynamicEditValue lengthEditMode, DynamicEditValue flagsEditMode, DynamicEditValue pregapEditMode, DynamicEditValue postgapEditMode)
    {
        public Track EditedTrack { get; } = editedTrack;
        public DynamicEditValue IsLinkedToPreviousTrackEditMode { get; } = isLinkedToPreviousTrackChanged;
        public DynamicEditValue PositionEditMode { get; } = positionEditMode;
        public DynamicEditValue ArtistEditMode { get; } = artistEditMode;
        public DynamicEditValue TitleEditMode { get; } = titleEditMode;
        public DynamicEditValue BeginEditMode { get; } = beginEditMode;
        public DynamicEditValue EndEditMode { get; } = endEditMode;
        public DynamicEditValue LengthEditMode { get; } = lengthEditMode;
        public DynamicEditValue FlagsEditMode { get; } = flagsEditMode;
        public DynamicEditValue PregapEditMode { get; } = pregapEditMode;
        public DynamicEditValue PostgapEditMode { get; } = postgapEditMode;
    }
}
