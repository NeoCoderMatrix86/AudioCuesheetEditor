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
using AudioCuesheetEditor.Model.AudioCuesheet;
using System.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using System.Text.Json;
using AudioCuesheetEditor.Extensions;

namespace AudioCuesheetEditor.Model.IO.Tests
{
    [TestClass()]
    public class ProjectfileTests
    {
        [TestMethod()]
        public void GenerateFileTest()
        {
            var cuesheet = new Cuesheet
            {
                Artist = "CuesheetArtist",
                Title = "CuesheetTitle",
                Audiofile = new Audiofile("AudioFile.mp3"),
                CDTextfile = new CDTextfile("CDTextfile.cdt"),
            };
            cuesheet.Cataloguenumber.Value = "A123";
            var begin = TimeSpan.Zero;
            for (int i = 1; i <= 10; i++)
            {
                var track = new Track
                {
                    Position = (uint)i,
                    Artist = String.Format("Artist {0}", i),
                    Title = String.Format("Title {0}", i),
                    Begin = begin
                };
                var rand = new Random();
                var flagsToAdd = rand.Next(1, 3);
                for (int x = 0; x < flagsToAdd; x++)
                {
                    track.SetFlag(Flag.AvailableFlags.ElementAt(x), SetFlagMode.Add);
                }
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track, new Options.ApplicationOptions() { LinkTracksWithPreviousOne = true });
            }
            var projectFile = new Projectfile(cuesheet);
            var generatedFile = projectFile.GenerateFile();
            Assert.IsNotNull(generatedFile);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            var fileContent = File.ReadAllLines(fileName);
            var json = JsonSerializer.Serialize<Cuesheet>(cuesheet, Projectfile.Options);
            Assert.AreEqual(json, fileContent.FirstOrDefault());
            File.Delete(fileName);
        }

        [TestMethod()]
        public void ImportFileTest()
        {
            var fileContent = Encoding.UTF8.GetBytes("{\"Artist\":\"CuesheetArtist\",\"Title\":\"CuesheetTitle\",\"Audiofile\":{\"FileName\":\"AudioFile.mp3\"},\"Tracks\":[{\"Position\":1,\"Artist\":\"Artist 1\",\"Title\":\"Title 1\",\"Begin\":\"00:00:00\",\"End\":\"00:01:01\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":2,\"Artist\":\"Artist 2\",\"Title\":\"Title 2\",\"Begin\":\"00:01:01\",\"End\":\"00:03:03\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":3,\"Artist\":\"Artist 3\",\"Title\":\"Title 3\",\"Begin\":\"00:03:03\",\"End\":\"00:06:06\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":4,\"Artist\":\"Artist 4\",\"Title\":\"Title 4\",\"Begin\":\"00:06:06\",\"End\":\"00:10:10\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":5,\"Artist\":\"Artist 5\",\"Title\":\"Title 5\",\"Begin\":\"00:10:10\",\"End\":\"00:15:15\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":6,\"Artist\":\"Artist 6\",\"Title\":\"Title 6\",\"Begin\":\"00:15:15\",\"End\":\"00:21:21\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":7,\"Artist\":\"Artist 7\",\"Title\":\"Title 7\",\"Begin\":\"00:21:21\",\"End\":\"00:28:28\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":8,\"Artist\":\"Artist 8\",\"Title\":\"Title 8\",\"Begin\":\"00:28:28\",\"End\":\"00:36:36\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":9,\"Artist\":\"Artist 9\",\"Title\":\"Title 9\",\"Begin\":\"00:36:36\",\"End\":\"00:45:45\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":10,\"Artist\":\"Artist 10\",\"Title\":\"Title 10\",\"Begin\":\"00:45:45\",\"End\":\"00:55:55\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true}],\"CDTextfile\":{\"FileName\":\"CDTextfile.cdt\"},\"Cataloguenumber\":{\"Value\":\"A123\"}}");
            var cuesheet = Projectfile.ImportFile(fileContent);
            Assert.AreEqual("CuesheetArtist", cuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", cuesheet.Title);
            Assert.AreEqual("AudioFile.mp3", cuesheet.Audiofile.FileName);
            Assert.IsFalse(cuesheet.Audiofile.IsRecorded);
            Assert.AreEqual("A123", cuesheet.Cataloguenumber.Value);
            Assert.IsTrue(cuesheet.Cataloguenumber.ValidationErrors.Count == 2);
            Assert.IsTrue(cuesheet.Tracks.Count == 10);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(3).Flags.Contains(Flag.DCP));
            Assert.IsTrue(cuesheet.Tracks.ElementAt(3).Flags.Contains(Flag.FourCH));
            Assert.AreEqual("Artist 10", cuesheet.Tracks.Last().Artist);
            Assert.AreEqual(new TimeSpan(0, 55, 55), cuesheet.Tracks.Last().End);
            Assert.IsTrue(Object.ReferenceEquals(cuesheet.Tracks.First(), cuesheet.GetPreviousLinkedTrack(cuesheet.Tracks.ElementAt(1))));
            Assert.AreEqual(cuesheet.Tracks.First(), cuesheet.GetPreviousLinkedTrack(cuesheet.Tracks.ElementAt(1)));
        }
    }
}