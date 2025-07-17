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
using AudioCuesheetEditor.Model.IO.Import;
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
        [TestMethod()]
        public async Task AnalyseImportfile_WithTextfile_SetsImportCuesheet()
        {
            // Arrange
            var fileContent = "This is just a test";
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var fileInputManagerMock = new Mock<IFileInputManager>();
            var textImportServiceMock = new Mock<ITextImportService>();
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
            var importfile = new Importfile()
            {
                FileContent = fileContent,
                FileType = ImportFileType.Textfile,
                AnalysedCuesheet = importCuesheet
            };
            textImportServiceMock.Setup(x => x.AnalyseAsync(fileContent)).ReturnsAsync(importfile);
            var loggerMock = new Mock<ILogger<ImportManager>>();
            var importManager = new ImportManager(sessionStateContainer, traceChangeManager, fileInputManagerMock.Object, textImportServiceMock.Object, loggerMock.Object);
            var testHelper = new TestHelper();
            sessionStateContainer.Importfile = new Importfile()
            {
                FileContent = "This is just a test",
                FileType = ImportFileType.Textfile
            };
            // Act
            await importManager.AnalyseImportfile();
            // Assert
            Assert.AreEqual(importfile, sessionStateContainer.Importfile);
            Assert.IsNotNull(sessionStateContainer.ImportCuesheet);
            Assert.AreEqual(importCuesheet.Artist, sessionStateContainer.ImportCuesheet.Artist);
            Assert.AreEqual(importCuesheet.Title, sessionStateContainer.ImportCuesheet.Title);
            Assert.IsNotNull(sessionStateContainer.ImportCuesheet.Audiofile);
            Assert.AreEqual(importCuesheet.Audiofile, sessionStateContainer.ImportCuesheet.Audiofile.Name);
            Assert.AreEqual(importCuesheet.Cataloguenumber, sessionStateContainer.ImportCuesheet.Cataloguenumber);
            Assert.IsNotNull(sessionStateContainer.ImportCuesheet.CDTextfile);
            Assert.AreEqual(importCuesheet.CDTextfile, sessionStateContainer.ImportCuesheet.CDTextfile.Name);
            Assert.AreEqual(importCuesheet.Tracks.First().Artist, sessionStateContainer.ImportCuesheet.Tracks.First().Artist);
            Assert.AreEqual(importCuesheet.Tracks.First().Title, sessionStateContainer.ImportCuesheet.Tracks.First().Title);
            Assert.AreEqual(importCuesheet.Tracks.First().Begin, sessionStateContainer.ImportCuesheet.Tracks.First().Begin);
            Assert.AreEqual(importCuesheet.Tracks.First().End, sessionStateContainer.ImportCuesheet.Tracks.First().End);
            CollectionAssert.AreEquivalent(importCuesheet.Tracks.First().Flags.ToList(), sessionStateContainer.ImportCuesheet.Tracks.First().Flags.ToList());
            Assert.AreEqual(importCuesheet.Tracks.First().Position, sessionStateContainer.ImportCuesheet.Tracks.First().Position);
            Assert.AreEqual(importCuesheet.Tracks.First().PreGap, sessionStateContainer.ImportCuesheet.Tracks.First().PreGap);
            Assert.AreEqual(importCuesheet.Tracks.First().PostGap, sessionStateContainer.ImportCuesheet.Tracks.First().PostGap);
        }

        [TestMethod()]
        public async Task AnalyseImportfile_WithoutAnalysedCuesheet_DoesNothing()
        {
            // Arrange
            var fileContent = "This is just a test";
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var fileInputManagerMock = new Mock<IFileInputManager>();
            var textImportServiceMock = new Mock<ITextImportService>();
            var importfile = new Importfile()
            {
                FileContent = fileContent,
                FileType = ImportFileType.Textfile,
            };
            textImportServiceMock.Setup(x => x.AnalyseAsync(fileContent)).ReturnsAsync(importfile);
            var loggerMock = new Mock<ILogger<ImportManager>>();
            var importManager = new ImportManager(sessionStateContainer, traceChangeManager, fileInputManagerMock.Object, textImportServiceMock.Object, loggerMock.Object);
            var testHelper = new TestHelper();
            sessionStateContainer.Importfile = importfile;
            // Act
            await importManager.AnalyseImportfile();
            // Assert
            Assert.AreEqual(importfile, sessionStateContainer.Importfile);
            Assert.IsNull(sessionStateContainer.ImportCuesheet);
        }

        [TestMethod]
        public async Task ImportFilesAsync_ProjectFile_ImportsCorrectly()
        {
            // Arrange
            var fileContent = "This is the content";
            var file = CreateBrowserFileMock("test.projectfile", fileContent);
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var fileInputManagerMock = new Mock<IFileInputManager>();
            var textImportServiceMock = new Mock<ITextImportService>();
            fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(true);
            fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(false);
            fileInputManagerMock.Setup(f => f.IsValidForImportView(file)).Returns(false);

            var loggerMock = new Mock<ILogger<ImportManager>>();
            var importManager = new ImportManager(sessionStateContainer, traceChangeManager, fileInputManagerMock.Object, textImportServiceMock.Object, loggerMock.Object);
            // Act
            await importManager.ImportFilesAsync([file]);

            // Assert
            Assert.IsNotNull(sessionStateContainer.Importfile);
            Assert.AreEqual(fileContent, sessionStateContainer.Importfile.FileContent);
            Assert.AreEqual(fileContent, sessionStateContainer.Importfile.FileContentRecognized);
            Assert.AreEqual(ImportFileType.ProjectFile, sessionStateContainer.Importfile.FileType);
        }

        [TestMethod]
        public async Task ImportFilesAsync_CuesheetFile_ImportsCorrectly()
        {
            // Arrange
            var fileContent = "Cuesheet file content";
            var file = CreateBrowserFileMock("test.cue", fileContent);
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var fileInputManagerMock = new Mock<IFileInputManager>();
            var textImportServiceMock = new Mock<ITextImportService>();
            fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(false);
            fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(true);
            fileInputManagerMock.Setup(f => f.IsValidForImportView(file)).Returns(false);

            var loggerMock = new Mock<ILogger<ImportManager>>();
            var importManager = new ImportManager(sessionStateContainer, traceChangeManager, fileInputManagerMock.Object, textImportServiceMock.Object, loggerMock.Object);
            // Act
            await importManager.ImportFilesAsync([file]);

            // Assert
            Assert.IsNotNull(sessionStateContainer.Importfile);
            Assert.AreEqual(fileContent, sessionStateContainer.Importfile.FileContent);
            Assert.AreEqual(fileContent, sessionStateContainer.Importfile.FileContentRecognized);
            Assert.AreEqual(ImportFileType.Cuesheet, sessionStateContainer.Importfile.FileType);
        }

        [TestMethod]
        public async Task ImportFilesAsync_TextFile_ImportsCorrectly()
        {
            // Arrange
            var fileContent = "TextFileContent";
            var file = CreateBrowserFileMock("test.txt", fileContent);
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var fileInputManagerMock = new Mock<IFileInputManager>();
            var textImportServiceMock = new Mock<ITextImportService>();
            fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(false);
            fileInputManagerMock.Setup(f => f.CheckFileMimeType(file, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(false);
            fileInputManagerMock.Setup(f => f.IsValidForImportView(file)).Returns(true);

            var loggerMock = new Mock<ILogger<ImportManager>>();
            var importManager = new ImportManager(sessionStateContainer, traceChangeManager, fileInputManagerMock.Object, textImportServiceMock.Object, loggerMock.Object);
            // Act
            await importManager.ImportFilesAsync([file]);

            // Assert
            Assert.IsNotNull(sessionStateContainer.Importfile);
            Assert.AreEqual(fileContent, sessionStateContainer.Importfile.FileContent);
            Assert.AreEqual(fileContent, sessionStateContainer.Importfile.FileContentRecognized);
            Assert.AreEqual(ImportFileType.Textfile, sessionStateContainer.Importfile.FileType);
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