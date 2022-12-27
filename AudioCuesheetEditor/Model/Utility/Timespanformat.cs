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
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.Model.Utility
{
    public class TimeSpanFormat : Validateable<TimeSpanFormat>
    {
        public const String Days = "Days";
        public const String Hours = "Hours";
        public const String Minutes = "Minutes";
        public const String Seconds = "Seconds";
        public const String Milliseconds = "Milliseconds";

        private string? scheme;

        public const String EnterRegularExpressionHere = "ENTER REGULAR EXPRESSION HERE";
        public static readonly IReadOnlyDictionary<String, String> AvailableTimespanScheme;

        static TimeSpanFormat()
        {
            var schemeDays = String.Format("(?'{0}.{1}'{2})", nameof(TimeSpanFormat), nameof(Days), EnterRegularExpressionHere);
            var schemeHours = String.Format("(?'{0}.{1}'{2})", nameof(TimeSpanFormat), nameof(Hours), EnterRegularExpressionHere);
            var schemeMinutes = String.Format("(?'{0}.{1}'{2})", nameof(TimeSpanFormat), nameof(Minutes), EnterRegularExpressionHere);
            var schemeSeconds = String.Format("(?'{0}.{1}'{2})", nameof(TimeSpanFormat), nameof(Seconds), EnterRegularExpressionHere);
            var schemeMilliseconds = String.Format("(?'{0}.{1}'{2})", nameof(TimeSpanFormat), nameof(Milliseconds), EnterRegularExpressionHere);

            AvailableTimespanScheme = new Dictionary<string, string>
            {
                { nameof(TimeSpanFormat.Days), schemeDays },
                { nameof(TimeSpanFormat.Hours), schemeHours },
                { nameof(TimeSpanFormat.Minutes), schemeMinutes },
                { nameof(TimeSpanFormat.Seconds), schemeSeconds },
                { nameof(TimeSpanFormat.Milliseconds), schemeMilliseconds }
            };
        }

        public event EventHandler? SchemeChanged;

        public String? Scheme
        {
            get => scheme;
            set
            {
                scheme = value;
                //TODO
                //OnValidateablePropertyChanged();
                SchemeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public TimeSpan? ParseTimeSpan(String input)
        {
            TimeSpan? timespan = null;
            if (String.IsNullOrEmpty(Scheme) == false)
            {
                var match = Regex.Match(input, Scheme.Replace(String.Format("{0}.", nameof(TimeSpanFormat)), ""));
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
        //TODO
        protected override ValidationResult Validate(string property)
        {
            throw new NotImplementedException();
        }

        //TODO
        //protected override void Validate()
        //{
        //    if (String.IsNullOrEmpty(Scheme))
        //    {
        //        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Scheme)), ValidationErrorType.Warning, "{0} has no value!", nameof(Scheme)));
        //    }
        //    else
        //    {
        //        if ((Scheme.Contains(Days) == false)
        //            && (Scheme.Contains(Hours) == false)
        //            && (Scheme.Contains(Minutes) == false)
        //            && (Scheme.Contains(Seconds) == false)
        //            && (Scheme.Contains(Milliseconds) == false))
        //        {
        //            validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Scheme)), ValidationErrorType.Warning, "{0} contains no placeholder!", nameof(Scheme)));
        //        }
        //        if (Scheme.Contains(EnterRegularExpressionHere))
        //        {
        //            validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Scheme)), ValidationErrorType.Warning, "Replace {0} by a regular expression!", EnterRegularExpressionHere));
        //        }
        //    }
        //}
    }
}
