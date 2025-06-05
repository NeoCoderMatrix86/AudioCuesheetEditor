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
namespace AudioCuesheetEditor.Model.IO.Audio
{
    public interface IAudiofile
    {
        AudioCodec? AudioCodec { get; }
        string? AudioFileType { get; }
        Stream? ContentStream { get; set; }
        TimeSpan? Duration { get; }
        bool IsContentStreamLoaded { get; }
        string Name { get; set; }
        string? ObjectURL { get; }
        bool PlaybackPossible { get; }

        event EventHandler? ContentStreamLoaded;
    }
}