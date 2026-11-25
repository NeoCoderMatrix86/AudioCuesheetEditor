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
using AudioCuesheetEditor.Model.IO.Audio;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.AudioCuesheet;

public class Track : Validateable, ITrack
{
    public ushort? Position { get; set; }
    public string? Artist { get; set; }
    public string? Title { get; set; }
    public TimeSpan? Begin { get; set; }        
    public TimeSpan? Finish { get; set; }
    [JsonIgnore]
    public TimeSpan? Length 
    {
        get
        {
            if ((Begin.HasValue == true) && (Finish.HasValue == true) && (Begin.Value <= Finish.Value))
            {
                return Finish - Begin;
            }
            else
            {
                return null;
            }
        }
        set
        {
            if ((Begin.HasValue == false) && (Finish.HasValue == false))
            {
                Finish = value;
            }
            else
            {
                if ((Begin.HasValue == true) && (Finish.HasValue == true))
                {
                    Finish = Begin.Value + value;
                }
                else
                {
                    if ((Finish.HasValue == false) && (Begin.HasValue))
                    {
                        Finish = Begin.Value + value;
                    }
                    if ((Begin.HasValue == false) && (Finish.HasValue))
                    {
                        Begin = Finish.Value - value;
                    }
                }
            }
        }
    }
    [JsonIgnore]
    public Cuesheet? Cuesheet { get; set; }
    [JsonInclude]
    public IEnumerable<Flag> Flags { get; set; } = [];
    /// <inheritdoc/>
    public TimeSpan? PreGap { get; set;  }
    /// <inheritdoc/>
    public TimeSpan? PostGap { get; set; }
    /// <summary>
    /// Set that this track is linked to the previous track in cuesheet
    /// </summary>
    public bool IsLinkedToPreviousTrack { get; set; }
    [JsonIgnore]
    public Audiofile? Audiofile { get; set; }

    public override ValidationResult Validate(string propertyName)
    {
        var validationStatus = ValidationStatus.NoValidation;
        List<ValidationMessage>? validationMessages = null;
        switch (propertyName)
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
                            var positionTrackShouldHave = Cuesheet.Audiofiles.SelectMany(x => x.Tracks).OrderBy(x => x.Begin ?? TimeSpan.MaxValue).ThenBy(x => x.Position).ToList().IndexOf(this) + 1;
                            if (positionTrackShouldHave != Position)
                            {
                                validationMessages ??= [];
                                validationMessages.Add(new ValidationMessage("Track({0},{1},{2},{3},{4}) does not have the correct position '{5}'!", Position, Artist ?? string.Empty, Title ?? string.Empty, Begin != null ? Begin : string.Empty, Finish != null ? Finish : string.Empty, positionTrackShouldHave));
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
                    if (Begin > Finish)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} should be less than or equal '{1}'!", nameof(Begin), nameof(Finish)));
                    }
                }
                break;
            case nameof(Finish):
                validationStatus = ValidationStatus.Success;
                if (Finish == null)
                {
                    validationMessages ??= [];
                    validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Finish)));
                }
                else
                {
                    if (Finish < TimeSpan.Zero)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} must be equal or greater zero!", nameof(Finish)));
                    }
                    if (Finish < Begin)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} should be greater than or equal '{1}'!", nameof(Finish), nameof(Begin)));
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
