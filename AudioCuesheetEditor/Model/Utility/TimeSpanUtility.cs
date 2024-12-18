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
namespace AudioCuesheetEditor.Model.Utility
{
    public class TimeSpanUtility
    {
        public static TimeSpan? ParseTimeSpan(String input, TimeSpanFormat? timeSpanFormat = null)
        {
            TimeSpan? result = null;
            if (String.IsNullOrEmpty(input) == false)
            {
                if (String.IsNullOrEmpty(timeSpanFormat?.Scheme))
                {
                    if (TimeSpan.TryParse(input, out var parsed))
                    {
                        result = parsed;
                    }
                }
                else
                {
                    result = timeSpanFormat.ParseTimeSpan(input);
                }
            }
            return result;
        }
    }
}
