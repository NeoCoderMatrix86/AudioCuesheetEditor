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

namespace AudioCuesheetEditor.Model.IO.Import
{
    public class TextImportScheme : Validateable
    {
        public static readonly IEnumerable<String> AvailableSchemeCuesheet;
        public static readonly IEnumerable<String> AvailableSchemesTrack;

        static TextImportScheme()
        {
            AvailableSchemeCuesheet = [nameof(Cuesheet.Artist), nameof(Cuesheet.Title), nameof(Cuesheet.Audiofile), nameof(Cuesheet.CDTextfile), nameof(Cuesheet.Cataloguenumber)];
            AvailableSchemesTrack = [nameof(Track.Artist), nameof(Track.Title), nameof(Track.Begin), nameof(Track.End), nameof(Track.Length), nameof(Track.Position), nameof(Track.Flags), nameof(Track.PreGap), nameof(Track.PostGap), nameof(ImportTrack.StartDateTime)];
        }

        public static readonly String DefaultSchemeCuesheet = @"(?'Artist'\w*) - (?'Title'\w*)\t{1,}(?'Audiofile'.*)";
        public static readonly String DefaultSchemeTracks = @"(?'Artist'.+?) - (?'Title'.+?)\s*\t+(?'End'.+)";

        public static readonly TextImportScheme DefaultTextImportScheme = new()
        { 
            SchemeCuesheet = DefaultSchemeCuesheet,
            SchemeTracks = DefaultSchemeTracks
        };

        private string? schemeTracks;
        private string? schemeCuesheet;

        public event EventHandler<String>? SchemeChanged;

        public String? SchemeTracks
        {
            get { return schemeTracks; }
            set
            {
                schemeTracks = value;
                SchemeChanged?.Invoke(this, nameof(SchemeTracks));
                OnValidateablePropertyChanged();
            }
        }

        public String? SchemeCuesheet
        {
            get { return schemeCuesheet; }
            set
            {
                schemeCuesheet = value;
                SchemeChanged?.Invoke(this, nameof(SchemeCuesheet));
                OnValidateablePropertyChanged();
            }
        }
        public override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(SchemeCuesheet):
                    //TODO: An empty scheme is also valid
                    validationStatus = ValidationStatus.Success;
                    Boolean containsPlaceHolder = false;
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
                    break;
                case nameof(SchemeTracks):
                    //TODO: An empty scheme is also valid
                    validationStatus = ValidationStatus.Success;
                    containsPlaceHolder = false;
                    enumerator = AvailableSchemesTrack.GetEnumerator();
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
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
}
