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
using Blazorise.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class ImportTrack : ITrack<ImportCuesheet>
    {
        private List<Flag> flags = new List<Flag>();
        public uint? Position { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public TimeSpan? Begin { get; set; }
        public TimeSpan? End { get; set; }
        public TimeSpan? Length { get; set; }

        [JsonInclude]
        public IReadOnlyCollection<Flag> Flags
        {
            get { return flags.AsReadOnly(); }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(Flags));
                }
                flags = value.ToList();
            }
        }

        public ImportCuesheet Cuesheet { get; set; }
        /// <inheritdoc/>
        public TimeSpan? PreGap { get; set; }
        /// <inheritdoc/>
        public TimeSpan? PostGap { get; set; }

        public string GetDisplayNameLocalized(ITextLocalizer localizer)
        {
            String identifierString = null;
            if (Position != null)
            {
                identifierString += String.Format("{0},", Position);
            }
            if (identifierString == null)
            {
                if (String.IsNullOrEmpty(Artist) == false)
                {
                    identifierString += String.Format("{0},", Artist);
                }
                if (String.IsNullOrEmpty(Title) == false)
                {
                    identifierString += String.Format("{0},", Title);
                }
            }
            return String.Format("{0} ({1})", localizer[nameof(Track)], identifierString);
        }
        /// <inheritdoc/>
        public void SetFlag(Flag flag, SetFlagMode flagMode)
        {
            if (flag == null)
            {
                throw new ArgumentNullException(nameof(flag));
            }
            if ((flagMode == SetFlagMode.Add) && (Flags.Contains(flag) == false))
            {
                flags.Add(flag);
            }
            if ((flagMode == SetFlagMode.Remove) && (Flags.Contains(flag)))
            {
                flags.Remove(flag);
            }
        }

        /// <inheritdoc/>
        public void SetFlags(IEnumerable<Flag> flags)
        {
            this.flags.Clear();
            this.flags.AddRange(flags);
        }
    }
}
