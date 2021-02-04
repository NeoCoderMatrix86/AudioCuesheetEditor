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
using AudioCuesheetEditor.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public enum SetFlagMode
    {
        Add,
        Remove
    }

    public interface ITrack : ICuesheetEntity, IEntityDisplayName
    {
        public uint? Position { get; set; }
        public String Artist { get; set; }
        public String Title { get; set; }
        public TimeSpan? Begin { get; set; }
        public TimeSpan? End { get; set; }
        public TimeSpan? Length { get; set; }
        /// <summary>
        /// Flags for this Track
        /// </summary>
        public IReadOnlyCollection<Flag> Flags { get; }
        /// <summary>
        /// Add or remove a flag
        /// </summary>
        /// <param name="flag">Flag to add/remove</param>
        /// <param name="flagMode">Add or remove</param>
        public void SetFlag(Flag flag, SetFlagMode flagMode);
        /// <summary>
        /// Set all flags contained in collection (all are added)
        /// </summary>
        /// <param name="flags"></param>
        public void SetFlags(IEnumerable<Flag> flags);
    }
}
