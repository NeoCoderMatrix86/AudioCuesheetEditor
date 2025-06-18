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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass()]
    public class TextImportServiceTests
    {
        [TestMethod()]
        public void Analyse_SampleCuesheet_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "CuesheetArtist - CuesheetTitle				c:\\tmp\\Testfile.mp3",
                "Sample Artist 1 - Sample Title 1				00:05:00",
                "Sample Artist 2 - Sample Title 2				00:09:23",
                "Sample Artist 3 - Sample Title 3				00:15:54",
                "Sample Artist 4 - Sample Title 4				00:20:13",
                "Sample Artist 5 - Sample Title 5				00:24:54",
                "Sample Artist 6 - Sample Title 6				00:31:54",
                "Sample Artist 7 - Sample Title 7				00:45:54",
                "Sample Artist 8 - Sample Title 8				01:15:54"
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet,
                SchemeTracks = TextImportScheme.DefaultSchemeTracks
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
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
        public void Analyse_InvalidSchemeTracks_CreatesAnalyseException()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "CuesheetArtist|CuesheetTitle				c:\\tmp\\TestTextFile.cdt",
                "1|Sample Artist 1 - Sample Title 1				00:05:00",
                "2|Sample Artist 2 - Sample Title 2				00:09:23",
                "3|Sample Artist 3 - Sample Title 3				00:15:54",
                "4|Sample Artist 4 - Sample Title 4				00:20:13",
                "5|Sample Artist 5 - Sample Title 5				00:24:54",
                "6|Sample Artist 6 - Sample Title 6				00:31:54",
                "7|Sample Artist 7 - Sample Title 7				00:45:54",
                "8|Sample Artist 8 - Sample Title 8				01:15:54"
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = @"(?'Cuesheet.Artist'\A.*)[|](?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.CDTextfile'.{1,})",
                SchemeTracks = @"(?'Track.Position'.{1,})|(?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})"
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
            // Assert
            Assert.IsNotNull(importfile.AnalyseException);
        }

        [TestMethod()]
        public void Analyse_InputfileWithExtraSeperator_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "CuesheetArtist|CuesheetTitle				c:\\tmp\\TestTextFile.cdt",
                "1|Sample Artist 1 - Sample Title 1				00:05:00",
                "2|Sample Artist 2 - Sample Title 2				00:09:23",
                "3|Sample Artist 3 - Sample Title 3				00:15:54",
                "4|Sample Artist 4 - Sample Title 4				00:20:13",
                "5|Sample Artist 5 - Sample Title 5				00:24:54",
                "6|Sample Artist 6 - Sample Title 6				00:31:54",
                "7|Sample Artist 7 - Sample Title 7				00:45:54",
                "8|Sample Artist 8 - Sample Title 8				01:15:54"
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = @"(?'Artist'\A.*)[|](?'Title'\w{1,})\t{1,}(?'CDTextfile'.{1,})",
                SchemeTracks = @"(?'Position'\d{1,})[|](?'Artist'.{1,}) - (?'Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'End'.{1,})"
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
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
        public void Analyse_InputfileWithSimplifiedScheme_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "CuesheetArtist|CuesheetTitle				c:\\tmp\\TestTextFile.cdt",
                "1|Sample Artist 1 - Sample Title 1				00:05:00",
                "2|Sample Artist 2 - Sample Title 2				00:09:23",
                "3|Sample Artist 3 - Sample Title 3				00:15:54",
                "4|Sample Artist 4 - Sample Title 4				00:20:13",
                "5|Sample Artist 5 - Sample Title 5				00:24:54",
                "6|Sample Artist 6 - Sample Title 6				00:31:54",
                "7|Sample Artist 7 - Sample Title 7				00:45:54",
                "8|Sample Artist 8 - Sample Title 8				01:15:54"
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = @"Artist|Title	CDTextfile",
                SchemeTracks = @"Position|Artist - Title	End"
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
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
        public void Analyse_InvalidScheme_CreatesAnalyseException()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "CuesheetArtist|CuesheetTitle				c:\\tmp\\TestTextFile.cdt				A83412346734",
                "1|Sample Artist 1 - Sample Title 1				00:05:00",
                "2|Sample Artist 2 - Sample Title 2				00:09:23",
                "3|Sample Artist 3 - Sample Title 3				00:15:54",
                "4|Sample Artist 4 - Sample Title 4				00:20:13",
                "5|Sample Artist 5 - Sample Title 5				00:24:54",
                "6|Sample Artist 6 - Sample Title 6				00:31:54",
                "7|Sample Artist 7 - Sample Title 7				00:45:54",
                "8|Sample Artist 8 - Sample Title 8				01:15:54",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = @"(?'Cuesheet.Artist'\A.*)[|](?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.CDTextfile'[a-zA-Z0-9_ .();äöü&:,\\]{1,})\t{1,}(?'Cuesheet.Cataloguenumber'.{1,})",
                SchemeTracks = @"(?'Track.Position'.{1,})|(?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})"
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
            // Assert
            Assert.IsNotNull(importfile.AnalyseException);
        }

        [TestMethod()]
        public void Analyse_CuesheetWithTextfileAndCatalogueNumber_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "CuesheetArtist|CuesheetTitle				c:\\tmp\\TestTextFile.cdt				A83412346734",
                "1|Sample Artist 1 - Sample Title 1				00:05:00",
                "2|Sample Artist 2 - Sample Title 2				00:09:23",
                "3|Sample Artist 3 - Sample Title 3				00:15:54",
                "4|Sample Artist 4 - Sample Title 4				00:20:13",
                "5|Sample Artist 5 - Sample Title 5				00:24:54",
                "6|Sample Artist 6 - Sample Title 6				00:31:54",
                "7|Sample Artist 7 - Sample Title 7				00:45:54",
                "8|Sample Artist 8 - Sample Title 8				01:15:54",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = @"(?'Artist'\A.*)[|](?'Title'\w{1,})\t{1,}(?'CDTextfile'[a-zA-Z0-9_ .();äöü&:,\\]{1,})\t{1,}(?'Cataloguenumber'.{1,})",
                SchemeTracks = @"(?'Position'.{1,})[|](?'Artist'.{1,}) - (?'Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'End'.{1,})"
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
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
        public void Analyse_CuesheeTracksOnly_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "Sample Artist 1 - Sample Title 1				00:05:00",
                "Sample Artist 2 - Sample Title 2				00:09:23",
                "Sample Artist 3 - Sample Title 3				00:15:54",
                "Sample Artist 4 - Sample Title 4				00:20:13",
                "Sample Artist 5 - Sample Title 5				00:24:54",
                "Sample Artist 6 - Sample Title 6				00:31:54",
                "Sample Artist 7 - Sample Title 7				00:45:54",
                "Sample Artist 8 - Sample Title 8				01:15:54",
                "Sample Artist 9 - Sample Title 9"
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = null,
                SchemeTracks = TextImportScheme.DefaultSchemeTracks
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual(9, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Sample Artist 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(1, 15, 54), importfile.AnalysedCuesheet.Tracks.ElementAt(7).End);
        }

        [TestMethod()]
        public void Analyse_CuesheetBug213_CreatesValidCuesheet()
        {
            // Arrange
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_Bug_213);
            using var reader = new StreamReader(textImportMemoryStream);
            List<string?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            var timeSpanFormat = new TimeSpanFormat() { Scheme = "Minutes:Seconds" };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = null,
                SchemeTracks = TextImportScheme.DefaultSchemeTracks
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent, timeSpanFormat);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual(4, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(2, 3, 23), importfile.AnalysedCuesheet.Tracks.ElementAt(3).End);
        }

        [TestMethod()]
        public void Analyse_CuesheetWithFlags_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "Sample Artist 1 - Sample Title 1				00:05:00	DCP",
                "Sample Artist 2 - Sample Title 2				00:09:23",
                "Sample Artist 3 - Sample Title 3				00:15:54	PRE, DCP",
                "Sample Artist 4 - Sample Title 4				00:20:13	4CH",
                "Sample Artist 5 - Sample Title 5				00:24:54",
                "Sample Artist 6 - Sample Title 6				00:31:54	PRE DCP 4CH",
                "Sample Artist 7 - Sample Title 7				00:45:54",
                "Sample Artist 8 - Sample Title 8				01:15:54	PRE DCP 4CH SCMS"
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = null,
                SchemeTracks = "(?'Artist'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Title'[a-zA-Z0-9_ .();äöü]{1,})\t{1,}(?'End'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'Flags'[a-zA-Z 0-9,]{1,})"
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
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
        public void Analyse_CuesheetWithPreGapAndPostGap_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "Sample Artist 1 - Sample Title 1		00:00:02		00:05:00		00:00:00",
                "Sample Artist 2 - Sample Title 2		00:00:04		00:09:23		00:00:00",
                "Sample Artist 3 - Sample Title 3		00:00:00		00:15:54		00:00:02",
                "Sample Artist 4 - Sample Title 4		00:00:00		00:20:13		00:00:03",
                "Sample Artist 5 - Sample Title 5		00:00:00		00:24:54		00:00:04",
                "Sample Artist 6 - Sample Title 6		00:00:00		00:31:54		00:00:01",
                "Sample Artist 7 - Sample Title 7		00:00:00		00:45:54		00:00:00",
                "Sample Artist 8 - Sample Title 8		00:00:02		01:15:54		00:00:00"
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = null,
                SchemeTracks = "(?'Artist'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Title'[a-zA-Z0-9_ .();äöü]{1,})\t{1,}(?'PreGap'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'End'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'PostGap'[0-9]{2}[:][0-9]{2}[:][0-9]{2})"
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
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
        public void Analyse_SchemeCuesheetOnly_CreatesFileContentRecognizedOnlyForCuesheet()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "CuesheetArtist - CuesheetTitle				c:\\tmp\\Testfile.mp3",
                "Sample Artist 1 - Sample Title 1				00:05:00",
                "Sample Artist 2 - Sample Title 2				00:09:23",
                "Sample Artist 3 - Sample Title 3				00:15:54",
                "Sample Artist 4 - Sample Title 4				00:20:13",
                "Sample Artist 5 - Sample Title 5				00:24:54",
                "Sample Artist 6 - Sample Title 6				00:31:54",
                "Sample Artist 7 - Sample Title 7				00:45:54",
                "Sample Artist 8 - Sample Title 8				01:15:54"
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet,
                SchemeTracks = null
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.IsNotNull(importfile.FileContentRecognizedLines);
            Assert.AreEqual(string.Format("{0} - {1}				{2}",
                string.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetArtist"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetTitle"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "c:\\tmp\\Testfile.mp3")), importfile.FileContentRecognizedLines.First());
            Assert.AreEqual("CuesheetArtist", importfile.AnalysedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importfile.AnalysedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\Testfile.mp3", importfile.AnalysedCuesheet.Audiofile);
            Assert.AreEqual(0, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Sample Artist 1 - Sample Title 1				00:05:00", importfile.FileContentRecognizedLines.ElementAt(1));
            Assert.AreEqual("Sample Artist 2 - Sample Title 2				00:09:23", importfile.FileContentRecognizedLines.ElementAt(2));
            Assert.AreEqual("Sample Artist 3 - Sample Title 3				00:15:54", importfile.FileContentRecognizedLines.ElementAt(3));
            Assert.AreEqual("Sample Artist 4 - Sample Title 4				00:20:13", importfile.FileContentRecognizedLines.ElementAt(4));
            Assert.AreEqual("Sample Artist 5 - Sample Title 5				00:24:54", importfile.FileContentRecognizedLines.ElementAt(5));
            Assert.AreEqual("Sample Artist 6 - Sample Title 6				00:31:54", importfile.FileContentRecognizedLines.ElementAt(6));
            Assert.AreEqual("Sample Artist 7 - Sample Title 7				00:45:54", importfile.FileContentRecognizedLines.ElementAt(7));
            Assert.AreEqual("Sample Artist 8 - Sample Title 8				01:15:54", importfile.FileContentRecognizedLines.ElementAt(8));
        }

        [TestMethod()]
        public void Analyse_CuesheetWithoutTracks_CreatesValidFileContentRecognized()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "CuesheetArtist - CuesheetTitle				c:\\tmp\\Testfile.mp3",
                "Sample Artist 1 - Sample Title 1				00:05:00",
                "Sample Artist 2 - Sample Title 2				00:09:23",
                "Sample Artist 3 - Sample Title 3				00:15:54",
                "Sample Artist 4 - Sample Title 4				00:20:13",
                "Sample Artist 5 - Sample Title 5				00:24:54",
                "Sample Artist 6 - Sample Title 6				00:31:54",
                "Sample Artist 7 - Sample Title 7				00:45:54",
                "Sample Artist 8 - Sample Title 8				01:15:54"
            };
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet,
                SchemeTracks = TextImportScheme.DefaultSchemeTracks
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.IsNotNull(importfile.FileContentRecognizedLines);
            Assert.AreEqual(8, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Sample Artist 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(0).End);
            Assert.AreEqual(string.Format("{0} - {1}				{2}",
                string.Format(CuesheetConstants.RecognizedMarkHTML, "Sample Artist 8"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "Sample Title 8"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "01:15:54")), importfile.FileContentRecognizedLines.Last());
        }

        [TestMethod()]
        public void Analyse_TextfileBug233_CreatesValidFileContentRecognized()
        {
            // Arrange
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_Bug__233);
            using var reader = new StreamReader(textImportMemoryStream);
            List<string?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = null,
                SchemeTracks = TextImportScheme.DefaultSchemeTracks
            };
            // Act
            var importfile = TextImportService.Analyse(textImportScheme, fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.IsNotNull(importfile.FileContentRecognizedLines);
            Assert.AreEqual(string.Format("{0} - {1}\t\t\t\t\t\t\t\t{2}",
                string.Format(CuesheetConstants.RecognizedMarkHTML, "Age Of Love"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "The Age Of Love (Charlotte De Witte & Enrico Sangiuliano Remix)"),
                string.Format(CuesheetConstants.RecognizedMarkHTML, "04:29:28")), importfile.FileContentRecognizedLines.ElementAt(53));
        }

        [TestMethod()]
        public async Task AnalyseAsync_WithRegularExpression_ReturnsCuesheetAsync()
        {
            // Arrange
            var profile = new Importprofile()
            {
                Id = Guid.NewGuid(),
                UseRegularExpression = true,
                SchemeTracks = "<tr>\\s*<td>\\d+</td>\\s*<td>(?<Artist>.*?)</td>\\s*<td>(?<Title>.*?)</td>\\s*<td>(?<StartDateTime>.*?)</td>\\s*</tr>"
            };
            var textImportMemoryStream = new MemoryStream(Resources.Traktor_Export);
            var reader = new StreamReader(textImportMemoryStream);
            var fileContent = reader.ReadToEnd();
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ApplicationOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(options);
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
                Id = Guid.NewGuid(),
                UseRegularExpression = false,
                SchemeCuesheet = "Artist - Title - Cataloguenumber",
                SchemeTracks = "Artist - Title\tBegin"
            };
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_with_Cuesheetdata);
            var reader = new StreamReader(textImportMemoryStream);
            var fileContent = reader.ReadToEnd();
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ApplicationOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(options);
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
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Nikolay Kirov")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Chasing the Sun (Original Mix)")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "SHDW & Obscure Shape")));
            Assert.IsTrue(importfile.FileContentRecognized.Contains(String.Format(CuesheetConstants.RecognizedMarkHTML, "Wächter der Nacht (Original Mix)")));
        }

        [TestMethod()]
        public async Task AnalyseAsync_TextfileWithStartDateTime_CreatesValidCuesheetAsync()
        {
            // Arrange
            var fileContent = @"Innellea~The Golden Fort~02.08.2024 20:10:48
Nora En Pure~Diving with Whales (Daniel Portman Remix)~02.08.2024 20:15:21
WhoMadeWho & Adriatique~Miracle (RÜFÜS DU SOL Remix)~02.08.2024 20:20:42
Ella Wild~Poison D'araignee (Original Mix)~02.08.2024 20:28:03
Stil & Bense~On The Edge (Original Mix)~02.08.2024 20:32:42
Nebula~Clairvoyant Dreams~02.08.2024 20:39:01
Valentina Black~I'm a Tree (Extended Mix)~02.08.2024 20:47:08
Nebula~Clairvoyant Dreams~02.08.2024 20:53:20
Kiko & Dave Davis feat. Phoebe~Living in Space (Dub Mix)~02.08.2024 20:58:11
Lilly Palmer~Before Acid~02.08.2024 21:03:53
Sofi Tukker~Drinkee (Vintage Culture & John Summit Extended Mix)~02.08.2024 21:09:52
CID & Truth x Lies~Caroline (Extended Mix)~02.08.2024 21:14:09
Moby~Why Does My Heart Feel So Bad? (Oxia Remix)~02.08.2024 21:17:15
Ammo Avenue~Little Gurl (Extended Mix)~02.08.2024 21:22:46
James Hurr & Smokin Jo & Stealth~Beggin' For Change~02.08.2024 21:28:37
Kristine Blond~Love Shy (Sam Divine & CASSIMM Extended Remix)~02.08.2024 21:30:47
Vanilla Ace~Work On You (Original Mix)~02.08.2024 21:36:28
Truth X Lies~Like This~02.08.2024 21:42:05
Terri-Anne~Round Round~02.08.2024 21:44:07
Joanna Magik~Maneater~02.08.2024 21:46:32
Jen Payne & Kevin McKay~Feed Your Soul~02.08.2024 21:48:45
Kevin McKay & Eppers & Notelle~On My Own~02.08.2024 21:51:37
Nader Razdar & Kevin McKay~Get Ur Freak On (Kevin McKay Extended Mix)~02.08.2024 21:53:49
Philip Z~Yala (Extended Mix)~02.08.2024 21:59:40
Kyle Kinch & Kevin McKay~Hella~02.08.2024 22:05:53
Roze Wild~B-O-D-Y~02.08.2024 22:08:26
Jey Kurmis~Snoop~02.08.2024 22:11:09
Bootie Brown & Tame Impala & Gorillaz~New Gold (Dom Dolla Remix Extended)~02.08.2024 22:16:23
Eli Brown & Love Regenerator~Don't You Want Me (Original Mix)~02.08.2024 22:21:23
Local Singles~Voices~02.08.2024 22:25:59";

            var profile = new Importprofile()
            {
                Id = Guid.NewGuid(),
                UseRegularExpression = false,
                SchemeTracks = $"{nameof(ImportTrack.Artist)}~{nameof(ImportTrack.Title)}~{nameof(ImportTrack.StartDateTime)}"
            };
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ApplicationOptions
            {
                SelectedImportProfile = profile
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptionsAsync<ApplicationOptions>()).ReturnsAsync(options);
            var service = new TextImportService(localStorageOptionsProviderMock.Object);
            // Act
            var importfile = await service.AnalyseAsync(fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.AreEqual(30, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Innellea", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("The Golden Fort", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new DateTime(2024, 8, 2, 20, 10, 48), importfile.AnalysedCuesheet.Tracks.ElementAt(0).StartDateTime);
            Assert.AreEqual("Nora En Pure", importfile.AnalysedCuesheet.Tracks.ElementAt(1).Artist);
            Assert.AreEqual("Diving with Whales (Daniel Portman Remix)", importfile.AnalysedCuesheet.Tracks.ElementAt(1).Title);
            Assert.AreEqual(new DateTime(2024, 8, 2, 20, 15, 21), importfile.AnalysedCuesheet.Tracks.ElementAt(1).StartDateTime);
            Assert.AreEqual("Local Singles", importfile.AnalysedCuesheet.Tracks.ElementAt(29).Artist);
            Assert.AreEqual("Voices", importfile.AnalysedCuesheet.Tracks.ElementAt(29).Title);
            Assert.AreEqual(new DateTime(2024, 8, 2, 22, 25, 59), importfile.AnalysedCuesheet.Tracks.ElementAt(29).StartDateTime);
        }
    }
}