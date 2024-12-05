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
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Tests.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass()]
    public class CuesheetImportServiceTests
    {
        [TestMethod()]
        public void Analyse_WithSampleCuesheet_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "PERFORMER \"Sample CD Artist\"",
                "TITLE \"Sample CD Title\"",
                "FILE \"AC DC - TNT.mp3\" MP3",
                "CDTEXTFILE \"Testfile.cdt\"",
                "CATALOG 0123456789012",
                "TRACK 01 AUDIO",
                "	PERFORMER \"Sample Artist 1\"",
                "	TITLE \"Sample Title 1\"",
                "	INDEX 01 00:00:00",
                "TRACK 02 AUDIO",
                "	PERFORMER \"Sample Artist 2\"",
                "	TITLE \"Sample Title 2\"",
                "	INDEX 01 05:00:00",
                "TRACK 03 AUDIO",
                "	PERFORMER \"Sample Artist 3\"",
                "	TITLE \"Sample Title 3\"",
                "	INDEX 01 09:23:00",
                "TRACK 04 AUDIO",
                "	PERFORMER \"Sample Artist 4\"",
                "	TITLE \"Sample Title 4\"",
                "	INDEX 01 15:54:00",
                "TRACK 05 AUDIO",
                "	PERFORMER \"Sample Artist 5\"",
                "	TITLE \"Sample Title 5\"",
                "	INDEX 01 20:13:00",
                "TRACK 06 AUDIO",
                "	PERFORMER \"Sample Artist 6\"",
                "	TITLE \"Sample Title 6\"",
                "	INDEX 01 24:54:00",
                "TRACK 07 AUDIO",
                "	PERFORMER \"Sample Artist 7\"",
                "	TITLE \"Sample Title 7\"",
                "	INDEX 01 31:54:00",
                "TRACK 08 AUDIO",
                "	PERFORMER \"Sample Artist 8\"",
                "	TITLE \"Sample Title 8\"",
                "	INDEX 01 45:51:00"
            };
            // Act
            var importFile = CuesheetImportService.Analyse(fileContent);
            // Assert
            Assert.IsNotNull(importFile);
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalysedCuesheet);
            Assert.AreEqual(8, importFile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual(string.Format(CuesheetConstants.RecognizedMarkHTML, "PERFORMER \"Sample CD Artist\""), importFile.FileContentRecognized?.ElementAt(0));
            Assert.AreEqual(string.Format(CuesheetConstants.RecognizedMarkHTML, "TITLE \"Sample CD Title\""), importFile.FileContentRecognized?.ElementAt(1));
            Assert.AreEqual(string.Format(CuesheetConstants.RecognizedMarkHTML, "FILE \"AC DC - TNT.mp3\" MP3"), importFile.FileContentRecognized?.ElementAt(2));
            Assert.AreEqual(string.Format(CuesheetConstants.RecognizedMarkHTML, "CDTEXTFILE \"Testfile.cdt\""), importFile.FileContentRecognized?.ElementAt(3));
            Assert.AreEqual(string.Format(CuesheetConstants.RecognizedMarkHTML, "CATALOG 0123456789012"), importFile.FileContentRecognized?.ElementAt(4));
            Assert.AreEqual(string.Format(CuesheetConstants.RecognizedMarkHTML, "TRACK 01 AUDIO"), importFile.FileContentRecognized?.ElementAt(5));
            Assert.AreEqual(string.Format("	{0}", string.Format(CuesheetConstants.RecognizedMarkHTML, "PERFORMER \"Sample Artist 1\"")), importFile.FileContentRecognized?.ElementAt(6));
            Assert.AreEqual(string.Format("	{0}", string.Format(CuesheetConstants.RecognizedMarkHTML, "TITLE \"Sample Title 1\"")), importFile.FileContentRecognized?.ElementAt(7));
            Assert.AreEqual(string.Format("	{0}", string.Format(CuesheetConstants.RecognizedMarkHTML, "INDEX 01 00:00:00")), importFile.FileContentRecognized?.ElementAt(8));
        }

        [TestMethod()]
        public void Analyse_WithCuesheetBug30_CreatesValidCuesheet()
        {
            //Arrange
            var textImportMemoryStream = new MemoryStream(Resources.Playlist_Bug_30);
            using var reader = new StreamReader(textImportMemoryStream);
            List<string?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            //Act
            var importFile = CuesheetImportService.Analyse(fileContent);
            //Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalysedCuesheet);
        }

        [TestMethod()]
        public void Analyse_WithCuesheetBug57_CreatesValidCuesheet()
        {
            //Arrange
            var textImportMemoryStream = new MemoryStream(Resources.Playlist_Bug_57);
            using var reader = new StreamReader(textImportMemoryStream);
            List<string?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            //Act
            var importFile = CuesheetImportService.Analyse(fileContent);
            //Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalysedCuesheet);
            Assert.AreEqual(39, importFile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(2, 8, 21), importFile.AnalysedCuesheet.Tracks.ElementAt(24).Begin);
        }

        [TestMethod()]
        public void Analyse_WithCuesheetBug36_CreatesValidCuesheet()
        {
            //Arrange
            var textImportMemoryStream = new MemoryStream(Resources.Playlist__36_Frames);
            using var reader = new StreamReader(textImportMemoryStream);
            List<string?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            //Act
            var importFile = CuesheetImportService.Analyse(fileContent);
            //Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalysedCuesheet);
            Assert.AreEqual(12, importFile.AnalysedCuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(0, 0, 9, 15, 600), importFile.AnalysedCuesheet.Tracks.ElementAt(2).Begin);
        }

        [TestMethod()]
        public void Analyse_WithCDTextFileCatalogueNumberAndPreAndPostGap_CreatesValidCuesheet()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "PERFORMER \"Sample CD Artist\"",
                "TITLE \"Sample CD Title\"",
                "FILE \"AC DC - TNT.mp3\" MP3",
                "CDTEXTFILE \"Testfile.cdt\"",
                "CATALOG 0123456789012",
                "TRACK 01 AUDIO",
                "	PERFORMER \"Sample Artist 1\"",
                "	TITLE \"Sample Title 1\"",
                "	FLAGS 4CH DCP PRE SCMS",
                "	INDEX 01 00:00:00",
                "TRACK 02 AUDIO",
                "	PERFORMER \"Sample Artist 2\"",
                "	TITLE \"Sample Title 2\"",
                "	FLAGS DCP PRE",
                "	INDEX 01 05:00:00",
                "TRACK 03 AUDIO",
                "	PERFORMER \"Sample Artist 3\"",
                "	TITLE \"Sample Title 3\"",
                "	INDEX 01 09:23:00",
                "TRACK 04 AUDIO",
                "	PERFORMER \"Sample Artist 4\"",
                "	TITLE \"Sample Title 4\"",
                "	INDEX 01 15:54:00",
                "TRACK 05 AUDIO",
                "	PERFORMER \"Sample Artist 5\"",
                "	TITLE \"Sample Title 5\"",
                "	INDEX 01 20:13:00",
                "	POSTGAP 00:02:00",
                "TRACK 06 AUDIO",
                "	PERFORMER \"Sample Artist 6\"",
                "	TITLE \"Sample Title 6\"",
                "	INDEX 01 24:54:00",
                "TRACK 07 AUDIO",
                "	PERFORMER \"Sample Artist 7\"",
                "	TITLE \"Sample Title 7\"",
                "	PREGAP 00:04:00",
                "	INDEX 01 31:54:00",
                "TRACK 08 AUDIO",
                "	PERFORMER \"Sample Artist 8\"",
                "	TITLE \"Sample Title 8\"",
                "	INDEX 01 45:51:00"
            };
            // Act
            var importFile = CuesheetImportService.Analyse(fileContent);
            // Assert
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.AnalysedCuesheet);
            Assert.AreEqual(string.Format("	{0}", string.Format(CuesheetConstants.RecognizedMarkHTML, "FLAGS 4CH DCP PRE SCMS")), importFile.FileContentRecognized?.ElementAt(8));
            Assert.AreEqual(string.Format("	{0}", string.Format(CuesheetConstants.RecognizedMarkHTML, "PREGAP 00:04:00")), importFile.FileContentRecognized?.ElementAt(35));
            Assert.AreEqual(8, importFile.AnalysedCuesheet.Tracks.Count);
            Assert.IsNotNull(importFile.AnalysedCuesheet.CDTextfile);
            Assert.AreEqual(4, importFile.AnalysedCuesheet.Tracks.ElementAt(0).Flags.Count);
            Assert.AreEqual(2, importFile.AnalysedCuesheet.Tracks.ElementAt(1).Flags.Count);
            Assert.IsNotNull(importFile.AnalysedCuesheet.Tracks.ElementAt(1).Flags.SingleOrDefault(x => x.CuesheetLabel == "DCP"));
            Assert.IsNotNull(importFile.AnalysedCuesheet.Tracks.ElementAt(1).Flags.SingleOrDefault(x => x.CuesheetLabel == "PRE"));
            Assert.AreEqual(new TimeSpan(0, 0, 2), importFile.AnalysedCuesheet.Tracks.ElementAt(4).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), importFile.AnalysedCuesheet.Tracks.ElementAt(6).PreGap);
        }
    }
}