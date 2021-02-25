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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AudioCuesheetEditorTests.Utility;
using AudioCuesheetEditor.Model.IO.Import;

namespace AudioCuesheetEditor.Model.IO.Import.Tests
{
    [TestClass()]
    public class TextImportFileTests
    {
        [TestMethod()]
        public void TextImportFileTest()
        {
            //Prepare text input file
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("CuesheetArtist - CuesheetTitle				c:\\tmp\\Testfile.mp3");
            builder.AppendLine("Sample Artist 1 - Sample Title 1				00:05:00");
            builder.AppendLine("Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("Sample Artist 3 - Sample Title 3				00:15:54");
            builder.AppendLine("Sample Artist 4 - Sample Title 4				00:20:13");
            builder.AppendLine("Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("Sample Artist 6 - Sample Title 6				00:31:54");
            builder.AppendLine("Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("Sample Artist 8 - Sample Title 8				01:15:54");

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            //Test TextImportFile
            var textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeTracks = "%Track.Artist% - %Track.Title%[\t]{1,}%Track.End%";
            textImportFile.TextImportScheme.SchemeCuesheet = "\\A.*%Cuesheet.Artist% - %Cuesheet.Title%[\t]{1,}%Cuesheet.AudioFile%";
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsNotNull(textImportFile.ImportCuesheet);
            Assert.AreEqual("CuesheetArtist", textImportFile.ImportCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", textImportFile.ImportCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\Testfile.mp3", textImportFile.ImportCuesheet.AudioFile.FileName);
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.Count == 8);
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].End, new TimeSpan(0, 5, 0));

            File.Delete(tempFile);

            //Prepare next Test
            builder.Clear();

            builder.AppendLine("CuesheetArtist|CuesheetTitle				c:\\tmp\\TestTextFile.cdt");
            builder.AppendLine("1|Sample Artist 1 - Sample Title 1				00:05:00");
            builder.AppendLine("2|Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("3|Sample Artist 3 - Sample Title 3				00:15:54");
            builder.AppendLine("4|Sample Artist 4 - Sample Title 4				00:20:13");
            builder.AppendLine("5|Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("6|Sample Artist 6 - Sample Title 6				00:31:54");
            builder.AppendLine("7|Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("8|Sample Artist 8 - Sample Title 8				01:15:54");

            File.WriteAllText(tempFile, builder.ToString());

            textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = "%Cuesheet.Artist%[|]%Cuesheet.Title%[\\t]{1,}%Cuesheet.CDTextfile%";
            textImportFile.TextImportScheme.SchemeTracks = "%Position%|%Artist% - %Title%[\t]{1,}%End%";            

            Assert.IsNotNull(textImportFile.AnalyseException);

            textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeTracks = "%Position%[|]%Artist% - %Title%[\t]{1,}%End%";

            Assert.IsNull(textImportFile.AnalyseException);
            Assert.AreEqual("CuesheetArtist", textImportFile.ImportCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", textImportFile.ImportCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\TestTextFile.cdt", textImportFile.ImportCuesheet.CDTextfile.FileName);
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ToArray()[5].Position == 6);
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].End, new TimeSpan(0, 5, 0));

            File.Delete(tempFile);

            //Prepare next Test
            builder.Clear();

            builder.AppendLine("CuesheetArtist|CuesheetTitle				c:\\tmp\\TestTextFile.cdt				A83412346734");
            builder.AppendLine("1|Sample Artist 1 - Sample Title 1				00:05:00");
            builder.AppendLine("2|Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("3|Sample Artist 3 - Sample Title 3				00:15:54");
            builder.AppendLine("4|Sample Artist 4 - Sample Title 4				00:20:13");
            builder.AppendLine("5|Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("6|Sample Artist 6 - Sample Title 6				00:31:54");
            builder.AppendLine("7|Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("8|Sample Artist 8 - Sample Title 8				01:15:54");
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);

            File.WriteAllText(tempFile, builder.ToString());

            textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = "%Cuesheet.Artist%[|]%Cuesheet.Title%[\\t]{1,}%Cuesheet.CDTextfile%[\\t]{1,}%Cuesheet.CatalogueNumber%";
            textImportFile.TextImportScheme.SchemeTracks = "%Position%|%Artist% - %Title%[\t]{1,}%End%";

            Assert.IsNotNull(textImportFile.AnalyseException);

            textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeTracks = "%Position%[|]%Artist% - %Title%[\t]{1,}%End%";

            Assert.IsNull(textImportFile.AnalyseException);
            Assert.AreEqual("CuesheetArtist", textImportFile.ImportCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", textImportFile.ImportCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\TestTextFile.cdt", textImportFile.ImportCuesheet.CDTextfile.FileName);
            Assert.AreEqual("A83412346734", textImportFile.ImportCuesheet.CatalogueNumber.Value);
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ToArray()[5].Position == 6);
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].End, new TimeSpan(0, 5, 0));

