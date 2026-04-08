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
using AudioCuesheetEditor.Model.UI;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class Track : Validateable, ITraceable, ITrack
    {
        /// <inheritdoc/>
        //TODO: Remove when ITraceable doesn't have event any more
        public event EventHandler<TraceablePropertiesChangedEventArgs>? TraceablePropertyChanged;
        
        public uint? Position { get; set; }
        public String? Artist { get; set; }
        public String? Title { get; set; }
        public TimeSpan? Begin { get; set; }        
        public TimeSpan? End { get; set; }

        [JsonIgnore]
        public TimeSpan? Length 
        {
            get
            {
                if ((Begin.HasValue == true) && (End.HasValue == true) && (Begin.Value <= End.Value))
                {
                    return End - Begin;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if ((Begin.HasValue == false) && (End.HasValue == false))
                {
                    Begin = TimeSpan.Zero;
                    End = Begin.Value + value;
                }
                else
                {
                    if ((Begin.HasValue == true) && (End.HasValue == true))
                    {
                        End = Begin.Value + value;
                    }
                    else
                    {
                        if ((End.HasValue == false) && (Begin.HasValue))
                        {
                            End = Begin.Value + value;
                        }
                        if ((Begin.HasValue == false) && (End.HasValue))
                        {
                            Begin = End.Value - value;
                        }
                    }
                }
            }
        }
        [JsonInclude]
        public IEnumerable<Flag> Flags { get; set; } = [];
        
        [JsonIgnore]
        public Cuesheet? Cuesheet { get; set; }
        
        /// <inheritdoc/>
        public TimeSpan? PreGap { get; set;  }
        /// <inheritdoc/>
        public TimeSpan? PostGap { get; set; }
        /// <summary>
        /// Set that this track is linked to the previous track in cuesheet
        /// </summary>
        public Boolean IsLinkedToPreviousTrack { get; set; }

        public override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(Position):
                    validationStatus = ValidationStatus.Success;
                    if (Position == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Position)));
                    }
                    else
                    {
                        if (Position == 0)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} may not be 0!", nameof(Position)));
                        }
                        else
                        {
                            // Check correct track position
                            if (Cuesheet != null)
                            {
                                var positionTrackShouldHave = Cuesheet.Tracks.OrderBy(x => x.Begin ?? TimeSpan.MaxValue).ThenBy(x => x.Position).ToList().IndexOf(this) + 1;
                                if (positionTrackShouldHave != Position)
                                {
                                    validationMessages ??= [];
                                    validationMessages.Add(new ValidationMessage("Track({0},{1},{2},{3},{4}) does not have the correct position '{5}'!", Position, Artist ?? String.Empty, Title ?? String.Empty, Begin != null ? Begin : String.Empty, End != null ? End : String.Empty, positionTrackShouldHave));
                                }
                            }
                        }
                    }
                    break;
                case nameof(Begin):
                    validationStatus = ValidationStatus.Success;
                    if (Begin == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Begin)));
                    }
                    else
                    {
                        if (Begin < TimeSpan.Zero)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must be equal or greater zero!", nameof(Begin)));
                        }
                        if (Begin > End)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} should be less than or equal '{1}'!", nameof(Begin), nameof(End)));
                        }
                    }
                    break;
                case nameof(End):
                    validationStatus = ValidationStatus.Success;
                    if (End == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(End)));
                    }
                    else
                    {
                        if (End < TimeSpan.Zero)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must be equal or greater zero!", nameof(End)));
                        }
                        if (End < Begin)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} should be greater than or equal '{1}'!", nameof(End), nameof(Begin)));
                        }
                    }
                    break;
                case nameof(Length):
                    validationStatus = ValidationStatus.Success;
                    if (Length == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Length)));
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
}
