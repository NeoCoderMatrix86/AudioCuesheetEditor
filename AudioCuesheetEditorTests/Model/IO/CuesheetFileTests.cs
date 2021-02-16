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
using System;
using System.Collections.Generic;
using System.Text;
using AudioCuesheetEditor.Model.AudioCuesheet;
using System.IO;
using System.Linq;
using AudioCuesheetEditor.Controller;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using AudioCuesheetEditor.Shared.ResourceFiles;
using AudioCuesheetEditorTests.Utility;
using AudioCuesheetEditorTests.Properties;
using AudioCuesheetEditor.Model.IO.Audio;

namespace AudioCuesheetEditor.Model.IO.Tests
{
    [TestClass()]
    public class CuesheetFileTests
    {
        [TestMethod()]
        public void GenerateCuesheetFileTest()
        {
            var testHelper = new TestHelper();
            Cuesheet cuesheet = new Cuesheet
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                AudioFile = new AudioFile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track);
            }
            var cuesheetFile = new CuesheetFile(cuesheet);
            var generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNotNull(generatedFile);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetFile.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetFile.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetFile.CuesheetFileName, cuesheet.AudioFile.FileName, cuesheet.AudioFile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 4)
            {
                var track = cuesheet.Tracks.Single(x => x.Position == position);
                position++;
                Assert.AreEqual(fileContent[i], String.Format("{0}{1} {2:00} {3}", CuesheetFile.Tab, CuesheetFile.CuesheetTrack, track.Position, CuesheetFile.CuesheetTrackAudio));
                Assert.AreEqual(fileContent[i + 1], String.Format("{0}{1}{2} \"{3}\"", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackTitle, track.Title));
                Assert.AreEqual(fileContent[i + 2], String.Format("{0}{1}{2} \"{3}\"", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackArtist, track.Artist));
                Assert.AreEqual(fileContent[i + 3], String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackIndex01, Math.Floor(track.Begin.Value.TotalMinutes), track.Begin.Value.Seconds, track.Begin.Value.Milliseconds / 75));
            }
            File.Delete(fileName);
            cuesheet.CDTextfile = new CDTextfile("Testfile.cdt");
            cuesheet.CatalogueNumber.Value = "0123456789123";
            generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNotNull(generatedFile);
            fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} {1}", CuesheetFile.CuesheetCatalogueNumber, cuesheet.CatalogueNumber.Value));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetFile.CuesheetCDTextfile, cuesheet.CDTextfile.FileName));
            File.Delete(fileName);
            cuesheet.CDTextfile = new CDTextfile("Testfile.cdt");
            cuesheet.CatalogueNumber.Value = "Testvalue";
            generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNull(generatedFile);
        }

        [TestMethod()]
        public void TestExportWithPreGapAndPostGap()
        {
            Cuesheet cuesheet = new Cuesheet
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                AudioFile = new AudioFile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                var rand = new Random();
                var flagsToAdd = rand.Next(1, 3);
                for (int x = 0; x < flagsToAdd; x++)
                {
                    track.SetFlag(Flag.AvailableFlags.ElementAt(x), SetFlagMode.Add);
                }
                track.PostGap = new TimeSpan(0, 0, 2);
                track.PreGap = new TimeSpan(0, 0, 3);
                cuesheet.AddTrack(track);
            }
            var cuesheetFile = new CuesheetFile(cuesheet);
            var generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNotNull(generatedFile);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetFile.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetFile.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetFile.CuesheetFileName, cuesheet.AudioFile.FileName, cuesheet.AudioFile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 7)
            {
                var track = cuesheet.Tracks.Single(x => x.Position == position);
                position++;
                Assert.AreEqual(String.Format("{0}{1} {2:00} {3}", CuesheetFile.Tab, CuesheetFile.CuesheetTrack, track.Position, CuesheetFile.CuesheetTrackAudio), fileContent[i]);
                Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackTitle, track.Title), fileContent[i + 1]);
                Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackArtist, track.Artist), fileContent[i + 2]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3}", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackFlags, String.Join(" ", track.Flags.Select(x => x.CuesheetLabel))), fileContent[i + 3]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackPreGap, Math.Floor(track.PreGap.Value.TotalMinutes), track.PreGap.Value.Seconds, track.PreGap.Value.Milliseconds / 75), fileContent[i + 4]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackIndex01, Math.Floor(track.Begin.Value.TotalMinutes), track.Begin.Value.Seconds, track.Begin.Value.Milliseconds / 75), fileContent[i + 5]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackPostGap, Math.Floor(track.PostGap.Value.TotalMinutes), track.PostGap.Value.Seconds, track.PostGap.Value.Milliseconds / 75), fileContent[i + 6]);
            }
            File.Delete(fileName);
        }

        [TestMethod()]
        public void TestExportFlags()
        {
            Cuesheet cuesheet = new Cuesheet
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                AudioFile = new AudioFile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                var rand = new Random();
                var flagsToAdd = rand.Next(1, 3);
                for (int x = 0; x < flagsToAdd; x++)
                {
                    track.SetFlag(Flag.AvailableFlags.ElementAt(x), SetFlagMode.Add);
                }
                cuesheet.AddTrack(track);
            }
            var cuesheetFile = new CuesheetFile(cuesheet);
            var generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNotNull(generatedFile);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetFile.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetFile.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetFile.CuesheetFileName, cuesheet.AudioFile.FileName, cuesheet.AudioFile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 5)
            {
                var track = cuesheet.Tracks.Single(x => x.Position == position);
                position++;
                Assert.AreEqual(fileContent[i], String.Format("{0}{1} {2:00} {3}", CuesheetFile.Tab, CuesheetFile.CuesheetTrack, track.Position, CuesheetFile.CuesheetTrackAudio));
                Assert.AreEqual(fileContent[i + 1], String.Format("{0}{1}{2} \"{3}\"", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackTitle, track.Title));
                Assert.AreEqual(fileContent[i + 2], String.Format("{0}{1}{2} \"{3}\"", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackArtist, track.Artist));
                Assert.AreEqual(fileContent[i + 3], String.Format("{0}{1}{2} {3}", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackFlags, String.Join(" ", track.Flags.Select(x => x.CuesheetLabel))));
                Assert.AreEqual(fileContent[i + 4], String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackIndex01, Math.Floor(track.Begin.Value.TotalMinutes), track.Begin.Value.Seconds, track.Begin.Value.Milliseconds / 75));
            }
            File.Delete(fileName);
        }

        [TestMethod()]
        public void ImportCuesheetTest()
        {
            //Prepare text input file
            StringBuilder builder = new StringBuilder();
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

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            var cuesheet = CuesheetFile.ImportCuesheet(new MemoryStream(File.ReadAllBytes(tempFile)));

            Assert.IsNotNull(cuesheet);
            Assert.IsTrue(cuesheet.IsValid);
            Assert.AreEqual(cuesheet.Tracks.Count, 8);
            Assert.IsNotNull(cuesheet.CDTextfile);

            File.Delete(tempFile);

            cuesheet = CuesheetFile.ImportCuesheet(new MemoryStream(Resources.Playlist_Bug_30));
            Assert.IsNotNull(cuesheet);
            var testHelper = new TestHelper();
            Assert.IsNull(cuesheet.GetValidationErrors(testHelper.Localizer, validationErrorFilterType: Entity.ValidationErrorFilterType.ErrorOnly));

            cuesheet = CuesheetFile.ImportCuesheet(new MemoryStream(Resources.Playlist_Bug_57));
            Assert.IsNotNull(cuesheet);
            Assert.IsTrue(cuesheet.Tracks.Count == 39);
            Assert.AreEqual(cuesheet.Tracks.ElementAt(24).Begin, new TimeSpan(2, 8, 21));

            builder = new StringBuilder();
            builder.AppendLine("PERFORMER \"Sample CD Artist\"");
            builder.AppendLine("TITLE \"Sample CD Title\"");
            builder.AppendLine("FILE \"AC DC - TNT.mp3\" MP3");
            builder.AppendLine("CDTEXTFILE \"Testfile.cdt\"");
            builder.AppendLine("CATALOG 0123456789012");
            builder.AppendLine("TRACK 01 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 1\"");
            builder.AppendLine("	TITLE \"Sample Title 1\"");
            builder.AppendLine("    FLAGS 4CH DCP PRE SCMS");
            builder.AppendLine("	INDEX 01 00:00:00");
            builder.AppendLine("TRACK 02 AUDIO");
            builder.AppendLine("	PERFORMER \"Sample Artist 2\"");
            builder.AppendLine("	TITLE \"Sample Title 2\"");
            builder.AppendLine("    FLAGS DCP PRE");
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
            builder.AppendLine("    POSTGAP 00:02:00");
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

            cuesheet = CuesheetFile.ImportCuesheet(new MemoryStream(File.ReadAllBytes(tempFile)));

            Assert.IsNotNull(cuesheet);
            Assert.IsTrue(cuesheet.IsValid);
            Assert.AreEqual(cuesheet.Tracks.Count, 8);
            Assert.IsNotNull(cuesheet.CDTextfile);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(0).Flags.Count == 4);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(1).Flags.Count == 2);
            Assert.IsNotNull(cuesheet.Tracks.ElementAt(1).Flags.SingleOrDefault(x => x.CuesheetLabel == "DCP"));
            Assert.IsNotNull(cuesheet.Tracks.ElementAt(1).Flags.SingleOrDefault(x => x.CuesheetLabel == "PRE"));
            Assert.AreEqual(new TimeSpan(0, 0, 2), cuesheet.Tracks.ElementAt(4).PostGap);
            Assert.AreEqual(new TimeSpan(0, 0, 4), cuesheet.Tracks.ElementAt(6).PreGap);

            File.Delete(tempFile);
        }
    }
}