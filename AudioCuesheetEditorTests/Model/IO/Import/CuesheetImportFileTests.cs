﻿//This file is part of AudioCuesheetEditor.

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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.IO.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditorTests.Utility;
using System.IO;
using AudioCuesheetEditorTests.Properties;

namespace AudioCuesheetEditor.Model.IO.Import.Tests
{
    [TestClass()]
    public class CuesheetImportFileTests
    {
        [TestMethod()]
        public void CuesheetImportFileTest()
        {
            //Prepare text input file
            StringBuilder builder = new();
            builder.AppendLine("PERFORMER \"Sample CD Artist\"");
            builder.AppendLine("TITLE \"Sample CD Title\"");
            builder.AppendLine("FILE \"AC DC - TNT.mp3\" MP3");
            builder.AppendLine("CDTEXTFILE \"Testfile.cdt\"");
            builder.AppendLine("CATALOG 0123456789012");
            builder.AppendLine("TRACK 01 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 1\"");
            builder.AppendLine("	TITLE \"Sample Title 1\"");
            builder.AppendLine("	INDEX 01 00:00:00");
            builder.AppendLine("TRACK 02 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 2\"");
            builder.AppendLine("	TITLE \"Sample Title 2\"");
            builder.AppendLine("	INDEX 01 05:00:00");
            builder.AppendLine("TRACK 03 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 3\"");
            builder.AppendLine("	TITLE \"Sample Title 3\"");
            builder.AppendLine("	INDEX 01 09:23:00");
            builder.AppendLine("TRACK 04 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 4\"");
            builder.AppendLine("	TITLE \"Sample Title 4\"");
            builder.AppendLine("	INDEX 01 15:54:00");
            builder.AppendLine("TRACK 05 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 5\"");
            builder.AppendLine("	TITLE \"Sample Title 5\"");
            builder.AppendLine("	INDEX 01 20:13:00");
            builder.AppendLine("TRACK 06 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 6\"");
            builder.AppendLine("	TITLE \"Sample Title 6\"");
            builder.AppendLine("	INDEX 01 24:54:00");
            builder.AppendLine("TRACK 07 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 7\"");
            builder.AppendLine("	TITLE \"Sample Title 7\"");
            builder.AppendLine("	INDEX 01 31:54:00");
            builder.AppendLine("TRACK 08 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 8\"");
            builder.AppendLine("	TITLE \"Sample Title 8\"");
            builder.AppendLine("	INDEX 01 45:51:00");

            var testHelper = new TestHelper();
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            var importFile = new CuesheetImportFile(new MemoryStream(File.ReadAllBytes(tempFile)), testHelper.ApplicationOptions);
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.Cuesheet);
            Assert.IsTrue(importFile.Cuesheet.IsValid);
            Assert.AreEqual(importFile.Cuesheet.Tracks.Count, 8);
            Assert.IsNotNull(importFile.Cuesheet.CDTextfile);
            Assert.AreEqual("<Mark>PERFORMER \"Sample CD Artist\"</Mark>", importFile.FileContentRecognized.ElementAt(0));
            Assert.AreEqual("<Mark>TITLE \"Sample CD Title\"</Mark>", importFile.FileContentRecognized.ElementAt(1));
            Assert.AreEqual("<Mark>FILE \"AC DC - TNT.mp3\" MP3</Mark>", importFile.FileContentRecognized.ElementAt(2));
            Assert.AreEqual("<Mark>CDTEXTFILE \"Testfile.cdt\"</Mark>", importFile.FileContentRecognized.ElementAt(3));
            Assert.AreEqual("<Mark>CATALOG 0123456789012</Mark>", importFile.FileContentRecognized.ElementAt(4));
            Assert.AreEqual("<Mark>TRACK 01 AUDIO</Mark>", importFile.FileContentRecognized.ElementAt(5));
            Assert.AreEqual("	<Mark>PERFORMER \"Sample Artist 1\"</Mark>", importFile.FileContentRecognized.ElementAt(6));
            Assert.AreEqual("	<Mark>TITLE \"Sample Title 1\"</Mark>", importFile.FileContentRecognized.ElementAt(7));
            Assert.AreEqual("	<Mark>INDEX 01 00:00:00</Mark>", importFile.FileContentRecognized.ElementAt(8));

            File.Delete(tempFile);

            importFile = new CuesheetImportFile(new MemoryStream(Resources.Playlist_Bug_30), testHelper.ApplicationOptions);
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.Cuesheet);
            Assert.IsNull(importFile.Cuesheet.GetValidationErrors(testHelper.Localizer, validationErrorFilterType: Entity.ValidationErrorFilterType.ErrorOnly));

