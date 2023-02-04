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
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.IO.Export
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Schemetype
    {
        Unknown,
        Header,
        Body,
        Footer
    }
    public class Exportscheme : Validateable<Exportscheme>
    {
        public const String SchemeCharacter = "%";

        public static readonly String SchemeCuesheetArtist;
        public static readonly String SchemeCuesheetTitle;
        public static readonly String SchemeCuesheetAudiofile;
        public static readonly String SchemeCuesheetCDTextfile;
        public static readonly String SchemeCuesheetCatalogueNumber;
        public static readonly String SchemeTrackArtist;
        public static readonly String SchemeTrackTitle;
        public static readonly String SchemeTrackBegin;
        public static readonly String SchemeTrackEnd;
        public static readonly String SchemeTrackLength;
        public static readonly String SchemeTrackPosition;
        public static readonly String SchemeTrackFlags;
        public static readonly String SchemeTrackPreGap;
        public static readonly String SchemeTrackPostGap;
        public static readonly String SchemeDate;
        public static readonly String SchemeDateTime;
        public static readonly String SchemeTime;

        public static readonly Dictionary<String, String> AvailableCuesheetSchemes;
        public static readonly Dictionary<String, String> AvailableTrackSchemes;

        private String? scheme;
        private Schemetype schemeType;

        static Exportscheme()
        {
            SchemeDate = String.Format("{0}Date{1}", SchemeCharacter, SchemeCharacter);
            SchemeDateTime = String.Format("{0}DateTime{1}", SchemeCharacter, SchemeCharacter);
            SchemeTime = String.Format("{0}Time{1}", SchemeCharacter, SchemeCharacter);

            SchemeCuesheetArtist = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.Artist), SchemeCharacter);
            SchemeCuesheetTitle = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.Title), SchemeCharacter);
            SchemeCuesheetAudiofile = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.Audiofile), SchemeCharacter);
            SchemeCuesheetCDTextfile = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.CDTextfile), SchemeCharacter);
            SchemeCuesheetCatalogueNumber = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.Cataloguenumber), SchemeCharacter);

            AvailableCuesheetSchemes = new Dictionary<string, string>
            {
                { nameof(Cuesheet.Artist), SchemeCuesheetArtist },
                { nameof(Cuesheet.Title), SchemeCuesheetTitle },
                { nameof(Cuesheet.Audiofile), SchemeCuesheetAudiofile },
                { nameof(Cuesheet.CDTextfile), SchemeCuesheetCDTextfile },
                { nameof(Cuesheet.Cataloguenumber), SchemeCuesheetCatalogueNumber },
                { "Date", SchemeDate },
                { "DateTime", SchemeDateTime },
                { "Time", SchemeTime }
            };

            SchemeTrackArtist = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Artist), SchemeCharacter);
            SchemeTrackTitle = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Title), SchemeCharacter);
            SchemeTrackBegin = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Begin), SchemeCharacter);
            SchemeTrackEnd = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.End), SchemeCharacter);
            SchemeTrackLength = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Length), SchemeCharacter);
            SchemeTrackPosition = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Position), SchemeCharacter);
            SchemeTrackFlags = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Flags), SchemeCharacter);
            SchemeTrackPreGap = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.PreGap), SchemeCharacter);
            SchemeTrackPostGap = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.PostGap), SchemeCharacter);

            AvailableTrackSchemes = new Dictionary<string, string>() 
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
        }

        public Exportscheme() { }

        public String? Scheme 
        {
            get => scheme;
            set { scheme = value; OnValidateablePropertyChanged(); }
        }
        
        public Schemetype SchemeType 
        {
            get => schemeType;
            set { schemeType = value; OnValidateablePropertyChanged(); }
        }
        
        protected override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(Scheme):
                    validationStatus = ValidationStatus.Success;
                    switch (SchemeType)
                    {
                        case Schemetype.Header:
                        case Schemetype.Footer:
                            foreach (var availableScheme in AvailableTrackSchemes)
                            {
                                if (Scheme?.Contains(availableScheme.Value) == true)
                                {
                                    validationMessages ??= new();
                                    validationMessages.Add(new ValidationMessage("{0} contains placeholder '{1}' that can not be resolved!", nameof(Scheme), availableScheme.Value));
                                    break;
                                }
                            }
                            break;
                        case Schemetype.Body:
                            foreach (var availableScheme in AvailableCuesheetSchemes)
                            {
                                if (Scheme?.Contains(availableScheme.Value) == true)
                                {
                                    validationMessages ??= new();
                                    validationMessages.Add(new ValidationMessage("{0} contains placeholder '{1}' that can not be resolved!", nameof(Scheme), availableScheme.Value));
                                    break;
                                }
                            }
                            break;
                    }
                    break;
                case nameof(SchemeType):
                    validationStatus = ValidationStatus.Success;
                    if (SchemeType == Schemetype.Unknown)
                    {
                        validationMessages ??= new();
                        validationMessages.Add(new ValidationMessage("{0} has invalid value!", nameof(SchemeType)));
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
}
