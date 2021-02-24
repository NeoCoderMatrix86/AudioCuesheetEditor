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
using AudioCuesheetEditor.Model.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO.Import
{
    public class TextImportScheme : Validateable
    {
        public const String SchemeCharacter = "%";

        public static readonly IReadOnlyDictionary<String, String> AvailableSchemeCuesheet;
        public static readonly IReadOnlyDictionary<String, String> AvailableSchemesTrack;

        static TextImportScheme()
        {
            var schemeCuesheetArtist = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.Artist), SchemeCharacter);
            var schemeCuesheetTitle = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.Title), SchemeCharacter);
            var schemeCuesheetAudioFile = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.AudioFile), SchemeCharacter);
            var schemeCuesheetCDTextfile = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.CDTextfile), SchemeCharacter);
            var schemeCuesheetCatalogueNumber = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Cuesheet), nameof(Cuesheet.CatalogueNumber), SchemeCharacter);

            AvailableSchemeCuesheet = new Dictionary<string, string>
            {
                {nameof(ImportCuesheet.Artist), schemeCuesheetArtist },
                {nameof(ImportCuesheet.Title), schemeCuesheetTitle },
                {nameof(ImportCuesheet.AudioFile), schemeCuesheetAudioFile },
                {nameof(ImportCuesheet.CatalogueNumber), schemeCuesheetCatalogueNumber },
                {nameof(ImportCuesheet.CDTextfile), schemeCuesheetCDTextfile }
            };

            var schemeTrackArtist = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Artist), SchemeCharacter);
            var schemeTrackTitle = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Title), SchemeCharacter);
            var schemeTrackBegin = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Begin), SchemeCharacter);
            var schemeTrackEnd = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.End), SchemeCharacter);
            var schemeTrackLength = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Length), SchemeCharacter);
            var schemeTrackPosition = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Position), SchemeCharacter);
            var schemeTrackFlags = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.Flags), SchemeCharacter);
            var schemeTrackPreGap = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.PreGap), SchemeCharacter);
            var schemeTrackPostGap = String.Format("{0}{1}.{2}{3}", SchemeCharacter, nameof(Track), nameof(Track.PostGap), SchemeCharacter);

            AvailableSchemesTrack = new Dictionary<string, string>
            {
                { nameof(ImportTrack.Position), schemeTrackPosition },
                { nameof(ImportTrack.Artist), schemeTrackArtist },
                { nameof(ImportTrack.Title), schemeTrackTitle },
                { nameof(ImportTrack.Begin), schemeTrackBegin },
                { nameof(ImportTrack.End), schemeTrackEnd },
                { nameof(ImportTrack.Length), schemeTrackLength },
                { nameof(ImportTrack.Flags), schemeTrackFlags },
                { nameof(ImportTrack.PreGap), schemeTrackPreGap },
                { nameof(ImportTrack.PostGap), schemeTrackPostGap }
            };
        }

        public static readonly String DefaultSchemeCuesheet = "\\A.*%Cuesheet.Artist% - %Cuesheet.Title%[\\t]{1,}%Cuesheet.AudioFile%";
        public static readonly String DefaultSchemeTracks = "%Track.Artist% - %Track.Title%[\\t]{1,}%Track.End%";

        public static readonly TextImportScheme DefaultTextImportScheme = new TextImportScheme 
        { 
            SchemeCuesheet = DefaultSchemeCuesheet,
            SchemeTracks = DefaultSchemeTracks
        };

        private string schemeTracks;
        private string schemeCuesheet;

        public event EventHandler<String> SchemeChanged;

        public String SchemeTracks
        {
            get { return schemeTracks; }
            set
            {
                schemeTracks = value;
                OnValidateablePropertyChanged();
                SchemeChanged?.Invoke(this, nameof(SchemeTracks));
            }
        }

        public String SchemeCuesheet
        {
            get { return schemeCuesheet; }
            set
            {
                schemeCuesheet = value;
                OnValidateablePropertyChanged();
                SchemeChanged?.Invoke(this, nameof(SchemeCuesheet));
            }
        }

        protected override void Validate()
        {
            if (String.IsNullOrEmpty(SchemeCuesheet))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(SchemeCuesheet)), ValidationErrorType.Warning, "HasNoValue", nameof(SchemeCuesheet)));
            }
            else
            {
                if (AvailableSchemesTrack != null)
                {
                    Boolean addValidationError = false;
                    foreach (var availableScheme in AvailableSchemesTrack)
                    {
                        if (SchemeCuesheet.Contains(availableScheme.Value) == true)
                        {
                            addValidationError = true;
                            break;
                        }
                    }
                    if (addValidationError == true)
                    {
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(SchemeCuesheet)), ValidationErrorType.Warning, "SchemeContainsPlaceholdersThatCanNotBeSolved"));
                    }
                }
            }
            if (String.IsNullOrEmpty(SchemeTracks))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(SchemeTracks)), ValidationErrorType.Warning, "HasNoValue", nameof(SchemeTracks)));
            }
            else
            {
                if (AvailableSchemeCuesheet != null)
                {
                    Boolean addValidationError = false;
                    foreach (var availableScheme in AvailableSchemeCuesheet)
                    {
                        if (SchemeTracks.Contains(availableScheme.Value) == true)
                        {
                            addValidationError = true;
                            break;
                        }
                    }
                    if (addValidationError == true)
                    {
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(SchemeTracks)), ValidationErrorType.Warning, "SchemeContainsPlaceholdersThatCanNotBeSolved"));
                    }
                }
            }
        }
    }
}
