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
using AudioCuesheetEditor.Services.AudioCuesheet;
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        private readonly Mock<ITrackManager> _trackManagerMock;
        public ImportManagerTests()
        {
            _traceChangeManagerMock = new();
            _sessionStateContainerMock = new();
            _fileInputManagerMock = new();
            _textImportServiceMock = new();
            _trackManagerMock = new();
            _trackManagerMock.Setup(x => x.CopyValues(It.IsAny<ITrack>(), It.IsAny<Track>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Callback<ITrack, Track, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool>((source, target, setIsLinkedToPreviousTrack, setPosition, setArtist, setTitle, setBegin, setEnd, setLength, setFlags, setPreGap, setPostGap) =>
            {
                if (setIsLinkedToPreviousTrack)
                {
                    TrackManager_SetValue(target, x => x.IsLinkedToPreviousTrack, source.IsLinkedToPreviousTrack, false);
                }
                if (setPosition)
                {
                    TrackManager_SetValue(target, x => x.Position, source.Position, false);
                }
                if (setArtist)
                {
                    TrackManager_SetValue(target, x => x.Artist, source.Artist, false);
                }
                if (setTitle)
                {
                    TrackManager_SetValue(target, x => x.Title, source.Title, false);
                }
                if (setBegin)
                {
                    TrackManager_SetValue(target, x => x.Begin, source.Begin, false);
                }
                if (setEnd)
                {
                    TrackManager_SetValue(target, x => x.End, source.End, false);
                }
                if (setLength)
                {
                    TrackManager_SetValue(target, x => x.Length, source.Length, false);
                }
                if (setFlags)
                {
                    TrackManager_SetValue(target, x => x.Flags, source.Flags, false);
                }
                if (setPreGap)
                {
                    TrackManager_SetValue(target, x => x.PreGap, source.PreGap, false);
                }
                if (setPostGap)
                {
                    TrackManager_SetValue(target, x => x.PostGap, source.PostGap, false);
                }
            });
            _trackManagerMock.Setup(x => x.Clone(It.IsAny<ITrack>())).Returns<ITrack>(track =>
            {
                var clone = new Track();
                Boolean setLength = true;
                if (track.Begin.HasValue && track.End.HasValue)
                {
                    setLength = false;
                }
                _trackManagerMock.Object.CopyValues(track, clone, setLength: setLength);
                return clone;
            });
            var loggerMock = new Mock<ILogger<ImportManager>>();
            _service = new ImportManager(_sessionStateContainerMock.Object, _traceChangeManagerMock.Object, _fileInputManagerMock.Object, _textImportServiceMock.Object, _trackManagerMock.Object, loggerMock.Object);
        }

        void TrackManager_SetValue<TProperty>(Track track, Expression<Func<Track, TProperty>> propertyExpression, TProperty value, Boolean signalTraceChangeManager = true)
        {
            if (propertyExpression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("Expression must be a property");
            }

            if (memberExpression.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException("Member is not a property");
            }

            var previousValue = (TProperty?)propertyInfo.GetValue(track);
            if (Equals(previousValue, value))
            {
                return;
            }

            propertyInfo.SetValue(track, value);
            if (signalTraceChangeManager)
            {
                _traceChangeManagerMock.Object.AddChange(new(track, new(previousValue, propertyInfo.Name)));
            }
        }

        [TestMethod()]
        public async Task AnalyseImportfile_WithTextfile_SetsImportCuesheet()
        {
            // Arrange
            var fileContent = "This is just a test";
            var importAudiofile = new ImportAudiofile()
            {
                Name = "Test Cuesheet Audiofile"
            };
            var importCuesheet = new ImportCuesheet()
            {
                Artist = "Test Cuesheet Artist",
                Title = "Test Cuesheet Title",
                Audiofiles = [importAudiofile],
                Cataloguenumber = "Test Cuesheet Cataloguenumber",
                CDTextfile = "Test Cuesheet CDTextfile"
            };
            importAudiofile.Tracks.Add(new()
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
            IList<Audiofile>? sessionStateContainerImportAudiofiles = [];
            _sessionStateContainerMock.SetupGet(x => x.ImportAudiofiles).Returns(() => sessionStateContainerImportAudiofiles);
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
            Assert.HasCount(1, sessionStateContainerImportCuesheet.Audiofiles);
            Assert.AreEqual(importAudiofile.Name, sessionStateContainerImportCuesheet.Audiofiles.First().Name);
            Assert.AreEqual(importCuesheet.Cataloguenumber, sessionStateContainerImportCuesheet.Cataloguenumber);
            Assert.IsNotNull(sessionStateContainerImportCuesheet.CDTextfile);
            Assert.AreEqual(importCuesheet.CDTextfile, sessionStateContainerImportCuesheet.CDTextfile.Name);
            Assert.AreEqual(importAudiofile.Tracks.First().Artist, sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().Artist);
            Assert.AreEqual(importAudiofile.Tracks.First().Title, sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().Title);
            Assert.AreEqual(importAudiofile.Tracks.First().Begin, sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().Begin);
            Assert.AreEqual(importAudiofile.Tracks.First().End, sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().End);
            CollectionAssert.AreEquivalent(importAudiofile.Tracks.First().Flags.ToList(), sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().Flags.ToList());
            Assert.AreEqual(importAudiofile.Tracks.First().Position, sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().Position);
            Assert.AreEqual(importAudiofile.Tracks.First().PreGap, sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().PreGap);
            Assert.AreEqual(importAudiofile.Tracks.First().PostGap, sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().PostGap);
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

        [TestMethod()]
        public async Task AnalyseImportfile_ExistingCuesheet_SetsImportfileCuesheet()
        {
            // Arrange
            var fileContent = @"PERFORMER ""Sample CD Artist""
TITLE ""Sample CD Title""
FILE ""Sample.mp3"" MP3
TRACK 01 AUDIO
	PERFORMER ""Sample Artist 1""
	TITLE ""Sample Title 1""
	INDEX 01 00:00:00
TRACK 02 AUDIO
	PERFORMER ""Sample Artist 2""
	TITLE ""Sample Title 2""
	INDEX 01 05:00:00
TRACK 03 AUDIO
	PERFORMER ""Sample Artist 3""
	TITLE ""Sample Title 3""
	INDEX 01 09:23:00
TRACK 04 AUDIO
	PERFORMER ""Sample Artist 4""
	TITLE ""Sample Title 4""
	INDEX 01 15:54:00
TRACK 05 AUDIO
	PERFORMER ""Sample Artist 5""
	TITLE ""Sample Title 5""
	INDEX 01 20:13:00
TRACK 06 AUDIO
	PERFORMER ""Sample Artist 6""
	TITLE ""Sample Title 6""
	INDEX 01 24:54:00
TRACK 07 AUDIO
	PERFORMER ""Sample Artist 7""
	TITLE ""Sample Title 7""
	INDEX 01 31:54:00
TRACK 08 AUDIO
	PERFORMER ""Sample Artist 8""
	TITLE ""Sample Title 8""
	INDEX 01 45:54:00
";
            IImportfile sessionStateContainerImportFile = new Importfile()
            {
                FileContent = fileContent,
                FileType = ImportFileType.Cuesheet
            };
            IList<Audiofile>? sessionStateContainerImportAudiofiles = [];
            _sessionStateContainerMock.SetupGet(x => x.ImportAudiofiles).Returns(() => sessionStateContainerImportAudiofiles);
            _sessionStateContainerMock.SetupGet(x => x.Importfile).Returns(() => sessionStateContainerImportFile);
            _sessionStateContainerMock.SetupSet(x => x.Importfile = It.IsAny<IImportfile>()).Callback<IImportfile>(importfile => sessionStateContainerImportFile = importfile);

            // Act
            await _service.AnalyseImportfile();
            // Assert
            Assert.IsNotNull(sessionStateContainerImportFile.AnalyzedCuesheet);
            Assert.AreEqual("Sample CD Artist", sessionStateContainerImportFile.AnalyzedCuesheet.Artist);
            Assert.AreEqual("Sample CD Title", sessionStateContainerImportFile.AnalyzedCuesheet.Title);
            Assert.HasCount(1, sessionStateContainerImportFile.AnalyzedCuesheet.Audiofiles);
            Assert.AreEqual("Sample.mp3", sessionStateContainerImportFile.AnalyzedCuesheet.Audiofiles.First().Name);
            Assert.HasCount(8, sessionStateContainerImportFile.AnalyzedCuesheet.Audiofiles.First().Tracks);
            Assert.AreEqual("Sample Artist 1", sessionStateContainerImportFile.AnalyzedCuesheet.Audiofiles.First().Tracks.First().Artist);
            Assert.AreEqual(TimeSpan.Zero, sessionStateContainerImportFile.AnalyzedCuesheet.Audiofiles.First().Tracks.First().Begin);
            Assert.AreEqual("Sample Title 1", sessionStateContainerImportFile.AnalyzedCuesheet.Audiofiles.First().Tracks.First().Title);
        }

        [TestMethod]
        public async Task UploadFilesAsync_ProjectFile_ImportsCorrectly()
        {
            // Arrange
            var fileContent = "This is the content";
            var file = new FileUpload("test.projectfile", "text/plain", fileContent);
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(true);
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.IsValidForImportView(file.ContentType, file.Name)).Returns(false);

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
            var file = new FileUpload("test.cue", "text/plain", fileContent);

            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(true);
            _fileInputManagerMock.Setup(f => f.IsValidForImportView(file.ContentType, file.Name)).Returns(false);

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
            var file = new FileUpload("test.txt", "text/plain", fileContent);

            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.IsValidForImportView(file.ContentType, file.Name)).Returns(true);

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
            var file = new FileUpload("test.mp3", "audio/mpeg");

            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Projectfile, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Cuesheet, It.IsAny<IEnumerable<string>>())).Returns(false);
            _fileInputManagerMock.Setup(f => f.IsValidForImportView(file.ContentType, file.Name)).Returns(false);
            _fileInputManagerMock.Setup(f => f.IsValidAudiofile(file.ContentType, file.Name)).Returns(true);
            _fileInputManagerMock.Setup(f => f.CreateAudiofileAsync(It.IsAny<FileUpload>())).ReturnsAsync(new Audiofile() { Name = file.Name });

            IImportfile? sessionStateContainerImportfile = null;
            _sessionStateContainerMock.SetupSet(x => x.Importfile = It.IsAny<IImportfile>()).Callback<IImportfile>(x => sessionStateContainerImportfile = x);
            IList<Audiofile>? sessionStateContainerImportAudiofiles = [];
            _sessionStateContainerMock.SetupGet(x => x.ImportAudiofiles).Returns(() => sessionStateContainerImportAudiofiles);
            _sessionStateContainerMock.SetupSet(x => x.ImportAudiofiles = It.IsAny<IList<Audiofile>>()).Callback<IList<Audiofile>>(x => sessionStateContainerImportAudiofiles = x);
            // Act
            await _service.UploadFilesAsync([file]);

            // Assert
            Assert.IsNull(sessionStateContainerImportfile);
            Assert.HasCount(1, sessionStateContainerImportAudiofiles);
        }

        [TestMethod]
        public void ImportCuesheet_WithImportCuesheetAvailable_ImportsCuesheetData()
        {
            // Arrange
            var track1 = new Track()
            {
                Artist = "Track Artist 1",
                Title = "Track Title 1",
                End = new TimeSpan(0, 4, 23),
            };
            var track2 = new Track()
            {
                Artist = "Track Artist 2",
                Title = "Track Title 2",
                End = new TimeSpan(0, 8, 54),
            };
            var sessionStateContainerImportCuesheet = new Cuesheet()
            {
                Artist = "Artist 123",
                Title = "Title 456",
                Audiofiles = [
                    new Audiofile()
                    {
                        Name = "song.mp3",
                        ObjectURL = "Just a test",
                        Tracks = [track1, track2]
                    }
                ]
            };
            track1.Cuesheet = sessionStateContainerImportCuesheet;
            track2.Cuesheet = sessionStateContainerImportCuesheet;
            _sessionStateContainerMock.SetupGet(x => x.ImportCuesheet).Returns(sessionStateContainerImportCuesheet);
            Cuesheet sessionStateContainerCuesheet = new();
            _sessionStateContainerMock.SetupSet(x => x.Cuesheet = It.IsAny<Cuesheet>()).Callback<Cuesheet>(cuesheet => sessionStateContainerCuesheet = cuesheet);
            _sessionStateContainerMock.SetupGet(x => x.Cuesheet).Returns(() => sessionStateContainerCuesheet);
            IList<Audiofile>? sessionStateContainerImportAudiofiles = [];
            _sessionStateContainerMock.SetupGet(x => x.ImportAudiofiles).Returns(() => sessionStateContainerImportAudiofiles);
            // Act
            _service.ImportCuesheet();
            // Assert
            Assert.IsNotNull(sessionStateContainerCuesheet);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Artist, sessionStateContainerCuesheet.Artist);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Title, sessionStateContainerCuesheet.Title);
            Assert.HasCount(1, sessionStateContainerCuesheet.Audiofiles);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().Artist, sessionStateContainerCuesheet.Audiofiles.First().Tracks.First().Artist);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().Title, sessionStateContainerCuesheet.Audiofiles.First().Tracks.First().Title);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().Begin, sessionStateContainerCuesheet.Audiofiles.First().Tracks.First().Begin);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.First().End, sessionStateContainerCuesheet.Audiofiles.First().Tracks.First().End);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.Last().Artist, sessionStateContainerCuesheet.Audiofiles.First().Tracks.Last().Artist);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.Last().Title, sessionStateContainerCuesheet.Audiofiles.First().Tracks.Last().Title);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.Last().Begin, sessionStateContainerCuesheet.Audiofiles.First().Tracks.Last().Begin);
            Assert.AreEqual(sessionStateContainerImportCuesheet.Audiofiles.First().Tracks.Last().End, sessionStateContainerCuesheet.Audiofiles.First().Tracks.Last().End);
            _traceChangeManagerMock.Verify(x => x.RemoveTracedChanges(It.IsAny<IEnumerable<object>>()));
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
    }
}