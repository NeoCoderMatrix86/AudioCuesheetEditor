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
using AudioCuesheetEditor.Model.Options;
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
    public class TextImportfile
    {
        public const String MimeType = "text/plain";
        public const String FileExtension = ".txt";

        private TextImportScheme textImportScheme;

        public TextImportfile(MemoryStream fileContent, ImportOptions? importOptions = null)
        {
            textImportScheme = new TextImportScheme();
            fileContent.Position = 0;
            using var reader = new StreamReader(fileContent);
            List<String?> lines = new();
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            FileContent = lines.AsReadOnly();
            if (importOptions == null)
            {
                TextImportScheme = TextImportScheme.DefaultTextImportScheme;
            }
            else
            {
                TextImportScheme = importOptions.TextImportScheme;
            }
        }

        /// <summary>
        /// File content (each element is a file line)
        /// </summary>
        public IReadOnlyCollection<String?> FileContent { get; private set; }

        /// <summary>
        /// File content with marking which passages has been reconized by scheme
        /// </summary>
        public IReadOnlyCollection<String?>? FileContentRecognized { get; private set; }

        public TextImportScheme TextImportScheme 
        {
            get { return textImportScheme; }
            set 
            { 
                if (textImportScheme != null)
                {
                    textImportScheme.SchemeChanged -= TextImportScheme_SchemeChanged;
                }
                textImportScheme = value;
                textImportScheme.SchemeChanged += TextImportScheme_SchemeChanged;
                Analyse();
            }
        }
        
        public Exception? AnalyseException { get; private set; }

        public Boolean IsValid { get { return AnalyseException == null; } }

        public Cuesheet? Cuesheet { get; private set; }

        private void TextImportScheme_SchemeChanged(object? sender, string e)
        {
            Analyse();
        }

        private void Analyse()
        {
            try
            {
                Cuesheet = new Cuesheet();
                FileContentRecognized = null;
                AnalyseException = null;
                Boolean cuesheetRecognized = false;
                List<String?> recognizedFileContent = new();
                foreach (var line in FileContent)
                {
                    var recognizedLine = line;
                    if (String.IsNullOrEmpty(line) == false)
                    {
                        Boolean recognized = false;
                        if ((recognized == false) && (cuesheetRecognized == false) && (String.IsNullOrEmpty(TextImportScheme.SchemeCuesheet) == false))
                        {
                            var regExCuesheet = new Regex(TextImportScheme.SchemeCuesheet);
                            recognizedLine = AnalyseLine(line, Cuesheet, regExCuesheet);
                            recognized = recognizedLine != null;
                            cuesheetRecognized = recognizedLine != null;
                        }
                        if ((recognized == false) && (String.IsNullOrEmpty(TextImportScheme.SchemeTracks) == false))
                        {
                            var regExTracks = new Regex(TextImportScheme.SchemeTracks);
                            var track = new Track();
                            recognizedLine = AnalyseLine(line, track, regExTracks);
                            recognized = recognizedLine != null;
                            Cuesheet.AddTrack(track);
                        }
                    }
                    recognizedFileContent.Add(recognizedLine);
                }
                if (recognizedFileContent.Count > 0)
                {
                    FileContentRecognized = recognizedFileContent.AsReadOnly();
                }
            }
            catch (Exception ex)
            {
                FileContentRecognized = FileContent;
                AnalyseException = ex;
                Cuesheet = null;
            }
        }

        /// <summary>
        /// Analyses a line and sets the properties on the entity
        /// </summary>
        /// <param name="line">Line to analyse</param>
        /// <param name="entity">Entity to set properties on</param>
        /// <param name="regex">Regular expression to use for analysis</param>
        /// <returns>Analysed line with marking what has been matched or empty string</returns>
        /// <exception cref="NullReferenceException">Occurs when property or group could not be found!</exception>
        private static String? AnalyseLine(String line, ICuesheetEntity entity, Regex regex)
        {
            String? recognized = null;
            string? recognizedLine = line;
            if (String.IsNullOrEmpty(line) == false)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    for (int groupCounter = 1; groupCounter < match.Groups.Count; groupCounter++)
                    {
                        var key = match.Groups.Keys.ElementAt(groupCounter);
                        var group = match.Groups.GetValueOrDefault(key);
                        if ((group != null) && (group.Success))
                        {
                            var property = entity.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (property != null)
                            {
                                SetValue(entity, property, group.Value);
                                recognizedLine = recognizedLine.Replace(group.Value, String.Format("<Mark>{0}</Mark>", group.Value));
                            }
                            else
                            {
                                throw new NullReferenceException(String.Format("Property '{0}' was not found for line content {1}", key, line));
                            }
                        }
                        else
                        {
                            throw new NullReferenceException(String.Format("Group '{0}' could not be found!", key));
                        }
                    }
                    if (recognizedLine.Contains("<Mark>"))
                    {
                        recognized = recognizedLine;
                    }
                }
            }
            return recognized;
        }

        /// <summary>
        /// Set the value on the entity
        /// </summary>
        /// <param name="entity">Entity object to set the value on</param>
        /// <param name="property">Property to set</param>
        /// <param name="value">Value to set</param>
        private static void SetValue(ICuesheetEntity entity, PropertyInfo property, string value)
        {
            if (property.PropertyType == typeof(TimeSpan?))
            {
                property.SetValue(entity, TimeSpan.Parse(value));
            }
            if (property.PropertyType == typeof(uint?))
            {
                property.SetValue(entity, Convert.ToUInt32(value));
            }
            if (property.PropertyType == typeof(String))
            {
                property.SetValue(entity, value);
            }
            if (property.PropertyType == typeof(IReadOnlyCollection<Flag>))
            {
                var list = Flag.AvailableFlags.Where(x => value.Contains(x.CuesheetLabel));
                ((Track)entity).SetFlags(list);
            }
            if (property.PropertyType == typeof(Audiofile))
            {
                property.SetValue(entity, new Audiofile(value));
            }
            if (property.PropertyType == typeof(Cataloguenumber))
            {
                ((Cuesheet)entity).Cataloguenumber.Value = value;
            }
            if (property.PropertyType == typeof(CDTextfile))
            {
                property.SetValue(entity, new CDTextfile(value));
            }
        }
    }
}
