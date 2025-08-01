﻿//This file is part of AudioCuesheetEditor.

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
using AudioCuesheetEditor.Model.Utility;
using AudioCuesheetEditor.Services.UI;
using System.Globalization;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.Options
{
    public class ApplicationOptions : IOptions
    {
        public const LogLevel DefaultLogLevel = LogLevel.Information;
        public String? CultureName { get; set; } = LocalizationService.DefaultCulture;
        [JsonIgnore]
        public CultureInfo Culture
        {
            get
            {
                if (String.IsNullOrEmpty(CultureName) == false)
                {
                    return new CultureInfo(CultureName);
                }
                else
                {
                    return CultureInfo.CurrentCulture;
                }
            }
        }
        public TimeSpanFormat? TimeSpanFormat { get; set; }
        public Boolean DefaultIsLinkedToPreviousTrack { get; set; } = true;
        public Boolean FixedTracksTableHeader { get; set; } = false;
        public String? DisplayTimeSpanFormat { get; set; }
        public LogLevel MinimumLogLevel { get; set; } = DefaultLogLevel;
    }
}
