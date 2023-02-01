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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Shared;
using System.Text;
using System.Xml.Linq;

namespace AudioCuesheetEditor.Model.IO
{
    public class CuesheetfileGenerator
    {
        public Cuesheet Cuesheet { get; private set; }

        /// <summary>
        /// Is an export of the <see cref="Cuesheet"/> possible?
        /// </summary>
        public Boolean CanWrite
        {
            get
            {
                if (Cuesheet.Validate().Status == ValidationStatus.Error)
                {
                    return false;
                }
                if (Cuesheet.Cataloguenumber.Validate().Status == ValidationStatus.Error)
                {
                    return false;
                }
                if (Cuesheet.Tracks.Any(x => x.Validate().Status == ValidationStatus.Error))
                {
                    return false;
                }
                return true;
            }
        }

        public CuesheetfileGenerator(Cuesheet cuesheet)
        {
            Cuesheet = cuesheet;
        }

        public IReadOnlyCollection<Cuesheetfile> GenerateCuesheetFiles(String? filename) 
        {
            List<Cuesheetfile> cuesheetfiles = new();
            if (CanWrite == true)
            {
                if (String.IsNullOrEmpty(filename) || (Cuesheetfile.ValidateFilename(filename).Status != ValidationStatus.Success))
                {
                    filename = Cuesheetfile.DefaultFilename;
                }
                if (Cuesheet.SplitPoints != null)
                {
                    TimeSpan? previousSplitPointMoment = null;
                    var counter = 1;
                    foreach (var splitPoint in Cuesheet.SplitPoints.OrderBy(x => x.Moment))
                    {
                        if (splitPoint.Validate().Status == ValidationStatus.Success)
                        {
                            var content = WriteCuesheet(previousSplitPointMoment, splitPoint.Moment);
                            var name = String.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(filename), counter, Cuesheet.FileExtension);
                            cuesheetfiles.Add(new Cuesheetfile() { Filename = name, Content = Encoding.UTF8.GetBytes(content), Begin = previousSplitPointMoment, End = splitPoint.Moment });
                            previousSplitPointMoment = splitPoint.Moment;
                            counter++;
                        }
                    }
                    var lastName = String.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(filename), counter, Cuesheet.FileExtension);
                    //Attach the last part after the SplitPoint
                    var lastContent = WriteCuesheet(previousSplitPointMoment);
                    cuesheetfiles.Add(new Cuesheetfile() { Filename = lastName, Content = Encoding.UTF8.GetBytes(lastContent), Begin = previousSplitPointMoment });
                }
                else
                {
                    var content = WriteCuesheet();
                    cuesheetfiles.Add(new Cuesheetfile() { Filename = filename, Content = Encoding.UTF8.GetBytes(content) }); ;
                }
            }
            return cuesheetfiles;
        }

        private String WriteCuesheet(TimeSpan? from = null, TimeSpan? to = null)
        {
            var builder = new StringBuilder();
            if ((Cuesheet.Cataloguenumber != null) && (String.IsNullOrEmpty(Cuesheet.Cataloguenumber.Value) == false) && (Cuesheet.Cataloguenumber.Validate().Status != Entity.ValidationStatus.Error))
            {
                builder.AppendLine(String.Format("{0} {1}", CuesheetConstants.CuesheetCatalogueNumber, Cuesheet.Cataloguenumber.Value));
            }
            if (Cuesheet.CDTextfile != null)
            {
                builder.AppendLine(String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetCDTextfile, Cuesheet.CDTextfile.FileName));
            }
            builder.AppendLine(String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, Cuesheet.Title));
            builder.AppendLine(String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, Cuesheet.Artist));
            //TODO: Split Audiofile!
            builder.AppendLine(String.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, Cuesheet.Audiofile?.Name, Cuesheet.Audiofile?.AudioFileType));
            IEnumerable<Track> tracks = Cuesheet.Tracks.OrderBy(x => x.Position);
            if ((from != null) && (to != null))
            {
                tracks = Cuesheet.Tracks.Where(x => x.Begin <= to && x.End >= from).OrderBy(x => x.Position);
            }
            else
            {
                if (from != null)
                {
                    tracks = Cuesheet.Tracks.Where(x => x.End >= from).OrderBy(x => x.Position);
                }
                if (to != null)
                {
                    tracks = Cuesheet.Tracks.Where(x => x.Begin <= to).OrderBy(x => x.Position);
                }
            }
            if (tracks.Any())
            {
                //Position and begin should always start from 0 even with splitpoints
                int positionDifference = 1 - Convert.ToInt32(tracks.First().Position);
                foreach (var track in tracks)
                {
                    builder.AppendLine(String.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, track.Position + positionDifference, CuesheetConstants.CuesheetTrackAudio));
                    builder.AppendLine(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title));
                    builder.AppendLine(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist));
                    if (track.Flags.Count > 0)
                    {
                        builder.AppendLine(String.Format("{0}{1}{2} {3}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackFlags, String.Join(" ", track.Flags.Select(x => x.CuesheetLabel))));
                    }
                    if (track.PreGap.HasValue)
                    {
                        builder.AppendLine(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackPreGap, Math.Floor(track.PreGap.Value.TotalMinutes), track.PreGap.Value.Seconds, (track.PreGap.Value.Milliseconds * 75) / 1000));
                    }
                    if (track.Begin.HasValue)
                    {
                        var begin = track.Begin.Value;
                        if (from != null)
                        {
                            if (from >= track.Begin)
                            {
                                begin = TimeSpan.Zero;
                            }
                            else
                            {
                                begin = track.Begin.Value - from.Value;
                            }
                        }
                        builder.AppendLine(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(begin.TotalMinutes), begin.Seconds, begin.Milliseconds * 75 / 1000));
                    }
                    else
                    {
                        throw new NullReferenceException(String.Format("{0} may not be null!", nameof(Track.Begin)));
                    }
                    if (track.PostGap.HasValue)
                    {
                        builder.AppendLine(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackPostGap, Math.Floor(track.PostGap.Value.TotalMinutes), track.PostGap.Value.Seconds, (track.PostGap.Value.Milliseconds * 75) / 1000));
                    }
                }
            }
            return builder.ToString();
        }
    }
}
