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
using AudioCuesheetEditor.Model.AudioCuesheet;
using System;
using System.Collections.Generic;
using System.Text;
using AudioCuesheetEditorTests.Utility;
using System.Linq;
using System.IO;
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditorTests.Properties;

namespace AudioCuesheetEditor.Model.AudioCuesheet.Tests
{
    [TestClass()]
    public class CuesheetTests
    {
        [TestMethod()]
        public void AddTrackTest()
        {
            var cuesheet = new Cuesheet();
            Assert.AreEqual(cuesheet.Tracks.Count, 0);
            cuesheet.AddTrack(new Track());
            Assert.AreEqual(cuesheet.Tracks.Count, 1);
        }

        [TestMethod()]
        public void CuesheetTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            Assert.IsNull(cuesheet.AudioFile);
            var validationErrorAudioFile = cuesheet.GetValidationErrorsFiltered(String.Format("{0}.{1}", nameof(Cuesheet), nameof(Cuesheet.AudioFile))).FirstOrDefault();
            Assert.IsNotNull(validationErrorAudioFile);
            cuesheet.AudioFile = new IO.AudioFile("AudioFile01.ogg");
            validationErrorAudioFile = cuesheet.GetValidationErrorsFiltered(nameof(Cuesheet.AudioFile)).FirstOrDefault();
            Assert.IsNull(validationErrorAudioFile);
        }

        [TestMethod()]
        public void EmptyCuesheetTracksValidationTest()
        {
            var cuesheet = new Cuesheet();
            Assert.AreEqual(cuesheet.Tracks.Count, 0);
            var validationErrorTracks = cuesheet.GetValidationErrorsFiltered(String.Format("{0}.{1}", nameof(Cuesheet), nameof(Cuesheet.Tracks))).FirstOrDefault();
            Assert.IsNotNull(validationErrorTracks);
            cuesheet.AddTrack(new Track());
            validationErrorTracks = cuesheet.GetValidationErrorsFiltered(nameof(Cuesheet.Tracks)).FirstOrDefault();
            Assert.IsNull(validationErrorTracks);
        }

        [TestMethod()]
        public void MoveTrackTest()
        {
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            cuesheet.AddTrack(track1);
            var track2 = new Track();
            cuesheet.AddTrack(track2);
            var track3 = new Track();
            cuesheet.AddTrack(track3);
            Assert.AreEqual(cuesheet.Tracks.Count, 3);
            Assert.IsTrue(track1.Position.Value == 1);
            cuesheet.MoveTrack(track1, MoveDirection.Up);
            Assert.IsTrue(track1.Position.Value == 1);
            Assert.IsTrue(track3.Position.Value == 3);
            cuesheet.MoveTrack(track3, MoveDirection.Down);
            Assert.IsTrue(track3.Position.Value == 3);
            Assert.IsTrue(track2.Position.Value == 2);
            cuesheet.MoveTrack(track2, MoveDirection.Up);
            Assert.IsTrue(track2.Position.Value == 1);
            Assert.IsTrue(track1.Position.Value == 2);
            cuesheet.MoveTrack(track2, MoveDirection.Down);
            cuesheet.MoveTrack(track2, MoveDirection.Down);
            Assert.IsTrue(track2.Position.Value == 3);
            Assert.IsTrue(track1.Position.Value == 1);
            Assert.IsTrue(track3.Position.Value == 2);
        }

