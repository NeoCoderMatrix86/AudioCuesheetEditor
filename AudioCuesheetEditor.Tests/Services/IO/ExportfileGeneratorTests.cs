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
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using Microsoft.Extensions.Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass]
    public class ExportfileGeneratorTests
    {
        private readonly Mock<ISessionStateContainer> _mockSessionStateContainer;
        private readonly ExportfileGenerator _exportfileGenerator;

        public ExportfileGeneratorTests()
        {
            _mockSessionStateContainer = new Mock<ISessionStateContainer>();
            var mockLocalizer = new Mock<IStringLocalizer<ValidationMessage>>();
            _exportfileGenerator = new ExportfileGenerator(_mockSessionStateContainer.Object, mockLocalizer.Object);
        }

        [TestMethod]
        public void GenerateExportFile_ShouldGenerateExportfile_WithoutSections()
        {
            // Arrange
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
            _mockSessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = _exportfileGenerator.GenerateExportfile(exportProfile);
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(exportProfile.Filename, result.Value!.Name);
            var content = result.Value!.Content;
            Assert.IsNotNull(content);
            Assert.AreEqual(@"Test artist cuesheet - Test title cuesheet
1 Test artist 1 - Test title 1
2 Test artist 2 - Test title 2

", content);
        }

        [TestMethod]
        public void GenerateExportFile_ShouldHandleEmptyProfile()
        {
            // Arrange
            var exportProfile = new Exportprofile();
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
            _mockSessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = _exportfileGenerator.GenerateExportfile(exportProfile);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(Exportprofile.DefaultFileName, result.Value!.Name);
            Assert.IsNotNull(result.Value!.Content);
        }

        [TestMethod]
        public void GenerateExportFile_ReturnsEmpty_WithInvalidTracks()
        {
            // Arrange
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
                Position = 1
            });
            cuesheet.AddTrack(new Track()
            {
                Artist = "Test artist 2",
                Title = "Test title 2",
                Position = 2
            });
            _mockSessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = _exportfileGenerator.GenerateExportfile(exportProfile);

            // Assert
            Assert.IsFalse(result.IsSuccess);
        }
    }
}