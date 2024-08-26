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
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.Utility;
using Blazorise;
using Microsoft.JSInterop;

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
    public class ImportManager(SessionStateContainer sessionStateContainer, LocalStorageOptionsProvider localStorageOptionsProvider, IJSRuntime jsRuntime, HttpClient httpClient, TextImportService textImportService)
    {
        private readonly SessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly LocalStorageOptionsProvider _localStorageOptionsProvider = localStorageOptionsProvider;
        private readonly IJSRuntime _jsRuntime = jsRuntime;
        private readonly HttpClient _httpClient = httpClient;
        private readonly TextImportService _textImportService = textImportService;

        public async Task<Dictionary<IFileEntry, ImportFileType>> ImportFilesAsync(IEnumerable<IFileEntry> files)
        {
            Dictionary<IFileEntry, ImportFileType> importFileTypes = [];
            foreach (var file in files)
            {
                if (IOUtility.CheckFileMimeType(file, Projectfile.MimeType, Projectfile.FileExtension))
                {
                    //TODO
                    //var fileContent = await ReadFileContentAsync(file);
                    //var cuesheet = Projectfile.ImportFile(fileContent.ToArray());
                    //if (cuesheet != null)
                    //{
                    //    _sessionStateContainer.ImportCuesheet = cuesheet;
                    //}
                    //importFileTypes.Add(file, ImportFileType.ProjectFile);
                }
                if (IOUtility.CheckFileMimeType(file, Cuesheet.MimeType, Cuesheet.FileExtension))
                {
                    var fileContent = await ReadFileContentAsync(file);
                    var options = await _localStorageOptionsProvider.GetOptions<ApplicationOptions>();
                    _sessionStateContainer.CuesheetImportFile = new CuesheetImportfile(fileContent, options);
                    importFileTypes.Add(file, ImportFileType.Cuesheet);
                }
                if (IOUtility.CheckFileMimeType(file, TextImportfile.MimeType, TextImportfile.FileExtension))
                {
                    var fileContent = await ReadFileContentAsync(file);
                    var options = await _localStorageOptionsProvider.GetOptions<ImportOptions>();
                    var applicationOptions = await _localStorageOptionsProvider.GetOptions<ApplicationOptions>();
                    fileContent.Position = 0;
                    using var reader = new StreamReader(fileContent);
                    List<String?> lines = [];
                    while (reader.EndOfStream == false)
                    {
                        lines.Add(reader.ReadLine());
                    }
                    _sessionStateContainer.TextImportFile = _textImportService.Analyse(options, lines);
                    if (_textImportService.AnalysedCuesheet != null)
                    {
                        var importCuesheet = new Cuesheet();
                        importCuesheet.Import(_textImportService.AnalysedCuesheet, applicationOptions);
                        _sessionStateContainer.ImportCuesheet = importCuesheet;
                    }
                    importFileTypes.Add(file, ImportFileType.Textfile);
                }
                if (IOUtility.CheckFileMimeTypeForAudioCodec(file))
                {
                    var audioFileObjectURL = await _jsRuntime.InvokeAsync<String>("getObjectURL", "dropFileInput");
                    var codec = IOUtility.GetAudioCodec(file);
                    var audiofile = new Audiofile(file.Name, audioFileObjectURL, codec, _httpClient);
                    _ = audiofile.LoadContentStream();
                    _sessionStateContainer.ImportAudiofile = audiofile;
                    importFileTypes.Add(file, ImportFileType.Audiofile);
                }
            }
            return importFileTypes;
        }
        private static async Task<MemoryStream> ReadFileContentAsync(IFileEntry file)
        {
            var fileContent = new MemoryStream();
            var stream = file.OpenReadStream();
            await stream.CopyToAsync(fileContent);
            stream.Close();
            return fileContent;
        }
    }
}
