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
using AudioCuesheetEditor.Model.IO.Export;
using System.Data;
using System.Text;

namespace AudioCuesheetEditor.Services.IO
{
    //TODO: Unit Tests
    public class ExportfileGenerator(SessionStateContainer sessionStateContainer)
    {
        private readonly SessionStateContainer _sessionStateContainer = sessionStateContainer;

        public IEnumerable<ValidationMessage> CanGenerateExportfiles(Exportprofile exportprofile)
        {
            List<ValidationMessage> validationMessages = [..exportprofile.Validate().ValidationMessages];
            validationMessages.AddRange(_sessionStateContainer.Cuesheet.Validate().ValidationMessages);
            return validationMessages;
        }

        public IReadOnlyCollection<Exportfile> GenerateExportfiles(Exportprofile exportprofile)
        {
            List<Exportfile> exportfiles = [];
            if (!CanGenerateExportfiles(exportprofile).Any())
            {
                if (_sessionStateContainer.Cuesheet.Sections.Count != 0)
                {
                    var counter = 1;
                    string? content = null;
                    string filename = string.Empty;
                    string? audioFileName = null;
                    foreach (var section in _sessionStateContainer.Cuesheet.Sections.OrderBy(x => x.Begin))
                    {
                        audioFileName = section.AudiofileName;
                        if (section.Validate().Status == ValidationStatus.Success)
                        {
                            content = WriteExport(exportprofile, audioFileName, section);
                            filename = string.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(exportprofile.Filename), counter, Path.GetExtension(exportprofile.Filename));
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
                        content = WriteExport(exportprofile, _sessionStateContainer.Cuesheet.Audiofile.Name);
                    }
                    if (content != null)
                    {
                        var begin = _sessionStateContainer.Cuesheet.Tracks.Min(x => x.Begin);
                        var end = _sessionStateContainer.Cuesheet.Tracks.Max(x => x.End);
                        exportfiles.Add(new Exportfile() { Name = exportprofile.Filename, Content = Encoding.UTF8.GetBytes(content), Begin = begin, End = end });
                    }
                }
            }
            return exportfiles;
        }

        private string WriteExport(Exportprofile exportprofile, string? audiofileName, CuesheetSection? section = null)
        {
            var builder = new StringBuilder();
            if (exportprofile != null)
            {
                var header = exportprofile.SchemeHead
                    .Replace(Exportprofile.SchemeCuesheetArtist, section != null ? section.Artist : _sessionStateContainer.Cuesheet.Artist)
                    .Replace(Exportprofile.SchemeCuesheetTitle, section != null ? section.Title : _sessionStateContainer.Cuesheet.Title)
                    .Replace(Exportprofile.SchemeCuesheetAudiofile, audiofileName)
                    .Replace(Exportprofile.SchemeCuesheetCDTextfile, _sessionStateContainer.Cuesheet.CDTextfile?.Name)
                    .Replace(Exportprofile.SchemeCuesheetCatalogueNumber, _sessionStateContainer.Cuesheet.Cataloguenumber)
                    .Replace(Exportprofile.SchemeDate, DateTime.Now.ToShortDateString())
                    .Replace(Exportprofile.SchemeDateTime, DateTime.Now.ToString())
                    .Replace(Exportprofile.SchemeTime, DateTime.Now.ToLongTimeString());
                builder.AppendLine(header);
                IEnumerable<Track> tracks = _sessionStateContainer.Cuesheet.Tracks.OrderBy(x => x.Position);
                if (section != null)
                {
                    tracks = _sessionStateContainer.Cuesheet.Tracks.Where(x => x.Begin <= section.End && x.End >= section.Begin).OrderBy(x => x.Position);
                }
                if (tracks.Any())
                {
                    //Position, Begin and End should always start from 0 even with splitpoints
                    int positionDifference = 1 - Convert.ToInt32(tracks.First().Position);
                    foreach (var track in tracks)
                    {
                        TimeSpan begin;
                        var end = track.End;
                        if (track.Begin.HasValue)
                        {
                            begin = track.Begin.Value;
                            if (section?.Begin != null)
                            {
                                if (section.Begin >= track.Begin)
                                {
                                    begin = TimeSpan.Zero;
                                }
                                else
                                {
                                    begin = track.Begin.Value - section.Begin.Value;
                                }
                                end = track.End - section.Begin.Value;
                            }
                        }
                        else
                        {
                            throw new NullReferenceException(string.Format("{0} may not be null!", nameof(Track.Begin)));
                        }
                        var trackLine = exportprofile.SchemeTracks
                            .Replace(Exportprofile.SchemeTrackArtist, track.Artist)
                            .Replace(Exportprofile.SchemeTrackTitle, track.Title)
                            .Replace(Exportprofile.SchemeTrackPosition, (track.Position + positionDifference).ToString())
                            .Replace(Exportprofile.SchemeTrackBegin, begin.ToString())
                            .Replace(Exportprofile.SchemeTrackEnd, end.ToString())
                            .Replace(Exportprofile.SchemeTrackLength, (end - begin).ToString())
                            .Replace(Exportprofile.SchemeTrackFlags, string.Join(" ", track.Flags.Select(x => x.CuesheetLabel)))
                            .Replace(Exportprofile.SchemeTrackPreGap, track.PreGap != null ? track.PreGap.Value.ToString() : string.Empty)
                            .Replace(Exportprofile.SchemeTrackPostGap, track.PostGap != null ? track.PostGap.Value.ToString() : string.Empty)
                            .Replace(Exportprofile.SchemeDate, DateTime.Now.ToShortDateString())
                            .Replace(Exportprofile.SchemeDateTime, DateTime.Now.ToString())
                            .Replace(Exportprofile.SchemeTime, DateTime.Now.ToLongTimeString());
                        builder.AppendLine(trackLine);
                    }
                }
                var footer = exportprofile.SchemeFooter
                    .Replace(Exportprofile.SchemeCuesheetArtist, section != null ? section.Artist : _sessionStateContainer.Cuesheet.Artist)
                    .Replace(Exportprofile.SchemeCuesheetTitle, section != null ? section.Title : _sessionStateContainer.Cuesheet.Title)
                    .Replace(Exportprofile.SchemeCuesheetAudiofile, audiofileName)
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
