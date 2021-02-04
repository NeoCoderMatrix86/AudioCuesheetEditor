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
using AudioCuesheetEditor.Controller;
using AudioCuesheetEditor.Model.AudioCuesheet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO
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

        public static readonly IReadOnlyDictionary<String, String> AvailableSchemes;

        public static readonly String DefaultImportScheme = "%Artist% - %Title%[\\t]{1,}%End%";

        static TextImportFile()
        {
            SchemeArtist = String.Format("{0}{1}{2}",SchemeCharacter, nameof(Track.Artist), SchemeCharacter);
            SchemeTitle = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.Title), SchemeCharacter);
            SchemeBegin = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.Begin), SchemeCharacter);
            SchemeEnd = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.End), SchemeCharacter);
            SchemeLength = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.Length), SchemeCharacter);
            SchemePosition = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.Position), SchemeCharacter);
            SchemeFlags = String.Format("{0}{1}{2}", SchemeCharacter, nameof(Track.Flags), SchemeCharacter);

            Dictionary<String, String> availableSchemes = new Dictionary<string, string>
            {
                { nameof(Track.Position), SchemePosition },
                { nameof(Track.Artist), SchemeArtist },
                { nameof(Track.Title), SchemeTitle },
                { nameof(Track.Begin), SchemeBegin },
                { nameof(Track.End), SchemeEnd },
                { nameof(Track.Length), SchemeLength },
                { nameof(Track.Flags), SchemeFlags }
            };

            AvailableSchemes = availableSchemes;
        }

        private readonly IReadOnlyCollection<String> fileLines;
        
        private String importScheme;
        private readonly List<ImportTrack> tracks = new List<ImportTrack>();

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
            ImportScheme = DefaultImportScheme;
        }

        public String ImportScheme 
        {
            get { return importScheme; }
            set 
            { 
                importScheme = value;
                Analyse();
            }
        }

        public Exception AnalyseException { get; private set; }

        public Boolean IsValid { get { return AnalyseException == null; } }

        private void Analyse()
        {
            try
            {
                AnalyseException = null;
                tracks.Clear();
                var regices = AnalyseScheme();
                foreach (var line in fileLines)
                {
                    if (String.IsNullOrEmpty(line) == false)
                    {
                        var track = new ImportTrack();
                        var index = 0;
                        foreach (var regexRelation in regices)
                        {
                            var match = regexRelation.Value.Match(line.Substring(index));
                            if (match.Success == true)
                            {
                                var propertyValueBefore = line.Substring(index, match.Index);
                                var propertyBefore = track.GetType().GetProperty(regexRelation.Key.Item1, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                var propertyValueAfter = line.Substring(index + match.Index + match.Length);
                                var propertyAfter = track.GetType().GetProperty(regexRelation.Key.Item2, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
                                    //TODO: Format
                                    propertyBefore.SetValue(track, TimeSpan.Parse(propertyValueBefore));
                                }
                                if (propertyBefore.PropertyType == typeof(uint?))
                                {
                                    propertyBefore.SetValue(track, Convert.ToUInt32(propertyValueBefore));
                                }
                                if (propertyBefore.PropertyType == typeof(String))
                                {
                                    propertyBefore.SetValue(track, propertyValueBefore);
                                }
                                if (propertyBefore.PropertyType == typeof(IReadOnlyCollection<Flag>))
                                {
                                    var list = Flag.AvailableFlags.Where(x => propertyValueBefore.Contains(x.CuesheetLabel));
                                    track.SetFlags(list);
                                }
                                if (otherMatchRegEx == false)
                                {
                                    if (propertyAfter.PropertyType == typeof(TimeSpan?))
                                    {
                                        //TODO: Format
                                        propertyAfter.SetValue(track, TimeSpan.Parse(propertyValueAfter));
                                    }
                                    if (propertyAfter.PropertyType == typeof(uint?))
                                    {
                                        propertyAfter.SetValue(track, Convert.ToUInt32(propertyValueAfter));
                                    }
                                    if (propertyAfter.PropertyType == typeof(String))
                                    {
                                        propertyAfter.SetValue(track, propertyValueAfter);
                                    }
                                    if (propertyAfter.PropertyType == typeof(IReadOnlyCollection<Flag>))
                                    {
                                        var list = Flag.AvailableFlags.Where(x => propertyValueAfter.Contains(x.CuesheetLabel));
                                        track.SetFlags(list);
                                    }
                                }
                                index = index + match.Index + match.Length;
                            }
                        }
                        tracks.Add(track);
                    }
                }
            }
            catch (Exception ex)
            {
                AnalyseException = ex;
            }
        }

        /// <summary>
        /// Analyse the input format scheme
        /// </summary>
        /// <returns>A read only dictionary with regular expressions and in format: "PropertyBefore", "PropertyAfter", Regular Expression </returns>
        private IReadOnlyDictionary<Tuple<String, String>, Regex> AnalyseScheme()
        {
            Dictionary<Tuple<String, String>, Regex> regices = new Dictionary<Tuple<String, String>, Regex>();
            try
            {
                AnalyseException = null;
                if (String.IsNullOrEmpty(ImportScheme) == false)
                {
                    for (int i = 0; i <= ImportScheme.Length; i++)
                    {
                        if (ImportScheme.Substring(i).StartsWith(SchemeCharacter) == true)
                        {
                            var endIndex = ImportScheme.IndexOf(SchemeCharacter, i + 1);
                            //Only search if there is somethine behind the Scheme identifier
                            if ((endIndex > 0) && ((endIndex + 1) < ImportScheme.Length))
                            {
                                var propertyBefore = ImportScheme.Substring(i + SchemeCharacter.Length, endIndex - (i + SchemeCharacter.Length));
                                var nextPropertyStartIndex = ImportScheme.IndexOf(SchemeCharacter, endIndex + 1);
                                var nextPropertyEndIndex = ImportScheme.IndexOf(SchemeCharacter, nextPropertyStartIndex + 1);
                                var propertyAfter = ImportScheme.Substring(nextPropertyStartIndex + 1, nextPropertyEndIndex - (nextPropertyStartIndex + 1));
                                var regExString = ImportScheme.Substring(endIndex + 1, nextPropertyStartIndex - (endIndex + 1));
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

        public IReadOnlyCollection<ImportTrack> Tracks
        {
            get { return tracks.OrderBy(x => x.Position).ToList().AsReadOnly(); }
        }
    }
}
