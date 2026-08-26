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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO.Audio;

namespace AudioCuesheetEditor.Model.IO.Export
{
    public class Exportprofile : Validateable
    {
        public static readonly String DefaultFileName = "Export.txt";

        public static readonly String SchemeCuesheetArtist = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.Artist), SchemeCharacter);
        public static readonly String SchemeCuesheetTitle = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.Title), SchemeCharacter);
        public static readonly String SchemeAudiofileName = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Audiofile), nameof(Audiofile.Name), SchemeCharacter);
        public static readonly String SchemeCuesheetCDTextfile = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.CDTextfile), SchemeCharacter);
        public static readonly String SchemeCuesheetCatalogueNumber = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.Cataloguenumber), SchemeCharacter);
        public static readonly String SchemeTrackArtist = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Artist), SchemeCharacter);
        public static readonly String SchemeTrackTitle = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Title), SchemeCharacter);
        public static readonly String SchemeTrackBegin = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Begin), SchemeCharacter);
        public static readonly String SchemeTrackEnd = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.End), SchemeCharacter);
        public static readonly String SchemeTrackLength = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Length), SchemeCharacter);
        public static readonly String SchemeTrackPosition = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Position), SchemeCharacter);
        public static readonly String SchemeTrackFlags = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Flags), SchemeCharacter);
        public static readonly String SchemeTrackPreGap = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.PreGap), SchemeCharacter);
        public static readonly String SchemeTrackPostGap = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.PostGap), SchemeCharacter);
        public static readonly String SchemeDate = String.Format("{0}Date{1}", SchemeCharacter, SchemeCharacter);
        public static readonly String SchemeDateTime = String.Format("{0}DateTime{1}", SchemeCharacter, SchemeCharacter);
        public static readonly String SchemeTime = String.Format("{0}Time{1}", SchemeCharacter, SchemeCharacter);

        public static readonly Dictionary<String, String> AvailableCuesheetSchemes = new()
        {
            { nameof(Cuesheet.Artist), SchemeCuesheetArtist },
            { nameof(Cuesheet.Title), SchemeCuesheetTitle },
            { nameof(Cuesheet.CDTextfile), SchemeCuesheetCDTextfile },
            { nameof(Cuesheet.Cataloguenumber), SchemeCuesheetCatalogueNumber },
            { "Date", SchemeDate },
            { "DateTime", SchemeDateTime },
            { "Time", SchemeTime }
        };
        public static readonly Dictionary<String, String> AvailableAudiofileSchemes = new()
        {
            { nameof(Audiofile.Name), SchemeAudiofileName }
        };
        public static readonly Dictionary<String, String> AvailableTrackSchemes = new()
        {
            { nameof(Track.Position), SchemeTrackPosition },
            { nameof(Track.Artist), SchemeTrackArtist },
            { nameof(Track.Title), SchemeTrackTitle },
            { nameof(Track.Begin), SchemeTrackBegin },
            { nameof(Track.End), SchemeTrackEnd },
            { nameof(Track.Length), SchemeTrackLength },
            { nameof(Track.Flags), SchemeTrackFlags },
            { nameof(Track.PreGap), SchemeTrackPreGap },
            { nameof(Track.PostGap), SchemeTrackPostGap }
        };

        public const String SchemeCharacter = "%";

        private String _schemeHead;
        private String _schemeAudiofiles;
        private String _schemeTracks;
        private String _schemeFooter;
        private String _filename;
        private String _name;

        public Exportprofile()
        {
            Id = Guid.NewGuid();
            _schemeHead = String.Empty;
            _schemeTracks = String.Empty;
            _schemeAudiofiles = String.Empty;
            _schemeFooter = String.Empty;
            _filename = DefaultFileName;
            var random = new Random();
            _name = String.Format("{0}_{1}", nameof(Exportprofile), random.Next(1, 100));
        }
        public Guid Id { get; init; }
        public String Name 
        {
            get => _name;
            set { _name = value; OnValidateablePropertyChanged(); }
        }
        public String SchemeHead 
        {
            get => _schemeHead;
            set { _schemeHead = value; OnValidateablePropertyChanged(); }
        }
        public String SchemeTracks 
        {
            get => _schemeTracks;
            set { _schemeTracks = value; OnValidateablePropertyChanged(); }
        }
        public String SchemeAudiofiles
        {
            get => _schemeAudiofiles;
            set { _schemeAudiofiles = value; OnValidateablePropertyChanged(); }
        }
        public String SchemeFooter 
        {
            get => _schemeFooter;
            set { _schemeFooter = value; OnValidateablePropertyChanged(); }
        }
        public String Filename 
        {
            get => _filename;
            set { _filename = value; OnValidateablePropertyChanged(); }
        }

        public override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(SchemeHead):
                    validationStatus = ValidationStatus.Success;
                    validationMessages = CheckForUnresolvablePlaceholders(SchemeHead, nameof(SchemeHead), [AvailableTrackSchemes, AvailableAudiofileSchemes]);
                    break;
                case nameof(SchemeTracks):
                    validationStatus = ValidationStatus.Success;
                    validationMessages = CheckForUnresolvablePlaceholders(SchemeTracks, nameof(SchemeTracks), [AvailableCuesheetSchemes, AvailableAudiofileSchemes]);
                    break;
                case nameof(SchemeAudiofiles):
                    validationStatus = ValidationStatus.Success;
                    validationMessages = CheckForUnresolvablePlaceholders(SchemeAudiofiles, nameof(SchemeAudiofiles), [AvailableTrackSchemes, AvailableTrackSchemes]);
                    break;
                case nameof(SchemeFooter):
                    validationStatus = ValidationStatus.Success;
                    validationMessages = CheckForUnresolvablePlaceholders(SchemeFooter, nameof(SchemeFooter), [AvailableTrackSchemes, AvailableAudiofileSchemes]);
                    break;
                case nameof(Filename):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Filename))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Filename)));
                    }
                    break;
                case nameof(Name):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Name))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Name)));
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }

        static List<ValidationMessage>? CheckForUnresolvablePlaceholders(String scheme, string schemeName, List<Dictionary<String, String>> schemesToCheck)
        {
            List<ValidationMessage>? validationMessages = null;
            foreach (var schemeToCheck in schemesToCheck)
            {
                foreach (var availableScheme in schemeToCheck)
                {
                    if (scheme.Contains(availableScheme.Value) == true)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} contains placeholder '{1}' that can not be resolved!", schemeName, availableScheme.Value));
                        break;
                    }
                }
            }
            return validationMessages;
        }
    }
}
