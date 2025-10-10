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

using AudioCuesheetEditor.Data.Options;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.AudioCuesheet.Import;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.Utility;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.Services.IO
{
    public class TextImportService(ILocalStorageOptionsProvider localStorageOptionsProvider) : ITextImportService
    {
        private readonly ILocalStorageOptionsProvider _localStorageOptionsProvider = localStorageOptionsProvider;

        public async Task<IImportfile> AnalyseAsync(string fileContent)
        {
            Importfile importfile = new()
            {
                FileContent = fileContent,
                FileContentRecognized = fileContent,
                AnalyzedCuesheet = new ImportCuesheet(),
                FileType = ImportFileType.Textfile
            };
            try
            {
                var options = await _localStorageOptionsProvider.GetOptionsAsync<ImportOptions>();
                var importprofile = options.SelectedImportProfile ?? throw new InvalidOperationException("Selected import profiles is not set!");
                SearchForCuesheetData(ref importfile, fileContent, importprofile);
                SearchForTrackData(ref importfile, fileContent, importprofile);
            }
            catch (Exception ex)
            {
                importfile.FileContentRecognized = fileContent;
                importfile.AnalyseException = ex;
                importfile.AnalyzedCuesheet = null;
            }
            return importfile;
        }

        private static string ApplyRegexAndMarkGroups(object entity, Regex regex, string input, TimeSpanFormat? timeSpanFormat)
        {
            return regex.Replace(input, match =>
            {
                string result = match.Value;
                var groupInfos = new List<(int RelIndex, int Length, string Value, string Key)>();
                for (int matchCounter = 1; matchCounter < match.Groups.Count; matchCounter++)
                {
                    var group = match.Groups[matchCounter];
                    var key = regex.GroupNameFromNumber(matchCounter);
                    if (!string.IsNullOrEmpty(key) && key != matchCounter.ToString())
                    {
                        var property = entity.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (property != null)
                        {
                            SetValue(entity, property, group.Value, timeSpanFormat);
                        }
                        int relIndex = group.Index - match.Index;
                        groupInfos.Add((relIndex, group.Length, group.Value, key));
                    }
                }
                if (groupInfos.Count == 0)
                {
                    return result;
                }
                groupInfos.Sort((a, b) => b.RelIndex.CompareTo(a.RelIndex));
                var sb = new StringBuilder(result);
                foreach (var (RelIndex, Length, Value, Key) in groupInfos)
                {
                    sb.Remove(RelIndex, Length);
                    sb.Insert(RelIndex, string.Format(CuesheetConstants.RecognizedMarkHTML, Value));
                }
                return sb.ToString();
            });
        }

        private static void SearchForCuesheetData(ref Importfile importfile, string fileContent, Importprofile importprofile)
        {
            if (string.IsNullOrWhiteSpace(importprofile.SchemeCuesheet) == false)
            {
                var cuesheet = importfile.AnalyzedCuesheet;
                Regex regex;
                if (importprofile.UseRegularExpression == true)
                {
                    regex = new Regex(importprofile.SchemeCuesheet, RegexOptions.Multiline);
                }
                else
                {
                    regex = CreateCuesheetRegexPattern(importprofile.SchemeCuesheet);
                }

                if (importprofile.UseRegularExpression)
                {
                    importfile.FileContentRecognized = ApplyRegexAndMarkGroups(cuesheet!, regex, fileContent, importprofile.TimeSpanFormat);
                }
                else
                {
                    var sb = new StringBuilder();
                    using (var reader = new StringReader(fileContent))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var markedLine = ApplyRegexAndMarkGroups(cuesheet!, regex, line, importprofile.TimeSpanFormat);
                            sb.AppendLine(markedLine);
                            if (!string.Equals(markedLine, line))
                            {
                                //We found the first occurence, break the loop
                                //Attach the rest of the file to FileContentRecognized
                                sb.Append(reader.ReadToEnd());
                                break;
                            }
                        }
                    }
                    importfile.FileContentRecognized = sb.ToString();
                }
            }
        }

        private static void SearchForTrackData(ref Importfile importfile, string fileContent, Importprofile importprofile)
        {
            if (string.IsNullOrWhiteSpace(importprofile.SchemeTracks) == false)
            {
                Regex regex;
                if (importprofile.UseRegularExpression == true)
                {
                    regex = new Regex(importprofile.SchemeTracks, RegexOptions.Multiline);
                }
                else
                {
                    regex = CreateTrackRegexPattern(importprofile.SchemeTracks);
                }

                var cuesheet = importfile.AnalyzedCuesheet;
                importfile.FileContentRecognized ??= fileContent;
                if (importprofile.UseRegularExpression)
                {
                    
                    importfile.FileContentRecognized = regex.Replace(importfile.FileContentRecognized,
                        match =>
                        {
                            var track = new ImportTrack();
                            string marked = ApplyRegexAndMarkGroups(track, regex, match.Value, importprofile.TimeSpanFormat);
                            cuesheet!.Tracks.Add(track);
                            return marked;
                        }
                    );
                }
                else
                {
                    var sb = new StringBuilder();
                    using (var reader = new StringReader(importfile.FileContentRecognized))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // Check if this line is already analyzed
                            if (line.Contains(CuesheetConstants.MarkHTMLStart) == false)
                            {
                                var track = new ImportTrack();
                                var markedLine = ApplyRegexAndMarkGroups(track, regex, line, importprofile.TimeSpanFormat);
                                if (!string.Equals(markedLine, line))
                                {
                                    cuesheet!.Tracks.Add(track);
                                }
                                sb.AppendLine(markedLine);
                            }
                            else
                            {
                                sb.AppendLine(line);
                            }
                        }
                    }
                    importfile.FileContentRecognized = sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
                }
            }
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
            string[] fieldNames =
            [
                nameof(ImportCuesheet.Artist),
                nameof(ImportCuesheet.Title),
                nameof(ImportCuesheet.Audiofile),
                nameof(ImportCuesheet.CDTextfile),
                nameof(ImportCuesheet.Cataloguenumber)
            ];
            var parts = new List<string>();
            int idx = 0;
            while (idx < scheme.Length)
            {
                var field = fieldNames.FirstOrDefault(fn => scheme.IndexOf(fn, idx, StringComparison.Ordinal) == idx);
                if (field != null)
                {
                    parts.Add(field);
                    idx += field.Length;
                }
                else
                {
                    int nextFieldIdx = scheme.Length;
                    foreach (var fn in fieldNames)
                    {
                        int pos = scheme.IndexOf(fn, idx, StringComparison.Ordinal);
                        if (pos >= 0 && pos < nextFieldIdx)
                        {
                            nextFieldIdx = pos;
                        }
                    }
                    string separator = scheme.Substring(idx, nextFieldIdx - idx);
                    parts.Add(separator);
                    idx = nextFieldIdx;
                }
            }

            var regexBuilder = new StringBuilder("^");
            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (fieldNames.Contains(part))
                {
                    bool isLast = i == parts.Count - 1 || parts.Skip(i + 1).All(p => !fieldNames.Contains(p));
                    if (isLast)
                    {
                        regexBuilder.Append($@"(?<{part}>.+)");
                    }
                    else
                    {
                        regexBuilder.Append($@"(?<{part}>.+?)");
                    }
                }
                else
                {
                    string sep = Regex.Escape(part).Replace("\\t", @"\t{1,}");
                    regexBuilder.Append(sep);
                }
            }
            regexBuilder.Append('$');

            return new Regex(regexBuilder.ToString());
        }

        private static Regex CreateTrackRegexPattern(string scheme)
        {
            string[] fieldNames =
            [
                nameof(ImportTrack.Artist),
                nameof(ImportTrack.Title),
                nameof(ImportTrack.Begin),
                nameof(ImportTrack.End),
                nameof(ImportTrack.Length),
                nameof(ImportTrack.Position),
                nameof(ImportTrack.Flags),
                nameof(ImportTrack.PreGap),
                nameof(ImportTrack.PostGap),
                nameof(ImportTrack.StartDateTime)
            ];
            var parts = new List<string>();
            int idx = 0;
            while (idx < scheme.Length)
            {
                var field = fieldNames.FirstOrDefault(fn => scheme.IndexOf(fn, idx, StringComparison.Ordinal) == idx);
                if (field != null)
                {
                    parts.Add(field);
                    idx += field.Length;
                }
                else
                {
                    int nextFieldIdx = scheme.Length;
                    foreach (var fn in fieldNames)
                    {
                        int pos = scheme.IndexOf(fn, idx, StringComparison.Ordinal);
                        if (pos >= 0 && pos < nextFieldIdx)
                        {
                            nextFieldIdx = pos;
                        }
                    }
                    string separator = scheme.Substring(idx, nextFieldIdx - idx);
                    parts.Add(separator);
                    idx = nextFieldIdx;
                }
            }

            var regexBuilder = new StringBuilder("^");
            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (fieldNames.Contains(part))
                {
                    bool isLast = i == parts.Count - 1 || parts.Skip(i + 1).All(p => !fieldNames.Contains(p));
                    if (isLast)
                    {
                        regexBuilder.Append($@"(?<{part}>.+)");
                    }
                    else
                    {
                        regexBuilder.Append($@"(?<{part}>.+?)");
                    }
                }
                else
                {
                    string sep = Regex.Escape(part).Replace("\\t", @"\t{1,}");
                    regexBuilder.Append(sep);
                }
            }
            regexBuilder.Append('$');

            return new Regex(regexBuilder.ToString());
        }
    }
}
