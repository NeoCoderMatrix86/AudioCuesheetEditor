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
using AudioCuesheetEditor.Model.UI;
using AudioCuesheetEditor.Services.AudioCuesheet;
using AudioCuesheetEditor.Services.UI;
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
    public class ImportManager(ISessionStateContainer sessionStateContainer, ITraceChangeManager traceChangeManager, IFileInputManager fileInputManager, ITextImportService textImportService, ITrackManager trackManager, ILogger<ImportManager> logger)
    {
        public event EventHandler<IEnumerable<string>>? UploadFilesFinished;

        private readonly ILogger<ImportManager> _logger = logger;
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
        private readonly IFileInputManager _fileInputManager = fileInputManager;
        private readonly ITextImportService _textImportService = textImportService;
        private readonly ITrackManager _trackManager = trackManager;

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
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("ImportData duration: {stopwatch.Elapsed}", stopwatch.Elapsed);
            }
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
                        var importedCuesheet = Projectfile.ImportFile(fileContent);
                        var previousValue = _sessionStateContainer.Cuesheet;
                        _sessionStateContainer.Cuesheet = importedCuesheet!;
                        _traceChangeManager.AddChange(new TracedChange(_sessionStateContainer, new(previousValue, nameof(SessionStateContainer.Cuesheet))));
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
                    case ImportFileType.Cuesheet:
                    case ImportFileType.Textfile:
                        var importCuesheet = new Cuesheet();
                        CopyCuesheet(importCuesheet, _sessionStateContainer.Importfile.AnalyzedCuesheet);
                        _sessionStateContainer.ImportCuesheet = importCuesheet;
                        break;
                }
            }
            stopwatch.Stop();
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("AnalyseImportfile duration: {stopwatch.Elapsed}", stopwatch.Elapsed);
            }
        }
        
        public void ImportCuesheet()
        {
            var stopwatch = Stopwatch.StartNew();
            ResetTracing();
            if (_sessionStateContainer.ImportCuesheet != null)
            {
                var newCuesheet = _sessionStateContainer.ImportCuesheet;
                CopyCuesheet(newCuesheet, _sessionStateContainer.ImportCuesheet);
                var previousValue = _sessionStateContainer.Cuesheet;
                _sessionStateContainer.Cuesheet = newCuesheet;
                _traceChangeManager.AddChange(new TracedChange(_sessionStateContainer, new(previousValue, nameof(SessionStateContainer.Cuesheet))));
            }
            _sessionStateContainer.ResetImport();
            stopwatch.Stop();
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("ImportCuesheet duration: {stopwatch.Elapsed}", stopwatch.Elapsed);
            }
        }

        public async Task UploadFilesAsync(IEnumerable<FileUpload> files)
        {
            var stopwatch = Stopwatch.StartNew();
            var invalidFiles = new List<string>();
            foreach (var file in files)
            {
                if (_fileInputManager.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Projectfile, [FileExtensions.Projectfile])
                    || _fileInputManager.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Cuesheet, [FileExtensions.Cuesheet])
                    || _fileInputManager.IsValidForImportView(file.ContentType, file.Name)
                    || _fileInputManager.IsValidAudiofile(file.ContentType, file.Name))
                {
                    if (_fileInputManager.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Projectfile, [FileExtensions.Projectfile]))
                    {
                        _sessionStateContainer.Importfile = new Importfile()
                        {
                            FileContent = file.Content,
                            FileContentRecognized = file.Content,
                            FileType = ImportFileType.ProjectFile
                        };
                    }
                    if (_fileInputManager.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Cuesheet, [FileExtensions.Cuesheet]))
                    {
                        _sessionStateContainer.Importfile = new Importfile()
                        {
                            FileContent = file.Content,
                            FileContentRecognized = file.Content,
                            FileType = ImportFileType.Cuesheet
                        };
                    }
                    if (_fileInputManager.IsValidForImportView(file.ContentType, file.Name))
                    {
                        _sessionStateContainer.Importfile = new Importfile()
                        {
                            FileContent = file.Content,
                            FileContentRecognized = file.Content,
                            FileType = ImportFileType.Textfile
                        };
                    }
                    if (_fileInputManager.IsValidAudiofile(file.ContentType, file.Name))
                    {
                        var audioFile = await _fileInputManager.CreateAudiofileAsync(file);
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
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("UploadFilesAsync duration: {stopwatch.Elapsed}", stopwatch.Elapsed);
            }
        }

        private void CopyCuesheet(Cuesheet target, ICuesheet cuesheetToCopy)
        {
            target.Artist = cuesheetToCopy.Artist;
            target.Title = cuesheetToCopy.Title;
            target.Cataloguenumber = cuesheetToCopy.Cataloguenumber;
            IEnumerable<ITrack>? tracks = null;
            if (cuesheetToCopy is Cuesheet originCuesheet)
            {
                tracks = originCuesheet.Tracks;
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
                IOrderedEnumerable<ITrack> sortedTracks;
                if (tracks.All(x => x.Position.HasValue))
                {
                    sortedTracks = tracks.OrderBy(x => x.Position);
                }
                else
                {
                    sortedTracks = tracks.OrderByDescending(x => x.Position.HasValue).ThenBy(x => x.Position);
                }
                if (sortedTracks.All(x => x.Begin.HasValue))
                {
                    sortedTracks = sortedTracks.ThenBy(x => x.Begin);
                }
                else
                {
                    sortedTracks = sortedTracks.ThenByDescending(x => x.Begin.HasValue).ThenBy(x => x.Begin);
                }
                if (sortedTracks.All(x => x.End.HasValue))
                {
                    sortedTracks = sortedTracks.ThenBy(x => x.End);
                }
                else
                {
                    sortedTracks = sortedTracks.ThenByDescending(x => x.End.HasValue).ThenBy(x => x.End);
                }
                List<Track> targetTracks = [];
                TimeSpan? begin = TimeSpan.Zero;
                ushort position = 1;
                foreach (var (importTrack, index) in sortedTracks.Select((track, i) => (track, i)))
                {
                    // Copy track
                    var track = _trackManager.Clone(importTrack);
                    track.Cuesheet = target;
                    // Special treatment for StartDateTime of ImportTrack
                    if (importTrack is ImportTrack importTrackReference && importTrackReference.StartDateTime != null)
                    {
                        if (index < sortedTracks.Count() - 1)
                        {
                            var nextTrack = (ImportTrack)sortedTracks.ElementAt(index + 1);
                            var length = nextTrack.StartDateTime - importTrackReference.StartDateTime;

                            track.Begin = begin;
                            track.End = begin + length;
                        }
                    }
                    // Calculate properties
                    if (track.Position.HasValue == false)
                    {
                        track.Position = position;
                    }
                    if (track.Begin.HasValue == false)
                    {
                        track.Begin = begin;
                    }
                    begin = track.End;
                    position++;
                    targetTracks.Add(track);
                }
                target.Tracks = targetTracks;
            }
            else
            {
                throw new NullReferenceException();
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
