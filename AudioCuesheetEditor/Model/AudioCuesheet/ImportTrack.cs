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
using AudioCuesheetEditor.Shared.ResourceFiles;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class ImportTrack : ITrack
    {
        public uint? Position { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public TimeSpan? Begin { get; set; }
        public TimeSpan? End { get; set; }
        public TimeSpan? Length { get; set; }

        public string GetDisplayNameLocalized(IStringLocalizer<Localization> localizer)
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
    }
}
