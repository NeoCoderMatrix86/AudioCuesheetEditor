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
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using AudioCuesheetEditor.Tests.Utility;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass()]
    public class ImportManagerTests
    {
        private readonly ImportManager _service;
        private readonly Mock<ISessionStateContainer> _sessionStateContainerMock;
        private readonly Mock<ITraceChangeManager> _traceChangeManagerMock;
        private readonly Mock<IFileInputManager> _fileInputManagerMock;
        private readonly Mock<ITextImportService> _textImportServiceMock;

        public ImportManagerTests()
        {
            _traceChangeManagerMock = new();
            _sessionStateContainerMock = new();
            _fileInputManagerMock = new();
            _textImportServiceMock = new();
            var loggerMock = new Mock<ILogger<ImportManager>>();
            _service = new ImportManager(_sessionStateContainerMock.Object, _traceChangeManagerMock.Object, _fileInputManagerMock.Object, _textImportServiceMock.Object, loggerMock.Object);
        }

        [TestMethod()]
        public async Task AnalyseImportfile_WithTextfile_SetsImportCuesheet()
        {
            // Arrange
            var fileContent = "This is just a test";
            var importCuesheet = new ImportCuesheet()
            {
                Artist = "Test Cuesheet Artist",
                Title = "Test Cuesheet Title",
                Audiofile = "Test Cuesheet Audiofile",
                Cataloguenumber = "Test Cuesheet Cataloguenumber",
                CDTextfile = "Test Cuesheet CDTextfile"
            };
            importCuesheet.Tracks.Add(new()
            {
                Artist = "Test Track Artist 1",
                Title = "Test Track Title 1",
                Begin = new TimeSpan(0, 3, 20),
                End = new TimeSpan(0, 7, 43),
                Flags = [Flag.DCP],
                Position = 1,
                PreGap = new TimeSpan(0, 0, 2),
                PostGap = new TimeSpan(0, 0, 4)
            });
            var importFile = new Importfile()
            {
                FileContent = fileContent,
                FileType = ImportFileType.Textfile,
                AnalyzedCuesheet = importCuesheet
            };
            _textImportServiceMock.Setup(x => x.AnalyseAsync(fileContent)).ReturnsAsync(importFile);
            _sessionStateContainerMock.SetupGet(x => x.Importfile).Returns(importFile);
            Cuesheet? sessionStateContainerImportCuesheet = null;
            _sessionStateContainerMock.SetupSet(x => x.ImportCuesheet = It.IsAny<Cuesheet>()).Callback<Cuesheet>(cs => sessionStateContainerImportCuesheet = cs);
            _sessionStateContainerMock.SetupGet(x => x.ImportCuesheet).Returns(() => sessionStateContainerImportCuesheet);
            IImportfile? sessionStateContainerImportfile = null;
            _sessionStateContainerMock.SetupSet(x => x.Importfile = It.IsAny<IImportfile>()).Callback<IImportfile>(x => sessionStateContainerImportfile = x);
            // Act
            await _service.AnalyseImportfile();
            // Assert
            Assert.AreEqual(importFile, sessionStateContainerImportfile);
            Assert.IsNotNull(sessionStateContainerImportCuesheet);
            Assert.AreEqual(importCuesheet.Artist, sessionStateContainerImportCuesheet.Artist);
            Assert.AreEqual(importCuesheet.Title, sessionStateContainerImportCuesheet.Title);
            Assert.IsNotNull(sessionStateContainerImportCuesheet.Audiofile);
            Assert.AreEqual(importCuesheet.Audiofile, sessionStateContainerImportCuesheet.Audiofile.Name);
            Assert.AreEqual(importCuesheet.Cataloguenumber, sessionStateContainerImportCuesheet.Cataloguenumber);
            Assert.IsNotNull(sessionStateContainerImportCuesheet.CDTextfile);
            Assert.AreEqual(importCuesheet.CDTextfile, sessionStateContainerImportCuesheet.CDTextfile.Name);
            Assert.AreEqual(importCuesheet.Tracks.First().Artist, sessionStateContainerImportCuesheet.Tracks.First().Artist);
            Assert.AreEqual(importCuesheet.Tracks.First().Title, sessionStateContainerImportCuesheet.Tracks.First().Title);
            Assert.AreEqual(importCuesheet.Tracks.First().Begin, sessionStateContainerImportCuesheet.Tracks.First().Begin);
            Assert.AreEqual(importCuesheet.Tracks.First().End, sessionStateContainerImportCuesheet.Tracks.First().End);
            CollectionAssert.AreEquivalent(importCuesheet.Tracks.First().Flags.ToList(), sessionStateContainerImportCuesheet.Tracks.First().Flags.ToList());
            Assert.AreEqual(importCuesheet.Tracks.First().Position, sessionStateContainerImportCuesheet.Tracks.First().Position);
            Assert.AreEqual(importCuesheet.Tracks.First().PreGap, sessionStateContainerImportCuesheet.Tracks.First().PreGap);
            Assert.AreEqual(importCuesheet.Tracks.First().PostGap, sessionStateContainerImportCuesheet.Tracks.First().PostGap);
            _traceChangeManagerMock.Verify(x => x.TraceChanges(It.IsAny<Cuesheet>()));
            _traceChangeManagerMock.Verify(x => x.TraceChanges(It.IsAny<Track>()));
        }

        [TestMethod()]
        public async Task AnalyseImportfile_WithoutAnalysedCuesheet_DoesNothing()
        {
            // Arrange
            var fileContent = "This is just a test";
            var importFile = new Importfile()
            {
                FileContent = fileContent,
                FileType = ImportFileType.Textfile,
            };
            _textImportServiceMock.Setup(x => x.AnalyseAsync(fileContent)).ReturnsAsync(importFile);
            _sessionStateContainerMock.SetupGet(x => x.Importfile).Returns(importFile);
            Cuesheet? sessionStateContainerImportCuesheet = null;
            _sessionStateContainerMock.SetupSet(x => x.ImportCuesheet = It.IsAny<Cuesheet>()).Callback<Cuesheet>(cs => sessionStateContainerImportCuesheet = cs);
            IImportfile? sessionStateContainerImportfile = null;
            _sessionStateContainerMock.SetupSet(x => x.Importfile = It.IsAny<IImportfile>()).Callback<IImportfile>(x => sessionStateContainerImportfile = x);
            // Act
            await _service.AnalyseImportfile();
            // Assert
            Assert.AreEqual(importFile, sessionStateContainerImportfile);
            Assert.IsNull(sessionStateContainerImportCuesheet);
        }

        [TestMethod]
        public async Task UploadFilesAsync_ProjectFile_ImportsCorrectly()
        {
            // Arrange
            var fileContent = "This is the content";
            var file = CreateBrowserFileMock("test.projectfile", fileContent);
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(true);
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.IsValidForImportView(file)).Returns(false);

            IImportfile? sessionStateContainerImportfile = null;
            _sessionStateContainerMock.SetupSet(x => x.Importfile = It.IsAny<IImportfile>()).Callback<IImportfile>(x => sessionStateContainerImportfile = x);
            // Act
            await _service.UploadFilesAsync([file]);

            // Assert
            Assert.IsNotNull(sessionStateContainerImportfile);
            Assert.AreEqual(fileContent, sessionStateContainerImportfile.FileContent);
            Assert.AreEqual(fileContent, sessionStateContainerImportfile.FileContentRecognized);
            Assert.AreEqual(ImportFileType.ProjectFile, sessionStateContainerImportfile.FileType);
        }

        [TestMethod]
        public async Task UploadFilesAsync_CuesheetFile_ImportsCorrectly()
        {
            // Arrange
            var fileContent = "Cuesheet file content";
            var file = CreateBrowserFileMock("test.cue", fileContent);
            
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(true);
            _fileInputManagerMock.Setup(f => f.IsValidForImportView(file)).Returns(false);

            IImportfile? sessionStateContainerImportfile = null;
            _sessionStateContainerMock.SetupSet(x => x.Importfile = It.IsAny<IImportfile>()).Callback<IImportfile>(x => sessionStateContainerImportfile = x);
            // Act
            await _service.UploadFilesAsync([file]);

            // Assert
            Assert.IsNotNull(sessionStateContainerImportfile);
            Assert.AreEqual(fileContent, sessionStateContainerImportfile.FileContent);
            Assert.AreEqual(fileContent, sessionStateContainerImportfile.FileContentRecognized);
            Assert.AreEqual(ImportFileType.Cuesheet, sessionStateContainerImportfile.FileType);
        }

        [TestMethod]
        public async Task UploadFilesAsync_TextFile_ImportsCorrectly()
        {
            // Arrange
            var fileContent = "TextFileContent";
            var file = CreateBrowserFileMock("test.txt", fileContent);
            
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.IsValidForImportView(file)).Returns(true);

            IImportfile? sessionStateContainerImportfile = null;
            _sessionStateContainerMock.SetupSet(x => x.Importfile = It.IsAny<IImportfile>()).Callback<IImportfile>(x => sessionStateContainerImportfile = x);
            // Act
            await _service.UploadFilesAsync([file]);

            // Assert
            Assert.IsNotNull(sessionStateContainerImportfile);
            Assert.AreEqual(fileContent, sessionStateContainerImportfile.FileContent);
            Assert.AreEqual(fileContent, sessionStateContainerImportfile.FileContentRecognized);
            Assert.AreEqual(ImportFileType.Textfile, sessionStateContainerImportfile.FileType);
        }

        [TestMethod]
        public async Task UploadFilesAsync_WithAudiofile_ImportsCorrectly()
        {
            // Arrange
            var file = CreateBrowserFileMock("test.mp3");
            
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.IsValidForImportView(file)).Returns(false);
            _fileInputManagerMock.Setup(f => f.IsValidAudiofile(file)).Returns(true);
            _fileInputManagerMock.Setup(f => f.CreateAudiofileAsync(It.IsAny<string>(), It.IsAny<IBrowserFile?>(), It.IsAny<Action<Task<Stream>>?>())).ReturnsAsync(new AudioCuesheetEditor.Model.IO.Audio.Audiofile(file.Name));

            IImportfile? sessionStateContainerImportfile = null;
            _sessionStateContainerMock.SetupSet(x => x.Importfile = It.IsAny<IImportfile>()).Callback<IImportfile>(x => sessionStateContainerImportfile = x);
            Audiofile? sessionStateContainerImportAudiofile = null;
            _sessionStateContainerMock.SetupSet(x => x.ImportAudiofile = It.IsAny<Audiofile>()).Callback<Audiofile>(x => sessionStateContainerImportAudiofile = x);
            // Act
            await _service.UploadFilesAsync([file]);

            // Assert
            Assert.IsNull(sessionStateContainerImportfile);
            Assert.IsNotNull(sessionStateContainerImportAudiofile);
        }

        [TestMethod]
        public void ImportCuesheet_WithImportCuesheetAvailable_ImportsCuesheetData()
        {
            // Arrange
            var sessionStateContainerImportCuesheet = new Cuesheet()
            {
                Artist = "Artist 123",
                Title = "Title 456"
            };
            sessionStateContainerImportCuesheet.AddTrack(new()
            {
                Artist = "Track Artist 1",
                Title = "Track Title 1",
                End = new TimeSpan(0, 4, 23),
            });
            sessionStateContainerImportCuesheet.AddTrack(new()
            {
                Artist = "Track Artist 2",
                Title = "Track Title 2",
                End = new TimeSpan(0, 8, 54),
            });
            _sessionStateContainerMock.SetupGet(x => x.ImportCuesheet).Returns(sessionStateContainerImportCuesheet);
            Cuesheet sessionStateContainerCuesheet = new();
            _sessionStateContainerMock.SetupSet(x => x.Cuesheet = It.IsAny<Cuesheet>()).Callback<Cuesheet>(cuesheet => sessionStateContainerCuesheet = cuesheet);
            _sessionStateContainerMock.SetupGet(x => x.Cuesheet).Returns(() => sessionStateContainerCuesheet);
            // Act
            _service.ImportCuesheet();
            // Assert
            Assert.IsNotNull(sessionStateContainerCuesheet);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Artist, sessionStateContainerCuesheet.Artist);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Title, sessionStateContainerCuesheet.Title);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Tracks.First().Artist, sessionStateContainerCuesheet.Tracks.First().Artist);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Tracks.First().Title, sessionStateContainerCuesheet.Tracks.First().Title);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Tracks.First().Begin, sessionStateContainerCuesheet.Tracks.First().Begin);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Tracks.First().End, sessionStateContainerCuesheet.Tracks.First().End);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Tracks.Last().Artist, sessionStateContainerCuesheet.Tracks.Last().Artist);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Tracks.Last().Title, sessionStateContainerCuesheet.Tracks.Last().Title);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Tracks.Last().Begin, sessionStateContainerCuesheet.Tracks.Last().Begin);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Tracks.Last().End, sessionStateContainerCuesheet.Tracks.Last().End);
            _traceChangeManagerMock.Verify(x => x.RemoveTracedChanges(It.IsAny<IEnumerable<ITraceable>>()));
        }

        [TestMethod]
        public void ImportData_ValidData_SetsImportfile()
        {
            // Arrange
            var importData = nameof(ImportData_ValidData_SetsImportfile);
            IImportfile? sessionStateContainerImportfile = null;
            _sessionStateContainerMock.SetupSet(x => x.Importfile = It.IsAny<IImportfile>()).Callback<IImportfile>(x => sessionStateContainerImportfile = x);
            // Act
            _service.ImportData(importData);
            // Assert
            Assert.IsNotNull(sessionStateContainerImportfile);
            Assert.AreEqual(importData, sessionStateContainerImportfile.FileContent);
            Assert.AreEqual(importData, sessionStateContainerImportfile.FileContentRecognized);
            Assert.AreEqual(ImportFileType.Textfile, sessionStateContainerImportfile.FileType);
        }

        private static IBrowserFile CreateBrowserFileMock(string name, string content = "TestContent")
        {
            var fileMock = new Mock<IBrowserFile>();
            fileMock.Setup(f => f.Name).Returns(name);
            fileMock.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            return fileMock.Object;
        }
    }
}