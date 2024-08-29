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
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.Utility;
using Blazorise;

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
    public class ImportManager(SessionStateContainer sessionStateContainer, ILocalStorageOptionsProvider localStorageOptionsProvider, TextImportService textImportService)
    {
        private readonly SessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly ILocalStorageOptionsProvider _localStorageOptionsProvider = localStorageOptionsProvider;
        private readonly TextImportService _textImportService = textImportService;

        public async Task<Dictionary<IFileEntry, ImportFileType>> ImportFilesAsync(IEnumerable<IFileEntry> files)
        {
            Dictionary<IFileEntry, ImportFileType> importFileTypes = [];
            foreach (var file in files)
            {
                if (IOUtility.CheckFileMimeType(file, FileMimeTypes.Projectfile, FileExtensions.Projectfile))
                {
                    var fileContent = await ReadFileContentAsync(file);
                    var cuesheet = Projectfile.ImportFile(fileContent.ToArray());
                    if (cuesheet != null)
                    {
                        _sessionStateContainer.ImportCuesheet = cuesheet;
                    }
                    importFileTypes.Add(file, ImportFileType.ProjectFile);
                }
                if (IOUtility.CheckFileMimeType(file, FileMimeTypes.Cuesheet, FileExtensions.Cuesheet))
                {
                    var fileContent = await ReadFileContentAsync(file);
                    fileContent.Position = 0;
                    using var reader = new StreamReader(fileContent);
                    List<String?> lines = [];
                    while (reader.EndOfStream == false)
                    {
                        lines.Add(reader.ReadLine());
                    }
                    await ImportCuesheetAsync(lines);
                    importFileTypes.Add(file, ImportFileType.Cuesheet);
                }
                if (IOUtility.CheckFileMimeType(file, FileMimeTypes.Text, FileExtensions.Text))
                {
                    var fileContent = await ReadFileContentAsync(file);
                    fileContent.Position = 0;
                    using var reader = new StreamReader(fileContent);
                    List<String?> lines = [];
                    while (reader.EndOfStream == false)
                    {
                        lines.Add(reader.ReadLine());
                    }
                    await ImportTextAsync([.. lines]);
                    importFileTypes.Add(file, ImportFileType.Textfile);
                }
            }
            return importFileTypes;
        }

        public async Task ImportTextAsync(IEnumerable<String?> fileContent)
        {
            var options = await _localStorageOptionsProvider.GetOptions<ImportOptions>();
            _sessionStateContainer.Importfile = _textImportService.Analyse(options, fileContent);
            if (_sessionStateContainer.Importfile.AnalysedCuesheet != null)
            {
                var applicationOptions = await _localStorageOptionsProvider.GetOptions<ApplicationOptions>();
                var importCuesheet = new Cuesheet();
                importCuesheet.Import(_sessionStateContainer.Importfile.AnalysedCuesheet, applicationOptions);
                _sessionStateContainer.ImportCuesheet = importCuesheet;
            }
        }

        private async Task ImportCuesheetAsync(IEnumerable<String?> fileContent)
        {
            _sessionStateContainer.Importfile = CuesheetImportService.Analyse(fileContent);
            if (_sessionStateContainer.Importfile.AnalysedCuesheet != null)
            {
                var applicationOptions = await _localStorageOptionsProvider.GetOptions<ApplicationOptions>();
                var importCuesheet = new Cuesheet();
                importCuesheet.Import(_sessionStateContainer.Importfile.AnalysedCuesheet, applicationOptions);
                _sessionStateContainer.ImportCuesheet = importCuesheet;
            }
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
