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
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace AudioCuesheetEditor.Tests.Model.IO
{
    [TestClass()]
    public class ProjectfileTests
    {
        [TestMethod()]
        public void GenerateFile_WithoutSections_GeneratesOneFile()
        {
            // Arrange
            var cuesheet = new Cuesheet
            {
                Artist = "CuesheetArtist",
                Title = "CuesheetTitle",
                Audiofile = new Audiofile("AudioFile.mp3"),
                CDTextfile = new CDTextfile("CDTextfile.cdt"),
                Cataloguenumber = "A123"
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i <= 10; i++)
            {
                var track = new Track
                {
                    Position = (uint)i,
                    Artist = string.Format("Artist {0}", i),
                    Title = string.Format("Title {0}", i),
                    Begin = begin
                };
                var rand = new Random();
                var flagsToAdd = rand.Next(1, 3);
                var flags = new List<Flag>();
                for (int x = 0; x < flagsToAdd; x++)
                {
                    flags.Add(Flag.AvailableFlags.ElementAt(x));
                }
                track.Flags = flags;
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track);
            }
            // Act
            var projectFile = new Projectfile(cuesheet);
            // Assert
            var generatedFile = projectFile.GenerateFile();
            Assert.IsNotNull(generatedFile);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            var fileContent = File.ReadAllLines(fileName);
            var json = JsonSerializer.Serialize(cuesheet, Projectfile.Options);
            Assert.AreEqual(json, fileContent.FirstOrDefault());
            File.Delete(fileName);
        }

        [TestMethod()]
        public void GenerateFile_WithSections_GeneratesOneFile()
        {
            // Arrange
            var cuesheet = new Cuesheet
            {
                Artist = "CuesheetArtist",
                Title = "CuesheetTitle",
                Audiofile = new Audiofile("AudioFile.mp3"),
                CDTextfile = new CDTextfile("CDTextfile.cdt"),
                Cataloguenumber = "A123"
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i <= 10; i++)
            {
                var track = new Track
                {
                    Position = (uint)i,
                    Artist = string.Format("Artist {0}", i),
                    Title = string.Format("Title {0}", i),
                    Begin = begin
                };
                var rand = new Random();
                var flagsToAdd = rand.Next(1, 3);
                var flags = new List<Flag>();
                for (int x = 0; x < flagsToAdd; x++)
                {
                    flags.Add(Flag.AvailableFlags.ElementAt(x));
                }
                track.Flags = flags;
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track);
            }
            var section = cuesheet.AddSection();
            section.Begin = new TimeSpan(0, 30, 0);
            section = cuesheet.AddSection();
            section.Begin = new TimeSpan(1, 0, 0);
            var projectFile = new Projectfile(cuesheet);
            // Act 
            var generatedFile = projectFile.GenerateFile();
            // Assert
            Assert.IsNotNull(generatedFile);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            var fileContent = File.ReadAllLines(fileName);
            var json = JsonSerializer.Serialize(cuesheet, Projectfile.Options);
            Assert.AreEqual(json, fileContent.FirstOrDefault());
            File.Delete(fileName);
        }

        [TestMethod()]
        public void ImportFile_ValidProjectfile_ShouldImportFile()
        {
            // Arrange
            var fileContent = Encoding.UTF8.GetBytes("{\"Artist\":\"CuesheetArtist\",\"Title\":\"CuesheetTitle\",\"Cataloguenumber\":\"A123\",\"Audiofile\":{\"Name\":\"AudioFile.mp3\"},\"Tracks\":[{\"Position\":1,\"Artist\":\"Artist 1\",\"Title\":\"Title 1\",\"Begin\":\"00:00:00\",\"End\":\"00:01:01\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":2,\"Artist\":\"Artist 2\",\"Title\":\"Title 2\",\"Begin\":\"00:01:01\",\"End\":\"00:03:03\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":3,\"Artist\":\"Artist 3\",\"Title\":\"Title 3\",\"Begin\":\"00:03:03\",\"End\":\"00:06:06\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":4,\"Artist\":\"Artist 4\",\"Title\":\"Title 4\",\"Begin\":\"00:06:06\",\"End\":\"00:10:10\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":5,\"Artist\":\"Artist 5\",\"Title\":\"Title 5\",\"Begin\":\"00:10:10\",\"End\":\"00:15:15\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":6,\"Artist\":\"Artist 6\",\"Title\":\"Title 6\",\"Begin\":\"00:15:15\",\"End\":\"00:21:21\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":7,\"Artist\":\"Artist 7\",\"Title\":\"Title 7\",\"Begin\":\"00:21:21\",\"End\":\"00:28:28\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":8,\"Artist\":\"Artist 8\",\"Title\":\"Title 8\",\"Begin\":\"00:28:28\",\"End\":\"00:36:36\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":9,\"Artist\":\"Artist 9\",\"Title\":\"Title 9\",\"Begin\":\"00:36:36\",\"End\":\"00:45:45\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":10,\"Artist\":\"Artist 10\",\"Title\":\"Title 10\",\"Begin\":\"00:45:45\",\"End\":\"00:55:55\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true}],\"CDTextfile\":{\"Name\":\"CDTextfile.cdt\"}}");
            // Act
            var cuesheet = Projectfile.ImportFile(fileContent);
            // Assert
            Assert.IsNotNull(cuesheet);
            Assert.IsTrue(cuesheet.Tracks.All(x => x.Cuesheet == cuesheet));
            Assert.AreEqual("CuesheetArtist", cuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", cuesheet.Title);
            Assert.AreEqual("AudioFile.mp3", cuesheet.Audiofile?.Name);
            Assert.IsFalse(cuesheet.Audiofile?.IsRecorded);
            Assert.AreEqual("A123", cuesheet.Cataloguenumber);
            Assert.AreEqual(2, cuesheet.Validate(nameof(Cuesheet.Cataloguenumber)).ValidationMessages?.Count);
            Assert.AreEqual(10, cuesheet.Tracks.Count);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(3).Flags.Contains(Flag.DCP));
            Assert.IsTrue(cuesheet.Tracks.ElementAt(3).Flags.Contains(Flag.FourCH));
            Assert.AreEqual("Artist 10", cuesheet.Tracks.Last().Artist);
            Assert.AreEqual(new TimeSpan(0, 55, 55), cuesheet.Tracks.Last().End);
            Assert.IsTrue(ReferenceEquals(cuesheet.Tracks.First(), cuesheet.GetPreviousLinkedTrack(cuesheet.Tracks.ElementAt(1))));
            Assert.AreEqual(cuesheet.Tracks.First(), cuesheet.GetPreviousLinkedTrack(cuesheet.Tracks.ElementAt(1)));
            Assert.AreEqual((uint)10, cuesheet.Tracks.Last().Position);
        }

        [TestMethod()]
        public void ImportFile_ValidProjectfileWithSections_ShouldImportFile()
        {
            //Arrange
            var fileContent = Encoding.UTF8.GetBytes("{\"Tracks\":[{\"Position\":1,\"Artist\":\"Artist 1\",\"Title\":\"Title 1\",\"Begin\":\"00:00:00\",\"End\":\"00:01:01\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":2,\"Artist\":\"Artist 2\",\"Title\":\"Title 2\",\"Begin\":\"00:01:01\",\"End\":\"00:03:03\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":3,\"Artist\":\"Artist 3\",\"Title\":\"Title 3\",\"Begin\":\"00:03:03\",\"End\":\"00:06:06\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":4,\"Artist\":\"Artist 4\",\"Title\":\"Title 4\",\"Begin\":\"00:06:06\",\"End\":\"00:10:10\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":5,\"Artist\":\"Artist 5\",\"Title\":\"Title 5\",\"Begin\":\"00:10:10\",\"End\":\"00:15:15\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":6,\"Artist\":\"Artist 6\",\"Title\":\"Title 6\",\"Begin\":\"00:15:15\",\"End\":\"00:21:21\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":7,\"Artist\":\"Artist 7\",\"Title\":\"Title 7\",\"Begin\":\"00:21:21\",\"End\":\"00:28:28\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":8,\"Artist\":\"Artist 8\",\"Title\":\"Title 8\",\"Begin\":\"00:28:28\",\"End\":\"00:36:36\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":9,\"Artist\":\"Artist 9\",\"Title\":\"Title 9\",\"Begin\":\"00:36:36\",\"End\":\"00:45:45\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":10,\"Artist\":\"Artist 10\",\"Title\":\"Title 10\",\"Begin\":\"00:45:45\",\"End\":\"00:55:55\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true}],\"Artist\":\"CuesheetArtist\",\"Title\":\"CuesheetTitle\",\"Audiofile\":{\"Name\":\"AudioFile.mp3\"},\"CDTextfile\":{\"Name\":\"CDTextfile.cdt\"},\"Cataloguenumber\":\"A123\",\"Sections\":[{\"Artist\":\"CuesheetArtist\",\"Title\":\"CuesheetTitle\",\"Begin\":\"00:30:00\"},{\"Artist\":\"CuesheetArtist\",\"Title\":\"CuesheetTitle\",\"Begin\":\"01:00:00\"}]}");
            // Act
            var cuesheet = Projectfile.ImportFile(fileContent);
            // Assert
            Assert.IsNotNull(cuesheet);
            Assert.IsTrue(cuesheet.Tracks.All(x => x.Cuesheet == cuesheet));
            Assert.AreEqual("CuesheetArtist", cuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", cuesheet.Title);
            Assert.AreEqual("AudioFile.mp3", cuesheet.Audiofile?.Name);
            Assert.IsFalse(cuesheet.Audiofile?.IsRecorded);
            Assert.AreEqual("A123", cuesheet.Cataloguenumber);
            Assert.AreEqual(2, cuesheet.Validate(nameof(Cuesheet.Cataloguenumber)).ValidationMessages?.Count);
            Assert.AreEqual(10, cuesheet.Tracks.Count);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(3).Flags.Contains(Flag.DCP));
            Assert.IsTrue(cuesheet.Tracks.ElementAt(3).Flags.Contains(Flag.FourCH));
            Assert.AreEqual("Artist 10", cuesheet.Tracks.Last().Artist);
            Assert.AreEqual(new TimeSpan(0, 55, 55), cuesheet.Tracks.Last().End);
            Assert.IsTrue(ReferenceEquals(cuesheet.Tracks.First(), cuesheet.GetPreviousLinkedTrack(cuesheet.Tracks.ElementAt(1))));
            Assert.AreEqual(cuesheet.Tracks.First(), cuesheet.GetPreviousLinkedTrack(cuesheet.Tracks.ElementAt(1)));
            Assert.AreEqual((uint)10, cuesheet.Tracks.Last().Position);
            Assert.AreEqual(2, cuesheet.Sections.Count);
            Assert.AreEqual(new TimeSpan(0, 30, 0), cuesheet.Sections.First().Begin);
            Assert.AreEqual(new TimeSpan(1, 0, 0), cuesheet.Sections.Last().Begin);
        }
    }
}