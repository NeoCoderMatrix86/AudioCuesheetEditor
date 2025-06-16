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
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.AudioCuesheet.Import;
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.Utility;

namespace AudioCuesheetEditor.Model.IO.Import
{
    public class Importprofile : Validateable
    {
        public static readonly IEnumerable<String> AvailableSchemeCuesheet;
        public static readonly IEnumerable<String> AvailableSchemesTrack;

        static Importprofile()
        {
            AvailableSchemeCuesheet = [nameof(Cuesheet.Artist), nameof(Cuesheet.Title), nameof(Cuesheet.Audiofile), nameof(Cuesheet.CDTextfile), nameof(Cuesheet.Cataloguenumber)];
            AvailableSchemesTrack = [nameof(Track.Artist), nameof(Track.Title), nameof(Track.Begin), nameof(Track.End), nameof(Track.Length), nameof(Track.Position), nameof(Track.Flags), nameof(Track.PreGap), nameof(Track.PostGap), nameof(ImportTrack.StartDateTime)];
        }
        public Guid Id { get; init; } = Guid.NewGuid();
        public String? Name { get; set; }
        public Boolean UseRegularExpression { get; set; }
        public String? SchemeCuesheet { get; set; }
        public String? SchemeTracks { get; set; }
        public TimeSpanFormat? TimeSpanFormat { get; set; }
        //TODO: Test
        public override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(SchemeCuesheet):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(SchemeCuesheet) == false)
                    {
                        var containsPlaceHolder = false;
                        var enumerator = AvailableSchemeCuesheet.GetEnumerator();
                        if (enumerator.MoveNext())
                        {
                            do
                            {
                                containsPlaceHolder = SchemeCuesheet?.Contains(enumerator.Current) == true;
                            } while ((containsPlaceHolder == false) && (enumerator.MoveNext()));
                        }
                        if (containsPlaceHolder == false)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} contains no placeholder!", nameof(SchemeCuesheet)));
                        }
                    }
                    break;
                case nameof(SchemeTracks):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(SchemeTracks) == false)
                    {
                        var containsPlaceHolder = false;
                        var enumerator = AvailableSchemesTrack.GetEnumerator();
                        if (enumerator.MoveNext())
                        {
                            do
                            {
                                containsPlaceHolder = SchemeTracks?.Contains(enumerator.Current) == true;
                            } while ((containsPlaceHolder == false) && (enumerator.MoveNext()));
                        }
                        if (containsPlaceHolder == false)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} contains no placeholder!", nameof(SchemeTracks)));
                        }
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
}
