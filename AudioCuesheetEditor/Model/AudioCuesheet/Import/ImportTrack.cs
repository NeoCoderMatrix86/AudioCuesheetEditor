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

namespace AudioCuesheetEditor.Model.AudioCuesheet.Import
{
    public class ImportTrack : ITrack
    {
        private readonly List<Flag> flags = [];
        public string? Artist { get; set; }
        public string? Title { get; set; }
        public uint? Position { get; set; }
        public TimeSpan? Begin { get; set; }
        public TimeSpan? End { get; set; }
        public TimeSpan? Length { get; set; }
        public IReadOnlyCollection<Flag> Flags => flags;
        public TimeSpan? PreGap { get; set; }
        public TimeSpan? PostGap { get; set; }
        public void SetFlags(IEnumerable<Flag> flags)
        {
            this.flags.Clear();
            this.flags.AddRange(flags);
        }
    }
}
