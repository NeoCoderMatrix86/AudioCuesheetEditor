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
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.Model.IO.Import
{
    public class CuesheetImportFile
    {
        /// <summary>
        /// File content (each element is a file line)
        /// </summary>
        public IReadOnlyCollection<String?>? FileContent { get; private set; }

        /// <summary>
        /// File content with marking which passages has been reconized by scheme
        /// </summary>
        public IReadOnlyCollection<String?>? FileContentRecognized { get; private set; }
        public Exception? AnalyseException { get; private set; }
        public Cuesheet? Cuesheet { get; private set; }

        public CuesheetImportFile(MemoryStream fileContent, ApplicationOptions applicationOptions)
        {
            try
            {
                Cuesheet = new Cuesheet();
                fileContent.Position = 0;
                using var reader = new StreamReader(fileContent);
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
                Track? track = null;
                List<String?> lines = new();
                List<String?>? recognizedLines = new();
                while (reader.EndOfStream == false)
                {
                    var line = reader.ReadLine();
                    lines.Add(line);
                    String? recognizedLine = line;
                    if (String.IsNullOrEmpty(line) == false)
                    {
                        if ((regexCuesheetArtist.IsMatch(line) == true) && (track == null))
                        {
                            var match = regexCuesheetArtist.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(cuesheetArtistGroupName);
                            if (matchGroup != null)
                            {
                                var artist = matchGroup.Value;
                                Cuesheet.Artist = artist;
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", cuesheetArtistGroupName));
                            }
                        }
                        if ((regexCuesheetTitle.IsMatch(line) == true) && (track == null))
                        {
                            var match = regexCuesheetTitle.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(cuesheetTitleGroupName);
                            if (matchGroup != null)
                            {
                                var title = matchGroup.Value;
                                Cuesheet.Title = title;
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", cuesheetTitleGroupName));
                            }
                        }
                        if (regexCuesheetFile.IsMatch(line) == true)
                        {
                            var match = regexCuesheetFile.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(cuesheetFileNameGroupName);
                            if (matchGroup != null)
                            {
                                var audioFile = matchGroup.Value;
                                Cuesheet.Audiofile = new Audiofile(audioFile);
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", cuesheetFileNameGroupName));
                            }
                        }
                        if (regexCDTextfile.IsMatch(line) == true)
                        {
                            var match = regexCDTextfile.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(cuesheetCDTextfileGroupName);
                            if (matchGroup != null)
                            {
                                var cdTextfile = matchGroup.Value;
                                Cuesheet.CDTextfile = new CDTextfile(cdTextfile);
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", cuesheetCDTextfileGroupName));
                            }
                        }
                        if (regexCatalogueNumber.IsMatch(line) == true)
                        {
                            var match = regexCatalogueNumber.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(cuesheetCatalogueNumberGroupName);
                            if (matchGroup != null)
                            {
                                var catalogueNumber = matchGroup.Value;
                                Cuesheet.Cataloguenumber.Value = catalogueNumber;
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", cuesheetCatalogueNumberGroupName));
                            }
                        }
                        if (regexTrackBegin.IsMatch(line) == true)
                        {
                            track = new Track();
                            recognizedLine = String.Format("<Mark>{0}</Mark>", line);
                        }
                        if ((regexTrackArtist.IsMatch(line) == true) && (track != null))
                        {
                            var match = regexTrackArtist.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(trackArtistGroupName);
                            if (matchGroup != null)
                            {
                                var artist = matchGroup.Value;
                                track.Artist = artist;
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", trackArtistGroupName));
                            }
                        }
                        if ((regexTrackTitle.IsMatch(line) == true) && (track != null))
                        {
                            var match = regexTrackTitle.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(trackTitleGroupName);
                            if (matchGroup != null)
                            {
                                var title = matchGroup.Value;
                                track.Title = title;
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", trackTitleGroupName));
                            }
                        }
                        if (regexTrackFlags.IsMatch(line) == true)
                        {
                            var match = regexTrackFlags.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(trackFlagsGroupName);
                            if (matchGroup != null)
                            {
                                var flags = matchGroup.Value;
                                var flagList = Flag.AvailableFlags.Where(x => flags.Contains(x.CuesheetLabel));
                                if (track != null)
                                {
                                    track.SetFlags(flagList);
                                }
                                else
                                {
                                    throw new NullReferenceException(String.Format("Track was null during input {0}", line));
                                }
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", trackFlagsGroupName));
                            }
                        }
                        if (regexTrackPreGap.IsMatch(line) == true)
                        {
                            var match = regexTrackPreGap.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(trackPreGapGroupName);
                            if (matchGroup != null)
                            {
                                var minutes = int.Parse(matchGroup.Value.Substring(0, matchGroup.Value.IndexOf(":")));
                                var seconds = int.Parse(matchGroup.Value.Substring(matchGroup.Value.IndexOf(":") + 1, 2));
                                var frames = int.Parse(matchGroup.Value.Substring(matchGroup.Value.LastIndexOf(":") + 1));
                                if (track != null)
                                {
                                    track.PreGap = new TimeSpan(0, 0, minutes, seconds, Convert.ToInt32((frames / 75.0) * 1000));
                                }
                                else
                                {
                                    throw new NullReferenceException(String.Format("Track was null during input {0}", line));
                                }
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", trackPreGapGroupName));
                            }
                        }
                        if (regexTrackIndex.IsMatch(line) == true)
                        {
                            var match = regexTrackIndex.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(trackIndex01GroupName);
                            if (matchGroup != null)
                            {
                                var minutes = int.Parse(matchGroup.Value.Substring(0, matchGroup.Value.IndexOf(":")));
                                var seconds = int.Parse(matchGroup.Value.Substring(matchGroup.Value.IndexOf(":") + 1, 2));
                                var frames = int.Parse(matchGroup.Value.Substring(matchGroup.Value.LastIndexOf(":") + 1));
                                if (track != null)
                                {
                                    track.Begin = new TimeSpan(0, 0, minutes, seconds, Convert.ToInt32((frames / 75.0) * 1000));
                                }
                                else
                                {
                                    throw new NullReferenceException(String.Format("Track was null during input {0}", line));
                                }
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", trackIndex01GroupName));
                            }
                            if (track != null)
                            {
                                Cuesheet.AddTrack(track, applicationOptions);
                            }
                            else
                            {
                                throw new NullReferenceException(String.Format("Track was null during input {0}", line));
                            }
                        }
                        if (regexTrackPostGap.IsMatch(line) == true)
                        {
                            var match = regexTrackPostGap.Match(line);
                            recognizedLine = line.Replace(match.Value, String.Format("<Mark>{0}</Mark>", match.Value));
                            var matchGroup = match.Groups.GetValueOrDefault(trackPostGapGroupName);
                            if (matchGroup != null)
                            {
                                var minutes = int.Parse(matchGroup.Value.Substring(0, matchGroup.Value.IndexOf(":")));
                                var seconds = int.Parse(matchGroup.Value.Substring(matchGroup.Value.IndexOf(":") + 1, 2));
                                var frames = int.Parse(matchGroup.Value.Substring(matchGroup.Value.LastIndexOf(":") + 1));
                                if (track != null)
                                {
                                    track.PostGap = new TimeSpan(0, 0, minutes, seconds, Convert.ToInt32((frames / 75.0) * 1000));
                                }
                                else
                                {
                                    throw new NullReferenceException(String.Format("Track was null during input {0}", line));
                                }
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Group '{0}' was null!", trackPostGapGroupName));
                            }
                        }
                    }
                    recognizedLines.Add(recognizedLine);
                }
                FileContent = lines.AsReadOnly();
                FileContentRecognized = recognizedLines.AsReadOnly();
            }
            catch(Exception ex)
            {
                AnalyseException = ex;
                Cuesheet = null;
                FileContentRecognized = null;
            }
        }
    }
}
