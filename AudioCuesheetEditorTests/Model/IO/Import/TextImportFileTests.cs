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
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditorTests.Properties;
using AudioCuesheetEditor.Model.Utility;

namespace AudioCuesheetEditor.Model.IO.Import.Tests
{
    [TestClass()]
    public class TextImportFileTests
    {
        [TestMethod()]
        public void TextImportFileTest()
        {
            //Prepare text input file
            StringBuilder builder = new();
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
            var textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet;
            textImportFile.TextImportScheme.SchemeTracks = TextImportScheme.DefaultSchemeTracks;
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsNotNull(textImportFile.Cuesheet);
            Assert.AreEqual("CuesheetArtist", textImportFile.Cuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", textImportFile.Cuesheet.Title);
            Assert.AreEqual("c:\\tmp\\Testfile.mp3", textImportFile.Cuesheet.Audiofile.FileName);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 8);
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].End, new TimeSpan(0, 5, 0));

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

            textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = @"(?'Cuesheet.Artist'\A.*)[|](?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.CDTextfile'.{1,})";
            textImportFile.TextImportScheme.SchemeTracks = @"(?'Track.Position'.{1,})|(?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})";

            Assert.IsNotNull(textImportFile.AnalyseException);

            textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeTracks = @"(?'Track.Position'\d{1,})[|](?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})";

