using AudioCuesheetEditor.Model.AudioCuesheet;
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
using AudioCuesheetEditorTests.Utility;
using System.Linq;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using System.IO;
using System.Text;
using AudioCuesheetEditorTests.Properties;
using System.Collections.Generic;

namespace AudioCuesheetEditor.Model.AudioCuesheet.Tests
{
    [TestClass()]
    public class CuesheetTests
    {
        [TestMethod()]
        public void AddTrackTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            var trackAddedFired = false;
            cuesheet.TrackAdded += delegate
            {
                trackAddedFired = true;
            };
            Assert.AreEqual(cuesheet.Tracks.Count, 0);
            cuesheet.AddTrack(new Track(), testHelper.ApplicationOptions);
            Assert.AreEqual(cuesheet.Tracks.Count, 1);
            Assert.IsTrue(trackAddedFired);
        }

        [TestMethod()]
        public void CuesheetTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            Assert.IsNull(cuesheet.Audiofile);
            var validationErrorAudioFile = cuesheet.GetValidationErrorsFiltered(String.Format("{0}.{1}", nameof(Cuesheet), nameof(Cuesheet.Audiofile))).FirstOrDefault();
            Assert.IsNotNull(validationErrorAudioFile);
            cuesheet.Audiofile = new Audiofile("AudioFile01.ogg");
            validationErrorAudioFile = cuesheet.GetValidationErrorsFiltered(nameof(Cuesheet.Audiofile)).FirstOrDefault();
            Assert.IsNull(validationErrorAudioFile);
        }

        [TestMethod()]
        public void EmptyCuesheetTracksValidationTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            Assert.AreEqual(cuesheet.Tracks.Count, 0);
            var validationErrorTracks = cuesheet.GetValidationErrorsFiltered(String.Format("{0}.{1}", nameof(Cuesheet), nameof(Cuesheet.Tracks))).FirstOrDefault();
            Assert.IsNotNull(validationErrorTracks);
            cuesheet.AddTrack(new Track(), testHelper.ApplicationOptions);
            validationErrorTracks = cuesheet.GetValidationErrorsFiltered(nameof(Cuesheet.Tracks)).FirstOrDefault();
            Assert.IsNull(validationErrorTracks);
        }

        [TestMethod()]
        public void MoveTrackTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            var track2 = new Track();
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            var track3 = new Track();
            cuesheet.AddTrack(track3, testHelper.ApplicationOptions);
            Assert.AreEqual(cuesheet.Tracks.Count, 3);
            Assert.IsTrue(track1.Position.Value == 1);
            cuesheet.MoveTrack(track1, MoveDirection.Up);
            Assert.IsTrue(track1.Position.Value == 1);
            Assert.IsTrue(track3.Position.Value == 3);
            cuesheet.MoveTrack(track3, MoveDirection.Down);
            Assert.IsTrue(track3.Position.Value == 3);
            Assert.IsTrue(track2.Position.Value == 2);
            cuesheet.MoveTrack(track2, MoveDirection.Up);
            Assert.AreEqual(track2, cuesheet.Tracks.ElementAt(0));
            Assert.AreEqual(track1, cuesheet.Tracks.ElementAt(1));
            cuesheet.MoveTrack(track2, MoveDirection.Down);
            cuesheet.MoveTrack(track2, MoveDirection.Down);
            Assert.AreEqual(track2, cuesheet.Tracks.ElementAt(2));
            Assert.AreEqual(track1, cuesheet.Tracks.ElementAt(0));
            Assert.AreEqual(track3, cuesheet.Tracks.ElementAt(1));
        }

        [TestMethod()]
        public void MoveAndDeleteTrackTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            var track1 = new Track
            {
                Artist = "Track 1",
                Title = "Track 1"
            };
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            var track2 = new Track
            {
                Title = "Track 2",
                Artist = "Track 2",
                Begin = new TimeSpan(0, 5, 0)
            };
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            var track3 = new Track
            {
                Artist = "Track 3",
                Title = "Track 3",
                Begin = new TimeSpan(0, 10, 0)
            };
            cuesheet.AddTrack(track3, testHelper.ApplicationOptions);
            var track4 = new Track
            {
                Artist = "Track 4",
                Title = "Track 4",
                Begin = new TimeSpan(0, 15, 0)
            };
            cuesheet.AddTrack(track4, testHelper.ApplicationOptions);
            var track5 = new Track
            {
                Artist = "Track 5",
                Title = "Track 5",
                Begin = new TimeSpan(0, 20, 0)
            };
            cuesheet.AddTrack(track5, testHelper.ApplicationOptions);
            cuesheet.RemoveTrack(track2);
            cuesheet.RemoveTrack(track4);
            Assert.AreEqual(3, cuesheet.Tracks.Count);
            track1.IsLinkedToPreviousTrack = true;
            track3.IsLinkedToPreviousTrack = true;
            track5.IsLinkedToPreviousTrack = true;
            var track1End = track1.End;
            testHelper.ApplicationOptions.LinkTracksWithPreviousOne = true;
            cuesheet.MoveTrack(track3, MoveDirection.Up);
            Assert.AreEqual((uint)1, track3.Position);
            Assert.AreEqual(track3, cuesheet.Tracks.ElementAt(0));
            Assert.AreEqual(track1, cuesheet.GetPreviousLinkedTrack(track5));
            Assert.AreEqual(TimeSpan.Zero, track3.Begin.Value);
            Assert.AreEqual(track1End, track1.Begin);
            cuesheet.MoveTrack(track5, MoveDirection.Up);
            Assert.AreEqual((uint)2, track5.Position);
            Assert.AreEqual(track5, cuesheet.Tracks.ElementAt(1));
            Assert.AreEqual(track5, cuesheet.GetPreviousLinkedTrack(track1));
            Assert.IsNull(cuesheet.Tracks.Last().End);
            //Reset for move down
            cuesheet.RemoveTracks(cuesheet.Tracks);
            track1 = new Track
            {
                Artist = "Track 1",
                Title = "Track 1"
            };
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            track2 = new Track
            {
                Title = "Track 2",
                Artist = "Track 2",
                Begin = new TimeSpan(0, 5, 0)
            };
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            track3 = new Track
            {
                Artist = "Track 3",
                Title = "Track 3",
                Begin = new TimeSpan(0, 10, 0)
            };
            cuesheet.AddTrack(track3, testHelper.ApplicationOptions);
            track4 = new Track
            {
                Artist = "Track 4",
                Title = "Track 4",
                Begin = new TimeSpan(0, 15, 0)
            };
            cuesheet.AddTrack(track4, testHelper.ApplicationOptions);
            track5 = new Track
            {
                Artist = "Track 5",
                Title = "Track 5",
                Begin = new TimeSpan(0, 20, 0)
            };
            cuesheet.AddTrack(track5, testHelper.ApplicationOptions);
            cuesheet.RemoveTrack(track2);
            cuesheet.RemoveTrack(track4);
            Assert.AreEqual(3, cuesheet.Tracks.Count);
            testHelper.ApplicationOptions.LinkTracksWithPreviousOne = true;
            track3.IsLinkedToPreviousTrack = true;
            track5.IsLinkedToPreviousTrack = true;
            track1End = track1.End;
            cuesheet.MoveTrack(track1, MoveDirection.Down);
            Assert.AreEqual(track1, cuesheet.Tracks.ElementAt(1));
            Assert.AreEqual(track3, cuesheet.GetPreviousLinkedTrack(track1));
            Assert.AreEqual((uint)2, track1.Position.Value);
            Assert.AreEqual((uint)3, track5.Position.Value);
            Assert.AreEqual(track1, cuesheet.GetPreviousLinkedTrack(track5));
            Assert.AreEqual(track1End, track3.End);
        }

        [TestMethod()]
        public void ImportTest()
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
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.IsValid);

            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            cuesheet.Import(textImportFile.Cuesheet, testHelper.ApplicationOptions);

            Assert.IsNull(cuesheet.CDTextfile);
            Assert.AreEqual(2, cuesheet.ValidationErrors.Count);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(0).IsValid);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(1).IsValid);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(2).IsValid);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(3).IsValid);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(4).IsValid);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(5).IsValid);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(6).IsValid);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(7).IsValid);

            File.Delete(tempFile);
        }

        [TestMethod()]
        public void ImportTestCalculateEndCorrectly()
        {
            var testHelper = new TestHelper();
            var textImportFile = new TextImportfile(new MemoryStream(Resources.Textimport_Bug_54));
            textImportFile.TextImportScheme.SchemeCuesheet = String.Empty;
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 39);
            Assert.IsTrue(textImportFile.IsValid);
            var cuesheet = new Cuesheet();
            cuesheet.Import(textImportFile.Cuesheet, testHelper.ApplicationOptions);
            Assert.IsTrue(cuesheet.Tracks.Count == 39);
            Assert.IsTrue(cuesheet.Tracks.ElementAt(0).End == new TimeSpan(0, 5, 24));
            Assert.IsTrue(cuesheet.Tracks.ElementAt(38).Begin == new TimeSpan(3, 13, 13));
        }

        [TestMethod()]
        public void RecordTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            Assert.IsFalse(cuesheet.IsRecording);
            Assert.IsNull(cuesheet.RecordingTime);
            cuesheet.StartRecording();
            Assert.IsTrue(cuesheet.IsRecording);
            Assert.IsNotNull(cuesheet.RecordingTime);
            var track = new Track();
            Assert.IsNull(track.Begin);
            Assert.IsNull(track.End);
            cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            Assert.AreEqual(TimeSpan.Zero, track.Begin);
            Assert.IsNull(track.End);
            var track2 = new Track();
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            Assert.IsNotNull(track.End);
            Assert.AreNotEqual(TimeSpan.Zero, track.End);
            //Now lets test with another RecordTimeSensitivity
            cuesheet = new Cuesheet();
            cuesheet.StartRecording();
            track = new Track();
            testHelper.ApplicationOptions.RecordTimeSensitivity = Options.TimeSensitivityMode.Seconds;
            cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            Assert.AreEqual(TimeSpan.Zero, track.Begin);
            Assert.IsNull(track.End);
            System.Threading.Thread.Sleep(3000);
            track2 = new Track();
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            Assert.IsNotNull(track.End);
            Assert.AreNotEqual(TimeSpan.Zero, track.End);
            Assert.AreEqual(0, track.End.Value.Milliseconds);
            cuesheet.StopRecording(testHelper.ApplicationOptions);
            Assert.AreEqual(track.End.Value, track2.Begin.Value);
            Assert.AreEqual(0, track2.End.Value.Milliseconds);
        }

        [TestMethod()]
        public void TrackRecalculationTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            var track3 = new Track();
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track3, testHelper.ApplicationOptions);
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
            Assert.IsNull(track1.End);
            Assert.IsNull(track2.End);
            track3.End = new TimeSpan(0, 12, 14);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.Begin);
            track3.Begin = new TimeSpan(0, 7, 56);
            Assert.IsNull(track2.End);
        }
        [TestMethod()]
        public void TrackOverlappingTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            var track3 = new Track();
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track3, testHelper.ApplicationOptions);
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

        [TestMethod()]
        public void RemoveTrackTest()
        {
            var testHelper = new TestHelper();
            testHelper.ApplicationOptions.LinkTracksWithPreviousOne = true;
            var cuesheet = new Cuesheet();
            var track1 = new Track() { Artist="1", Title="1"};
            var track2 = new Track() { Artist = "2", Title = "2" };
            var track3 = new Track() { Artist = "3", Title = "3" };
            var track4 = new Track() { Artist = "4", Title = "4" };
            var track5 = new Track() { Artist = "5", Title = "5" };
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track3, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track4, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track5, testHelper.ApplicationOptions);
            track1.End = new TimeSpan(0, 5, 0);
            track2.End = new TimeSpan(0, 10, 0);
            track3.End = new TimeSpan(0, 15, 0);
            track4.End = new TimeSpan(0, 20, 0);
            track5.End = new TimeSpan(0, 25, 0);
            Assert.AreEqual(5, cuesheet.Tracks.Count);
            cuesheet.RemoveTrack(track2);
            Assert.AreEqual((uint)2, track3.Position.Value);
            Assert.AreEqual((uint)3, track4.Position.Value);
            Assert.AreEqual((uint)4, track5.Position.Value);
            testHelper = new TestHelper();
            testHelper.ApplicationOptions.LinkTracksWithPreviousOne = true;
            cuesheet = new Cuesheet();
            track1 = new Track
            {
                Artist = "Track 1",
                Title = "Track 1"
            };
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            track2 = new Track
            {
                Title = "Track 2",
                Artist = "Track 2",
                Begin = new TimeSpan(0, 5, 0)
            };
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            track3 = new Track
            {
                Artist = "Track 3",
                Title = "Track 3",
                Begin = new TimeSpan(0, 10, 0)
            };
            cuesheet.AddTrack(track3, testHelper.ApplicationOptions);
            track4 = new Track
            {
                Artist = "Track 4",
                Title = "Track 4",
                Begin = new TimeSpan(0, 15, 0)
            };
            cuesheet.AddTrack(track4, testHelper.ApplicationOptions);
            track5 = new Track
            {
                Artist = "Track 5",
                Title = "Track 5",
                Begin = new TimeSpan(0, 20, 0)
            };
            cuesheet.AddTrack(track5, testHelper.ApplicationOptions);
            var list = new List<Track>() { track2, track4 };
            cuesheet.RemoveTracks(list.AsReadOnly());
            Assert.AreEqual(3, cuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(0, 5, 0), track3.Begin);
            Assert.AreEqual(new TimeSpan(0, 15, 0), track5.Begin);
            Assert.AreEqual((uint)1, track1.Position);
            Assert.AreEqual((uint)2, track3.Position);
            Assert.AreEqual((uint)3, track5.Position);
        }

        [TestMethod()]
        public void UnlickedTrackTest()
        {
            var cuesheet = new Cuesheet();
            var testHelper = new TestHelper();
            var track1 = new Track
            {
                Position = 1,
                End = new TimeSpan(0, 3, 23)
            };
            var track2 = new Track 
            { 
                Position = 2
            };
            var track3 = new Track
            {
                Position = 3
            };
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track3, testHelper.ApplicationOptions);
            Assert.AreEqual(track1.End, track2.Begin);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.Begin);
            track3.Begin = new TimeSpan(0, 7, 32);
            Assert.IsNull(track2.End);
        }

        [TestMethod()]
        public void TrackPositionChangedTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            var track1 = new Track
            {
                Position = 3,
                End = new TimeSpan(0, 5, 0)
            };
            var track2 = new Track
            {
                Position = 2,
                Begin = track1.End,
                End = new TimeSpan(0, 7, 30)
            };
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            Assert.AreEqual(track2, cuesheet.Tracks.First());
            Assert.AreEqual(track1, cuesheet.Tracks.Last());
            track1.Position = 1;
            Assert.AreEqual(track1, cuesheet.Tracks.First());
            Assert.AreEqual(track2, cuesheet.Tracks.Last());
        }

        [TestMethod()]
        public void TrackLengthChangedWithIsLinkedToPreivousTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            cuesheet.AddTrack(track2, testHelper.ApplicationOptions);
            track1.IsLinkedToPreviousTrack = true;
            track2.IsLinkedToPreviousTrack = true;
            Assert.IsNull(track2.Begin);
            Assert.IsNotNull(track1.Begin);
            var editedTrack = new Track() { Begin = TimeSpan.Zero, Length = new TimeSpan(0, 2, 23) };
            track1.CopyValues(editedTrack);
            Assert.IsNotNull(track1.End);
            Assert.AreEqual(track1.End, track2.Begin);
            Assert.AreEqual(editedTrack.End, track2.Begin);
        }
    }
}