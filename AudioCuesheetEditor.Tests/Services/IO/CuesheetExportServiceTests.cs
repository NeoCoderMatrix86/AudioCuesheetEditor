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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using Microsoft.Extensions.Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass()]
    public class CuesheetExportServiceTests
    {
        private readonly Mock<ISessionStateContainer> _sessionStateContainerMock;
        private readonly Mock<IStringLocalizer<ValidationMessage>> _localizerMock;
        private readonly CuesheetExportService _cuesheetExportService;

        public CuesheetExportServiceTests()
        {
            _sessionStateContainerMock = new Mock<ISessionStateContainer>();
            _localizerMock = new Mock<IStringLocalizer<ValidationMessage>>();
            _cuesheetExportService = new CuesheetExportService(_sessionStateContainerMock.Object, _localizerMock.Object);
        }

        [TestMethod]
        public void CanGenerateExportfile_InvalidExtension_ReturnsValidationMessage()
        {
            // Arrange
            string invalidFilename = "test.txt";
            _sessionStateContainerMock.SetupProperty(x => x.Cuesheet, new Cuesheet());
            _localizerMock.Setup(x => x["File extension is not '{0}'", It.IsAny<object[]>()]).Returns(new LocalizedString("File extension is not '{0}'", "File extension is not '{0}'"));

            // Act
            var result = _cuesheetExportService.CanGenerateExportfile(invalidFilename);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Error?.Message.Contains("File extension is not"));
        }

        [TestMethod]
        public void CanGenerateExportfiles_ValidExtension_ReturnsSuccess()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Artist = "Test Artist",
                Title = "Test Title",
                Audiofile = new Audiofile("Audio.mp3")
            };
            cuesheet.AddTrack(new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 3, 43)
            });
            _sessionStateContainerMock.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = _cuesheetExportService.CanGenerateExportfile("test.cue");

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void GenerateExportfiles_WithoutSections_ReturnsExportFile()
        {
            // Arrange
            var filename = "Test valid Filename.cue";
            var cuesheet = new Cuesheet()
            {
                Artist = "Test artist cuesheet",
                Title = "Test title cuesheet",
                Audiofile = new Audiofile("Test audiofile.mp3")
            };
            cuesheet.AddTrack(new Track()
            {
                Artist = "Test artist 1",
                Title = "Test title 1",
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 4, 12),
                Position = 1
            });
            cuesheet.AddTrack(new Track()
            {
                Artist = "Test artist 2",
                Title = "Test title 2",
                End = new TimeSpan(0, 8, 32),
                Position = 2
            });
            _sessionStateContainerMock.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = _cuesheetExportService.GenerateExportfile(filename);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(filename, result.Value!.Name);
            var content = result.Value!.Content;
            Assert.IsNotNull(content);
            Assert.AreEqual(@"TITLE ""Test title cuesheet""
PERFORMER ""Test artist cuesheet""
FILE ""Test audiofile.mp3"" MP3
	TRACK 01 AUDIO
		TITLE ""Test title 1""
		PERFORMER ""Test artist 1""
		INDEX 01 00:00:00
	TRACK 02 AUDIO
		TITLE ""Test title 2""
		PERFORMER ""Test artist 2""
		INDEX 01 04:12:00
", content);
        }

        [TestMethod]
        public void GenerateExportfiles_WithInvalidTracks_ReturnsFailure()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Artist = "Test Artist",
                Title = "Test Title",
                Audiofile = new Audiofile("Audio.mp3")
            };
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
            };
            var track2 = new Track()
            {
                Position = 2,
            };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            _sessionStateContainerMock.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = _cuesheetExportService.GenerateExportfile("test.cue");

            // Assert
            Assert.IsFalse(result.IsSuccess);
        }
    }
}