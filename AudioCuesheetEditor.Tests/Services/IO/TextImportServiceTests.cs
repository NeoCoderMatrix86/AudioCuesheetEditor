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
using AudioCuesheetEditor.Tests.Properties;
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
            var options = new ImportOptions
            {
                SelectedImportProfile = ImportOptions.DefaultSelectedImportprofile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ImportOptions>()).ReturnsAsync(options);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual("CuesheetArtist", importfile.AnalysedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importfile.AnalysedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\Testfile.mp3", importfile.AnalysedCuesheet.Audiofile);
            Assert.AreEqual(8, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Sample Artist 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(0).End);
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual("CuesheetArtist", importfile.AnalysedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importfile.AnalysedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\TestTextFile.cdt", importfile.AnalysedCuesheet.CDTextfile);
            Assert.AreEqual(8, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual((uint)6, importfile.AnalysedCuesheet.Tracks.ElementAt(5).Position);
            Assert.AreEqual("Sample Artist 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(0).End);
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual("CuesheetArtist", importfile.AnalysedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importfile.AnalysedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\TestTextFile.cdt", importfile.AnalysedCuesheet.CDTextfile);
            Assert.AreEqual(8, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual((uint)6, importfile.AnalysedCuesheet.Tracks.ElementAt(5).Position);
            Assert.AreEqual("Sample Artist 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(0).End);
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNotNull(importfile.AnalyseException);
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual("CuesheetArtist", importfile.AnalysedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importfile.AnalysedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\TestTextFile.cdt", importfile.AnalysedCuesheet.CDTextfile);
            Assert.AreEqual("A83412346734", importfile.AnalysedCuesheet.Cataloguenumber);
            Assert.AreEqual(8, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual((uint)6, importfile.AnalysedCuesheet.Tracks.ElementAt(5).Position);
            Assert.AreEqual("Sample Artist 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(0).End);
        }

        [TestMethod()]
        public async Task AnalyseAsync_CuesheeTracksOnly_CreatesValidCuesheetAsync()
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual(9, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Sample Artist 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(1, 15, 54), importfile.AnalysedCuesheet.Tracks.ElementAt(7).End);
        }

        [TestMethod()]
        public async Task AnalyseAsync_CuesheetBug213_CreatesValidCuesheetAsync()
        {
            // Arrange
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_Bug_213);
            using var reader = new StreamReader(textImportMemoryStream);
            var fileContent = reader.ReadToEnd();
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual(4, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(2, 3, 23), importfile.AnalysedCuesheet.Tracks.ElementAt(3).End);
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual(8, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(0).Flags.Contains(Flag.DCP));
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(2).Flags.Contains(Flag.DCP));
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(2).Flags.Contains(Flag.PRE));
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(3).Flags.Contains(Flag.FourCH));
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(5).Flags.Contains(Flag.FourCH));
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(5).Flags.Contains(Flag.PRE));
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(5).Flags.Contains(Flag.DCP));
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(7).Flags.Contains(Flag.DCP));
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(7).Flags.Contains(Flag.PRE));
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(7).Flags.Contains(Flag.FourCH));
            Assert.IsTrue(importfile.AnalysedCuesheet.Tracks.ElementAt(7).Flags.Contains(Flag.SCMS));
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual(8, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(0, 0, 2), importfile.AnalysedCuesheet.Tracks.ElementAt(0).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(0).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), importfile.AnalysedCuesheet.Tracks.ElementAt(1).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(1).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(2).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 2), importfile.AnalysedCuesheet.Tracks.ElementAt(2).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(3).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 3), importfile.AnalysedCuesheet.Tracks.ElementAt(3).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(4).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), importfile.AnalysedCuesheet.Tracks.ElementAt(4).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(5).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 1), importfile.AnalysedCuesheet.Tracks.ElementAt(5).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(6).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(6).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 2), importfile.AnalysedCuesheet.Tracks.ElementAt(7).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(7).PostGap);
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.IsNotNull(importfile.FileContentRecognized);
            var lines = importfile.FileContentRecognized.Split(Environment.NewLine);
            Assert.AreEqual(string.Format("{0} - {1}				{2}",
                string.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetArtist"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetTitle"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "c:\\tmp\\Testfile.mp3")), lines.First());
            Assert.AreEqual("CuesheetArtist", importfile.AnalysedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importfile.AnalysedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\Testfile.mp3", importfile.AnalysedCuesheet.Audiofile);
            Assert.AreEqual(0, importfile.AnalysedCuesheet.Tracks.Count);
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.IsNotNull(importfile.FileContentRecognized);
            Assert.AreEqual(8, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Sample Artist 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(0).End);
            var lines = importfile.FileContentRecognized.Split(Environment.NewLine);
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
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_Bug__233);
            using var reader = new StreamReader(textImportMemoryStream);            
            var fileContent = reader.ReadToEnd();
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
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.IsNotNull(importfile.FileContentRecognized);
            var lines = importfile.FileContentRecognized.Split(Environment.NewLine);
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
            var textImportMemoryStream = new MemoryStream(Resources.Traktor_Export);
            var reader = new StreamReader(textImportMemoryStream);
            var fileContent = reader.ReadToEnd();
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
            Assert.AreEqual(fileContent, importfile.FileContent);
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.IsNull(importfile.AnalysedCuesheet.Artist);
            Assert.IsNull(importfile.AnalysedCuesheet.Title);
            Assert.AreEqual(41, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Nachap", importfile.AnalysedCuesheet.Tracks.First().Artist);
            Assert.AreEqual("Glass", importfile.AnalysedCuesheet.Tracks.First().Title);
            Assert.AreEqual(new DateTime(2025, 1, 29, 18, 52, 10), importfile.AnalysedCuesheet.Tracks.First().StartDateTime);
            Assert.AreEqual("Inache", importfile.AnalysedCuesheet.Tracks.Last().Artist);
            Assert.AreEqual("Andale (MONTA (TN) Remix)", importfile.AnalysedCuesheet.Tracks.Last().Title);
            Assert.AreEqual(new DateTime(2025, 1, 29, 22, 30, 3), importfile.AnalysedCuesheet.Tracks.Last().StartDateTime);
            Assert.IsNotNull(importfile.FileContentRecognized);
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Sasha Fashion")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "2025/1/29 21:48:55")));
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
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_with_Cuesheetdata);
            var reader = new StreamReader(textImportMemoryStream);
            var fileContent = reader.ReadToEnd();
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
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual("DJFreezeT", importfile.AnalysedCuesheet.Artist);
            Assert.AreEqual("Rabbit Hole Mix", importfile.AnalysedCuesheet.Title);
            Assert.AreEqual("0123456789123", importfile.AnalysedCuesheet.Cataloguenumber);
            Assert.AreEqual("Adriatique", importfile.AnalysedCuesheet.Tracks.First().Artist);
            Assert.AreEqual("X.", importfile.AnalysedCuesheet.Tracks.First().Title);
            Assert.AreEqual(new TimeSpan(0, 0, 5, 24, 250), importfile.AnalysedCuesheet.Tracks.First().Begin);
            Assert.AreEqual("Nikolay Kirov", importfile.AnalysedCuesheet.Tracks.Last().Artist);
            Assert.AreEqual("Chasing the Sun (Original Mix)", importfile.AnalysedCuesheet.Tracks.Last().Title);
            Assert.IsNull(importfile.AnalysedCuesheet.Tracks.Last().Begin);
            Assert.IsNotNull(importfile.FileContentRecognized);
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "DJFreezeT")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Rabbit Hole Mix")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "0123456789123")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Nikolay Kirov")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Chasing the Sun (Original Mix)")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "SHDW & Obscure Shape")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Wächter der Nacht (Original Mix)")));
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
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual(30, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Innellea", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("The Golden Fort", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new DateTime(2024, 8, 14, 20, 10, 48), importfile.AnalysedCuesheet.Tracks.ElementAt(0).StartDateTime);
            Assert.AreEqual("Nora En Pure", importfile.AnalysedCuesheet.Tracks.ElementAt(1).Artist);
            Assert.AreEqual("Diving with Whales (Daniel Portman Remix)", importfile.AnalysedCuesheet.Tracks.ElementAt(1).Title);
            Assert.AreEqual(new DateTime(2024, 8, 14, 20, 15, 21), importfile.AnalysedCuesheet.Tracks.ElementAt(1).StartDateTime);
            Assert.AreEqual("Local Singles", importfile.AnalysedCuesheet.Tracks.ElementAt(29).Artist);
            Assert.AreEqual("Voices", importfile.AnalysedCuesheet.Tracks.ElementAt(29).Title);
            Assert.AreEqual(new DateTime(2024, 8, 14, 22, 25, 59), importfile.AnalysedCuesheet.Tracks.ElementAt(29).StartDateTime);
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
            var textImportMemoryStream = new MemoryStream(Resources.Sample_Inputfile);
            var reader = new StreamReader(textImportMemoryStream);
            var fileContent = reader.ReadToEnd();
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
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual("CuesheetArtist", importfile.AnalysedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importfile.AnalysedCuesheet.Title);
            Assert.AreEqual("c:\\AudioFile.mp3", importfile.AnalysedCuesheet.Audiofile);
            Assert.AreEqual(8, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.IsNotNull(importfile.FileContentRecognized);
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetArtist")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetTitle")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "c:\\AudioFile.mp3")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Sample Artist 8")));
        }
    }
}