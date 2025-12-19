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
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.AudioCuesheet.Import;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.Utility;
using AudioCuesheetEditor.Services.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass()]
    public class TextImportServiceTests
    {
        [TestMethod()]
        public async Task AnalyseAsync_SampleCuesheet_CreatesValidCuesheetAsync()
        {
            // Arrange
            var fileContent = @"CuesheetArtist - CuesheetTitle				c:\tmp\Testfile.mp3
Sample Artist 1 - Sample Title 1				00:05:00
Sample Artist 2 - Sample Title 2				00:09:23
Sample Artist 3 - Sample Title 3				00:15:54
Sample Artist 4 - Sample Title 4				00:20:13
Sample Artist 5 - Sample Title 5				00:24:54
Sample Artist 6 - Sample Title 6				00:31:54
Sample Artist 7 - Sample Title 7				00:45:54
Sample Artist 8 - Sample Title 8				01:15:54";
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var importOptions = new ImportOptions
            {
                SelectedImportProfile = ImportOptions.DefaultSelectedImportprofile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(importOptions);
            var applicationOptions = new ApplicationOptions()
            {
                DefaultIsLinkedToPreviousTrack = true
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.AreEqual("CuesheetArtist", importFile.AnalyzedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importFile.AnalyzedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\Testfile.mp3", importFile.AnalyzedCuesheet.Audiofile);
            Assert.HasCount(8, importFile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual("Sample Artist 1", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(0).End);
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(0).IsLinkedToPreviousTrack);
        }

        [TestMethod()]
        public async Task AnalyseAsync_InvalidSchemeTracks_CreatesAnalyseExceptionAsync()
        {
            // Arrange
            var fileContent = @"CuesheetArtist|CuesheetTitle				c:\tmp\TestTextFile.cdt
1|Sample Artist 1 - Sample Title 1				00:05:00
2|Sample Artist 2 - Sample Title 2				00:09:23
3|Sample Artist 3 - Sample Title 3				00:15:54
4|Sample Artist 4 - Sample Title 4				00:20:13
5|Sample Artist 5 - Sample Title 5				00:24:54
6|Sample Artist 6 - Sample Title 6				00:31:54
7|Sample Artist 7 - Sample Title 7				00:45:54
8|Sample Artist 8 - Sample Title 8				01:15:54";
            var profile = new Importprofile()
            {
                UseRegularExpression = true,
                SchemeCuesheet = @"(?'Cuesheet.Artist'\A.*)[|](?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.CDTextfile'.{1,})",
                SchemeTracks = @"(?'Track.Position'.{1,})|(?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNotNull(importfile.AnalyseException);
        }

        [TestMethod()]
        public async Task AnalyseAsync_InputfileWithExtraSeperator_CreatesValidCuesheetAsync()
        {
            // Arrange
            var fileContent = @"CuesheetArtist|CuesheetTitle				c:\tmp\TestTextFile.cdt
1|Sample Artist 1 - Sample Title 1				00:05:00
2|Sample Artist 2 - Sample Title 2				00:09:23
3|Sample Artist 3 - Sample Title 3				00:15:54
4|Sample Artist 4 - Sample Title 4				00:20:13
5|Sample Artist 5 - Sample Title 5				00:24:54
6|Sample Artist 6 - Sample Title 6				00:31:54
7|Sample Artist 7 - Sample Title 7				00:45:54
8|Sample Artist 8 - Sample Title 8				01:15:54";
            var profile = new Importprofile()
            {
                UseRegularExpression = true,
                SchemeCuesheet = @"(?'Artist'\A.*)[|](?'Title'\w{1,})\t{1,}(?'CDTextfile'[^\r\n]+)",
                SchemeTracks = @"(?'Position'\d{1,})[|](?'Artist'.{1,}) - (?'Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'End'.{1,})"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalyzedCuesheet);
            Assert.AreEqual("CuesheetArtist", importfile.AnalyzedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importfile.AnalyzedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\TestTextFile.cdt", importfile.AnalyzedCuesheet.CDTextfile);
            Assert.HasCount(8, importfile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual((uint)6, importfile.AnalyzedCuesheet.Tracks.ElementAt(5).Position);
            Assert.AreEqual("Sample Artist 1", importfile.AnalyzedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importfile.AnalyzedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importfile.AnalyzedCuesheet.Tracks.ElementAt(0).End);
        }
        
        [TestMethod()]
        public async Task AnalyseAsync_InputfileWithSimplifiedScheme_CreatesValidCuesheetAsync()
        {
            var fileContent = @"CuesheetArtist|CuesheetTitle				c:\tmp\TestTextFile.cdt
1|Sample Artist 1 - Sample Title 1				00:05:00
2|Sample Artist 2 - Sample Title 2				00:09:23
3|Sample Artist 3 - Sample Title 3				00:15:54
4|Sample Artist 4 - Sample Title 4				00:20:13
5|Sample Artist 5 - Sample Title 5				00:24:54
6|Sample Artist 6 - Sample Title 6				00:31:54
7|Sample Artist 7 - Sample Title 7				00:45:54
8|Sample Artist 8 - Sample Title 8				01:15:54";
            var profile = new Importprofile()
            {
                UseRegularExpression = false,
                SchemeCuesheet = @"Artist|Title	CDTextfile",
                SchemeTracks = @"Position|Artist - Title	End"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.AreEqual("CuesheetArtist", importFile.AnalyzedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importFile.AnalyzedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\TestTextFile.cdt", importFile.AnalyzedCuesheet.CDTextfile);
            Assert.HasCount(8, importFile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual((uint)6, importFile.AnalyzedCuesheet.Tracks.ElementAt(5).Position);
            Assert.AreEqual("Sample Artist 1", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(0).End);
        }

        [TestMethod()]
        public async Task AnalyseAsync_InvalidScheme_CreatesAnalyseExceptionAsync()
        {
            // Arrange
            var fileContent = @"CuesheetArtist|CuesheetTitle				c:\tmp\TestTextFile.cdt				A83412346734
1|Sample Artist 1 - Sample Title 1				00:05:00
2|Sample Artist 2 - Sample Title 2				00:09:23
3|Sample Artist 3 - Sample Title 3				00:15:54
4|Sample Artist 4 - Sample Title 4				00:20:13
5|Sample Artist 5 - Sample Title 5				00:24:54
6|Sample Artist 6 - Sample Title 6				00:31:54
7|Sample Artist 7 - Sample Title 7				00:45:54
8|Sample Artist 8 - Sample Title 8				01:15:54








";
            var profile = new Importprofile()
            {
                UseRegularExpression = true,
                SchemeCuesheet = @"(?'Cuesheet.Artist'\A.*)[|](?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.CDTextfile'[a-zA-Z0-9_ .();äöü&:,\\]{1,})\t{1,}(?'Cuesheet.Cataloguenumber'.{1,})",
                SchemeTracks = @"(?'Track.Position'.{1,})|(?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNotNull(importFile.AnalyseException);
        }

        [TestMethod()]
        public async Task AnalyseAsync_CuesheetWithTextfileAndCatalogueNumber_CreatesValidCuesheetAsync()
        {
            // Arrange
            var fileContent = @"CuesheetArtist|CuesheetTitle				c:\tmp\TestTextFile.cdt				A83412346734
1|Sample Artist 1 - Sample Title 1				00:05:00
2|Sample Artist 2 - Sample Title 2				00:09:23
3|Sample Artist 3 - Sample Title 3				00:15:54
4|Sample Artist 4 - Sample Title 4				00:20:13
5|Sample Artist 5 - Sample Title 5				00:24:54
6|Sample Artist 6 - Sample Title 6				00:31:54
7|Sample Artist 7 - Sample Title 7				00:45:54
8|Sample Artist 8 - Sample Title 8				01:15:54








";
            var profile = new Importprofile()
            {
                UseRegularExpression = true,
                SchemeCuesheet = @"(?'Artist'\A.*)[|](?'Title'\w{1,})\t{1,}(?'CDTextfile'[a-zA-Z0-9_ .();äöü&:,\\]{1,})\t{1,}(?'Cataloguenumber'[^\r\n]+)",
                SchemeTracks = @"(?'Position'.{1,})[|](?'Artist'.{1,}) - (?'Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'End'.{1,})"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.AreEqual("CuesheetArtist", importFile.AnalyzedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importFile.AnalyzedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\TestTextFile.cdt", importFile.AnalyzedCuesheet.CDTextfile);
            Assert.AreEqual("A83412346734", importFile.AnalyzedCuesheet.Cataloguenumber);
            Assert.HasCount(8, importFile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual((uint)6, importFile.AnalyzedCuesheet.Tracks.ElementAt(5).Position);
            Assert.AreEqual("Sample Artist 1", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(0).End);
        }

        [TestMethod()]
        public async Task AnalyseAsync_CuesheetTracksOnly_CreatesValidCuesheetAsync()
        {
            // Arrange
            var fileContent = @"Sample Artist 1 - Sample Title 1				00:05:00
Sample Artist 2 - Sample Title 2				00:09:23
Sample Artist 3 - Sample Title 3				00:15:54
Sample Artist 4 - Sample Title 4				00:20:13
Sample Artist 5 - Sample Title 5				00:24:54
Sample Artist 6 - Sample Title 6				00:31:54
Sample Artist 7 - Sample Title 7				00:45:54
Sample Artist 8 - Sample Title 8				01:15:54
Sample Artist 9 - Sample Title 9				";
            var profile = new Importprofile()
            {
                SchemeTracks = $"{nameof(ImportTrack.Artist)} - {nameof(ImportTrack.Title)}\t{nameof(ImportTrack.End)}"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.HasCount(9, importFile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual("Sample Artist 1", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(1, 15, 54), importFile.AnalyzedCuesheet.Tracks.ElementAt(7).End);
        }

        [TestMethod()]
        public async Task AnalyseAsync_CuesheetBug213_CreatesValidCuesheetAsync()
        {
            // Arrange
            var fileContent = File.ReadAllText("Resources/Textimport_Bug_213.txt");
            var profile = new Importprofile()
            {
                SchemeTracks = $"{nameof(ImportTrack.Artist)} - {nameof(ImportTrack.Title)}\t{nameof(ImportTrack.End)}",
                TimeSpanFormat = new() { Scheme = $"{nameof(TimeSpanFormat.Minutes)}:{nameof(TimeSpanFormat.Seconds)}" }
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.HasCount(4, importFile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual(new TimeSpan(2, 3, 23), importFile.AnalyzedCuesheet.Tracks.ElementAt(3).End);
        }

        [TestMethod()]
        public async Task AnalyseAsync_CuesheetWithFlags_CreatesValidCuesheetAsync()
        {
            // Arrange
            var fileContent = @"Sample Artist 1 - Sample Title 1				00:05:00	DCP
Sample Artist 2 - Sample Title 2				00:09:23
Sample Artist 3 - Sample Title 3				00:15:54	PRE, DCP
Sample Artist 4 - Sample Title 4				00:20:13	4CH
Sample Artist 5 - Sample Title 5				00:24:54
Sample Artist 6 - Sample Title 6				00:31:54	PRE DCP 4CH
Sample Artist 7 - Sample Title 7				00:45:54
Sample Artist 8 - Sample Title 8				01:15:54	PRE DCP 4CH SCMS";
            var profile = new Importprofile()
            {
                UseRegularExpression = true,
                SchemeTracks = "(?'Artist'[a-zA-Z0-9_ .();äöü&:,]+) - (?'Title'[a-zA-Z0-9_ .();äöü]+)[\t ]+(?'End'[0-9]{2}:[0-9]{2}:[0-9]{2})(?:[\t ]+(?'Flags'[A-Za-z0-9 ,]+))?(?=[\t ]*(?:\r?\n|$))"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.HasCount(8, importFile.AnalyzedCuesheet.Tracks);
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Flags.Contains(Flag.DCP));
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(2).Flags.Contains(Flag.DCP));
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(2).Flags.Contains(Flag.PRE));
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(3).Flags.Contains(Flag.FourCH));
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(5).Flags.Contains(Flag.FourCH));
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(5).Flags.Contains(Flag.PRE));
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(5).Flags.Contains(Flag.DCP));
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(7).Flags.Contains(Flag.DCP));
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(7).Flags.Contains(Flag.PRE));
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(7).Flags.Contains(Flag.FourCH));
            Assert.IsTrue(importFile.AnalyzedCuesheet.Tracks.ElementAt(7).Flags.Contains(Flag.SCMS));
        }

        [TestMethod()]
        public async Task AnalyseAsync_CuesheetWithPreGapAndPostGap_CreatesValidCuesheetAsync()
        {
            // Arrange
            var fileContent = @"Sample Artist 1 - Sample Title 1		00:00:02		00:05:00		00:00:00
Sample Artist 2 - Sample Title 2		00:00:04		00:09:23		00:00:00
Sample Artist 3 - Sample Title 3		00:00:00		00:15:54		00:00:02
Sample Artist 4 - Sample Title 4		00:00:00		00:20:13		00:00:03
Sample Artist 5 - Sample Title 5		00:00:00		00:24:54		00:00:04
Sample Artist 6 - Sample Title 6		00:00:00		00:31:54		00:00:01
Sample Artist 7 - Sample Title 7		00:00:00		00:45:54		00:00:00
Sample Artist 8 - Sample Title 8		00:00:02		01:15:54		00:00:00";
            var profile = new Importprofile()
            {
                UseRegularExpression = true,
                SchemeTracks = "(?'Artist'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Title'[a-zA-Z0-9_ .();äöü]{1,})\t{1,}(?'PreGap'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'End'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'PostGap'[0-9]{2}[:][0-9]{2}[:][0-9]{2})"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.HasCount(8, importFile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual(new TimeSpan(0, 0, 2), importFile.AnalyzedCuesheet.Tracks.ElementAt(0).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(0).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), importFile.AnalyzedCuesheet.Tracks.ElementAt(1).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(1).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(2).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 2), importFile.AnalyzedCuesheet.Tracks.ElementAt(2).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(3).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 3), importFile.AnalyzedCuesheet.Tracks.ElementAt(3).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(4).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), importFile.AnalyzedCuesheet.Tracks.ElementAt(4).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(5).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 1), importFile.AnalyzedCuesheet.Tracks.ElementAt(5).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(6).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(6).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 2), importFile.AnalyzedCuesheet.Tracks.ElementAt(7).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(7).PostGap);
        }

        [TestMethod()]
        public async Task AnalyseAsync_SchemeCuesheetOnly_CreatesFileContentRecognizedOnlyForCuesheetAsync()
        {
            // Arrange
            var fileContent = @"CuesheetArtist - CuesheetTitle				c:\tmp\Testfile.mp3
Sample Artist 1 - Sample Title 1				00:05:00
Sample Artist 2 - Sample Title 2				00:09:23
Sample Artist 3 - Sample Title 3				00:15:54
Sample Artist 4 - Sample Title 4				00:20:13
Sample Artist 5 - Sample Title 5				00:24:54
Sample Artist 6 - Sample Title 6				00:31:54
Sample Artist 7 - Sample Title 7				00:45:54
Sample Artist 8 - Sample Title 8				01:15:54";
            var profile = new Importprofile()
            {
                SchemeCuesheet = $"{nameof(ImportCuesheet.Artist)} - {nameof(ImportCuesheet.Title)}\t{nameof(ImportCuesheet.Audiofile)}"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.IsNotNull(importFile.FileContentRecognized);
            var lines = importFile.FileContentRecognized.Split(Environment.NewLine);
            Assert.AreEqual(string.Format("{0} - {1}				{2}",
                string.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetArtist"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetTitle"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "c:\\tmp\\Testfile.mp3")), lines.First());
            Assert.AreEqual("CuesheetArtist", importFile.AnalyzedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importFile.AnalyzedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\Testfile.mp3", importFile.AnalyzedCuesheet.Audiofile);
            Assert.HasCount(0, importFile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual("Sample Artist 1 - Sample Title 1				00:05:00", lines.ElementAt(1));
            Assert.AreEqual("Sample Artist 2 - Sample Title 2				00:09:23", lines.ElementAt(2));
            Assert.AreEqual("Sample Artist 3 - Sample Title 3				00:15:54", lines.ElementAt(3));
            Assert.AreEqual("Sample Artist 4 - Sample Title 4				00:20:13", lines.ElementAt(4));
            Assert.AreEqual("Sample Artist 5 - Sample Title 5				00:24:54", lines.ElementAt(5));
            Assert.AreEqual("Sample Artist 6 - Sample Title 6				00:31:54", lines.ElementAt(6));
            Assert.AreEqual("Sample Artist 7 - Sample Title 7				00:45:54", lines.ElementAt(7));
            Assert.AreEqual("Sample Artist 8 - Sample Title 8				01:15:54", lines.ElementAt(8));
        }

        [TestMethod()]
        public async Task AnalyseAsync_CuesheetWithoutTracks_CreatesValidFileContentRecognizedAsync()
        {
            // Arrange
            var fileContent = @"CuesheetArtist - CuesheetTitle				c:\tmp\Testfile.mp3
Sample Artist 1 - Sample Title 1				00:05:00
Sample Artist 2 - Sample Title 2				00:09:23
Sample Artist 3 - Sample Title 3				00:15:54
Sample Artist 4 - Sample Title 4				00:20:13
Sample Artist 5 - Sample Title 5				00:24:54
Sample Artist 6 - Sample Title 6				00:31:54
Sample Artist 7 - Sample Title 7				00:45:54
Sample Artist 8 - Sample Title 8				01:15:54";
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = ImportOptions.DefaultSelectedImportprofile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.IsNotNull(importFile.FileContentRecognized);
            Assert.HasCount(8, importFile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual("Sample Artist 1", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importFile.AnalyzedCuesheet.Tracks.ElementAt(0).End);
            var lines = importFile.FileContentRecognized.Split(Environment.NewLine);
            Assert.AreEqual(string.Format("{0} - {1}				{2}",
                string.Format(CuesheetConstants.RecognizedMarkHTML, "Sample Artist 8"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "Sample Title 8"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "01:15:54")), lines.Last());
        }

        [TestMethod()]
        public async Task AnalyseAsync_TextfileBug233_CreatesValidFileContentRecognizedAsync()
        {
            // Arrange
            var profile = new Importprofile()
            {
                SchemeTracks = $"{nameof(ImportTrack.Artist)} - {nameof(ImportTrack.Title)}\t{nameof(ImportTrack.End)}"
            };
            var fileContent = File.ReadAllText("Resources/Textimport_Bug_#233.txt");
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.IsNotNull(importFile.FileContentRecognized);
            var lines = importFile.FileContentRecognized.Split(Environment.NewLine);
            Assert.AreEqual(string.Format("{0} - {1}\t\t\t\t\t\t\t\t{2}",
                string.Format(CuesheetConstants.RecognizedMarkHTML, "Age Of Love"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "The Age Of Love (Charlotte De Witte & Enrico Sangiuliano Remix)"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "04:29:28")), lines.ElementAt(53));
        }

        [TestMethod()]
        public async Task AnalyseAsync_WithRegularExpression_ReturnsCuesheetAsync()
        {
            // Arrange
            var profile = new Importprofile()
            {
                UseRegularExpression = true,
                SchemeTracks = "<tr>\\s*<td>\\d+</td>\\s*<td>(?<Artist>.*?)</td>\\s*<td>(?<Title>.*?)</td>\\s*<td>(?<StartDateTime>.*?)</td>\\s*</tr>"
            };
            var fileContent = File.ReadAllText("Resources/Traktor Export.html");
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.AreEqual(fileContent, importFile.FileContent);
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.IsNull(importFile.AnalyzedCuesheet.Artist);
            Assert.IsNull(importFile.AnalyzedCuesheet.Title);
            Assert.HasCount(41, importFile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual("Nachap", importFile.AnalyzedCuesheet.Tracks.First().Artist);
            Assert.AreEqual("Glass", importFile.AnalyzedCuesheet.Tracks.First().Title);
            Assert.AreEqual(new DateTime(2025, 1, 29, 18, 52, 10), importFile.AnalyzedCuesheet.Tracks.First().StartDateTime);
            Assert.AreEqual("Inache", importFile.AnalyzedCuesheet.Tracks.Last().Artist);
            Assert.AreEqual("Andale (MONTA (TN) Remix)", importFile.AnalyzedCuesheet.Tracks.Last().Title);
            Assert.AreEqual(new DateTime(2025, 1, 29, 22, 30, 3), importFile.AnalyzedCuesheet.Tracks.Last().StartDateTime);
            Assert.IsNotNull(importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Sasha Fashion"), importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "2025/1/29 21:48:55"), importFile.FileContentRecognized);
        }

        [TestMethod()]
        public async Task AnalyseAsync_WithoutRegularExpression_ReturnsCuesheetAsync()
        {
            // Arrange
            var profile = new Importprofile()
            {
                UseRegularExpression = false,
                SchemeCuesheet = "Artist - Title - Cataloguenumber",
                SchemeTracks = "Artist - Title\tBegin"
            };
            var fileContent = File.ReadAllText("Resources/Textimport with Cuesheetdata.txt");
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.AreEqual("DJFreezeT", importFile.AnalyzedCuesheet.Artist);
            Assert.AreEqual("Rabbit Hole Mix", importFile.AnalyzedCuesheet.Title);
            Assert.AreEqual("0123456789123", importFile.AnalyzedCuesheet.Cataloguenumber);
            Assert.AreEqual("Adriatique", importFile.AnalyzedCuesheet.Tracks.First().Artist);
            Assert.AreEqual("X.", importFile.AnalyzedCuesheet.Tracks.First().Title);
            Assert.AreEqual(new TimeSpan(0, 0, 5, 24, 250), importFile.AnalyzedCuesheet.Tracks.First().Begin);
            Assert.AreEqual("Nikolay Kirov", importFile.AnalyzedCuesheet.Tracks.Last().Artist);
            Assert.AreEqual("Chasing the Sun (Original Mix)", importFile.AnalyzedCuesheet.Tracks.Last().Title);
            Assert.IsNull(importFile.AnalyzedCuesheet.Tracks.Last().Begin);
            Assert.IsNotNull(importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "DJFreezeT"), importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Rabbit Hole Mix"), importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "0123456789123"), importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Nikolay Kirov"), importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Chasing the Sun (Original Mix)"), importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "SHDW & Obscure Shape"), importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Wächter der Nacht (Original Mix)"), importFile.FileContentRecognized);
        }

        [TestMethod()]
        public async Task AnalyseAsync_TextfileWithStartDateTime_CreatesValidCuesheetAsync()
        {
            // Arrange
            var fileContent = $@"Innellea~The Golden Fort~{new DateTime(2024, 8, 14, 20, 10, 48)}
Nora En Pure~Diving with Whales (Daniel Portman Remix)~{new DateTime(2024, 8, 14, 20, 15, 21)}
WhoMadeWho & Adriatique~Miracle (RÜFÜS DU SOL Remix)~{new DateTime(2024, 8, 14, 20, 20, 42)}
Ella Wild~Poison D'araignee (Original Mix)~{new DateTime(2024, 8, 14, 20, 28, 03)}
Stil & Bense~On The Edge (Original Mix)~{new DateTime(2024, 8, 14, 20, 32, 42)}
Nebula~Clairvoyant Dreams~{new DateTime(2024, 8, 14, 20, 39, 1)}
Valentina Black~I'm a Tree (Extended Mix)~{new DateTime(2024, 8, 14, 20, 47, 08)}
Nebula~Clairvoyant Dreams~{new DateTime(2024, 8, 14, 20, 53, 20)}
Kiko & Dave Davis feat. Phoebe~Living in Space (Dub Mix)~{new DateTime(2024, 8, 14, 20, 58, 11)}
Lilly Palmer~Before Acid~{new DateTime(2024, 8, 14, 21, 03, 53)}
Sofi Tukker~Drinkee (Vintage Culture & John Summit Extended Mix)~{new DateTime(2024, 8, 14, 21, 09, 52)}
CID & Truth x Lies~Caroline (Extended Mix)~{new DateTime(2024, 8, 14, 21, 14, 09)}
Moby~Why Does My Heart Feel So Bad? (Oxia Remix)~{new DateTime(2024, 8, 14, 21, 17, 15)}
Ammo Avenue~Little Gurl (Extended Mix)~{new DateTime(2024, 8, 14, 21, 22, 46)}
James Hurr & Smokin Jo & Stealth~Beggin' For Change~{new DateTime(2024, 8, 14, 21, 28, 37)}
Kristine Blond~Love Shy (Sam Divine & CASSIMM Extended Remix)~{new DateTime(2024, 8, 14, 21, 30, 47)}
Vanilla Ace~Work On You (Original Mix)~{new DateTime(2024, 8, 14, 21, 36, 28)}
Truth X Lies~Like This~{new DateTime(2024, 8, 14, 21, 42, 05)}
Terri-Anne~Round Round~{new DateTime(2024, 8, 14, 21, 44, 07)}
Joanna Magik~Maneater~{new DateTime(2024, 8, 14, 21, 46, 32)}
Jen Payne & Kevin McKay~Feed Your Soul~1{new DateTime(2024, 8, 14, 21, 48, 45)}
Kevin McKay & Eppers & Notelle~On My Own~{new DateTime(2024, 8, 14, 21, 51, 37)}
Nader Razdar & Kevin McKay~Get Ur Freak On (Kevin McKay Extended Mix)~{new DateTime(2024, 8, 14, 21, 53, 49)}
Philip Z~Yala (Extended Mix)~{new DateTime(2024, 8, 14, 21, 59, 40)}
Kyle Kinch & Kevin McKay~Hella~{new DateTime(2024, 8, 14, 22, 05, 53)}
Roze Wild~B-O-D-Y~{new DateTime(2024, 8, 14, 22, 08, 26)}
Jey Kurmis~Snoop~{new DateTime(2024, 8, 14, 22, 11, 09)}
Bootie Brown & Tame Impala & Gorillaz~New Gold (Dom Dolla Remix Extended)~{new DateTime(2024, 8, 14, 22, 16, 23)}
Eli Brown & Love Regenerator~Don't You Want Me (Original Mix)~{new DateTime(2024, 8, 14, 22, 21, 23)}
Local Singles~Voices~{new DateTime(2024, 8, 14, 22, 25, 59)}";

            var profile = new Importprofile()
            {
                UseRegularExpression = false,
                SchemeTracks = $"{nameof(ImportTrack.Artist)}~{nameof(ImportTrack.Title)}~{nameof(ImportTrack.StartDateTime)}"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.HasCount(30, importFile.AnalyzedCuesheet.Tracks);
            Assert.AreEqual("Innellea", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("The Golden Fort", importFile.AnalyzedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new DateTime(2024, 8, 14, 20, 10, 48), importFile.AnalyzedCuesheet.Tracks.ElementAt(0).StartDateTime);
            Assert.AreEqual("Nora En Pure", importFile.AnalyzedCuesheet.Tracks.ElementAt(1).Artist);
            Assert.AreEqual("Diving with Whales (Daniel Portman Remix)", importFile.AnalyzedCuesheet.Tracks.ElementAt(1).Title);
            Assert.AreEqual(new DateTime(2024, 8, 14, 20, 15, 21), importFile.AnalyzedCuesheet.Tracks.ElementAt(1).StartDateTime);
            Assert.AreEqual("Local Singles", importFile.AnalyzedCuesheet.Tracks.ElementAt(29).Artist);
            Assert.AreEqual("Voices", importFile.AnalyzedCuesheet.Tracks.ElementAt(29).Title);
            Assert.AreEqual(new DateTime(2024, 8, 14, 22, 25, 59), importFile.AnalyzedCuesheet.Tracks.ElementAt(29).StartDateTime);
        }

        [TestMethod()]
        public async Task AnalyseAsync_WithCommonDataMatchingMultipleLines_SetsCommonDataOnceAsync()
        {
            // Arrange
            var profile = new Importprofile()
            {
                UseRegularExpression = false,
                SchemeCuesheet = "Artist - Title\tAudiofile",
                SchemeTracks = "Artist - Title\tBegin"
            };
            var fileContent = File.ReadAllText("Resources/Sample_Inputfile.txt");
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ImportOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var applicationOptions = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(applicationOptions);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importFile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalyzedCuesheet);
            Assert.AreEqual("CuesheetArtist", importFile.AnalyzedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importFile.AnalyzedCuesheet.Title);
            Assert.AreEqual("c:\\AudioFile.mp3", importFile.AnalyzedCuesheet.Audiofile);
            Assert.HasCount(8, importFile.AnalyzedCuesheet.Tracks);
            Assert.IsNotNull(importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetArtist"), importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetTitle"), importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "c:\\AudioFile.mp3"), importFile.FileContentRecognized);
            Assert.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Sample Artist 8"), importFile.FileContentRecognized);
        }
    }
}