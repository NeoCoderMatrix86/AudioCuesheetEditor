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
using AudioCuesheetEditor.Model.Utility;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.Model.IO.Import
{
    public class TextImportfile : IDisposable
    {
        public const String MimeType = "text/plain";
        public const String FileExtension = ".txt";

        public EventHandler? AnalysisFinished;

        private TextImportScheme textImportScheme;
        private TimeSpanFormat? timeSpanFormat;
        private bool disposedValue;
        private IEnumerable<String?> fileContent;

        public TextImportfile(MemoryStream fileContentStream, ImportOptions? importOptions = null)
        {
            textImportScheme = new TextImportScheme();
            fileContent = [];
            fileContentStream.Position = 0;
            using var reader = new StreamReader(fileContentStream);
            List<String?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            FileContent = lines.AsReadOnly();
            TimeSpanFormat = new TimeSpanFormat();
            if (importOptions == null)
            {
                TextImportScheme = TextImportScheme.DefaultTextImportScheme;
            }
            else
            {
                if (importOptions.TimeSpanFormat != null)
                {
                    TimeSpanFormat = importOptions.TimeSpanFormat;
                }
                TextImportScheme = importOptions.TextImportScheme;
            }
        }

        /// <summary>
        /// File content (each element is a file line)
        /// </summary>
        public IEnumerable<String?> FileContent 
        {
            get => fileContent;
            set
            {
                fileContent = value;
                Analyse();
            }
        }

        /// <summary>
        /// File content with marking which passages has been reconized by scheme
        /// </summary>
        public IEnumerable<String?>? FileContentRecognized { get; private set; }

        public TextImportScheme TextImportScheme 
        {
            get { return textImportScheme; }
            set 
            { 
                textImportScheme.SchemeChanged -= TextImportScheme_SchemeChanged;
                textImportScheme = value;
                textImportScheme.SchemeChanged += TextImportScheme_SchemeChanged;
                Analyse();
            }
        }
        
        public Exception? AnalyseException { get; private set; }

        public Boolean IsValid { get { return AnalyseException == null; } }

        public Cuesheet? Cuesheet { get; private set; }

        public TimeSpanFormat TimeSpanFormat 
        {
            get 
            { 
                timeSpanFormat ??= new TimeSpanFormat();
                return timeSpanFormat;
            }
            set
            {
                if (timeSpanFormat != null)
                {
                    timeSpanFormat.SchemeChanged -= TimeSpanFormat_SchemeChanged;
                }
                timeSpanFormat = value;
                if (timeSpanFormat != null)
                {
                    timeSpanFormat.SchemeChanged += TimeSpanFormat_SchemeChanged;
                }
            }
        }

        private void TextImportScheme_SchemeChanged(object? sender, string e)
        {
            Analyse();
        }

        private void TimeSpanFormat_SchemeChanged(object? sender, EventArgs eventArgs)
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
                List<String?> recognizedFileContent = [];
                foreach (var line in FileContent)
                {
                    var recognizedLine = line;
                    if (String.IsNullOrEmpty(line) == false)
                    {
                        Boolean recognized = false;
                        if ((recognized == false) && (cuesheetRecognized == false) && (String.IsNullOrEmpty(TextImportScheme.SchemeCuesheet) == false))
                        {
                            //Remove entity names
                            var expression = TextImportScheme.SchemeCuesheet.Replace(String.Format("{0}.", nameof(AudioCuesheet.Cuesheet)), String.Empty).Replace(String.Format("{0}.", nameof(Track)), String.Empty);
                            var regExCuesheet = new Regex(expression);
                            recognizedLine = AnalyseLine(line, Cuesheet, regExCuesheet);
                            recognized = recognizedLine != null;
                            cuesheetRecognized = recognizedLine != null;
                        }
                        if ((recognized == false) && (String.IsNullOrEmpty(TextImportScheme.SchemeTracks) == false))
                        {
                            //Remove entity names
                            var expression = TextImportScheme.SchemeTracks.Replace(String.Format("{0}.", nameof(AudioCuesheet.Cuesheet)), String.Empty).Replace(String.Format("{0}.", nameof(Track)), String.Empty);
                            var regExTracks = new Regex(expression);
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
            AnalysisFinished?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Analyses a line and sets the properties on the entity
        /// </summary>
        /// <param name="line">Line to analyse</param>
        /// <param name="entity">Entity to set properties on</param>
        /// <param name="regex">Regular expression to use for analysis</param>
        /// <returns>Analysed line with marking what has been matched or empty string</returns>
        /// <exception cref="NullReferenceException">Occurs when property or group could not be found!</exception>
        private String? AnalyseLine(String line, ICuesheetEntity entity, Regex regex)
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
                                recognizedLine = string.Concat(recognizedLine.AsSpan(0, group.Index + (13 * (groupCounter - 1)))
                                    , String.Format(CuesheetConstants.RecognizedMarkHTML, group.Value)
                                    , recognizedLine.AsSpan(group.Index + (13 * (groupCounter - 1)) + group.Length));
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
                    if (recognizedLine.Contains(CuesheetConstants.RecognizedMarkHTML.Substring(0, CuesheetConstants.RecognizedMarkHTML.IndexOf("{0}"))))
                    {
                        recognized = recognizedLine;
                    }
                }
                else
                {
                    recognized = line;
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
        private void SetValue(ICuesheetEntity entity, PropertyInfo property, string value)
        {
            if (property.PropertyType == typeof(TimeSpan?))
            {
                var utility = new DateTimeUtility(TimeSpanFormat);
                property.SetValue(entity, utility.ParseTimeSpan(value));
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TimeSpanFormat.SchemeChanged -= TimeSpanFormat_SchemeChanged;
                    textImportScheme.SchemeChanged -= TextImportScheme_SchemeChanged;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
