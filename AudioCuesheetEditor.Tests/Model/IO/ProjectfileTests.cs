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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                Audiofiles = [
                    new () { Name = "AudioFile.mp3" },
                    new () { Name = "Other audiofile.wav" }
                ],
                CDTextfile = new CDTextfile("CDTextfile.cdt"),
                Cataloguenumber = "A123"
            };
            var begin = TimeSpan.Zero;
            var tracks = new List<Track>();
            for (ushort i = 1; i <= 10; i++)
            {
                var track = new Track
                {
                    Position = i,
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
                track.Cuesheet = cuesheet;
                tracks.Add(track);
            }
            cuesheet.Audiofiles.First().Tracks = tracks.GetRange(0, 5);
            cuesheet.Audiofiles.Last().Tracks = tracks.GetRange(4, 5);
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
        public void ImportFile_ValidProjectfile_ShouldImportFile()
        {
            // Arrange
            var fileContent = "{\"Artist\":\"CuesheetArtist\",\"Title\":\"CuesheetTitle\",\"Cataloguenumber\":\"A123\",\"Audiofiles\":[{\"Name\":\"AudioFile.mp3\",\"Duration\":\"00:05:48.0608330\",\"AudioCodec\":{\"MimeType\":\"audio/mpeg\",\"FileExtension\":\".mp3\",\"Name\":\"AudioCodec MP3\"},\"Tracks\":[{\"Position\":1,\"Artist\":\"Artist 1\",\"Title\":\"Title 1\",\"Begin\":\"00:00:00\",\"End\":\"00:01:01\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":2,\"Artist\":\"Artist 2\",\"Title\":\"Title 2\",\"Begin\":\"00:01:01\",\"End\":\"00:03:03\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":3,\"Artist\":\"Artist 3\",\"Title\":\"Title 3\",\"Begin\":\"00:03:03\",\"End\":\"00:06:06\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":4,\"Artist\":\"Artist 4\",\"Title\":\"Title 4\",\"Begin\":\"00:06:06\",\"End\":\"00:10:10\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":5,\"Artist\":\"Artist 5\",\"Title\":\"Title 5\",\"Begin\":\"00:10:10\",\"End\":\"00:15:15\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true}]},{\"Name\":\"Other audiofile.wav\",\"Duration\":\"00:05:48.0608330\",\"AudioCodec\":{\"MimeType\":\"audio/wav\",\"FileExtension\":\".wav\",\"Name\":\"AudioCodec WAVE\"},\"Tracks\":[{\"Position\":6,\"Artist\":\"Artist 6\",\"Title\":\"Title 6\",\"Begin\":\"00:15:15\",\"End\":\"00:21:21\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":7,\"Artist\":\"Artist 7\",\"Title\":\"Title 7\",\"Begin\":\"00:21:21\",\"End\":\"00:28:28\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":8,\"Artist\":\"Artist 8\",\"Title\":\"Title 8\",\"Begin\":\"00:28:28\",\"End\":\"00:36:36\",\"Flags\":[\"4CH\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":9,\"Artist\":\"Artist 9\",\"Title\":\"Title 9\",\"Begin\":\"00:36:36\",\"End\":\"00:45:45\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true},{\"Position\":10,\"Artist\":\"Artist 10\",\"Title\":\"Title 10\",\"Begin\":\"00:45:45\",\"End\":\"00:55:55\",\"Flags\":[\"4CH\",\"DCP\"],\"IsLinkedToPreviousTrack\":true}]}],\"CDTextfile\":{\"Name\":\"CDTextfile.cdt\"}}";
            // Act
            var cuesheet = Projectfile.ImportFile(fileContent);
            // Assert
            Assert.IsNotNull(cuesheet);
            Assert.HasCount(2, cuesheet.Audiofiles);
            Assert.HasCount(5, cuesheet.Audiofiles.First().Tracks);
            Assert.HasCount(5, cuesheet.Audiofiles.Last().Tracks);
            Assert.IsTrue(cuesheet.Audiofiles.SelectMany(x => x.Tracks).All(x => x.Cuesheet == cuesheet));
            Assert.IsTrue(cuesheet.Audiofiles.SelectMany(x => x.Tracks).All(x => x.IsLinkedToPreviousTrack));
            Assert.AreEqual("CuesheetArtist", cuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", cuesheet.Title);
            Assert.AreEqual("AudioFile.mp3", cuesheet.Audiofiles.First().Name);
            Assert.AreEqual("A123", cuesheet.Cataloguenumber);
            Assert.AreEqual(2, cuesheet.Validate(nameof(Cuesheet.Cataloguenumber)).ValidationMessages?.Count);
            Assert.HasCount(10, cuesheet.Audiofiles.SelectMany(x => x.Tracks));
            Assert.Contains(Flag.DCP, cuesheet.Audiofiles.First().Tracks.ElementAt(3).Flags);
            Assert.Contains(Flag.FourCH, cuesheet.Audiofiles.First().Tracks.ElementAt(3).Flags);
            Assert.AreEqual("Artist 10", cuesheet.Audiofiles.Last().Tracks.Last().Artist);
            Assert.AreEqual(new TimeSpan(0, 55, 55), cuesheet.Audiofiles.Last().Tracks.Last().End);
            Assert.AreEqual((ushort)10, cuesheet.Audiofiles.Last().Tracks.Last().Position);
        }
    }
}