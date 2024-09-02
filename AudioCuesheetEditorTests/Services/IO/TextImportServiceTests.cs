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
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.Utility;
using AudioCuesheetEditorTests.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AudioCuesheetEditor.Services.IO.Tests
{
    [TestClass()]
    public class TextImportServiceTests
    {
        [TestMethod()]
        public void Analyse_SampleCuesheet_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<String>
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
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet,
                    SchemeTracks = TextImportScheme.DefaultSchemeTracks
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
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
            var fileContent = new List<String>
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
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = @"(?'Cuesheet.Artist'\A.*)[|](?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.CDTextfile'.{1,})",
                    SchemeTracks = @"(?'Track.Position'.{1,})|(?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})"
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
            // Assert
            Assert.IsNotNull(importfile.AnalyseException);
        }

        [TestMethod()]
        public void Analyse_InputfileWithExtraSeperator_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<String>
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
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = @"(?'Cuesheet.Artist'\A.*)[|](?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.CDTextfile'.{1,})",
                    SchemeTracks = @"(?'Track.Position'\d{1,})[|](?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})"
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
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
            var fileContent = new List<String>
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
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty
            };
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = @"(?'Cuesheet.Artist'\A.*)[|](?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.CDTextfile'[a-zA-Z0-9_ .();äöü&:,\\]{1,})\t{1,}(?'Cuesheet.Cataloguenumber'.{1,})",
                    SchemeTracks = @"(?'Track.Position'.{1,})|(?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})"
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
            // Assert
            Assert.IsNotNull(importfile.AnalyseException);
        }

        [TestMethod()]
        public void Analyse_CuesheetWithTextfileAndCatalogueNumber_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<String>
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
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty
            };
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = @"(?'Cuesheet.Artist'\A.*)[|](?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.CDTextfile'[a-zA-Z0-9_ .();äöü&:,\\]{1,})\t{1,}(?'Cuesheet.Cataloguenumber'.{1,})",
                    SchemeTracks = @"(?'Track.Position'.{1,})[|](?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})"
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
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
            var fileContent = new List<String>
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
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = null,
                    SchemeTracks = TextImportScheme.DefaultSchemeTracks
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
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
            List<String?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            var importService = new TextImportService();
            var importOptions = new ImportOptions() 
            { 
                TimeSpanFormat = new TimeSpanFormat() { Scheme = "(?'TimeSpanFormat.Minutes'\\d{1,})[:](?'TimeSpanFormat.Seconds'\\d{1,})" },
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = null,
                    SchemeTracks = TextImportScheme.DefaultSchemeTracks
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
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
            var fileContent = new List<String>
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
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = null,
                    SchemeTracks = "(?'Track.Artist'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Track.Title'[a-zA-Z0-9_ .();äöü]{1,})\t{1,}(?'Track.End'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'Track.Flags'[a-zA-Z 0-9,]{1,})"
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
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
            var fileContent = new List<String>
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
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = null,
                    SchemeTracks = "(?'Track.Artist'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Track.Title'[a-zA-Z0-9_ .();äöü]{1,})\t{1,}(?'Track.PreGap'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'Track.End'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'Track.PostGap'[0-9]{2}[:][0-9]{2}[:][0-9]{2})"
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
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
            var fileContent = new List<String>
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
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet,
                    SchemeTracks = null
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.IsNotNull(importfile.FileContentRecognized);
            Assert.AreEqual(String.Format("{0} - {1}				{2}",
                String.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetArtist"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetTitle"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "c:\\tmp\\Testfile.mp3")), importfile.FileContentRecognized.First());
            Assert.AreEqual("CuesheetArtist", importfile.AnalysedCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", importfile.AnalysedCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\Testfile.mp3", importfile.AnalysedCuesheet.Audiofile);
            Assert.AreEqual(0, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Sample Artist 1 - Sample Title 1				00:05:00", importfile.FileContentRecognized.ElementAt(1));
            Assert.AreEqual("Sample Artist 2 - Sample Title 2				00:09:23", importfile.FileContentRecognized.ElementAt(2));
            Assert.AreEqual("Sample Artist 3 - Sample Title 3				00:15:54", importfile.FileContentRecognized.ElementAt(3));
            Assert.AreEqual("Sample Artist 4 - Sample Title 4				00:20:13", importfile.FileContentRecognized.ElementAt(4));
            Assert.AreEqual("Sample Artist 5 - Sample Title 5				00:24:54", importfile.FileContentRecognized.ElementAt(5));
            Assert.AreEqual("Sample Artist 6 - Sample Title 6				00:31:54", importfile.FileContentRecognized.ElementAt(6));
            Assert.AreEqual("Sample Artist 7 - Sample Title 7				00:45:54", importfile.FileContentRecognized.ElementAt(7));
            Assert.AreEqual("Sample Artist 8 - Sample Title 8				01:15:54", importfile.FileContentRecognized.ElementAt(8));
        }

        [TestMethod()]
        public void Analyse_CuesheetWithoutTracks_CreatesValidFileContentRecognized()
        {
            // Arrange
            var fileContent = new List<String>
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
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet,
                    SchemeTracks = TextImportScheme.DefaultSchemeTracks
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.IsNotNull(importfile.FileContentRecognized);
            Assert.AreEqual(8, importfile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual("Sample Artist 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("Sample Title 1", importfile.AnalysedCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(new TimeSpan(0, 5, 0), importfile.AnalysedCuesheet.Tracks.ElementAt(0).End);
            Assert.AreEqual(String.Format("{0} - {1}				{2}",
                String.Format(CuesheetConstants.RecognizedMarkHTML, "Sample Artist 8"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "Sample Title 8"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "01:15:54")), importfile.FileContentRecognized.Last());
        }

        [TestMethod()]
        public void Analyse_TextfileBug233_CreatesValidFileContentRecognized()
        {
            // Arrange
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_Bug__233);
            using var reader = new StreamReader(textImportMemoryStream);
            List<String?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            var importService = new TextImportService();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = null,
                    SchemeTracks = TextImportScheme.DefaultSchemeTracks
                }
            };
            // Act
            var importfile = importService.Analyse(importOptions, fileContent);
            // Assert
            Assert.IsNull(importfile.AnalyseException);
            Assert.IsNotNull(importfile.AnalysedCuesheet);
            Assert.IsNotNull(importfile.FileContentRecognized);
            Assert.AreEqual(String.Format("{0} - {1}\t\t\t\t\t\t\t\t{2}",
                String.Format(CuesheetConstants.RecognizedMarkHTML, "Age Of Love"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "The Age Of Love (Charlotte De Witte & Enrico Sangiuliano Remix)"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "04:29:28")), importfile.FileContentRecognized.ElementAt(53));
        }
    }
}