        [TestMethod()]
        public void ImportTest()
        {
            //Prepare text input file
            StringBuilder builder = new StringBuilder();
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
            var textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)))
            {
                ImportScheme = "%Artist% - %Title%[\t]{1,}%End%"
            };
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.IsValid);

            var cuesheet = new Cuesheet();
            cuesheet.Import(textImportFile);

            Assert.IsNull(cuesheet.CDTextfile);
            Assert.AreEqual(cuesheet.ValidationErrors.Count, 5);

            File.Delete(tempFile);
        }

        [TestMethod()]
        public void ImportTestCalculateEndCorrectly()
        {
            var textImportFile = new TextImportFile(new MemoryStream(Resources.Textimport_Bug_54))
            {
                ImportScheme = "%Artist% - %Title%[\t]{1,}%End%"
            };
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Tracks.Count == 39);
            Assert.IsTrue(textImportFile.IsValid);
            var cuesheet = new Cuesheet();
            cuesheet.Import(textImportFile);
            Assert.IsTrue(cuesheet.Tracks.Count == 39);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(0).End == new TimeSpan(0, 5, 24));
            Assert.IsTrue(cuesheet.Tracks.ElementAt(38).Begin == new TimeSpan(3, 13, 13));
        }

        [TestMethod()]
        public void RecordTest()
        {
            var cuesheet = new Cuesheet();
            Assert.IsFalse(cuesheet.IsRecording);
            Assert.IsNull(cuesheet.RecordingTime);
            cuesheet.StartRecording();
            Assert.IsTrue(cuesheet.IsRecording);
            Assert.IsNotNull(cuesheet.RecordingTime);
        }

        [TestMethod()]
        public void TrackRecalculationTest()
        {
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            var track3 = new Track();
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            Assert.AreEqual(track1.Position.Value, (uint)1);
            Assert.AreEqual(track2.Position.Value, (uint)2);
            Assert.AreEqual(track3.Position.Value, (uint)3);
            Assert.AreEqual(track1.Begin, TimeSpan.Zero);
            Assert.IsNull(track1.End);
            Assert.IsNull(track2.Begin);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.Begin);
            Assert.IsNull(track3.End);
            track1.Begin = TimeSpan.Zero;
            track2.Begin = new TimeSpan(0, 2, 43);
            Assert.AreEqual(track1.End, new TimeSpan(0, 2, 43));
            Assert.IsNull(track2.End);
            track3.End = new TimeSpan(0, 12, 14);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.Begin);
            track3.Begin = new TimeSpan(0, 7, 56);
            Assert.AreEqual(track2.End, new TimeSpan(0, 7, 56));
        }
        [TestMethod()]
        public void TrackOverlappingTest()
        {
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            var track3 = new Track();
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            Assert.AreEqual(track1.Position.Value, (uint)1);
            Assert.AreEqual(track2.Position.Value, (uint)2);
            Assert.AreEqual(track3.Position.Value, (uint)3);
            track1.Position = 1;
            track2.Position = 1;
            track3.Position = 1;
            track1.End = new TimeSpan(0, 2, 30);
            track2.Begin = new TimeSpan(0, 2, 0);
            track2.End = new TimeSpan(0, 5, 30);
            track3.Begin = new TimeSpan(0, 4, 54);
            track3.End = new TimeSpan(0, 8, 12);
            var validationErrors = track1.GetValidationErrorsFiltered(nameof(Track.Position));
            Assert.IsTrue(validationErrors.Count >= 1);
            validationErrors = track2.GetValidationErrorsFiltered(nameof(Track.Position));
            Assert.IsTrue(validationErrors.Count >= 1);
            validationErrors = track3.GetValidationErrorsFiltered(nameof(Track.Position));
            Assert.IsTrue(validationErrors.Count >= 1);
            validationErrors = track2.GetValidationErrorsFiltered(nameof(Track.Begin));
            Assert.IsTrue(validationErrors.Count >= 1);
            validationErrors = track3.GetValidationErrorsFiltered(nameof(Track.Begin));
            Assert.IsTrue(validationErrors.Count >= 1);
            track2.End = new TimeSpan(0, 5, 15);
            validationErrors = track2.GetValidationErrorsFiltered(nameof(Track.End));
            Assert.IsTrue(validationErrors.Count >= 1);
            track1.Position = 1;
            track2.Position = 2;
            track3.Position = 3;
            var clone = track1.Clone();
            validationErrors = clone.GetValidationErrorsFiltered(nameof(Track.Position));
            Assert.IsTrue(validationErrors.Count == 0);
            clone.Position = 2;
            validationErrors = clone.GetValidationErrorsFiltered(nameof(Track.Position));
            Assert.IsTrue(validationErrors.Count == 1);
            clone.Position = 4;
            validationErrors = clone.GetValidationErrorsFiltered(nameof(Track.Position));
            Assert.IsTrue(validationErrors.Count == 0);
        }
    }
}