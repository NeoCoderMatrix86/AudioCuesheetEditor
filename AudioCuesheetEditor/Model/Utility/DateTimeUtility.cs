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
using AudioCuesheetEditor.Model.Options;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.Model.Utility
{
    public class DateTimeUtility
    {
        public const String Days = "Days";
        public const String Hours = "Hours";
        public const String Minutes = "Minutes";
        public const String Seconds = "Seconds";
        public const String Milliseconds = "Milliseconds";

        public static TimeSpan? ParseTimeSpan(String input, Timespanformat? timespanformat = null)
        {
            TimeSpan? result = null;
            if (String.IsNullOrEmpty(input) == false)
            {
                if (timespanformat == null)
                {
                    if (TimeSpan.TryParse(input, out var parsed))
                    {
                        result = parsed;
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(timespanformat.Scheme) == false)
                    {
                        var match = Regex.Match(input, timespanformat.Scheme);
                        if (match.Success)
                        {
                            int days = 0;
                            int hours = 0;
                            int minutes = 0;
                            int seconds = 0;
                            int milliseconds = 0;
                            for (int groupCounter = 1; groupCounter < match.Groups.Count; groupCounter++)
                            {
                                var key = match.Groups.Keys.ElementAt(groupCounter);
                                var group = match.Groups.GetValueOrDefault(key);
                                if ((group != null) && group.Success)
                                {
                                    switch (key)
                                    {
                                        case Days:
                                            days = Convert.ToInt32(group.Value);
                                            break;
                                        case Hours:
                                            hours = Convert.ToInt32(group.Value);
                                            break;
                                        case Minutes:
                                            minutes = Convert.ToInt32(group.Value);
                                            break;
                                        case Seconds:
                                            seconds = Convert.ToInt32(group.Value);
                                            break;
                                        case Milliseconds:
                                            milliseconds = Convert.ToInt32(group.Value);
                                            break;
                                    }
                                }
                            }
                            result = new TimeSpan(days, hours, minutes, seconds, milliseconds);
                        }
                    }
                }
            }
            return result;
        }
    }
}
