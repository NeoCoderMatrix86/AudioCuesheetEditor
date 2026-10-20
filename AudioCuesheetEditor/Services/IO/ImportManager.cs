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

namespace AudioCuesheetEditor.Services.IO;

public enum ImportFileType
{
    Unknown,
    ProjectFile,
    Cuesheet,
    Textfile,
    Audiofile
}
public partial class ImportManager(ISessionStateContainer sessionStateContainer, ITraceChangeManager traceChangeManager, IFileInputManager fileInputManager, ITextImportService textImportService, ITrackManager trackManager, IAudioImportService audioImportService, ILogger<ImportManager> logger)
{
    public event EventHandler<IEnumerable<string>>? UploadFilesFinished;

    readonly ILogger<ImportManager> _logger = logger;
    readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
    readonly ITraceChangeManager _traceChangeManager = traceChangeManager;
    readonly IFileInputManager _fileInputManager = fileInputManager;
    readonly ITextImportService _textImportService = textImportService;
    readonly ITrackManager _trackManager = trackManager;
    readonly IAudioImportService _audioImportService = audioImportService;

    public void ImportData(string? data)
    {
        var stopwatch = Stopwatch.StartNew();
        _sessionStateContainer.Importfile = new Importfile()
        {
            FileContent = data,
            FileContentRecognized = data,
            FileType = ImportFileType.Textfile
        };
        stopwatch.Stop();
        LogImportDataFinished(stopwatch.Elapsed);
    }
    
    public async Task<bool> AnalyseImportfile()
    {
        ResetTracing();
        var stopwatch = Stopwatch.StartNew();
        var fileContent = _sessionStateContainer.Importfile?.FileContent;
        if (string.IsNullOrEmpty(fileContent) == false)
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
        var cancelImport = await _audioImportService.MapAudioImportAsync();
        stopwatch.Stop();
        LogAnalyseImportfileFinished(stopwatch.Elapsed);
        return cancelImport;
    }
    
    public async Task ImportCuesheetAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        ResetTracing();
        if (_sessionStateContainer.ImportCuesheet != null)
        {
            var newCuesheet = new Cuesheet();
            CopyCuesheet(newCuesheet, _sessionStateContainer.ImportCuesheet);
            var previousValue = _sessionStateContainer.Cuesheet;
            _sessionStateContainer.Cuesheet = newCuesheet;
            _traceChangeManager.AddChange(new TracedChange(_sessionStateContainer, new(previousValue, nameof(SessionStateContainer.Cuesheet))));
        }
        await _sessionStateContainer.ResetImportAsync(false);
        stopwatch.Stop();
        LogImportCuesheetFinished(stopwatch.Elapsed);
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
                || _fileInputManager.IsValidAudiofile(file.Name, file.ContentType))
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
                if (_fileInputManager.IsValidAudiofile(file.Name, file.ContentType))
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
        LogImportCuesheetFinished(stopwatch.Elapsed);
    }

    void CopyCuesheet(Cuesheet target, ICuesheet cuesheetToCopy)
    {
        target.Artist = cuesheetToCopy.Artist;
        target.Title = cuesheetToCopy.Title;
        target.Cataloguenumber = cuesheetToCopy.Cataloguenumber;
        IEnumerable<IAudiofile> audiofiles = [];
        if (cuesheetToCopy is Cuesheet originCuesheet)
        {
            target.CDTextfile = originCuesheet.CDTextfile;
            audiofiles = originCuesheet.Audiofiles;
        }
        if (cuesheetToCopy is ImportCuesheet importCuesheet)
        {
            if (string.IsNullOrEmpty(importCuesheet.CDTextfile) == false)
            {
                target.CDTextfile = new CDTextfile(importCuesheet.CDTextfile);
            }
            audiofiles = importCuesheet.Audiofiles;
        }
        AttachClonedAudiofiles(target, audiofiles);
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
                    AudioCodec = string.IsNullOrEmpty(importAudiofile.Name) == false ? _fileInputManager.GetAudioCodec(importAudiofile.Name) : null
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
            if (sortedTracks.All(x => x.Finish.HasValue))
            {
                sortedTracks = sortedTracks.ThenBy(x => x.Finish);
            }
            else
            {
                sortedTracks = sortedTracks.ThenByDescending(x => x.Finish.HasValue).ThenBy(x => x.Finish);
            }
            TimeSpan? begin = TimeSpan.Zero;
            ushort position = 1;
            for (var i = 0; i < sortedTracks.Count(); i++)
            {
                var track = sortedTracks.ElementAt(i);
                ITrack? nextTrack = null;
                if (i < sortedTracks.Count() - 1)
                {
                    nextTrack = sortedTracks.ElementAt(i + 1);
                }
                var clone = _trackManager.Clone(track);
                clone.Cuesheet = target;
                clone.Audiofile = targetAudiofile;
                // Special treatment for StartDateTime of ImportTrack
                if (track is ImportTrack importTrack && importTrack.StartDateTime != null && nextTrack is ImportTrack nextImportTrack)
                {
                    var length = nextImportTrack.StartDateTime - importTrack.StartDateTime;
                    clone.Begin = begin;
                    clone.Finish = begin + length;
                }
                // Calculate properties
                if (clone.Position.HasValue == false)
                {
                    clone.Position = position;
                }
                if (clone.Begin.HasValue == false)
                {
                    clone.Begin = begin;
                }
                if ((clone.Finish.HasValue == false) && (nextTrack?.Begin.HasValue == true))
                {
                    clone.Finish = nextTrack.Begin;
                }
                begin = clone.Finish;
                position++;
                targetAudiofile.Tracks.Add(clone);
            }
            //Set last track end
            var lastTrack = targetAudiofile.Tracks.LastOrDefault();
            if ((lastTrack?.Finish.HasValue == false) && (targetAudiofile.Duration.HasValue == true))
            {
                _trackManager.SetProperty(lastTrack, x => x.Finish, targetAudiofile.Duration);
            }
            target.Audiofiles.Add(targetAudiofile);
        }
    }

    void ResetTracing()
    {
        if (_sessionStateContainer.ImportCuesheet != null)
        {
            _traceChangeManager.RemoveTracedChanges([_sessionStateContainer.ImportCuesheet, .. _sessionStateContainer.ImportCuesheet.Audiofiles, .. _sessionStateContainer.ImportCuesheet.Audiofiles.SelectMany(x => x.Tracks)]);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "ImportData duration: {Duration}")]
    partial void LogImportDataFinished(TimeSpan duration);

    [LoggerMessage(Level = LogLevel.Debug, Message = "AnalyseImportfile duration: {Duration}")]
    partial void LogAnalyseImportfileFinished(TimeSpan duration);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ImportCuesheet duration: {Duration}")]
    partial void LogImportCuesheetFinished(TimeSpan duration);
}
