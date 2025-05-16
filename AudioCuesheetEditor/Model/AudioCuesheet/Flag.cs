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
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    [JsonConverter(typeof(FlagJsonConverter))]
    public class Flag
    {
        /// <summary>
        /// 4CH (Four channel audio)
        /// </summary>
        public static readonly Flag FourCH = new("4 channel audio (4CH)", "4CH");
        /// <summary>
        /// DCP (Digital copy permitted)
        /// </summary>
        public static readonly Flag DCP = new("Digital copy permitted (DCP)", "DCP");
        /// <summary>
        /// PRE (Pre-emphasis enabled)
        /// </summary>
        public static readonly Flag PRE = new("Pre-emphasis enabled (PRE)", "PRE");
        /// <summary>
        /// SCMS (Serial copy management system)
        /// </summary>
        public static readonly Flag SCMS = new("Serial copy management system (SCMS)", "SCMS");

        public static readonly IReadOnlyCollection<Flag> AvailableFlags;
        static Flag()
        {
            var list = new List<Flag>
            {
                FourCH,
                DCP,
                PRE,
                SCMS
            };
            AvailableFlags = list.AsReadOnly();
        }

        public String Name { get; private set; }
        public String CuesheetLabel { get; private set; }
        private Flag(String name, String cuesheetLabel)
        {
            if (String.IsNullOrEmpty(name) == true)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (String.IsNullOrEmpty(cuesheetLabel) == true)
            {
                throw new ArgumentNullException(nameof(cuesheetLabel));
            }
            Name = name;
            CuesheetLabel = cuesheetLabel;
        }
    }
}
