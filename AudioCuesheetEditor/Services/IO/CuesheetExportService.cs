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
using AudioCuesheetEditor.Extensions;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Export;
using System.Text;

namespace AudioCuesheetEditor.Services.IO
{
    //TODO: Unit Tests
    public class CuesheetExportService(SessionStateContainer sessionStateContainer)
    {
        private readonly SessionStateContainer _sessionStateContainer = sessionStateContainer;

        public Boolean CanGenerateExportfiles(string filename)
        {
            var extension = Path.GetExtension(filename);
            return extension.Equals(FileExtensions.Cuesheet, StringComparison.OrdinalIgnoreCase) && _sessionStateContainer.Cuesheet.Validate().Status == ValidationStatus.Success;
        }

        public IReadOnlyCollection<Exportfile> GenerateExportfiles(string filename)
        {
            List<Exportfile> exportfiles = [];
            if (CanGenerateExportfiles(filename))
            {
                if (_sessionStateContainer.Cuesheet.Sections.Count != 0)
                {
                    var counter = 1;
                    string? content = null;
                    string? audioFileName = null;
                    foreach (var section in _sessionStateContainer.Cuesheet.Sections.OrderBy(x => x.Begin))
                    {
                        audioFileName = section.AudiofileName;
                        if (section.Validate().Status == ValidationStatus.Success)
                        {
                            content = WriteCuesheet(audioFileName, section);
                            filename = string.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(filename), counter, FileExtensions.Cuesheet);
                            if (content != null)
                            {
                                exportfiles.Add(new Exportfile() { Name = filename, Content = Encoding.UTF8.GetBytes(content), Begin = section.Begin, End = section.End });
                            }
                            counter++;
                        }
                    }
                }
                else
                {
                    string? content = null;
                    if (_sessionStateContainer.Cuesheet.Audiofile != null)
                    {
                        content = WriteCuesheet(_sessionStateContainer.Cuesheet.Audiofile.Name);
                    }
                    if (content != null)
                    {
                        var begin = _sessionStateContainer.Cuesheet.Tracks.Min(x => x.Begin);
                        var end = _sessionStateContainer.Cuesheet.Tracks.Max(x => x.End);
                        exportfiles.Add(new Exportfile() { Name = filename, Content = Encoding.UTF8.GetBytes(content), Begin = begin, End = end });
                    }
                }
            }
            return exportfiles;
        }

        private string WriteCuesheet(string? audiofileName, CuesheetSection? section = null)
        {
            var builder = new StringBuilder();
            if (string.IsNullOrEmpty(_sessionStateContainer.Cuesheet.Cataloguenumber) == false)
            {
                builder.AppendLine(string.Format("{0} {1}", CuesheetConstants.CuesheetCatalogueNumber, _sessionStateContainer.Cuesheet.Cataloguenumber));
            }
            if (_sessionStateContainer.Cuesheet.CDTextfile != null)
            {
                builder.AppendLine(string.Format("{0} \"{1}\"", CuesheetConstants.CuesheetCDTextfile, _sessionStateContainer.Cuesheet.CDTextfile.Name));
            }
            builder.AppendLine(string.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, section != null ? section.Title : _sessionStateContainer.Cuesheet.Title));
            builder.AppendLine(string.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, section != null ? section.Artist : _sessionStateContainer.Cuesheet.Artist));
            builder.AppendLine(string.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, audiofileName, _sessionStateContainer.Cuesheet.Audiofile?.AudioFileType));
            IEnumerable<Track> tracks = _sessionStateContainer.Cuesheet.Tracks.OrderBy(x => x.Position);
            if (section != null)
            {
                tracks = _sessionStateContainer.Cuesheet.Tracks.Where(x => x.Begin <= section.End && x.End >= section.Begin).OrderBy(x => x.Position);
            }
            if (tracks.Any())
            {
                //Position and begin should always start from 0 even with splitpoints
                int positionDifference = 1 - Convert.ToInt32(tracks.First().Position);
                foreach (var track in tracks)
                {
                    builder.AppendLine(string.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, track.Position + positionDifference, CuesheetConstants.CuesheetTrackAudio));
                    builder.AppendLine(string.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title));
                    builder.AppendLine(string.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist));
                    if (track.Flags.Any())
                    {
                        builder.AppendLine(string.Format("{0}{1}{2} {3}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackFlags, string.Join(" ", track.Flags.Select(x => x.CuesheetLabel))));
                    }
                    if (track.PreGap.HasValue)
                    {
                        builder.AppendLine(string.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackPreGap, Math.Floor(track.PreGap.Value.TotalMinutes), track.PreGap.Value.Seconds, track.PreGap.Value.Milliseconds * 75 / 1000));
                    }
                    if (track.Begin.HasValue)
                    {
                        var begin = track.Begin.Value;
                        if (section != null && section.Begin.HasValue)
                        {
                            if (section.Begin >= track.Begin)
                            {
                                begin = TimeSpan.Zero;
                            }
                            else
                            {
                                begin = track.Begin.Value - section.Begin.Value;
                            }
                        }
                        builder.AppendLine(string.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(begin.TotalMinutes), begin.Seconds, begin.Milliseconds * 75 / 1000));
                    }
                    else
                    {
                        throw new NullReferenceException(string.Format("{0} may not be null!", nameof(Track.Begin)));
                    }
                    if (track.PostGap.HasValue)
                    {
                        builder.AppendLine(string.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackPostGap, Math.Floor(track.PostGap.Value.TotalMinutes), track.PostGap.Value.Seconds, track.PostGap.Value.Milliseconds * 75 / 1000));
                    }
                }
            }
            return builder.ToString();
        }
    }
}
