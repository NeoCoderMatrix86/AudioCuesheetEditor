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
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.Utility;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.Services.IO
{
    public class TextImportService
    {
        public ImportOptions? ImportOptions { get; private set; }
        public IImportfile Analyse(ImportOptions importOptions, IEnumerable<String?> fileContent)
        {
            Importfile importfile = new()
            {
                FileType = ImportFileType.Textfile
            };
            try
            {
                importfile.FileContent = fileContent;
                ImportOptions = importOptions;
                importfile.AnalysedCuesheet = new ImportCuesheet();
                Boolean cuesheetRecognized = false;
                List<String?> recognizedFileContent = [];
                foreach (var line in fileContent)
                {
                    var recognizedLine = line;
                    if (String.IsNullOrEmpty(line) == false)
                    {
                        Boolean recognized = false;
                        if ((recognized == false) && (cuesheetRecognized == false) && (String.IsNullOrEmpty(ImportOptions.TextImportScheme.SchemeCuesheet) == false))
                        {
                            //Remove entity names
                            var expression = ImportOptions.TextImportScheme.SchemeCuesheet.Replace(String.Format("{0}.", nameof(Cuesheet)), String.Empty).Replace(String.Format("{0}.", nameof(Track)), String.Empty);
                            var regExCuesheet = new Regex(expression);
                            recognizedLine = AnalyseLine(line, importfile.AnalysedCuesheet, regExCuesheet);
                            recognized = recognizedLine != null;
                            cuesheetRecognized = recognizedLine != null;
                        }
                        if ((recognized == false) && (String.IsNullOrEmpty(ImportOptions.TextImportScheme.SchemeTracks) == false))
                        {
                            //Remove entity names
                            var expression = ImportOptions.TextImportScheme.SchemeTracks.Replace(String.Format("{0}.", nameof(Cuesheet)), String.Empty).Replace(String.Format("{0}.", nameof(Track)), String.Empty);
                            var regExTracks = new Regex(expression);
                            var track = new ImportTrack();
                            recognizedLine = AnalyseLine(line, track, regExTracks);
                            recognized = recognizedLine != null;
                            importfile.AnalysedCuesheet.Tracks.Add(track);
                        }
                    }
                    recognizedFileContent.Add(recognizedLine);
                }
                if (recognizedFileContent.Count > 0)
                {
                    importfile.FileContentRecognized = recognizedFileContent.AsReadOnly();
                }
            }
            catch (Exception ex)
            {
                importfile.FileContentRecognized = fileContent;
                importfile.AnalyseException = ex;
                importfile.AnalysedCuesheet = null;
            }
            return importfile;
        }

        private String? AnalyseLine(String line, object entity, Regex regex)
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

        private void SetValue(object entity, PropertyInfo property, string value)
        {
            if (property.PropertyType == typeof(TimeSpan?))
            {
                property.SetValue(entity, TimeSpanUtility.ParseTimeSpan(value, ImportOptions?.TimeSpanFormat));
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
                ((ITrack)entity).SetFlags(list);
            }
            if (property.PropertyType == typeof(Audiofile))
            {
                property.SetValue(entity, new Audiofile(value));
            }
            if (property.PropertyType == typeof(Cataloguenumber))
            {
                ((Cuesheet)entity).Cataloguenumber.Value = value;
            }
            if (property.PropertyType == typeof(DateTime?))
            {
                if (DateTime.TryParse(value, out var date))
                {
                    property.SetValue(entity, date);
                }
            }
        }
    }
}
