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
using AudioCuesheetEditor.Model.Options;
using System.Data;
using System.Diagnostics.Metrics;
using System.Text;
using System.Xml.Linq;

namespace AudioCuesheetEditor.Model.IO.Export
{
    public enum ExportType
    {
        Cuesheet = 0,
        Exportprofile
    }

    public class ExportfileGenerator
    {
        public Cuesheet Cuesheet { get; }
        public Exportprofile? Exportprofile { get; set; }
        public ApplicationOptions? ApplicationOptions { get; set; }

        public ExportfileGenerator(Cuesheet cuesheet, Exportprofile? exportprofile = null, ApplicationOptions? applicationOptions = null)
        {
            Cuesheet = cuesheet;
            Exportprofile = exportprofile;
            ApplicationOptions = applicationOptions;
        }

        /// <summary>
        /// Is an export of the <see cref="Cuesheet"/> possible?
        /// </summary>
        /// <param name="exportType">Which type of export should be done?</param>
        /// <returns>Boolean indicating if export is possible or not</returns>
        public Boolean CanWrite(ExportType exportType)
        {
            Boolean canWrite = false;
            switch (exportType)
            {
                case ExportType.Cuesheet:
                    canWrite = (Cuesheet.Validate().Status != ValidationStatus.Error) 
                        && (Cuesheet.Cataloguenumber.Validate().Status != ValidationStatus.Error)
                        && Cuesheet.Tracks.All(x => x.Validate().Status != ValidationStatus.Error)
                        && (ApplicationOptions?.Validate(x => x.CuesheetFilename).Status != ValidationStatus.Error);
                    break;
                case ExportType.Exportprofile:
                    if (Exportprofile != null)
                    {
                        canWrite = (Cuesheet.Validate().Status != ValidationStatus.Error)
                            && (Cuesheet.Cataloguenumber.Validate().Status != ValidationStatus.Error)
                            && Cuesheet.Tracks.All(x => x.Validate().Status != ValidationStatus.Error)
                            && (ApplicationOptions?.Validate(x => x.CuesheetFilename).Status != ValidationStatus.Error)
                            && (Exportprofile.Validate().Status != ValidationStatus.Error);
                    }
                    break;
            }
            return canWrite;
        }

