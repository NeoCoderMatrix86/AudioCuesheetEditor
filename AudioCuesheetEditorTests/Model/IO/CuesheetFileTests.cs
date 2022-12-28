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
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditorTests.Properties;
using AudioCuesheetEditorTests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AudioCuesheetEditor.Model.IO.Tests
{
    [TestClass()]
    public class CuesheetfileTests
    {
        [TestMethod()]
        public void GenerateCuesheetFileTest()
        {
            var testHelper = new TestHelper();
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
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
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            }
            var cuesheetFile = new Cuesheetfile(cuesheet);
            var generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNotNull(generatedFile);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, cuesheet.Audiofile.FileName, cuesheet.Audiofile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 4)
            {
                var track = cuesheet.Tracks.Single(x => x.Position == position);
                position++;
                Assert.AreEqual(fileContent[i], String.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, track.Position, CuesheetConstants.CuesheetTrackAudio));
                Assert.AreEqual(fileContent[i + 1], String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title));
                Assert.AreEqual(fileContent[i + 2], String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist));
                Assert.AreEqual(fileContent[i + 3], String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(track.Begin.Value.TotalMinutes), track.Begin.Value.Seconds, track.Begin.Value.Milliseconds / 75));
            }
            File.Delete(fileName);
            cuesheet.CDTextfile = new CDTextfile("Testfile.cdt");
            cuesheet.Cataloguenumber.Value = "0123456789123";
            generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNotNull(generatedFile);
            fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} {1}", CuesheetConstants.CuesheetCatalogueNumber, cuesheet.Cataloguenumber.Value));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetCDTextfile, cuesheet.CDTextfile.FileName));
            File.Delete(fileName);
            cuesheet.CDTextfile = new CDTextfile("Testfile.cdt");
            cuesheet.Cataloguenumber.Value = "Testvalue";
            generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNull(generatedFile);
        }

        [TestMethod()]
        public void TestExportWithPreGapAndPostGap()
        {
            var testHelper = new TestHelper();
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
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
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            }
            var cuesheetFile = new Cuesheetfile(cuesheet);
            var generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNotNull(generatedFile);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, cuesheet.Audiofile.FileName, cuesheet.Audiofile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 7)
            {
                var track = cuesheet.Tracks.Single(x => x.Position == position);
                position++;
                Assert.AreEqual(String.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, track.Position, CuesheetConstants.CuesheetTrackAudio), fileContent[i]);
                Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title), fileContent[i + 1]);
                Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist), fileContent[i + 2]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackFlags, String.Join(" ", track.Flags.Select(x => x.CuesheetLabel))), fileContent[i + 3]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackPreGap, Math.Floor(track.PreGap.Value.TotalMinutes), track.PreGap.Value.Seconds, track.PreGap.Value.Milliseconds / 75), fileContent[i + 4]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(track.Begin.Value.TotalMinutes), track.Begin.Value.Seconds, track.Begin.Value.Milliseconds / 75), fileContent[i + 5]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackPostGap, Math.Floor(track.PostGap.Value.TotalMinutes), track.PostGap.Value.Seconds, track.PostGap.Value.Milliseconds / 75), fileContent[i + 6]);
            }
            File.Delete(fileName);
        }

        [TestMethod()]
        public void TestExportFlags()
        {
            var testHelper = new TestHelper();
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
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
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            }
            var cuesheetFile = new Cuesheetfile(cuesheet);
            var generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNotNull(generatedFile);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, cuesheet.Audiofile.FileName, cuesheet.Audiofile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 5)
            {
                var track = cuesheet.Tracks.Single(x => x.Position == position);
                position++;
                Assert.AreEqual(fileContent[i], String.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, track.Position, CuesheetConstants.CuesheetTrackAudio));
                Assert.AreEqual(fileContent[i + 1], String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title));
                Assert.AreEqual(fileContent[i + 2], String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist));
                Assert.AreEqual(fileContent[i + 3], String.Format("{0}{1}{2} {3}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackFlags, String.Join(" ", track.Flags.Select(x => x.CuesheetLabel))));
                Assert.AreEqual(fileContent[i + 4], String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(track.Begin.Value.TotalMinutes), track.Begin.Value.Seconds, track.Begin.Value.Milliseconds / 75));
            }
            File.Delete(fileName);
        }

        [TestMethod()]
        public void TestExportWithIncorrectPositions()
        {
            //TODO
            //var testHelper = new TestHelper();
            //Cuesheet cuesheet = new()
            //{
            //    Artist = "Demo Artist",
            //    Title = "Demo Title",
            //    Audiofile = new Audiofile("Testfile.mp3")
            //};
            //var begin = TimeSpan.Zero;
            //var random = new Random();
            //for (int i = 1; i < 6; i++)
            //{
            //    var track = new Track
            //    {
            //        Artist = String.Format("Demo Track Artist {0}", i),
            //        Title = String.Format("Demo Track Title {0}", i),
            //        Begin = begin,
            //        Position = (uint)(i + random.Next(1, 10))
            //    };
            //    begin = begin.Add(new TimeSpan(0, i, i));
            //    track.End = begin;
            //    cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            //}
            //var cuesheetFile = new Cuesheetfile(cuesheet);
            //Assert.IsFalse(cuesheetFile.IsExportable);
            ////Rearrange positions
            //cuesheet.Tracks.ElementAt(0).Position = 1;
            //cuesheet.Tracks.ElementAt(1).Position = 2;
            //cuesheet.Tracks.ElementAt(2).Position = 3;
            //cuesheet.Tracks.ElementAt(3).Position = 4;
            //cuesheet.Tracks.ElementAt(4).Position = 5;
            //Assert.IsTrue(cuesheetFile.IsExportable);
            //var generatedFile = cuesheetFile.GenerateCuesheetFile();
            //Assert.IsNotNull(generatedFile);
            //var fileName = Path.GetTempFileName();
            //File.WriteAllBytes(fileName, generatedFile);
            //var fileContent = File.ReadAllLines(fileName);
            //Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, cuesheet.Title));
            //Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, cuesheet.Artist));
            //Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, cuesheet.Audiofile.FileName, cuesheet.Audiofile.AudioFileType));
            //var position = 1;
            //for (int i = 3; i < fileContent.Length; i += 4)
            //{
            //    var track = cuesheet.Tracks.ElementAt(position - 1);
            //    Assert.AreEqual(String.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, position, CuesheetConstants.CuesheetTrackAudio), fileContent[i]);
            //    Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title), fileContent[i + 1]);
            //    Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist), fileContent[i + 2]);
            //    Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(track.Begin.Value.TotalMinutes), track.Begin.Value.Seconds, track.Begin.Value.Milliseconds / 75), fileContent[i + 3]);
            //    position++;
            //}
            //File.Delete(fileName);
        }
    }
}