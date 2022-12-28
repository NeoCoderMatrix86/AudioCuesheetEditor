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

        public String? Scheme { get; set; }
        
        public Schemetype SchemeType { get; set; }
        
        public String? GetExportResult(ICuesheetEntity cuesheetEntity)
        {
            String? result = null;
            if (String.IsNullOrEmpty(Scheme) == false)
            {
                switch (SchemeType)
                {
                    case Schemetype.Header:
                    case Schemetype.Footer:
                        var cuesheet = (Cuesheet)cuesheetEntity;
                        result = Scheme
                            .Replace(SchemeCuesheetArtist, cuesheet.Artist)
                            .Replace(SchemeCuesheetTitle, cuesheet.Title)
                            .Replace(SchemeCuesheetAudiofile, cuesheet.Audiofile?.FileName)
                            .Replace(SchemeCuesheetCDTextfile, cuesheet.CDTextfile?.FileName)
                            .Replace(SchemeCuesheetCatalogueNumber, cuesheet.Cataloguenumber?.Value)
                            .Replace(SchemeDate, DateTime.Now.ToShortDateString())
                            .Replace(SchemeDateTime, DateTime.Now.ToString())
                            .Replace(SchemeTime, DateTime.Now.ToLongTimeString());
                        break;
                    case Schemetype.Body:
                        var track = (Track)cuesheetEntity;
                        result = Scheme
                            .Replace(SchemeTrackArtist, track.Artist)
                            .Replace(SchemeTrackTitle, track.Title)
                            .Replace(SchemeTrackPosition, track.Position != null ? track.Position.Value.ToString() : String.Empty)
                            .Replace(SchemeTrackBegin, track.Begin != null ? track.Begin.Value.ToString() : String.Empty)
                            .Replace(SchemeTrackEnd, track.End != null ? track.End.Value.ToString() : String.Empty)
                            .Replace(SchemeTrackLength, track.Length != null ? track.Length.Value.ToString() : String.Empty)
                            .Replace(SchemeTrackFlags, String.Join(" ", track.Flags.Select(x => x.CuesheetLabel)))
                            .Replace(SchemeTrackPreGap, track.PreGap != null ? track.PreGap.Value.ToString() : String.Empty)
                            .Replace(SchemeTrackPostGap, track.PostGap != null ? track.PostGap.Value.ToString() : String.Empty)
                            .Replace(SchemeDate, DateTime.Now.ToShortDateString())
                            .Replace(SchemeDateTime, DateTime.Now.ToString())
                            .Replace(SchemeTime, DateTime.Now.ToLongTimeString());
                        break;
                    default:
                        //Nothing to do
                        break;
                }
            }
            return result;
        }

        protected override ValidationResult Validate(string property)
        {
            var result = new ValidationResult() { Status = ValidationStatus.NoValidation };
            List<String>? errors = null;
            switch (property)
            {
                case nameof(SchemeType):
                    if (SchemeType == Schemetype.Unknown)
                    {
                        errors ??= new();
                        errors.Add(String.Format("{0} has invalid value!", nameof(SchemeType)));
                    }
                    else
                    {
                        result.Status = ValidationStatus.Success;
                    }
                    break;
            }
            result.ErrorMessages = errors;
            return result;
        }
    }
}
