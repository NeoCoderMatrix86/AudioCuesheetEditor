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
        EnteredValueEquals = 0,
        EnteredValueAdd = 1,
        EnteredValueSubstract = 2
    }
    public class EditMultipleTracksModalResult(Track editedTrack, Boolean isLinkedToPreviousTrackChanged, Boolean positionChanged, Boolean artistChanged, Boolean titleChanged, Boolean beginChanged, Boolean endChanged, Boolean lengthChanged, Boolean flagsChanged, Boolean pregapChanged, Boolean postgapChanged, DynamicEditValue? positionEditMode = null, DynamicEditValue? beginEditMode = null, DynamicEditValue? endEditMode = null, DynamicEditValue? lengthEditMode = null, DynamicEditValue? pregapEditMode = null, DynamicEditValue? postgapEditMode = null)
    {
        public Track EditedTrack { get; } = editedTrack;
        public Boolean IsLinkedToPreviousTrackChanged { get; } = isLinkedToPreviousTrackChanged;
        public Boolean PositionChanged { get; } = positionChanged;
        public DynamicEditValue? PositionEditMode { get; } = positionEditMode;
        public Boolean ArtistChanged { get; } = artistChanged;
        public Boolean TitleChanged { get; } = titleChanged;
        public Boolean BeginChanged { get; } = beginChanged;
        public DynamicEditValue? BeginEditMode { get; } = beginEditMode;
        public Boolean EndChanged { get; } = endChanged;
        public DynamicEditValue? EndEditMode { get; } = endEditMode;
        public Boolean LengthChanged { get; } = lengthChanged;
        public DynamicEditValue? LengthEditMode { get; } = lengthEditMode;
        public Boolean FlagsChanged { get; } = flagsChanged;
        public Boolean PregapChanged { get; } = pregapChanged;
        public DynamicEditValue? PregapEditMode { get; } = pregapEditMode;
        public Boolean PostgapChanged { get; } = postgapChanged;
        public DynamicEditValue? PostgapEditMode { get; } = postgapEditMode;
    }
}
