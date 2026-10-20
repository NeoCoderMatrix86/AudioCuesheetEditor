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
using AudioCuesheetEditor.Services.UI;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text;

namespace AudioCuesheetEditor.Services.IO;

public class CuesheetExportService(ISessionStateContainer sessionStateContainer, IStringLocalizer<ValidationMessage> localizer)
{
    readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
    readonly IStringLocalizer<ValidationMessage> _localizer = localizer;

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
        validationMessages.AddRange(_sessionStateContainer.Cuesheet.Audiofiles.Select(x => x.Validate()).SelectMany(x => x.ValidationMessages));
        validationMessages.AddRange(_sessionStateContainer.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).Select(x => x.Validate()).SelectMany(x => x.ValidationMessages));
        if (validationMessages.Count != 0)
        {
            return Result.Failure(new FailureDetails(ErrorType.ValidationFailed, string.Join(Environment.NewLine, validationMessages.Select(x => x.GetMessageLocalized(_localizer)))));
        }
        return Result.Success();
    }

    public Result<Exportfile> GenerateExportfile(string? filename)
    {
        var validationResult = CanGenerateExportfile(filename);
        if (validationResult.IsSuccess == false)
        {
            return Result.Failure(new FailureDetails(ErrorType.ValidationFailed, validationResult.FailureDetails!.Message));
        }
        return Result.Success(new Exportfile() { Name = filename!, Content = WriteCuesheet() });
    }

    string WriteCuesheet()
    {
        var builder = new StringBuilder();
        if (string.IsNullOrEmpty(_sessionStateContainer.Cuesheet.Cataloguenumber) == false)
        {
            _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.CuesheetCatalogueNumber} {_sessionStateContainer.Cuesheet.Cataloguenumber}");
        }
        if (_sessionStateContainer.Cuesheet.CDTextfile != null)
        {
            _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.CuesheetCDTextfile} \"{_sessionStateContainer.Cuesheet.CDTextfile.Name}\"");
        }
        _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.CuesheetTitle} \"{_sessionStateContainer.Cuesheet.Title}\"");
        _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.CuesheetArtist} \"{_sessionStateContainer.Cuesheet.Artist}\"");
        foreach (var audiofile in _sessionStateContainer.Cuesheet.Audiofiles)
        {
            _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.CuesheetFileName} \"{audiofile.Name}\" {audiofile.AudioCodec?.FileExtension.Replace(".", string.Empty).ToUpper(CultureInfo.InvariantCulture)}");
            foreach(var track in audiofile.Tracks)
            {
                _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.Tab}{CuesheetConstants.CuesheetTrack} {track.Position:00} {CuesheetConstants.CuesheetTrackAudio}");
                _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.Tab}{CuesheetConstants.Tab}{CuesheetConstants.TrackTitle} \"{track.Title}\"");
                _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.Tab}{CuesheetConstants.Tab}{CuesheetConstants.TrackArtist} \"{track.Artist}\"");
                if (track.Flags.Any())
                {
                    _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.Tab}{CuesheetConstants.Tab}{CuesheetConstants.TrackFlags} {string.Join(" ", track.Flags.Select(x => x.CuesheetLabel))}");
                }
                if (track.PreGap.HasValue)
                {
                    _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.Tab}{CuesheetConstants.Tab}{CuesheetConstants.TrackPreGap} {Math.Floor(track.PreGap.Value.TotalMinutes):00}:{track.PreGap.Value.Seconds:00}:{track.PreGap.Value.Milliseconds * 75 / 1000:00}");
                }
                if (track.Begin.HasValue)
                {
                    var begin = track.Begin.Value;
                    _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.Tab}{CuesheetConstants.Tab}{CuesheetConstants.TrackIndex01} {Math.Floor(begin.TotalMinutes):00}:{begin.Seconds:00}:{begin.Milliseconds * 75 / 1000:00}");
                }
                if (track.PostGap.HasValue)
                {
                    _ = builder.AppendLine(CultureInfo.CurrentCulture, $"{CuesheetConstants.Tab}{CuesheetConstants.Tab}{CuesheetConstants.TrackPostGap} {Math.Floor(track.PostGap.Value.TotalMinutes):00}:{track.PostGap.Value.Seconds:00}:{track.PostGap.Value.Milliseconds * 75 / 1000:00}");
                }
            }
        }
        return builder.ToString();
    }
}