            Assert.IsNull(textImportFile.AnalyseException);
            Assert.AreEqual("CuesheetArtist", textImportFile.Cuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", textImportFile.Cuesheet.Title);
            Assert.AreEqual("c:\\tmp\\TestTextFile.cdt", textImportFile.Cuesheet.CDTextfile.FileName);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ToArray()[5].Position == 6);
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].End, new TimeSpan(0, 5, 0));

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

            textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = @"(?'Cuesheet.Artist'\A.*)[|](?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.CDTextfile'[a-zA-Z0-9_ .();äöü&:,\\]{1,})\t{1,}(?'Cuesheet.Cataloguenumber'.{1,})";
            textImportFile.TextImportScheme.SchemeTracks = @"(?'Track.Position'.{1,})|(?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})";

            Assert.IsNotNull(textImportFile.AnalyseException);

            textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeTracks = @"(?'Track.Position'.{1,})[|](?'Track.Artist'.{1,}) - (?'Track.Title'[a-zA-Z0-9_ ]{1,})\t{1,}(?'Track.End'.{1,})";

            Assert.IsNull(textImportFile.AnalyseException);
            Assert.AreEqual("CuesheetArtist", textImportFile.Cuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", textImportFile.Cuesheet.Title);
            Assert.AreEqual("c:\\tmp\\TestTextFile.cdt", textImportFile.Cuesheet.CDTextfile.FileName);
            Assert.AreEqual("A83412346734", textImportFile.Cuesheet.Cataloguenumber.Value);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ToArray()[5].Position == 6);
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].End, new TimeSpan(0, 5, 0));

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
            textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = String.Empty;
            textImportFile.TextImportScheme.SchemeTracks = TextImportScheme.DefaultSchemeTracks;

            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 9);
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[7].End, new TimeSpan(1, 15, 54));

            File.Delete(tempFile);

            var timespanFormat = new TimeSpanFormat() { Scheme = "(?'TimeSpanFormat.Minutes'\\d{1,})[:](?'TimeSpanFormat.Seconds'\\d{1,})" };
            textImportFile = new TextImportfile(new MemoryStream(Resources.Textimport_Bug_213), timeSpanFormat: timespanFormat);
            textImportFile.TextImportScheme.SchemeCuesheet = "(?'Track.Artist'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Track.Title'[a-zA-Z0-9_ .();äöü]{1,})\t{1,}(?'Track.End'.{1,})";
            textImportFile.TextImportScheme.SchemeCuesheet = String.Empty;
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 4);
            Assert.AreEqual(new TimeSpan(2, 3, 23), textImportFile.Cuesheet.Tracks.ToArray()[3].End);
        }

        [TestMethod()]
        public void TextImportFileTestFlags()
        {
            //Prepare text input file
            StringBuilder builder = new();
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
            var textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = String.Empty;
            textImportFile.TextImportScheme.SchemeTracks = "(?'Track.Artist'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Track.Title'[a-zA-Z0-9_ .();äöü]{1,})\t{1,}(?'Track.End'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'Track.Flags'[a-zA-Z 0-9,]{1,})";
            
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(0).Flags.Contains(AudioCuesheet.Flag.DCP));
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(2).Flags.Contains(AudioCuesheet.Flag.DCP));
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(2).Flags.Contains(AudioCuesheet.Flag.PRE));
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(3).Flags.Contains(AudioCuesheet.Flag.FourCH));
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(5).Flags.Contains(AudioCuesheet.Flag.FourCH));
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(5).Flags.Contains(AudioCuesheet.Flag.PRE));
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(5).Flags.Contains(AudioCuesheet.Flag.DCP));
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(7).Flags.Contains(AudioCuesheet.Flag.DCP));
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(7).Flags.Contains(AudioCuesheet.Flag.PRE));
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(7).Flags.Contains(AudioCuesheet.Flag.FourCH));
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.ElementAt(7).Flags.Contains(AudioCuesheet.Flag.SCMS));

            File.Delete(tempFile);
        }

        [TestMethod()]
        public void TextImportFileTestPreGapAndPostGap()
        {
            //Prepare text input file
            StringBuilder builder = new();
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
            var textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = String.Empty;
            textImportFile.TextImportScheme.SchemeTracks = "(?'Track.Artist'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Track.Title'[a-zA-Z0-9_ .();äöü]{1,})\t{1,}(?'Track.PreGap'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'Track.End'[0-9]{2}[:][0-9]{2}[:][0-9]{2})\t{1,}(?'Track.PostGap'[0-9]{2}[:][0-9]{2}[:][0-9]{2})";
            
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 8);
            Assert.AreEqual(new TimeSpan(0, 0, 2), textImportFile.Cuesheet.Tracks.ElementAt(0).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.Cuesheet.Tracks.ElementAt(0).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), textImportFile.Cuesheet.Tracks.ElementAt(1).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.Cuesheet.Tracks.ElementAt(1).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.Cuesheet.Tracks.ElementAt(2).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 2), textImportFile.Cuesheet.Tracks.ElementAt(2).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.Cuesheet.Tracks.ElementAt(3).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 3), textImportFile.Cuesheet.Tracks.ElementAt(3).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.Cuesheet.Tracks.ElementAt(4).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), textImportFile.Cuesheet.Tracks.ElementAt(4).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.Cuesheet.Tracks.ElementAt(5).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 1), textImportFile.Cuesheet.Tracks.ElementAt(5).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.Cuesheet.Tracks.ElementAt(6).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.Cuesheet.Tracks.ElementAt(6).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 2), textImportFile.Cuesheet.Tracks.ElementAt(7).PreGap);
            Assert.AreEqual(new TimeSpan(0, 0, 0), textImportFile.Cuesheet.Tracks.ElementAt(7).PostGap);

            File.Delete(tempFile);
        }

        [TestMethod()]
        public void FileContentRecognizedTests()
        {
            //Prepare text input file
            StringBuilder builder = new();
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
            var textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes(tempFile)));
            textImportFile.TextImportScheme.SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet;
            textImportFile.TextImportScheme.SchemeTracks = String.Empty;
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsNotNull(textImportFile.Cuesheet);
            Assert.IsNotNull(textImportFile.FileContentRecognized);
            Assert.AreEqual(String.Format("{0} - {1}				{2}", 
                String.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetArtist"), 
                String.Format(CuesheetConstants.RecognizedMarkHTML, "CuesheetTitle"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "c:\\tmp\\Testfile.mp3")), textImportFile.FileContentRecognized.First());
            Assert.AreEqual("CuesheetArtist", textImportFile.Cuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", textImportFile.Cuesheet.Title);
            Assert.AreEqual("c:\\tmp\\Testfile.mp3", textImportFile.Cuesheet.Audiofile.FileName);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 0);
            Assert.AreEqual("Sample Artist 1 - Sample Title 1				00:05:00", textImportFile.FileContentRecognized.ElementAt(1));
            Assert.AreEqual("Sample Artist 2 - Sample Title 2				00:09:23", textImportFile.FileContentRecognized.ElementAt(2));
            Assert.AreEqual("Sample Artist 3 - Sample Title 3				00:15:54", textImportFile.FileContentRecognized.ElementAt(3));
            Assert.AreEqual("Sample Artist 4 - Sample Title 4				00:20:13", textImportFile.FileContentRecognized.ElementAt(4));
            Assert.AreEqual("Sample Artist 5 - Sample Title 5				00:24:54", textImportFile.FileContentRecognized.ElementAt(5));
            Assert.AreEqual("Sample Artist 6 - Sample Title 6				00:31:54", textImportFile.FileContentRecognized.ElementAt(6));
            Assert.AreEqual("Sample Artist 7 - Sample Title 7				00:45:54", textImportFile.FileContentRecognized.ElementAt(7));
            Assert.AreEqual("Sample Artist 8 - Sample Title 8				01:15:54", textImportFile.FileContentRecognized.ElementAt(8));
            textImportFile.TextImportScheme.SchemeTracks = TextImportScheme.DefaultSchemeTracks;
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 8);
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.Cuesheet.Tracks.ToArray()[0].End, new TimeSpan(0, 5, 0));
            Assert.AreEqual(String.Format("{0} - {1}				{2}",
                String.Format(CuesheetConstants.RecognizedMarkHTML, "Sample Artist 8"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "Sample Title 8"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "01:15:54")), textImportFile.FileContentRecognized.Last());

            File.Delete(tempFile);

            textImportFile = new TextImportfile(new MemoryStream(Resources.Textimport_Bug__233));
            textImportFile.TextImportScheme.SchemeCuesheet = String.Empty;
            textImportFile.TextImportScheme.SchemeTracks = TextImportScheme.DefaultSchemeTracks;
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsNotNull(textImportFile.Cuesheet);
            Assert.IsNotNull(textImportFile.FileContentRecognized);
            Assert.AreEqual(String.Format("{0} - {1}\t\t\t\t\t\t\t\t{2}",
                String.Format(CuesheetConstants.RecognizedMarkHTML, "Age Of Love"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "The Age Of Love (Charlotte De Witte & Enrico Sangiuliano Remix)"),
                String.Format(CuesheetConstants.RecognizedMarkHTML, "04:29:28")), textImportFile.FileContentRecognized.ElementAt(53));
        }
    }
}