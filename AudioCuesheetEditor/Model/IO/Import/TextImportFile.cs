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
using AudioCuesheetEditor.Model.IO.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO.Import
{
    public class TextImportFile
    {
        public const String SchemeCharacter = "%";

        public const String MimeType = "text/plain";
        public const String FileExtension = ".txt";

        public static readonly String SchemeArtist;
        public static readonly String SchemeTitle;
        public static readonly String SchemeBegin;
        public static readonly String SchemeEnd;
        public static readonly String SchemeLength;
        public static readonly String SchemePosition;
        public static readonly String SchemeFlags;
        public static readonly String SchemePreGap;
        public static readonly String SchemePostGap;

        public static readonly IReadOnlyDictionary<String, String> AvailableSchemesTrack;

        public static readonly String DefaultImportSchemeTracks = "%Artist% - %Title%[\\t]{1,}%End%";

        static TextImportFile()
        {
            SchemeArtist = String.Format("{0}{1}{2}",SchemeCharacter, nameof(Track.Artist), SchemeCharacter);
            SchemeTitle = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.Title), SchemeCharacter);
            SchemeBegin = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.Begin), SchemeCharacter);
            SchemeEnd = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.End), SchemeCharacter);
            SchemeLength = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.Length), SchemeCharacter);
            SchemePosition = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.Position), SchemeCharacter);
            SchemeFlags = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.Flags), SchemeCharacter);
            SchemePreGap = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.PreGap), SchemeCharacter);
            SchemePostGap = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.PostGap), SchemeCharacter);

            AvailableSchemesTrack = new Dictionary<string, string>
            {
                { nameof(Track.Position), SchemePosition },
                { nameof(Track.Artist), SchemeArtist },
                { nameof(Track.Title), SchemeTitle },
                { nameof(Track.Begin), SchemeBegin },
                { nameof(Track.End), SchemeEnd },
                { nameof(Track.Length), SchemeLength },
                { nameof(Track.Flags), SchemeFlags },
                { nameof(Track.PreGap), SchemePreGap },
                { nameof(Track.PostGap), SchemePostGap }
            };
        }

        private readonly IReadOnlyCollection<String> fileLines;

        public TextImportFile(MemoryStream fileContent)
        {
            if (fileContent == null)
            {
                throw new ArgumentNullException(nameof(fileContent));
            }
            fileContent.Position = 0;
            using var reader = new StreamReader(fileContent);
            List<String> lines = new List<String>();
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            fileLines = lines.AsReadOnly();
            TextImportScheme = new TextImportScheme();
            TextImportScheme.SchemeChanged += TextImportScheme_SchemeChanged;
        }

        public TextImportScheme TextImportScheme { get; private set; }
        
        public Exception AnalyseException { get; private set; }

        public Boolean IsValid { get { return AnalyseException == null; } }

        public ImportCuesheet ImportCuesheet { get; private set; }

        private void TextImportScheme_SchemeChanged(object sender, string e)
        {
            Analyse();
        }

        private void Analyse()
        {
            try
            {
                ImportCuesheet = new ImportCuesheet();

                AnalyseException = null;
                var regicesCuesheet = AnalyseScheme(TextImportScheme.SchemeCuesheet);
                var regicesTracks = AnalyseScheme(TextImportScheme.SchemeTracks);
                Boolean cuesheetRecognized = false;
                foreach (var line in fileLines)
                {
                    if (String.IsNullOrEmpty(line) == false)
                    {
                        Boolean recognized = false;
                        if ((recognized == false) && (cuesheetRecognized == false) && (regicesCuesheet.Count > 0))
                        {
                            var firstRegiceCuesheet = regicesCuesheet.First();
                            if (firstRegiceCuesheet.Value.IsMatch(line))
                            {
                                recognized = true;
                                cuesheetRecognized = true;
                                AnalyseLine(line, ImportCuesheet, regicesCuesheet);
                            }
                        }
                        if ((recognized == false) && (regicesTracks.Count > 0))
                        {
                            var firstRegiceTrack = regicesCuesheet.First();
                            if (firstRegiceTrack.Value.IsMatch(line))
                            {
                                recognized = true;
                                var track = new ImportTrack();
                                AnalyseLine(line, track, regicesTracks);
                                ImportCuesheet.AddTrack(track);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AnalyseException = ex;
                ImportCuesheet = null;
            }
        }

        /// <summary>
        /// Analyse the input format scheme
        /// </summary>
        /// <param name="scheme">Scheme to analyse</param>
        /// <returns>A read only dictionary with regular expressions and in format: "PropertyBefore", "PropertyAfter", Regular Expression </returns>
        private IReadOnlyDictionary<Tuple<String, String>, Regex> AnalyseScheme(String scheme)
        {
            Dictionary<Tuple<String, String>, Regex> regices = new Dictionary<Tuple<String, String>, Regex>();
            try
            {
                AnalyseException = null;
                if (String.IsNullOrEmpty(scheme) == false)
                {
                    for (int i = 0; i <= scheme.Length; i++)
                    {
                        if (scheme.Substring(i).StartsWith(SchemeCharacter) == true)
                        {
                            var endIndex = scheme.IndexOf(SchemeCharacter, i + 1);
                            //Only search if there is somethine behind the Scheme identifier
                            if ((endIndex > 0) && ((endIndex + 1) < scheme.Length))
                            {
                                var propertyBefore = scheme.Substring(i + SchemeCharacter.Length, endIndex - (i + SchemeCharacter.Length));
                                var nextPropertyStartIndex = scheme.IndexOf(SchemeCharacter, endIndex + 1);
                                var nextPropertyEndIndex = scheme.IndexOf(SchemeCharacter, nextPropertyStartIndex + 1);
                                var propertyAfter = scheme.Substring(nextPropertyStartIndex + 1, nextPropertyEndIndex - (nextPropertyStartIndex + 1));
                                var regExString = scheme.Substring(endIndex + 1, nextPropertyStartIndex - (endIndex + 1));
                                regices.Add(new Tuple<string, string>(propertyBefore, propertyAfter), new Regex(regExString));
                                //Recalculate next index
                                i = nextPropertyStartIndex - 1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AnalyseException = ex;
            }
            return regices;
        }

        /// <summary>
        /// Analyses a line and sets the properties on the entity
        /// </summary>
        /// <param name="line">Line to analyse</param>
        /// <param name="entity">Entity to set properties on</param>
        /// <param name="regices">Regular expressions for analysing</param>
        private static void AnalyseLine(String line, ICuesheetEntity entity, IReadOnlyDictionary<Tuple<String, String>, Regex> regices)
        {
            if ((String.IsNullOrEmpty(line) == false) && (entity != null) && (regices != null))
            {
                var index = 0;
                foreach (var regexRelation in regices)
                {
                    var match = regexRelation.Value.Match(line.Substring(index));
                    if (match.Success == true)
                    {
                        var propertyValueBefore = line.Substring(index, match.Index);
                        var propertyBefore = entity.GetType().GetProperty(regexRelation.Key.Item1, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var propertyValueAfter = line.Substring(index + match.Index + match.Length);
                        var propertyAfter = entity.GetType().GetProperty(regexRelation.Key.Item2, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        //Check if other regices might match the value after
                        Boolean otherMatchRegEx = false;
                        var elementIndex = regices.ToList().IndexOf(regexRelation);
                        for (int i = elementIndex; i < regices.Count; i++)
                        {
                            var nextRegExRelation = regices.ElementAt(i);
                            if (nextRegExRelation.Value.IsMatch(propertyValueAfter) == true)
                            {
                                otherMatchRegEx = true;
                                i = regices.Count;
                            }
                        }
                        if (propertyBefore.PropertyType == typeof(TimeSpan?))
                        {
                            propertyBefore.SetValue(entity, TimeSpan.Parse(propertyValueBefore));
                        }
                        if (propertyBefore.PropertyType == typeof(uint?))
                        {
                            propertyBefore.SetValue(entity, Convert.ToUInt32(propertyValueBefore));
                        }
                        if (propertyBefore.PropertyType == typeof(String))
                        {
                            propertyBefore.SetValue(entity, propertyValueBefore);
                        }
                        if (propertyBefore.PropertyType == typeof(IReadOnlyCollection<Flag>))
                        {
                            var list = Flag.AvailableFlags.Where(x => propertyValueBefore.Contains(x.CuesheetLabel));
                            ((ImportTrack)entity).SetFlags(list);
                        }
                        if (propertyBefore.PropertyType == typeof(AudioFile))
                        {
                            propertyBefore.SetValue(entity, new AudioFile(propertyValueBefore));
                        }
                        if (otherMatchRegEx == false)
                        {
                            if (propertyAfter.PropertyType == typeof(TimeSpan?))
                            {
                                propertyAfter.SetValue(entity, TimeSpan.Parse(propertyValueAfter));
                            }
                            if (propertyAfter.PropertyType == typeof(uint?))
                            {
                                propertyAfter.SetValue(entity, Convert.ToUInt32(propertyValueAfter));
                            }
                            if (propertyAfter.PropertyType == typeof(String))
                            {
                                propertyAfter.SetValue(entity, propertyValueAfter);
                            }
                            if (propertyAfter.PropertyType == typeof(IReadOnlyCollection<Flag>))
                            {
                                var list = Flag.AvailableFlags.Where(x => propertyValueAfter.Contains(x.CuesheetLabel));
                                ((ImportTrack)entity).SetFlags(list);
                            }
                            if (propertyAfter.PropertyType == typeof(AudioFile))
                            {
                                propertyAfter.SetValue(entity, new AudioFile(propertyValueAfter));
                            }
                        }
                        index = index + match.Index + match.Length;
                    }
                }
            }
        }
    }
}
