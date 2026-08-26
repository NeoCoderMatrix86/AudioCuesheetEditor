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
using AudioCuesheetEditor.Services.UI;
using Microsoft.Extensions.Localization;
using System.Data;
using System.Text;

namespace AudioCuesheetEditor.Services.IO
{
    public class ExportfileGenerator(ISessionStateContainer sessionStateContainer, IStringLocalizer<ValidationMessage> localizer)
    {
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly IStringLocalizer<ValidationMessage> _localizer = localizer;

        public Result CanGenerateExportfile(Exportprofile exportprofile)
        {
            List<ValidationMessage> validationMessages = [];
            validationMessages.AddRange(exportprofile.Validate().ValidationMessages);
            validationMessages.AddRange(_sessionStateContainer.Cuesheet.Validate().ValidationMessages);
            validationMessages.AddRange(_sessionStateContainer.Cuesheet.Audiofiles.Select(x => x.Validate()).SelectMany(x => x.ValidationMessages));
            validationMessages.AddRange(_sessionStateContainer.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).Select(x => x.Validate()).SelectMany(x => x.ValidationMessages));
            if (validationMessages.Count != 0)
            {
                return Result.Failure(new Error(ErrorType.ValidationFailed, string.Join(Environment.NewLine, validationMessages.Select(x => x.GetMessageLocalized(_localizer)))));
            }
            return Result.Success();
        }

        public Result<Exportfile> GenerateExportfile(Exportprofile exportprofile)
        {
            var validationResult = CanGenerateExportfile(exportprofile);
            if (validationResult.IsSuccess == false)
            {
                return Result<Exportfile>.Failure(new Error(ErrorType.ValidationFailed, validationResult.Error!.Message));
            }
            return Result<Exportfile>.Success(new Exportfile() { Name = exportprofile.Filename, Content = WriteExport(exportprofile) });
        }

        private string WriteExport(Exportprofile exportprofile)
        {
            var builder = new StringBuilder();
            if (exportprofile != null)
            {
                var header = exportprofile.SchemeHead
                    .Replace(Exportprofile.SchemeCuesheetArtist, _sessionStateContainer.Cuesheet.Artist)
                    .Replace(Exportprofile.SchemeCuesheetTitle, _sessionStateContainer.Cuesheet.Title)
                    .Replace(Exportprofile.SchemeCuesheetCDTextfile, _sessionStateContainer.Cuesheet.CDTextfile?.Name)
                    .Replace(Exportprofile.SchemeCuesheetCatalogueNumber, _sessionStateContainer.Cuesheet.Cataloguenumber)
                    .Replace(Exportprofile.SchemeDate, DateTime.Now.ToShortDateString())
                    .Replace(Exportprofile.SchemeDateTime, DateTime.Now.ToString())
                    .Replace(Exportprofile.SchemeTime, DateTime.Now.ToLongTimeString());
                builder.AppendLine(header);
                foreach (var audiofile in _sessionStateContainer.Cuesheet.Audiofiles)
                {
                    var audiofileLine = exportprofile.SchemeAudiofiles
                        .Replace(Exportprofile.SchemeAudiofileName, audiofile.Name);
                    builder.AppendLine(audiofileLine);
                    IEnumerable<Track> tracks = audiofile.Tracks.OrderBy(x => x.Position);
                    if (tracks.Any())
                    {
                        foreach (var track in tracks)
                        {
                            var trackLine = exportprofile.SchemeTracks
                                .Replace(Exportprofile.SchemeTrackArtist, track.Artist)
                                .Replace(Exportprofile.SchemeTrackTitle, track.Title)
                                .Replace(Exportprofile.SchemeTrackPosition, track.Position.ToString())
                                .Replace(Exportprofile.SchemeTrackBegin, track.Begin.ToString())
                                .Replace(Exportprofile.SchemeTrackEnd, track.End.ToString())
                                .Replace(Exportprofile.SchemeTrackLength, track.Length.ToString())
                                .Replace(Exportprofile.SchemeTrackFlags, string.Join(" ", track.Flags.Select(x => x.CuesheetLabel)))
                                .Replace(Exportprofile.SchemeTrackPreGap, track.PreGap != null ? track.PreGap.Value.ToString() : string.Empty)
                                .Replace(Exportprofile.SchemeTrackPostGap, track.PostGap != null ? track.PostGap.Value.ToString() : string.Empty)
                                .Replace(Exportprofile.SchemeDate, DateTime.Now.ToShortDateString())
                                .Replace(Exportprofile.SchemeDateTime, DateTime.Now.ToString())
                                .Replace(Exportprofile.SchemeTime, DateTime.Now.ToLongTimeString());
                            builder.AppendLine(trackLine);
                        }
                    }
                }
                
                var footer = exportprofile.SchemeFooter
                    .Replace(Exportprofile.SchemeCuesheetArtist, _sessionStateContainer.Cuesheet.Artist)
                    .Replace(Exportprofile.SchemeCuesheetTitle, _sessionStateContainer.Cuesheet.Title)
                    .Replace(Exportprofile.SchemeCuesheetCDTextfile, _sessionStateContainer.Cuesheet.CDTextfile?.Name)
                    .Replace(Exportprofile.SchemeCuesheetCatalogueNumber, _sessionStateContainer.Cuesheet.Cataloguenumber)
                    .Replace(Exportprofile.SchemeDate, DateTime.Now.ToShortDateString())
                    .Replace(Exportprofile.SchemeDateTime, DateTime.Now.ToString())
                    .Replace(Exportprofile.SchemeTime, DateTime.Now.ToLongTimeString());
                builder.AppendLine(footer);
            }
            return builder.ToString();
        }
    }
}
