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
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Services.UI;
using Microsoft.Extensions.Localization;
using System.Text;

namespace AudioCuesheetEditor.Services.IO
{
    public class CuesheetExportService(ISessionStateContainer sessionStateContainer, IStringLocalizer<ValidationMessage> localizer)
    {
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly IStringLocalizer<ValidationMessage> _localizer = localizer;

        public Result CanGenerateExportfile(string? filename)
        {
            List<ValidationMessage> validationMessages = [];
            var extension = Path.GetExtension(filename);
            if (extension?.Equals(FileExtensions.Cuesheet, StringComparison.OrdinalIgnoreCase) == false)
            {
                validationMessages ??= [];
                validationMessages.Add(new ValidationMessage("File extension is not '{0}'", FileExtensions.Cuesheet));
            }
            validationMessages.AddRange(_sessionStateContainer.Cuesheet.Validate().ValidationMessages);
            validationMessages.AddRange(_sessionStateContainer.Cuesheet.Tracks.Select(x => x.Validate()).SelectMany(x => x.ValidationMessages));
            if (validationMessages.Count != 0)
            {
                return Result.Failure(new Error(ErrorType.ValidationFailed, string.Join(Environment.NewLine, validationMessages.Select(x => x.GetMessageLocalized(_localizer)))));
            }
            return Result.Success();
        }

        public Result<Exportfile> GenerateExportfile(string? filename)
        {
            var validationResult = CanGenerateExportfile(filename);
            if (validationResult.IsSuccess == false)
            {
                return Result<Exportfile>.Failure(new Error(ErrorType.ValidationFailed, validationResult.Error!.Message));
            }
            string? content = null;
            var extension = Path.GetExtension(filename);
            if (extension?.Equals(FileExtensions.Cuesheet, StringComparison.OrdinalIgnoreCase) == false)
            {
                filename = $"{filename}{FileExtensions.Cuesheet}";
            }
            if (_sessionStateContainer.Cuesheet.Audiofile != null)
            {
                content = WriteCuesheet(_sessionStateContainer.Cuesheet.Audiofile.Name);
            }
            var begin = _sessionStateContainer.Cuesheet.Tracks.Min(x => x.Begin);
            var end = _sessionStateContainer.Cuesheet.Tracks.Max(x => x.End);
            return Result<Exportfile>.Success(new Exportfile() { Name = filename!, Content = content, Begin = begin, End = end });
        }

        private string WriteCuesheet(string? audiofileName)
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
            builder.AppendLine(string.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, _sessionStateContainer.Cuesheet.Title));
            builder.AppendLine(string.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, _sessionStateContainer.Cuesheet.Artist));
            builder.AppendLine(string.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, audiofileName, _sessionStateContainer.Cuesheet.Audiofile?.AudioFileType));
            IEnumerable<Track> tracks = _sessionStateContainer.Cuesheet.Tracks.OrderBy(x => x.Position);
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
