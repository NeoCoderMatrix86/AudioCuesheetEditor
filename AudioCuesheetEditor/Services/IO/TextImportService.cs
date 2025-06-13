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
using AudioCuesheetEditor.Model.Utility;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.Services.IO
{
    public class TextImportService
    {
        public static IImportfile Analyse(string fileContent, Importprofile importprofile)
        {
            Importfile importfile = new()
            {
                FileType = ImportFileType.Textfile
            };
            try
            {
                //TODO
                //importfile.FileContent = fileContent;
                importfile.AnalysedCuesheet = new ImportCuesheet();
                if (importprofile.UseRegularExpression)
                {
                    if (String.IsNullOrEmpty(importprofile.SchemeCuesheet) == false)
                    {
                        var regExCuesheet = new Regex(importprofile.SchemeCuesheet);
                        SearchFilecontentForCuesheetData(ref importfile, fileContent, importprofile.TimeSpanFormat, regExCuesheet);
                    }
                    if (String.IsNullOrEmpty(importprofile.SchemeTracks) == false)
                    {
                        var regExTracks = new Regex(importprofile.SchemeTracks);
                        SearchFilecontentForTracks(ref importfile, fileContent, importprofile.TimeSpanFormat, regExTracks);
                    }
                }
                else
                {
                    //TODO
                }
            }
            catch (Exception ex)
            {
                //TODO:
                //importfile.FileContent = fileContent;
                //importfile.FileContentRecognized = fileContent;
                importfile.AnalyseException = ex;
                importfile.AnalysedCuesheet = null;
            }
            return importfile;
        }

        [Obsolete("Will be deleted")]
        public static IImportfile Analyse(TextImportScheme textImportScheme, IEnumerable<String?> fileContent, TimeSpanFormat? timeSpanFormat = null)
        {
            Importfile importfile = new()
            {
                FileType = ImportFileType.Textfile
            };
            try
            {
                importfile.FileContent = fileContent;
                importfile.AnalysedCuesheet = new ImportCuesheet();
                Boolean cuesheetRecognized = false;
                List<String?> recognizedFileContent = [];
                Regex? regExCuesheet = null, regExTracks = null;
                if (String.IsNullOrEmpty(textImportScheme.SchemeCuesheet) == false)
                {
                    regExCuesheet = CreateCuesheetRegexPattern(textImportScheme.SchemeCuesheet);
                }
                if (String.IsNullOrEmpty(textImportScheme.SchemeTracks) == false)
                {
                    regExTracks = CreateTrackRegexPattern(textImportScheme.SchemeTracks);
                }
                foreach (var line in fileContent)
                {
                    var recognizedLine = line;
                    if (String.IsNullOrEmpty(line) == false)
                    {
                        Boolean recognized = false;
                        if ((recognized == false) && (cuesheetRecognized == false) && (regExCuesheet != null))
                        {
                            recognizedLine = AnalyseLine(line, importfile.AnalysedCuesheet, regExCuesheet, timeSpanFormat);
                            recognized = recognizedLine != null;
                            cuesheetRecognized = recognizedLine != null;
                        }
                        if ((recognized == false) && (regExTracks != null))
                        {
                            var track = new ImportTrack();
                            recognizedLine = AnalyseLine(line, track, regExTracks, timeSpanFormat);
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
                importfile.FileContent = fileContent;
                importfile.FileContentRecognized = fileContent;
                importfile.AnalyseException = ex;
                importfile.AnalysedCuesheet = null;
            }
            return importfile;
        }

        private static String? AnalyseLine(String line, object entity, Regex regex, TimeSpanFormat? timeSpanFormat)
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
                                SetValue(entity, property, group.Value, timeSpanFormat);
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

        private static void SetValue(object entity, PropertyInfo property, string value, TimeSpanFormat? timeSpanFormat)
        {
            if (property.PropertyType == typeof(TimeSpan?))
            {
                property.SetValue(entity, TimeSpanUtility.ParseTimeSpan(value, timeSpanFormat));
            }
            if (property.PropertyType == typeof(uint?))
            {
                property.SetValue(entity, Convert.ToUInt32(value));
            }
            if (property.PropertyType == typeof(String))
            {
                property.SetValue(entity, value);
            }
            if (property.PropertyType == typeof(IEnumerable<Flag>))
            {
                ((ITrack)entity).Flags = Flag.AvailableFlags.Where(x => value.Contains(x.CuesheetLabel));
            }
            if (property.PropertyType == typeof(Audiofile))
            {
                property.SetValue(entity, new Audiofile(value));
            }
            if (property.PropertyType == typeof(DateTime?))
            {
                if (DateTime.TryParse(value, out var date))
                {
                    property.SetValue(entity, date);
                }
            }
        }

        private static Regex CreateCuesheetRegexPattern(string scheme)
        {
            var regex = new Regex(scheme);
            var groupNames = regex.GetGroupNames();
            //GroupNames always has a group "0", so we count for more than one group
            if (groupNames.Any(x => x != "0"))
            {
                return regex;
            }
            else
            {
                var regexString = Regex.Escape(scheme);

                regexString = regexString.Replace(nameof(Cuesheet.Artist), $"(?<{nameof(Cuesheet.Artist)}>.+)");
                regexString = regexString.Replace(nameof(Cuesheet.Title), $"(?<{nameof(Cuesheet.Title)}>.+)");
                regexString = regexString.Replace(nameof(Cuesheet.Audiofile), $"(?<{nameof(Cuesheet.Audiofile)}>.+)");
                regexString = regexString.Replace(nameof(Cuesheet.CDTextfile), $"(?<{nameof(Cuesheet.CDTextfile)}>.+)");
                regexString = regexString.Replace(nameof(Cuesheet.Cataloguenumber), $"(?<{nameof(Cuesheet.Cataloguenumber)}>.+)");
                //Replace tab with non matching group
                regexString = regexString.Replace("\\t", "(?:...\\t)");

                return new Regex(regexString);
            }
        }

        private static Regex CreateTrackRegexPattern(string scheme)
        {
            var regex = new Regex(scheme);
            var groupNames = regex.GetGroupNames();
            //GroupNames always has a group "0", so we count for more than one group
            if (groupNames.Any(x => x != "0"))
            {
                return regex;
            }
            else
            {
                var regexString = Regex.Escape(scheme);

                regexString = regexString.Replace(nameof(ImportTrack.Artist), $"(?<{nameof(ImportTrack.Artist)}>.+)");
                regexString = regexString.Replace(nameof(ImportTrack.Title), $"(?<{nameof(ImportTrack.Title)}>.+)");
                regexString = regexString.Replace(nameof(ImportTrack.Begin), $"(?<{nameof(ImportTrack.Begin)}>.+)");
                regexString = regexString.Replace(nameof(ImportTrack.End), $"(?<{nameof(ImportTrack.End)}>.+)");
                regexString = regexString.Replace(nameof(ImportTrack.Length), $"(?<{nameof(ImportTrack.Length)}>.+)");
                regexString = regexString.Replace(nameof(ImportTrack.Position), $"(?<{nameof(ImportTrack.Position)}>.+)");
                regexString = regexString.Replace(nameof(ImportTrack.Flags), $"(?<{nameof(ImportTrack.Flags)}>.+)");
                regexString = regexString.Replace(nameof(ImportTrack.PreGap), $"(?<{nameof(ImportTrack.PreGap)}>.+)");
                regexString = regexString.Replace(nameof(ImportTrack.PostGap), $"(?<{nameof(ImportTrack.PostGap)}>.+)");
                regexString = regexString.Replace(nameof(ImportTrack.StartDateTime), $"(?<{nameof(ImportTrack.StartDateTime)}>.+)");
                //Replace tab with non matching group
                regexString = regexString.Replace("\\t", "(?:...\\t)");

                return new Regex(regexString);
            }
        }

        private static void SearchFilecontentForCuesheetData(ref Importfile importfile, string fileContent, TimeSpanFormat? timeSpanFormat, Regex regex)
        {
            var match = regex.Match(fileContent);
            if (match.Success)
            {
                var entity = new ImportTrack();
                for (int groupCounter = 1; groupCounter < match.Groups.Count; groupCounter++)
                {
                    var key = match.Groups.Keys.ElementAt(groupCounter);
                    var group = match.Groups.GetValueOrDefault(key);
                    if (group?.Success == true)
                    {
                        var property = entity.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (property != null)
                        {
                            SetValue(entity, property, group.Value, timeSpanFormat);
                            //TODO: Mark the found entry
                            //recognizedLine = string.Concat(recognizedLine.AsSpan(0, group.Index + (13 * (groupCounter - 1)))
                            //    , String.Format(CuesheetConstants.RecognizedMarkHTML, group.Value)
                            //    , recognizedLine.AsSpan(group.Index + (13 * (groupCounter - 1)) + group.Length));
                        }
                        else
                        {
                            throw new NullReferenceException(String.Format("Property '{0}' was not found", key));
                        }
                    }
                    else
                    {
                        throw new NullReferenceException(String.Format("Group '{0}' could not be found!", key));
                    }
                }
                importfile.AnalysedCuesheet!.Tracks.Add(entity);
            }
        }

        private static void SearchFilecontentForTracks(ref Importfile importfile, string fileContent, TimeSpanFormat? timeSpanFormat, Regex regex)
        {
            var matches = regex.Matches(fileContent);
            for (int matchCounter = 0; matchCounter < matches.Count; matchCounter++)
            {
                var match = matches[matchCounter];
                if (match.Success)
                {
                    var entity = new ImportTrack();
                    for (int groupCounter = 1; groupCounter < match.Groups.Count; groupCounter++)
                    {
                        var key = match.Groups.Keys.ElementAt(groupCounter);
                        var group = match.Groups.GetValueOrDefault(key);
                        if (group?.Success == true)
                        {
                            var property = entity.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (property != null)
                            {
                                SetValue(entity, property, group.Value, timeSpanFormat);
                                //TODO: Mark the found entry
                                //recognizedLine = string.Concat(recognizedLine.AsSpan(0, group.Index + (13 * (groupCounter - 1)))
                                //    , String.Format(CuesheetConstants.RecognizedMarkHTML, group.Value)
                                //    , recognizedLine.AsSpan(group.Index + (13 * (groupCounter - 1)) + group.Length));
                            }
                            else
                            {
                                throw new NullReferenceException(String.Format("Property '{0}' was not found", key));
                            }
                        }
                        else
                        {
                            throw new NullReferenceException(String.Format("Group '{0}' could not be found!", key));
                        }
                    }
                    importfile.AnalysedCuesheet!.Tracks.Add(entity);
                }
            }
        }
    }
}
