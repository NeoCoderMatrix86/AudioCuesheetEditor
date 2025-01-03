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
using AudioCuesheetEditor.Extensions;
using AudioCuesheetEditor.Model.AudioCuesheet;

namespace AudioCuesheetEditor.Services.Audio
{
    public class PlaybackService(SessionStateContainer sessionStateContainer)
    {
        private readonly SessionStateContainer _sessionStateContainer = sessionStateContainer;
        //TODO: here comes all playback related stuff
        public TimeSpan? CurrentPosition { get; private set; }
        public TimeSpan? TotalTime { get; private set; }
        //Refer to Cuesheet (not ImportCuesheet) since playback will always be done on the imported cuesheet
        public Boolean PlaybackPossible => _sessionStateContainer.Cuesheet.Audiofile != null ? _sessionStateContainer.Cuesheet.Audiofile.PlaybackPossible : false;

    }
}
