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
using AudioCuesheetEditor.Model.Reflection;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.Model.Utility
{
    public class Timespanformat : Validateable
    {
        public const String Days = "Days";
        public const String Hours = "Hours";
        public const String Minutes = "Minutes";
        public const String Seconds = "Seconds";
        public const String Milliseconds = "Milliseconds";

        private string? scheme;

        public const String EnterRegularExpressionHere = "ENTER REGULAR EXPRESSION HERE";
        public static readonly IReadOnlyDictionary<String, String> AvailableTimespanScheme;

        static Timespanformat()
        {
            var schemeDays = String.Format("(?'{0}.{1}'{2})", nameof(Timespanformat), nameof(Timespanformat.Days), EnterRegularExpressionHere);
            var schemeHours = String.Format("(?'{0}.{1}'{2})", nameof(Timespanformat), nameof(Timespanformat.Hours), EnterRegularExpressionHere);
            var schemeMinutes = String.Format("(?'{0}.{1}'{2})", nameof(Timespanformat), nameof(Timespanformat.Minutes), EnterRegularExpressionHere);
            var schemeSeconds = String.Format("(?'{0}.{1}'{2})", nameof(Timespanformat), nameof(Timespanformat.Seconds), EnterRegularExpressionHere);
            var schemeMilliseconds = String.Format("(?'{0}.{1}'{2})", nameof(Timespanformat), nameof(Timespanformat.Milliseconds), EnterRegularExpressionHere);

            AvailableTimespanScheme = new Dictionary<string, string>
            {
                { nameof(Timespanformat.Days), schemeDays },
                { nameof(Timespanformat.Hours), schemeHours },
                { nameof(Timespanformat.Minutes), schemeMinutes },
                { nameof(Timespanformat.Seconds), schemeSeconds },
                { nameof(Timespanformat.Milliseconds), schemeMilliseconds }
            };
        }

        public String? Scheme
        {
            get => scheme;
            set
            {
                scheme = value;
                OnValidateablePropertyChanged();
            }
        }

        public TimeSpan? ParseTimeSpan(String input)
        {
            TimeSpan? timespan = null;
            if (String.IsNullOrEmpty(Scheme) == false)
            {
                var match = Regex.Match(input, Scheme);
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
                    timespan = new TimeSpan(days, hours, minutes, seconds, milliseconds);
                }
            }
            return timespan;
        }

        protected override void Validate()
        {
            //TODO
            //throw new NotImplementedException();
        }
    }
}
