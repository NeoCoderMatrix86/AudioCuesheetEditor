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

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class Cuesheet() : Validateable, ICuesheet
    {
        [JsonInclude]
        public IEnumerable<Track> Tracks { get; set; } = [];

        public String? Artist { get; set; }
        
        public String? Title { get; set; }

        public Audiofile? Audiofile { get; set; }

        public CDTextfile? CDTextfile { get; set; }

        public String? Cataloguenumber { get; set; }

        [JsonIgnore]
        public bool IsRecording => RecordingStart.HasValue;
        
        [JsonIgnore]
        public DateTime? RecordingStart { get; set; }

        [JsonIgnore]
        public Boolean IsImporting { get; set; }

        public override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(Tracks):
                    validationStatus = ValidationStatus.Success;
                    if (!Tracks.Any())
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has invalid Count ({1})!", nameof(Tracks), 0));
                    }
                    else
                    {
                        //Check track overlapping
                        var tracksWithSamePosition = Tracks
                            .GroupBy(x => x.Position)
                            .Where(grp => grp.Count() > 1);
                        if (tracksWithSamePosition.Any())
                        {
                            validationMessages ??= [];
                            foreach (var track in tracksWithSamePosition)
                            {
                                foreach (var trackWithSamePosition in track)
                                {
                                    validationMessages.Add(new ValidationMessage("{0} {1} '{2}' is used also by {3}({4},{5},{6},{7},{8}). Positions must be unique!", nameof(Track), nameof(Track.Position), track.Key != null ? track.Key : String.Empty, nameof(Track), trackWithSamePosition.Position != null ? trackWithSamePosition.Position : String.Empty, trackWithSamePosition.Artist ?? String.Empty, trackWithSamePosition.Title ?? String.Empty, trackWithSamePosition.Begin != null ? trackWithSamePosition.Begin : String.Empty, trackWithSamePosition.End != null ? trackWithSamePosition.End : String.Empty));
                                }
                            }
                        }
                        foreach (var track in Tracks.OrderBy(x => x.Position))
                        {
                            var tracksBetween = Tracks.Where(x => ((track.Begin >= x.Begin && track.Begin < x.End)
                                                        || (x.Begin < track.End && track.End <= x.End))
                                                        && (x.Equals(track) == false));
                            if (tracksBetween.Any())
                            {
                                validationMessages ??= [];
                                foreach (var trackBetween in tracksBetween)
                                {
                                    validationMessages.Add(new ValidationMessage("{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!", nameof(Track), track.Position != null ? track.Position : String.Empty, track.Artist ?? String.Empty, track.Title ?? String.Empty, track.Begin != null ? track.Begin : String.Empty, track.End != null ? track.End : String.Empty, trackBetween.Position != null ? trackBetween.Position : String.Empty, trackBetween.Artist ?? String.Empty, trackBetween.Title ?? String.Empty, trackBetween.Begin != null ? trackBetween.Begin : String.Empty, trackBetween.End != null ? trackBetween.End : String.Empty));
                                }
                            }
                        }
                    }
                    break;
                case nameof(Audiofile):
                    validationStatus = ValidationStatus.Success;
                    if (Audiofile == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Audiofile)));
                    }
                    break;
                case nameof(Artist):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Artist))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Artist)));
                    }
                    break;
                case nameof(Title):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Title))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Title)));
                    }
                    break;
                case nameof(Cataloguenumber):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Cataloguenumber) == false)
                    {
                        if (Cataloguenumber.All(Char.IsDigit) == false)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must only contain numbers!", nameof(Cataloguenumber)));
                        }
                        if (Cataloguenumber.Length != 13)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} has an invalid length. Allowed length is {1}!", nameof(Cataloguenumber), 13));
                        }
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
}
