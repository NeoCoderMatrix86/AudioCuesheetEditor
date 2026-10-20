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
using AudioCuesheetEditor.Model.IO.Import;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.Services.IO;

public class CuesheetImportService
{
    public static IImportfile Analyse(string fileContent)
    {
        Importfile importfile = new()
        {
            FileContent = fileContent,
            FileType = ImportFileType.Cuesheet
        };
        try
        {
            importfile.AnalyzedCuesheet = new();
            var cuesheetArtistGroupName = "CuesheetArtist";
            var cuesheetTitleGroupName = "CuesheetTitle";
            var cuesheetFileNameGroupName = "CuesheetFileName";
            var cuesheetCDTextfileGroupName = "CuesheetCDTextfile";
            var cuesheetCatalogueNumberGroupName = "CuesheetCatalogueNumber";
            var trackArtistGroupName = "TrackArtist";
            var trackTitleGroupName = "TrackTitle";
            var trackFlagsGroupName = "TrackFlags";
            var trackPreGapGroupName = "TrackPostGap";
            var trackPostGapGroupName = "TrackPostGap";
            var trackIndex01GroupName = "TrackIndex01";
            var regexCuesheetArtist = new Regex("^" + CuesheetConstants.CuesheetArtist + " \"(?'" + cuesheetArtistGroupName + "'.{0,})\"");
            var regexCuesheetTitle = new Regex("^" + CuesheetConstants.CuesheetTitle + " \"(?'" + cuesheetTitleGroupName + "'.{0,})\"");
            var regexCuesheetFile = new Regex("^" + CuesheetConstants.CuesheetFileName + " \"(?'" + cuesheetFileNameGroupName + "'.{0,})\" \\w{1,}");
            var regexTrackBegin = new Regex(CuesheetConstants.CuesheetTrack + " [0-9]{1,} " + CuesheetConstants.CuesheetTrackAudio);
            var regexTrackArtist = new Regex(CuesheetConstants.TrackArtist + " \"(?'" + trackArtistGroupName + "'.{0,})\"");
            var regexTrackTitle = new Regex(CuesheetConstants.TrackTitle + " \"(?'" + trackTitleGroupName + "'.{0,})\"");
            var regexTrackIndex = new Regex(CuesheetConstants.TrackIndex01 + "(?'" + trackIndex01GroupName + "'.{0,})");
            var regexTrackFlags = new Regex(CuesheetConstants.TrackFlags + "(?'" + trackFlagsGroupName + "'.{0,})");
            var regexTrackPreGap = new Regex(CuesheetConstants.TrackPreGap + "(?'" + trackPreGapGroupName + "'.{0,})");
            var regexTrackPostGap = new Regex(CuesheetConstants.TrackPostGap + "(?'" + trackPostGapGroupName + "'.{0,})");
            var regexCDTextfile = new Regex("^" + CuesheetConstants.CuesheetCDTextfile + " \"(?'" + cuesheetCDTextfileGroupName + "'.{0,})\"");
            var regexCatalogueNumber = new Regex("^" + CuesheetConstants.CuesheetCatalogueNumber + " (?'" + cuesheetCatalogueNumberGroupName + "'.{0,})");
            ImportTrack? track = null;
            ImportAudiofile? audiofile = null;
            StringBuilder recognizedContent = new();
            foreach (var line in fileContent.Split(Environment.NewLine))
            {
                var recognizedLine = line;
                if (string.IsNullOrEmpty(line) == false)
                {
                    if ((regexCuesheetArtist.IsMatch(line) == true) && (track == null))
                    {
                        var match = regexCuesheetArtist.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(cuesheetArtistGroupName);
                        if (matchGroup != null)
                        {
                            var artist = matchGroup.Value;
                            importfile.AnalyzedCuesheet.Artist = artist;
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{cuesheetArtistGroupName}' was null!");
                        }
                    }
                    if ((regexCuesheetTitle.IsMatch(line) == true) && (track == null))
                    {
                        var match = regexCuesheetTitle.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(cuesheetTitleGroupName);
                        if (matchGroup != null)
                        {
                            var title = matchGroup.Value;
                            importfile.AnalyzedCuesheet.Title = title;
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{cuesheetTitleGroupName}' was null!");
                        }
                    }
                    if (regexCuesheetFile.IsMatch(line) == true)
                    {
                        var match = regexCuesheetFile.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(cuesheetFileNameGroupName);
                        if (matchGroup != null)
                        {
                            audiofile = new() { Name = matchGroup.Value };
                            importfile.AnalyzedCuesheet.Audiofiles.Add(audiofile);
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{cuesheetFileNameGroupName}' was null!");
                        }
                    }
                    if (regexCDTextfile.IsMatch(line) == true)
                    {
                        var match = regexCDTextfile.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(cuesheetCDTextfileGroupName);
                        if (matchGroup != null)
                        {
                            var cdTextfile = matchGroup.Value;
                            importfile.AnalyzedCuesheet.CDTextfile = cdTextfile;
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{cuesheetCDTextfileGroupName}' was null!");
                        }
                    }
                    if (regexCatalogueNumber.IsMatch(line) == true)
                    {
                        var match = regexCatalogueNumber.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(cuesheetCatalogueNumberGroupName);
                        if (matchGroup != null)
                        {
                            var catalogueNumber = matchGroup.Value;
                            importfile.AnalyzedCuesheet.Cataloguenumber = catalogueNumber;
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{cuesheetCatalogueNumberGroupName}' was null!");
                        }
                    }
                    if (regexTrackBegin.IsMatch(line) == true)
                    {
                        track = new ImportTrack();
                        recognizedLine = CuesheetConstants.GetMarkedText(line);
                    }
                    if ((regexTrackArtist.IsMatch(line) == true) && (track != null))
                    {
                        var match = regexTrackArtist.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(trackArtistGroupName);
                        if (matchGroup != null)
                        {
                            var artist = matchGroup.Value;
                            track.Artist = artist;
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{trackArtistGroupName}' was null!");
                        }
                    }
                    if ((regexTrackTitle.IsMatch(line) == true) && (track != null))
                    {
                        var match = regexTrackTitle.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(trackTitleGroupName);
                        if (matchGroup != null)
                        {
                            var title = matchGroup.Value;
                            track.Title = title;
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{trackTitleGroupName}' was null!");
                        }
                    }
                    if ((regexTrackFlags.IsMatch(line) == true) && (track != null))
                    {
                        var match = regexTrackFlags.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(trackFlagsGroupName);
                        if (matchGroup != null)
                        {
                            var flags = matchGroup.Value;
                            var flagList = Flag.AvailableFlags.Where(x => flags.Contains(x.CuesheetLabel));
                            track.Flags = flagList;
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{trackFlagsGroupName}' was null!");
                        }
                    }
                    if ((regexTrackPreGap.IsMatch(line) == true) && (track != null))
                    {
                        var match = regexTrackPreGap.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(trackPreGapGroupName);
                        if (matchGroup != null)
                        {
                            var minutes = int.Parse(matchGroup.Value[..matchGroup.Value.IndexOf(':')], CultureInfo.CurrentCulture);
                            var seconds = int.Parse(matchGroup.Value.AsSpan(matchGroup.Value.IndexOf(':') + 1, 2), CultureInfo.CurrentCulture);
                            var frames = int.Parse(matchGroup.Value[(matchGroup.Value.LastIndexOf(':') + 1)..], CultureInfo.CurrentCulture);
                            track.PreGap = new TimeSpan(0, 0, minutes, seconds, Convert.ToInt32(frames / 75.0 * 1000));
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{trackPreGapGroupName}' was null!");
                        }
                    }
                    if ((regexTrackIndex.IsMatch(line) == true) && (track != null))
                    {
                        var match = regexTrackIndex.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(trackIndex01GroupName);
                        if (matchGroup != null)
                        {
                            var minutes = int.Parse(matchGroup.Value[..matchGroup.Value.IndexOf(':')], CultureInfo.CurrentCulture);
                            var seconds = int.Parse(matchGroup.Value.AsSpan(matchGroup.Value.IndexOf(':') + 1, 2), CultureInfo.CurrentCulture);
                            var frames = int.Parse(matchGroup.Value[(matchGroup.Value.LastIndexOf(':') + 1)..], CultureInfo.CurrentCulture);
                            track.Begin = new TimeSpan(0, 0, minutes, seconds, Convert.ToInt32(frames / 75.0 * 1000));
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{trackIndex01GroupName}' was null!");
                        }
                        audiofile?.Tracks.Add(track);
                    }
                    if ((regexTrackPostGap.IsMatch(line) == true) && (track != null))
                    {
                        var match = regexTrackPostGap.Match(line);
                        recognizedLine = line.Replace(match.Value, CuesheetConstants.GetMarkedText(match.Value));
                        var matchGroup = match.Groups.GetValueOrDefault(trackPostGapGroupName);
                        if (matchGroup != null)
                        {
                            var minutes = int.Parse(matchGroup.Value[..matchGroup.Value.IndexOf(':')], CultureInfo.CurrentCulture);
                            var seconds = int.Parse(matchGroup.Value.AsSpan(matchGroup.Value.IndexOf(':') + 1, 2), CultureInfo.CurrentCulture);
                            var frames = int.Parse(matchGroup.Value[(matchGroup.Value.LastIndexOf(':') + 1)..], CultureInfo.CurrentCulture);
                            track.PostGap = new TimeSpan(0, 0, minutes, seconds, Convert.ToInt32(frames / 75.0 * 1000));
                        }
                        else
                        {
                            throw new ArgumentException($"Group '{trackPostGapGroupName}' was null!");
                        }
                    }
                }
                _ = recognizedContent.AppendLine(recognizedLine);
            }
            importfile.FileContentRecognized = recognizedContent.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }
        catch (Exception ex)
        {
            importfile.AnalyseException = ex;
            importfile.AnalyzedCuesheet = null;
            importfile.FileContentRecognized = fileContent;
        }
        return importfile;
    }
}
