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
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Services.UI;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics;

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
    public class ImportManager(ISessionStateContainer sessionStateContainer, ITraceChangeManager traceChangeManager, IFileInputManager fileInputManager, ITextImportService textImportService, ILogger<ImportManager> logger)
    {
        public event EventHandler<IEnumerable<string>>? UploadFilesFinished;

        private readonly ILogger<ImportManager> _logger = logger;
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
        private readonly IFileInputManager _fileInputManager = fileInputManager;
        private readonly ITextImportService _textImportService = textImportService;

        public void ImportData(String? data)
        {
            var stopwatch = Stopwatch.StartNew();
            _sessionStateContainer.Importfile = new Importfile()
            {
                FileContent = data,
                FileContentRecognized = data,
                FileType = ImportFileType.Textfile
            };
            stopwatch.Stop();
            _logger.LogDebug("ImportData duration: {stopwatch.Elapsed}", stopwatch.Elapsed);
        }
        
        public async Task AnalyseImportfile()
        {
            ResetTracing();
            var stopwatch = Stopwatch.StartNew();
            var fileContent = _sessionStateContainer.Importfile?.FileContent;
            if (String.IsNullOrEmpty(fileContent) == false)
            {
                switch (_sessionStateContainer.Importfile?.FileType)
                {
                    case ImportFileType.ProjectFile:
                        _sessionStateContainer.Cuesheet = Projectfile.ImportFile(fileContent)!;
                        break;
                    case ImportFileType.Textfile:
                        _sessionStateContainer.Importfile = await _textImportService.AnalyseAsync(fileContent);
                        break;
                    case ImportFileType.Cuesheet:
                        _sessionStateContainer.Importfile = CuesheetImportService.Analyse(fileContent);
                        break;
                }
            }
            if (_sessionStateContainer.Importfile?.AnalyzedCuesheet != null)
            {
                switch (_sessionStateContainer.Importfile?.FileType)
                {
                    case ImportFileType.Textfile:
                        var importCuesheet = new Cuesheet();
                        CopyCuesheet(importCuesheet, _sessionStateContainer.Importfile.AnalyzedCuesheet);
                        _sessionStateContainer.ImportCuesheet = importCuesheet;
                        StartTracing();
                        break;
                    case ImportFileType.Cuesheet:
                        _traceChangeManager.BulkEdit = true;
                        _sessionStateContainer.Cuesheet = new();
                        CopyCuesheet(_sessionStateContainer.Cuesheet, _sessionStateContainer.Importfile.AnalyzedCuesheet);
                        _traceChangeManager.BulkEdit = false;
                        break;
                }
            }
            stopwatch.Stop();
            _logger.LogDebug("ImportTextAsync duration: {stopwatch.Elapsed}", stopwatch.Elapsed);
        }
        
        public void ImportCuesheet()
        {
            var stopwatch = Stopwatch.StartNew();
            ResetTracing();
            if (_sessionStateContainer.ImportCuesheet != null)
            {
                _traceChangeManager.BulkEdit = true;
                CopyCuesheet(_sessionStateContainer.Cuesheet, _sessionStateContainer.ImportCuesheet);
                _traceChangeManager.BulkEdit = false;
            }
            _sessionStateContainer.ResetImport();
            stopwatch.Stop();
            _logger.LogDebug("ImportCuesheet duration: {stopwatch.Elapsed}", stopwatch.Elapsed);
        }

        public async Task UploadFilesAsync(IEnumerable<IBrowserFile> files, String? fileInputId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var invalidFiles = new List<string>();
            foreach (var file in files)
            {
                if (_fileInputManager.CheckFileMimeType(file, FileMimeTypes.Projectfile, [FileExtensions.Projectfile])
                    || _fileInputManager.CheckFileMimeType(file, FileMimeTypes.Cuesheet, [FileExtensions.Cuesheet])
                    || _fileInputManager.IsValidForImportView(file)
                    || _fileInputManager.IsValidAudiofile(file))
                {
                    if (_fileInputManager.CheckFileMimeType(file, FileMimeTypes.Projectfile, [FileExtensions.Projectfile]))
                    {
                        var fileContent = await ReadFileContentAsync(file);
                        fileContent.Position = 0;
                        using var reader = new StreamReader(fileContent);
                        var stringFileContent = reader.ReadToEnd();
                        _sessionStateContainer.Importfile = new Importfile()
                        {
                            FileContent = stringFileContent,
                            FileContentRecognized = stringFileContent,
                            FileType = ImportFileType.ProjectFile
                        };
                    }
                    if (_fileInputManager.CheckFileMimeType(file, FileMimeTypes.Cuesheet, [FileExtensions.Cuesheet]))
                    {
                        var fileContent = await ReadFileContentAsync(file);
                        fileContent.Position = 0;
                        using var reader = new StreamReader(fileContent);
                        var stringFileContent = reader.ReadToEnd();
                        _sessionStateContainer.Importfile = new Importfile()
                        {
                            FileContent = stringFileContent,
                            FileContentRecognized = stringFileContent,
                            FileType = ImportFileType.Cuesheet
                        };
                    }
                    if (_fileInputManager.IsValidForImportView(file))
                    {
                        var fileContent = await ReadFileContentAsync(file);
                        fileContent.Position = 0;
                        using var reader = new StreamReader(fileContent);
                        var stringFileContent = reader.ReadToEnd();
                        _sessionStateContainer.Importfile = new Importfile()
                        {
                            FileContent = stringFileContent,
                            FileContentRecognized = stringFileContent,
                            FileType = ImportFileType.Textfile
                        };
                    }
                    if (_fileInputManager.IsValidAudiofile(file))
                    {
                        var audioFile = await _fileInputManager.CreateAudiofileAsync(fileInputId, file);
                        _sessionStateContainer.ImportAudiofile = audioFile;
                    }
                }
                else
                {
                    invalidFiles.Add(file.Name);
                }
            }
            UploadFilesFinished?.Invoke(this, invalidFiles);
            stopwatch.Stop();
            _logger.LogDebug("UploadFilesAsync duration: {stopwatch.Elapsed}", stopwatch.Elapsed);
        }

        private static async Task<MemoryStream> ReadFileContentAsync(IBrowserFile file)
        {
            var fileContent = new MemoryStream();
            var stream = file.OpenReadStream();
            await stream.CopyToAsync(fileContent);
            stream.Close();
            return fileContent;
        }

        private static void CopyCuesheet(Cuesheet target, ICuesheet cuesheetToCopy)
        {
            target.IsImporting = true;
            target.Artist = cuesheetToCopy.Artist;
            target.Title = cuesheetToCopy.Title;
            target.Cataloguenumber = cuesheetToCopy.Cataloguenumber;
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
                    target.AddTrack(track);
                }
            }
            else
            {
                throw new NullReferenceException();
            }
            target.IsImporting = false;
        }

        private void StartTracing()
        {
            if (_sessionStateContainer.ImportCuesheet != null)
            {
                _traceChangeManager.TraceChanges(_sessionStateContainer.ImportCuesheet);
                foreach (var track in _sessionStateContainer.ImportCuesheet.Tracks)
                {
                    _traceChangeManager.TraceChanges(track);
                }
            }
        }

        private void ResetTracing()
        {
            if (_sessionStateContainer.ImportCuesheet != null)
            {
                _traceChangeManager.RemoveTracedChanges([_sessionStateContainer.ImportCuesheet, .. _sessionStateContainer.ImportCuesheet.Tracks]);
            }
        }
    }
}
