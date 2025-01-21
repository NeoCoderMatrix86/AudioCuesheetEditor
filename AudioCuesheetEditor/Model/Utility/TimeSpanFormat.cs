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
    public class TimeSpanFormat : Validateable
    {
        public const String Days = "Days";
        public const String Hours = "Hours";
        public const String Minutes = "Minutes";
        public const String Seconds = "Seconds";
        public const String Milliseconds = "Milliseconds";

        public static readonly IEnumerable<String> AvailableTimespanScheme;

        static TimeSpanFormat()
        {
            AvailableTimespanScheme = [Days, Hours, Minutes, Seconds, Milliseconds];
        }

        public event EventHandler? SchemeChanged;

        private string? scheme;

        public String? Scheme
        {
            get => scheme;
            set
            {
                scheme = value;
                SchemeChanged?.Invoke(this, EventArgs.Empty);
                OnValidateablePropertyChanged();
            }
        }

        public TimeSpan? ParseTimeSpan(String input)
        {
            TimeSpan? timespan = null;
            if ((String.IsNullOrEmpty(Scheme) == false) && (String.IsNullOrEmpty(input) == false))
            {
                var pattern = CreateFlexibleRegexPattern(Scheme);
                var match = Regex.Match(input, pattern);
                if (match.Success)
                {
                    int days = 0, hours = 0, minutes = 0, seconds = 0, milliseconds = 0;
                    foreach (var groupName in match.Groups.Keys)
                    {
                        switch (groupName)
                        {
                            case nameof(Days):
                                days = Convert.ToInt32(match.Groups[groupName].Value);
                                break;
                            case nameof(Hours):
                                hours = Convert.ToInt32(match.Groups[groupName].Value);
                                break;
                            case nameof(Minutes):
                                minutes = Convert.ToInt32(match.Groups[groupName].Value);
                                break;
                            case nameof(Seconds):
                                seconds = Convert.ToInt32(match.Groups[groupName].Value);
                                break;
                            case nameof(Milliseconds):
                                milliseconds = Convert.ToInt32(match.Groups[groupName].Value);
                                break;
                        }
                    }
                    timespan = new TimeSpan(days, hours, minutes, seconds, milliseconds);
                }
            }
            return timespan;
        }

        public override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(Scheme):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Scheme) == false)
                    {
                        if ((Scheme.Contains(Days) == false)
                            && (Scheme.Contains(Hours) == false)
                            && (Scheme.Contains(Minutes) == false)
                            && (Scheme.Contains(Seconds) == false)
                            && (Scheme.Contains(Milliseconds) == false))
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} contains no placeholder!", nameof(Scheme)));
                        }
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }

        private static string CreateFlexibleRegexPattern(string scheme)
        {
            var regex = Regex.Escape(scheme);

            regex = regex.Replace(Days, $"(?<{nameof(Days)}>\\d+)");
            regex = regex.Replace(Hours, $"(?<{nameof(Hours)}>\\d+)");
            regex = regex.Replace(Minutes, $"(?<{nameof(Minutes)}>\\d+)");
            regex = regex.Replace(Seconds, $"(?<{nameof(Seconds)}>\\d+)");
            regex = regex.Replace(Milliseconds, $"(?<{nameof(Milliseconds)}>\\d+)");
            regex = regex.Replace(@"\\:", @"[\s|:.,-]?");

            return $"^{regex}$";
        }
    }
}