            importFile = new CuesheetImportFile(new MemoryStream(Resources.Playlist_Bug_57), testHelper.ApplicationOptions);
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.Cuesheet);
            Assert.IsTrue(importFile.Cuesheet.Tracks.Count == 39);
            Assert.AreEqual(importFile.Cuesheet.Tracks.ElementAt(24).Begin, new TimeSpan(2, 8, 21));

            importFile = new CuesheetImportFile(new MemoryStream(Resources.Playlist__36_Frames), testHelper.ApplicationOptions);
            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.Cuesheet);
            Assert.IsTrue(importFile.Cuesheet.Tracks.Count == 12);
            Assert.AreEqual(importFile.Cuesheet.Tracks.ElementAt(2).Begin, new TimeSpan(0, 0, 9, 15, 600));

            builder = new StringBuilder();
            builder.AppendLine("PERFORMER \"Sample CD Artist\"");
            builder.AppendLine("TITLE \"Sample CD Title\"");
            builder.AppendLine("FILE \"AC DC - TNT.mp3\" MP3");
            builder.AppendLine("CDTEXTFILE \"Testfile.cdt\"");
            builder.AppendLine("CATALOG 0123456789012");
            builder.AppendLine("TRACK 01 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 1\"");
            builder.AppendLine("	TITLE \"Sample Title 1\"");
            builder.AppendLine("	FLAGS 4CH DCP PRE SCMS");
            builder.AppendLine("	INDEX 01 00:00:00");
            builder.AppendLine("TRACK 02 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 2\"");
            builder.AppendLine("	TITLE \"Sample Title 2\"");
            builder.AppendLine("	FLAGS DCP PRE");
            builder.AppendLine("	INDEX 01 05:00:00");
            builder.AppendLine("TRACK 03 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 3\"");
            builder.AppendLine("	TITLE \"Sample Title 3\"");
            builder.AppendLine("	INDEX 01 09:23:00");
            builder.AppendLine("TRACK 04 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 4\"");
            builder.AppendLine("	TITLE \"Sample Title 4\"");
            builder.AppendLine("	INDEX 01 15:54:00");
            builder.AppendLine("TRACK 05 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 5\"");
            builder.AppendLine("	TITLE \"Sample Title 5\"");
            builder.AppendLine("	INDEX 01 20:13:00");
            builder.AppendLine("	POSTGAP 00:02:00");
            builder.AppendLine("TRACK 06 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 6\"");
            builder.AppendLine("	TITLE \"Sample Title 6\"");
            builder.AppendLine("	INDEX 01 24:54:00");
            builder.AppendLine("TRACK 07 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 7\"");
            builder.AppendLine("	TITLE \"Sample Title 7\"");
            builder.AppendLine("	PREGAP 00:04:00");
            builder.AppendLine("	INDEX 01 31:54:00");
            builder.AppendLine("TRACK 08 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 8\"");
            builder.AppendLine("	TITLE \"Sample Title 8\"");
            builder.AppendLine("	INDEX 01 45:51:00");

            tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            importFile = new CuesheetImportFile(new MemoryStream(File.ReadAllBytes(tempFile)), testHelper.ApplicationOptions);

            Assert.IsNull(importFile.AnalyseException);
            Assert.IsNotNull(importFile.Cuesheet);
            Assert.AreEqual("	<Mark>FLAGS 4CH DCP PRE SCMS</Mark>", importFile.FileContentRecognized.ElementAt(8));
            Assert.AreEqual("	<Mark>PREGAP 00:04:00</Mark>", importFile.FileContentRecognized.ElementAt(35));
            Assert.IsTrue(importFile.Cuesheet.IsValid);
            Assert.AreEqual(importFile.Cuesheet.Tracks.Count, 8);
            Assert.IsNotNull(importFile.Cuesheet.CDTextfile);
            Assert.IsTrue(importFile.Cuesheet.Tracks.ElementAt(0).Flags.Count == 4);
            Assert.IsTrue(importFile.Cuesheet.Tracks.ElementAt(1).Flags.Count == 2);
            Assert.IsNotNull(importFile.Cuesheet.Tracks.ElementAt(1).Flags.SingleOrDefault(x => x.CuesheetLabel == "DCP"));
            Assert.IsNotNull(importFile.Cuesheet.Tracks.ElementAt(1).Flags.SingleOrDefault(x => x.CuesheetLabel == "PRE"));
            Assert.AreEqual(new TimeSpan(0, 0, 2), importFile.Cuesheet.Tracks.ElementAt(4).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), importFile.Cuesheet.Tracks.ElementAt(6).PreGap);

            File.Delete(tempFile);
        }
    }
}