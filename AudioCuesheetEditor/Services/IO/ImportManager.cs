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
                        SortTracks(importCuesheet);
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
                var newCuesheet = new Cuesheet();
                CopyCuesheet(newCuesheet, _sessionStateContainer.ImportCuesheet);
                SortTracks(newCuesheet);
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
                        if (audioFile != null)
                        {
                            _sessionStateContainer.ImportAudiofiles.Add(audioFile);
                        }
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

        void CopyCuesheet(Cuesheet target, ICuesheet cuesheetToCopy)
        {
            target.Artist = cuesheetToCopy.Artist;
            target.Title = cuesheetToCopy.Title;
            target.Cataloguenumber = cuesheetToCopy.Cataloguenumber;
            if (cuesheetToCopy is Cuesheet originCuesheet)
            {
                target.CDTextfile = originCuesheet.CDTextfile;
                AttachClonedAudiofiles(target, originCuesheet.Audiofiles);
            }
            if (cuesheetToCopy is ImportCuesheet importCuesheet)
            {
                if (String.IsNullOrEmpty(importCuesheet.CDTextfile) == false)
                {
                    target.CDTextfile = new CDTextfile(importCuesheet.CDTextfile);
                }
                AttachClonedAudiofiles(target, importCuesheet.Audiofiles);
            }
        }

        void AttachClonedAudiofiles(Cuesheet target, IEnumerable<IAudiofile> audiofiles) 
        {
            foreach (var audiofile in audiofiles)
            {
                Audiofile? targetAudiofile = null;
                IEnumerable<ITrack>? tracks = null;
                // Map uploaded import audiofiles by name
                var importAudiofileFound = _sessionStateContainer.ImportAudiofiles.FirstOrDefault(x => x.Name == audiofile.Name);
                if (importAudiofileFound != null)
                {
                    targetAudiofile = new Audiofile(importAudiofileFound.Name, importAudiofileFound.ObjectURL, importAudiofileFound.AudioCodec, importAudiofileFound.Duration);
                }
                if (audiofile is ImportAudiofile importAudiofile)
                {
                    targetAudiofile ??= new Audiofile()
                    {
                        Name = importAudiofile.Name,
                    };
                    tracks = importAudiofile.Tracks;
                }
                if (audiofile is Audiofile sourceAudiofile)
                {
                    targetAudiofile ??= new Audiofile(sourceAudiofile.Name, sourceAudiofile.ObjectURL, sourceAudiofile.AudioCodec, sourceAudiofile.Duration);
                    tracks = sourceAudiofile.Tracks;
                }
                if (targetAudiofile == null || tracks == null)
                {
                    throw new NullReferenceException();
                }
                foreach (var track in tracks)
                {
                    var clone = _trackManager.Clone(track);
                    targetAudiofile.Tracks.Add(clone);
                }
                target.Audiofiles.Add(targetAudiofile);
            }
        }

        void SortTracks(Cuesheet target)
        {
            foreach (var audiofile in target.Audiofiles)
            {
                var tracks = audiofile.Tracks;
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
                    ITrack? nextTrack = null;
                    if (index < sortedTracks.Count() - 1)
                    {
                        nextTrack = sortedTracks.ElementAt(index + 1);
                    }
                    // Copy track
                    var track = _trackManager.Clone(importTrack);
                    track.Cuesheet = target;
                    track.Audiofile = audiofile;
                    // Special treatment for StartDateTime of ImportTrack
                    if (importTrack is ImportTrack importTrackReference && importTrackReference.StartDateTime != null && nextTrack is ImportTrack nextImportTrackReference)
                    {
                        var length = nextImportTrackReference.StartDateTime - importTrackReference.StartDateTime;
                        track.Begin = begin;
                        track.End = begin + length;
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
                    if ((track.End.HasValue == false) && (nextTrack?.Begin.HasValue == true))
                    {
                        track.End = nextTrack.Begin;
                    }
                    begin = track.End;
                    position++;
                    targetTracks.Add(track);
                }
                audiofile.Tracks = targetTracks;
            }
        }

        void ResetTracing()
        {
            if (_sessionStateContainer.ImportCuesheet != null)
            {
                _traceChangeManager.RemoveTracedChanges([_sessionStateContainer.ImportCuesheet, .. _sessionStateContainer.ImportCuesheet.Audiofiles, .. _sessionStateContainer.ImportCuesheet.Audiofiles.SelectMany(x => x.Tracks)]);
            }
        }
    }
}
