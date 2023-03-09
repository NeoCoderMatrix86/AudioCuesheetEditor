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

namespace AudioCuesheetEditor.Model.IO.Export
{
    public class Exportfile
    {
        public static readonly string DefaultCuesheetFilename = "Cuesheet.cue";
        //TODO: Rename to Name
        public string Filename { get; set; } = String.Empty;
        public byte[]? Content { get; set; }
        public TimeSpan? Begin { get; set; }
        public TimeSpan? End { get; set; }
        public ExportAudiofile? ExportAudiofile { get; set; }
    }
}
