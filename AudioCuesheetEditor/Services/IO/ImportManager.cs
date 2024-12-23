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
using AudioCuesheetEditor.Extensions;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.AudioCuesheet.Import;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.UI;

namespace AudioCuesheetEditor.Services.IO
{
    public enum ImportFileType
    {
        Unknown,
        ProjectFile,
        Cuesheet,
        Textfile,
        Audiofile
    }
    public class ImportManager(SessionStateContainer sessionStateContainer, ILocalStorageOptionsProvider localStorageOptionsProvider, TextImportService textImportService, TraceChangeManager traceChangeManager)
    {
        private readonly SessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly ILocalStorageOptionsProvider _localStorageOptionsProvider = localStorageOptionsProvider;
        private readonly TextImportService _textImportService = textImportService;
        private readonly TraceChangeManager _traceChangeManager = traceChangeManager;
        
        //TODO
        //public async Task<Dictionary<IFileEntry, ImportFileType>> ImportFilesAsync(IEnumerable<IFileEntry> files)
        //{
        //    Dictionary<IFileEntry, ImportFileType> importFileTypes = [];
        //    foreach (var file in files)
        //    {
        //        if (IOUtility.CheckFileMimeType(file, FileMimeTypes.Projectfile, FileExtensions.Projectfile))
        //        {
        //            var fileContent = await ReadFileContentAsync(file);
        //            var cuesheet = Projectfile.ImportFile(fileContent.ToArray());
        //            if (cuesheet != null)
        //            {
        //                _sessionStateContainer.ImportCuesheet = cuesheet;
        //            }
        //            importFileTypes.Add(file, ImportFileType.ProjectFile);
        //        }
        //        if (IOUtility.CheckFileMimeType(file, FileMimeTypes.Cuesheet, FileExtensions.Cuesheet))
        //        {
        //            var fileContent = await ReadFileContentAsync(file);
        //            fileContent.Position = 0;
        //            using var reader = new StreamReader(fileContent);
        //            List<String?> lines = [];
        //            while (reader.EndOfStream == false)
        //            {
        //                lines.Add(reader.ReadLine());
        //            }
        //            await ImportCuesheetAsync(lines);
        //            importFileTypes.Add(file, ImportFileType.Cuesheet);
        //        }
        //        if (IOUtility.CheckFileMimeType(file, FileMimeTypes.Text, FileExtensions.Text))
        //        {
        //            var fileContent = await ReadFileContentAsync(file);
        //            fileContent.Position = 0;
        //            using var reader = new StreamReader(fileContent);
        //            List<String?> lines = [];
        //            while (reader.EndOfStream == false)
        //            {
        //                lines.Add(reader.ReadLine());
        //            }
        //            await ImportTextAsync([.. lines]);
        //            importFileTypes.Add(file, ImportFileType.Textfile);
        //        }
        //    }
        //    return importFileTypes;
        //}

        public async Task ImportTextAsync(IEnumerable<String?> fileContent)
        {
            var options = await _localStorageOptionsProvider.GetOptions<ImportOptions>();
            _sessionStateContainer.Importfile = _textImportService.Analyse(options, fileContent);
            if (_sessionStateContainer.Importfile.AnalysedCuesheet != null)
            {
                var importCuesheet = new Cuesheet();
                await CopyCuesheetAsync(importCuesheet, _sessionStateContainer.Importfile.AnalysedCuesheet);
                _sessionStateContainer.ImportCuesheet = importCuesheet;
            }
        }

        public async Task ImportCuesheetAsync()
        {
            if (_sessionStateContainer.ImportCuesheet != null)
            {
                _traceChangeManager.BulkEdit = true;
                await CopyCuesheetAsync(_sessionStateContainer.Cuesheet, _sessionStateContainer.ImportCuesheet);
                _traceChangeManager.BulkEdit = false;
            }
            _sessionStateContainer.ResetImport();
        }

        private async Task ImportCuesheetAsync(IEnumerable<String?> fileContent)
        {
            _sessionStateContainer.Importfile = CuesheetImportService.Analyse(fileContent);
            if (_sessionStateContainer.Importfile.AnalysedCuesheet != null)
            {
                var importCuesheet = new Cuesheet();
                await CopyCuesheetAsync(importCuesheet, _sessionStateContainer.Importfile.AnalysedCuesheet);
                _sessionStateContainer.ImportCuesheet = importCuesheet;
            }
        }
        //TODO
        //private static async Task<MemoryStream> ReadFileContentAsync(IFileEntry file)
        //{
        //    var fileContent = new MemoryStream();
        //    var stream = file.OpenReadStream();
        //    await stream.CopyToAsync(fileContent);
        //    stream.Close();
        //    return fileContent;
        //}

        private async Task CopyCuesheetAsync(Cuesheet target, ICuesheet cuesheetToCopy)
        {
            target.IsImporting = true;
            target.Artist = cuesheetToCopy.Artist;
            target.Title = cuesheetToCopy.Title;
            target.Cataloguenumber = cuesheetToCopy.CatalogueNumber;
            IEnumerable<ITrack>? tracks = null;
            if (cuesheetToCopy is Cuesheet originCuesheet)
            {
                tracks = originCuesheet.Tracks;
                // Copy sections
                foreach (var section in originCuesheet.Sections)
                {
                    var newSplitPoint = target.AddSection();
                    newSplitPoint.CopyValues(section);
                }
                target.Audiofile = originCuesheet.Audiofile;
                target.CDTextfile = originCuesheet.CDTextfile;
                target.Cataloguenumber = originCuesheet.Cataloguenumber;
            }
            if (cuesheetToCopy is ImportCuesheet importCuesheet)
            {
                tracks = importCuesheet.Tracks;
                if (String.IsNullOrEmpty(importCuesheet.Audiofile) == false)
                {
                    target.Audiofile = new Audiofile(importCuesheet.Audiofile);
                }
                if (String.IsNullOrEmpty(importCuesheet.CDTextfile) == false)
                {
                    target.CDTextfile = new CDTextfile(importCuesheet.CDTextfile);
                }
            }
            if (tracks != null)
            {
                var applicationOptions = await _localStorageOptionsProvider.GetOptions<ApplicationOptions>();
                var begin = TimeSpan.Zero;
                for (int i = 0; i < tracks.Count(); i++)
                {
                    var importTrack = tracks.ElementAt(i);
                    //We don't want to copy the cuesheet reference since we are doing a copy and want to assign the track to this object
                    var track = new Track(importTrack, false);
                    if (importTrack is ImportTrack importTrackReference)
                    {
                        if (importTrackReference.StartDateTime != null)
                        {
                            if (i < tracks.Count() - 1)
                            {
                                var nextTrack = (ImportTrack)tracks.ElementAt(i + 1);
                                var length = nextTrack.StartDateTime - importTrackReference.StartDateTime;
                                track.Begin = begin;
                                track.End = begin + length;
                                if (track.End.HasValue)
                                {
                                    begin = track.End.Value;
                                }
                            }
                        }
                    }
                    target.AddTrack(track, applicationOptions);
                }
            }
            else
            {
                throw new NullReferenceException();
            }
            target.IsImporting = false;
            _sessionStateContainer.FireCuesheetImported();
        }
    }
}
