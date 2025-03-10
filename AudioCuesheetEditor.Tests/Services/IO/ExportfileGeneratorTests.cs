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
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass]
    public class ExportfileGeneratorTests
    {
        private readonly Mock<ISessionStateContainer> mockSessionStateContainer;
        private readonly ExportfileGenerator exportfileGenerator;

        public ExportfileGeneratorTests()
        {
            mockSessionStateContainer = new Mock<ISessionStateContainer>();
            exportfileGenerator = new ExportfileGenerator(mockSessionStateContainer.Object);
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
                Audiofile = new("Test audiofile.mp3")
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
            mockSessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = exportfileGenerator.GenerateExportfiles(exportProfile);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(exportProfile.Filename, result.First().Name);
            Assert.AreEqual(TimeSpan.Zero, result.First().Begin);
            Assert.AreEqual(new TimeSpan(0, 8, 32), result.First().End);
            var content = result.First().Content;
            Assert.IsNotNull(content);
            var contentString = System.Text.Encoding.UTF8.GetString(content);
            Assert.AreEqual(@"Test artist cuesheet - Test title cuesheet
1 Test artist 1 - Test title 1
2 Test artist 2 - Test title 2

", contentString);
        }

        [TestMethod]
        public void GenerateExportFile_ShouldGenerateExportfiles_WithSections()
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
                Audiofile = new("Test audiofile.mp3")
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
            mockSessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = exportfileGenerator.GenerateExportfiles(exportProfile);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("TestExport(3).txt", result.Last().Name);
            Assert.AreEqual(section3.Begin, result.Last().Begin);
            Assert.AreEqual(section3.End, result.Last().End);
            var content = result.Last().Content;
            Assert.IsNotNull(content);
            var contentString = System.Text.Encoding.UTF8.GetString(content);
            Assert.AreEqual(@"Test artist cuesheet - Test title cuesheet
1 Test artist 5 - Test title 5
2 Test artist 6 - Test title 6

", contentString);
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
                Audiofile = new("Test audiofile.mp3")
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
            mockSessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);

            // Act
            var result = exportfileGenerator.GenerateExportfiles(exportProfile);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(Exportprofile.DefaultFileName, result.First().Name);
            Assert.AreEqual(TimeSpan.Zero, result.First().Begin);
            Assert.AreEqual(new TimeSpan(0, 8, 32), result.First().End);
            Assert.IsNotNull(result.First().Content);
        }
    }
}