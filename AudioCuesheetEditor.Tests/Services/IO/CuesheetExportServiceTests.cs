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
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Text;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass()]
    public class CuesheetExportServiceTests
    {
        private readonly Mock<ISessionStateContainer> sessionStateContainerMock;
        private readonly CuesheetExportService cuesheetExportService;

        public CuesheetExportServiceTests()
        {
            sessionStateContainerMock = new Mock<ISessionStateContainer>();
            cuesheetExportService = new CuesheetExportService(sessionStateContainerMock.Object);
        }

        [TestMethod]
        public void CanGenerateExportfiles_InvalidExtension_ReturnsValidationMessage()
        {
            // Arrange
            string invalidFilename = "test.txt";
            sessionStateContainerMock.SetupProperty(x => x.Cuesheet, new Cuesheet());

            // Act
            var result = cuesheetExportService.CanGenerateExportfiles(invalidFilename);

            // Assert
            Assert.IsTrue(result.Any(vm => vm.Message.Contains("File extension is not")));
        }

        [TestMethod]
        public void CanGenerateExportfiles_ValidExtension_ReturnsEmpty()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Artist = "Test Artist",
                Title = "Test Title",
                Audiofile = new Audiofile("Audio.mp3")
            };
            cuesheet.AddTrack(new Track());
            sessionStateContainerMock.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = cuesheetExportService.CanGenerateExportfiles("test.cue");

            // Assert
            Assert.AreEqual(0, result.Count());
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
            sessionStateContainerMock.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = cuesheetExportService.GenerateExportfiles(filename);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(filename, result.First().Name);
            Assert.AreEqual(TimeSpan.Zero, result.First().Begin);
            Assert.AreEqual(new TimeSpan(0, 8, 32), result.First().End);
            var content = result.First().Content;
            Assert.IsNotNull(content);
            var contentString = Encoding.UTF8.GetString(content);
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
", contentString);
        }

        [TestMethod]
        public void GenerateExportfiles_WithSections_ReturnsExportFiles()
        {
            // Arrange
            var filename = "Test valid Filename.cue";
            var exportProfile = new Exportprofile
            {
                Name = "TestProfile",
                SchemeHead = "%Cuesheet.Artist% - %Cuesheet.Title%",
                SchemeTracks = "%Track.Position% %Track.Artist% - %Track.Title%",
                Filename = "TestExport.txt"
            };
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
            cuesheet.AddTrack(new Track()
            {
                Artist = "Test artist 3",
                Title = "Test title 3",
                End = new TimeSpan(0, 12, 31),
                Position = 3
            });
            cuesheet.AddTrack(new Track()
            {
                Artist = "Test artist 4",
                Title = "Test title 4",
                End = new TimeSpan(0, 16, 8),
                Position = 4
            });
            cuesheet.AddTrack(new Track()
            {
                Artist = "Test artist 5",
                Title = "Test title 5",
                End = new TimeSpan(0, 21, 54),
                Position = 5
            });
            cuesheet.AddTrack(new Track()
            {
                Artist = "Test artist 6",
                Title = "Test title 6",
                End = new TimeSpan(0, 31, 32),
                Position = 6
            });
            var section1 = cuesheet.AddSection();
            section1.Begin = TimeSpan.Zero;
            section1.End = new TimeSpan(0, 10, 0);
            var section2 = cuesheet.AddSection();
            section2.Begin = section1.End;
            section2.End = new TimeSpan(0, 20, 0);
            var section3 = cuesheet.AddSection();
            section3.Begin = section2.End;
            section3.End = new TimeSpan(0, 30, 0);
            sessionStateContainerMock.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = cuesheetExportService.GenerateExportfiles(filename);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Test valid Filename(3).cue", result.Last().Name);
            Assert.AreEqual(section3.Begin, result.Last().Begin);
            Assert.AreEqual(section3.End, result.Last().End);
            var content = result.Last().Content;
            Assert.IsNotNull(content);
            var contentString = Encoding.UTF8.GetString(content);
            Assert.AreEqual(@"TITLE ""Test title cuesheet""
PERFORMER ""Test artist cuesheet""
FILE ""Test audiofile.mp3"" MP3
	TRACK 01 AUDIO
		TITLE ""Test title 5""
		PERFORMER ""Test artist 5""
		INDEX 01 00:00:00
	TRACK 02 AUDIO
		TITLE ""Test title 6""
		PERFORMER ""Test artist 6""
		INDEX 01 01:54:00
", contentString);
        }

        [TestMethod]
        public void GenerateExportfiles_WithInvalidTracks_ReturnsEmpty()
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
            sessionStateContainerMock.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = cuesheetExportService.GenerateExportfiles("test.cue");

            // Assert
            Assert.AreEqual(0, result.Count);
        }
    }
}