        public IReadOnlyCollection<Exportfile> GenerateExportfiles(ExportType exportType)
        {
            List<Exportfile> exportfiles = new();
            if (CanWrite(exportType))
            {
                if (Cuesheet.SplitPoints.Count != 0)
                {
                    TimeSpan? previousSplitPointMoment = null;
                    var counter = 1;
                    String? content = null;
                    String filename = String.Empty;
                    foreach (var splitPoint in Cuesheet.SplitPoints.OrderBy(x => x.Moment))
                    {
                        if (splitPoint.Validate().Status == ValidationStatus.Success)
                        {
                            switch (exportType)
                            {
                                case ExportType.Cuesheet:
                                    content = WriteCuesheet(previousSplitPointMoment, splitPoint.Moment);
                                    filename = String.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(ApplicationOptions?.CuesheetFilename), counter, Cuesheet.FileExtension);
                                    break;
                                case ExportType.Exportprofile:
                                    if (Exportprofile != null)
                                    {
                                        content = WriteExport(previousSplitPointMoment, splitPoint.Moment);
                                        filename = String.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(Exportprofile.Filename), counter, Path.GetExtension(Exportprofile.Filename));
                                    }
                                    break;
                            }
                            if (content != null)
                            {
                                exportfiles.Add(new Exportfile() { Filename = filename, Content = Encoding.UTF8.GetBytes(content), Begin = previousSplitPointMoment, End = splitPoint.Moment });
                            }
                            previousSplitPointMoment = splitPoint.Moment;
                            counter++;
                        }
                    }
                    //After a split point attach the last part
                    switch (exportType)
                    {
                        case ExportType.Cuesheet:
                            content = WriteCuesheet(previousSplitPointMoment);
                            filename = String.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(ApplicationOptions?.CuesheetFilename), counter, Cuesheet.FileExtension);
                            break;
                        case ExportType.Exportprofile:
                            if (Exportprofile != null)
                            {
                                content = WriteExport(previousSplitPointMoment);
                                filename = String.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(Exportprofile.Filename), counter, Path.GetExtension(Exportprofile.Filename));
                            }
                            break;
                    }
                    if (content != null)
                    {
                        exportfiles.Add(new Exportfile() { Filename = filename, Content = Encoding.UTF8.GetBytes(content), Begin = previousSplitPointMoment});
                    }
                }
                else
                {
                    String filename = String.Empty;
                    String? content = null;
                    switch (exportType)
                    {
                        case ExportType.Cuesheet:
                            var cuesheetfilename = ApplicationOptions?.CuesheetFilename;
                            if (String.IsNullOrEmpty(cuesheetfilename) == false)
                            {
                                filename = cuesheetfilename;
                            }
                            else
                            {
                                filename = Exportfile.DefaultCuesheetFilename;
                            }
                            content = WriteCuesheet();
                            break;
                        case ExportType.Exportprofile:
                            if (Exportprofile != null)
                            {
                                filename = Exportprofile.Filename;
                                content = WriteExport();
                            }
                            break;
                    }
                    if (content != null)
                    {
                        exportfiles.Add(new Exportfile() { Filename = filename, Content = Encoding.UTF8.GetBytes(content) });
                    }
                }
            }
            return exportfiles;
        }

        private string WriteCuesheet(TimeSpan? from = null, TimeSpan? to = null)
        {
            var builder = new StringBuilder();
            if (Cuesheet.Cataloguenumber != null && string.IsNullOrEmpty(Cuesheet.Cataloguenumber.Value) == false && Cuesheet.Cataloguenumber.Validate().Status != ValidationStatus.Error)
            {
                builder.AppendLine(string.Format("{0} {1}", CuesheetConstants.CuesheetCatalogueNumber, Cuesheet.Cataloguenumber.Value));
            }
            if (Cuesheet.CDTextfile != null)
            {
                builder.AppendLine(string.Format("{0} \"{1}\"", CuesheetConstants.CuesheetCDTextfile, Cuesheet.CDTextfile.FileName));
            }
            builder.AppendLine(string.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, Cuesheet.Title));
            builder.AppendLine(string.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, Cuesheet.Artist));
            //TODO: Split Audiofile!
            builder.AppendLine(string.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, Cuesheet.Audiofile?.Name, Cuesheet.Audiofile?.AudioFileType));
            IEnumerable<Track> tracks = Cuesheet.Tracks.OrderBy(x => x.Position);
            if (from != null && to != null)
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
                    builder.AppendLine(string.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, track.Position + positionDifference, CuesheetConstants.CuesheetTrackAudio));
                    builder.AppendLine(string.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title));
                    builder.AppendLine(string.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist));
                    if (track.Flags.Count > 0)
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

        private String WriteExport(TimeSpan? from = null, TimeSpan? to = null)
        {
            var builder = new StringBuilder();
            if (Exportprofile != null)
            {
                //TODO: Split Audiofile!
                var header = Exportprofile.SchemeHead
                    .Replace(Exportprofile.SchemeCuesheetArtist, Cuesheet.Artist)
                    .Replace(Exportprofile.SchemeCuesheetTitle, Cuesheet.Title)
                    .Replace(Exportprofile.SchemeCuesheetAudiofile, Cuesheet.Audiofile?.Name)
                    .Replace(Exportprofile.SchemeCuesheetCDTextfile, Cuesheet.CDTextfile?.FileName)
                    .Replace(Exportprofile.SchemeCuesheetCatalogueNumber, Cuesheet.Cataloguenumber?.Value)
                    .Replace(Exportprofile.SchemeDate, DateTime.Now.ToShortDateString())
                    .Replace(Exportprofile.SchemeDateTime, DateTime.Now.ToString())
                    .Replace(Exportprofile.SchemeTime, DateTime.Now.ToLongTimeString());
                builder.AppendLine(header);
                IEnumerable<Track> tracks = Cuesheet.Tracks.OrderBy(x => x.Position);
                if (from != null && to != null)
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
                    //TODO Position and begin should always start from 0 even with splitpoints
                    foreach (var track in tracks)
                    {
                        var trackLine = Exportprofile.SchemeTracks
                            .Replace(Exportprofile.SchemeTrackArtist, track.Artist)
                            .Replace(Exportprofile.SchemeTrackTitle, track.Title)
                            .Replace(Exportprofile.SchemeTrackPosition, track.Position != null ? track.Position.Value.ToString() : String.Empty)
                            .Replace(Exportprofile.SchemeTrackBegin, track.Begin != null ? track.Begin.Value.ToString() : String.Empty)
                            .Replace(Exportprofile.SchemeTrackEnd, track.End != null ? track.End.Value.ToString() : String.Empty)
                            .Replace(Exportprofile.SchemeTrackLength, track.Length != null ? track.Length.Value.ToString() : String.Empty)
                            .Replace(Exportprofile.SchemeTrackFlags, String.Join(" ", track.Flags.Select(x => x.CuesheetLabel)))
                            .Replace(Exportprofile.SchemeTrackPreGap, track.PreGap != null ? track.PreGap.Value.ToString() : String.Empty)
                            .Replace(Exportprofile.SchemeTrackPostGap, track.PostGap != null ? track.PostGap.Value.ToString() : String.Empty)
                            .Replace(Exportprofile.SchemeDate, DateTime.Now.ToShortDateString())
                            .Replace(Exportprofile.SchemeDateTime, DateTime.Now.ToString())
                            .Replace(Exportprofile.SchemeTime, DateTime.Now.ToLongTimeString());
                        builder.AppendLine(trackLine);
                    }
                }
                //TODO: Split Audiofile!
                var footer = Exportprofile.SchemeFooter
                    .Replace(Exportprofile.SchemeCuesheetArtist, Cuesheet.Artist)
                    .Replace(Exportprofile.SchemeCuesheetTitle, Cuesheet.Title)
                    .Replace(Exportprofile.SchemeCuesheetAudiofile, Cuesheet.Audiofile?.Name)
                    .Replace(Exportprofile.SchemeCuesheetCDTextfile, Cuesheet.CDTextfile?.FileName)
                    .Replace(Exportprofile.SchemeCuesheetCatalogueNumber, Cuesheet.Cataloguenumber?.Value)
                    .Replace(Exportprofile.SchemeDate, DateTime.Now.ToShortDateString())
                    .Replace(Exportprofile.SchemeDateTime, DateTime.Now.ToString())
                    .Replace(Exportprofile.SchemeTime, DateTime.Now.ToLongTimeString());
                builder.AppendLine(footer);
            }
            return builder.ToString();
        }
    }
}