            File.Delete(tempFile);

            //Prepare next test

            //Prepare text input file
            builder = new StringBuilder();
            builder.AppendLine("Sample Artist 1 - Sample Title 1				00:05:00");
            builder.AppendLine("Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("Sample Artist 3 - Sample Title 3				00:15:54");
            builder.AppendLine("Sample Artist 4 - Sample Title 4				00:20:13");
            builder.AppendLine("Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("Sample Artist 6 - Sample Title 6				00:31:54");
            builder.AppendLine("Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("Sample Artist 8 - Sample Title 8				01:15:54");
            builder.AppendLine("Sample Artist 9 - Sample Title 9");

            tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            //Test TextImportFile
            textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = String.Empty;
            textImportFile.TextImportScheme.SchemeTracks = "%Artist% - %Title%[\t]{1,}%End%";

            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.Count == 9);
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[7].End, new TimeSpan(1, 15, 54));

            File.Delete(tempFile);
        }

        [TestMethod()]
        public void TextImportFileTestFlags()
        {
            //Prepare text input file
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Sample Artist 1 - Sample Title 1				00:05:00	DCP");
            builder.AppendLine("Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("Sample Artist 3 - Sample Title 3				00:15:54	PRE, DCP");
            builder.AppendLine("Sample Artist 4 - Sample Title 4				00:20:13	4CH");
            builder.AppendLine("Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("Sample Artist 6 - Sample Title 6				00:31:54	PRE DCP 4CH");
            builder.AppendLine("Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("Sample Artist 8 - Sample Title 8				01:15:54	PRE DCP 4CH SCMS");

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            //Test TextImportFile
            var textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = String.Empty;
            textImportFile.TextImportScheme.SchemeTracks = "%Artist% - %Title%[\t]{1,}%End%[\t]{1,}%Flags%";
            
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(0).Flags.Contains(AudioCuesheet.Flag.DCP));
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(2).Flags.Contains(AudioCuesheet.Flag.DCP));
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(2).Flags.Contains(AudioCuesheet.Flag.PRE));
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(3).Flags.Contains(AudioCuesheet.Flag.FourCH));
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(5).Flags.Contains(AudioCuesheet.Flag.FourCH));
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(5).Flags.Contains(AudioCuesheet.Flag.PRE));
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(5).Flags.Contains(AudioCuesheet.Flag.DCP));
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(7).Flags.Contains(AudioCuesheet.Flag.DCP));
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(7).Flags.Contains(AudioCuesheet.Flag.PRE));
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(7).Flags.Contains(AudioCuesheet.Flag.FourCH));
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.ElementAt(7).Flags.Contains(AudioCuesheet.Flag.SCMS));

            File.Delete(tempFile);
        }

        [TestMethod()]
        public void TextImportFileTestPreGapAndPostGap()
        {
            //Prepare text input file
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Sample Artist 1 - Sample Title 1		00:00:02		00:05:00		00:00:00");
            builder.AppendLine("Sample Artist 2 - Sample Title 2		00:00:04		00:09:23		00:00:00");
            builder.AppendLine("Sample Artist 3 - Sample Title 3		00:00:00		00:15:54		00:00:02");
            builder.AppendLine("Sample Artist 4 - Sample Title 4		00:00:00		00:20:13		00:00:03");
            builder.AppendLine("Sample Artist 5 - Sample Title 5		00:00:00		00:24:54		00:00:04");
            builder.AppendLine("Sample Artist 6 - Sample Title 6		00:00:00		00:31:54		00:00:01");
            builder.AppendLine("Sample Artist 7 - Sample Title 7		00:00:00		00:45:54		00:00:00");
            builder.AppendLine("Sample Artist 8 - Sample Title 8		00:00:02		01:15:54		00:00:00");

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            //Test TextImportFile
            var textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = String.Empty;
            textImportFile.TextImportScheme.SchemeTracks = "%Artist% - %Title%[\t]{1,}%PreGap%[\t]{1,}%End%[\t]{1,}%PostGap%";
            
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.Count == 8);
            Assert.AreEqual(new TimeSpan(0, 0, 2), textImportFile.ImportCuesheet.Tracks.ElementAt(0).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.ImportCuesheet.Tracks.ElementAt(0).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), textImportFile.ImportCuesheet.Tracks.ElementAt(1).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.ImportCuesheet.Tracks.ElementAt(1).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.ImportCuesheet.Tracks.ElementAt(2).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 2), textImportFile.ImportCuesheet.Tracks.ElementAt(2).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.ImportCuesheet.Tracks.ElementAt(3).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 3), textImportFile.ImportCuesheet.Tracks.ElementAt(3).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.ImportCuesheet.Tracks.ElementAt(4).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), textImportFile.ImportCuesheet.Tracks.ElementAt(4).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.ImportCuesheet.Tracks.ElementAt(5).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 1), textImportFile.ImportCuesheet.Tracks.ElementAt(5).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.ImportCuesheet.Tracks.ElementAt(6).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.ImportCuesheet.Tracks.ElementAt(6).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 2), textImportFile.ImportCuesheet.Tracks.ElementAt(7).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.ImportCuesheet.Tracks.ElementAt(7).PostGap);

            File.Delete(tempFile);
        }

        [TestMethod()]
        public void FileContentRecognizedTests()
        {
            //Prepare text input file
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("CuesheetArtist - CuesheetTitle				c:\\tmp\\Testfile.mp3");
            builder.AppendLine("Sample Artist 1 - Sample Title 1				00:05:00");
            builder.AppendLine("Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("Sample Artist 3 - Sample Title 3				00:15:54");
            builder.AppendLine("Sample Artist 4 - Sample Title 4				00:20:13");
            builder.AppendLine("Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("Sample Artist 6 - Sample Title 6				00:31:54");
            builder.AppendLine("Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("Sample Artist 8 - Sample Title 8				01:15:54");

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            //Test TextImportFile
            var textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeTracks = "%Track.Artist% - %Track.Title%[\t]{1,}%Track.End%";
            textImportFile.TextImportScheme.SchemeCuesheet = "\\A.*%Cuesheet.Artist% - %Cuesheet.Title%[\t]{1,}%Cuesheet.AudioFile%";
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsNotNull(textImportFile.ImportCuesheet);
            Assert.IsNotNull(textImportFile.FileContentRecognized);
            Assert.AreEqual("<Mark>CuesheetArtist</Mark> - <Mark>CuesheetTitle</Mark>				<Mark>c:\\tmp\\Testfile.mp3</Mark>", textImportFile.FileContentRecognized.First());
            Assert.AreEqual("CuesheetArtist", textImportFile.ImportCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", textImportFile.ImportCuesheet.Title);
            Assert.AreEqual("c:\\tmp\\Testfile.mp3", textImportFile.ImportCuesheet.AudioFile.FileName);
            Assert.IsTrue(textImportFile.ImportCuesheet.Tracks.Count == 8);
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.ImportCuesheet.Tracks.ToArray()[0].End, new TimeSpan(0, 5, 0));
            Assert.AreEqual("<Mark>Sample Artist 8</Mark> - <Mark>Sample Title 8</Mark>				<Mark>01:15:54</Mark>", textImportFile.FileContentRecognized.Last());

            File.Delete(tempFile);
        }
    }
}