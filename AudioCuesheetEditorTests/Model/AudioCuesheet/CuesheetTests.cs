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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditorTests.Properties;
using AudioCuesheetEditorTests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            var validationErrorAudioFile = cuesheet.Validate(x => x.Audiofile);
            Assert.AreEqual(ValidationStatus.Error, validationErrorAudioFile.Status);
            cuesheet.Audiofile = new Audiofile("AudioFile01.ogg");
            validationErrorAudioFile = cuesheet.Validate(x => x.Audiofile);
            Assert.AreEqual(ValidationStatus.Success, validationErrorAudioFile.Status);
        }

        [TestMethod()]
        public void EmptyCuesheetTracksValidationTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            Assert.AreEqual(cuesheet.Tracks.Count, 0);
            var validationErrorTracks = cuesheet.Validate(x => x.Tracks);
            Assert.AreEqual(ValidationStatus.Error, validationErrorTracks.Status);
            cuesheet.AddTrack(new Track(), testHelper.ApplicationOptions);
            validationErrorTracks = cuesheet.Validate(x => x.Tracks);
            Assert.AreEqual(ValidationStatus.Success, validationErrorTracks.Status);
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
            Assert.AreEqual((uint)1, track1.Position);
            cuesheet.MoveTrack(track1, MoveDirection.Up);
            Assert.AreEqual((uint)1, track1.Position);
            Assert.AreEqual((uint)3, track3.Position);
            cuesheet.MoveTrack(track3, MoveDirection.Down);
            Assert.AreEqual((uint)3, track3.Position);
            Assert.AreEqual((uint)2, track2.Position);
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
            Assert.AreEqual((uint)2, track1.Position);
            Assert.AreEqual((uint)3, track5.Position);
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
            Assert.IsNotNull(textImportFile.Cuesheet);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.IsValid);

            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            cuesheet.Import(textImportFile.Cuesheet, testHelper.ApplicationOptions);

            Assert.IsNull(cuesheet.CDTextfile);
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Tracks.ElementAt(0).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Tracks.ElementAt(1).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Tracks.ElementAt(2).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Tracks.ElementAt(3).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Tracks.ElementAt(4).Validate().Status);    
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Tracks.ElementAt(5).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Tracks.ElementAt(6).Validate().Status);    
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Tracks.ElementAt(7).Validate().Status);

            File.Delete(tempFile);
        }

        [TestMethod()]
        public void ImportTestCalculateEndCorrectly()
        {
            var testHelper = new TestHelper();
            var textImportFile = new TextImportfile(new MemoryStream(Resources.Textimport_Bug_54));
            textImportFile.TextImportScheme.SchemeCuesheet = String.Empty;
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsNotNull(textImportFile.Cuesheet);
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
            Assert.IsNotNull(track.End);
            Assert.AreEqual(track.End, track2.Begin);
            Assert.AreEqual(0, track2.End?.Milliseconds);
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
            Assert.AreEqual((uint)1, track1.Position);
            Assert.AreEqual((uint)2, track2.Position);
            Assert.AreEqual((uint)3, track3.Position);
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
            Assert.AreEqual((uint)1, track1.Position);
            Assert.AreEqual((uint)2, track2.Position);
            Assert.AreEqual((uint)3, track3.Position);
            var validationResult = cuesheet.Validate(x => x.Tracks);
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
            track1.Position = 1;
            track2.Position = 1;
            track3.Position = 1;
            track1.End = new TimeSpan(0, 2, 30);
            track2.Begin = new TimeSpan(0, 2, 0);
            track2.End = new TimeSpan(0, 5, 30);
            track3.Begin = new TimeSpan(0, 4, 54);
            track3.End = new TimeSpan(0, 8, 12);
            validationResult = cuesheet.Validate(x => x.Tracks);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual(7, validationResult.ValidationMessages?.Count);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0} {1} '{2}' is used also by {3}({4},{5},{6},{7},{8}). Positions must be unique!" && x.Parameter != null && x.Parameter[2].Equals(track1.Position) && x.Parameter[7].Equals(track1.Begin) && x.Parameter[8].Equals(track1.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0} {1} '{2}' is used also by {3}({4},{5},{6},{7},{8}). Positions must be unique!" && x.Parameter != null && x.Parameter[2].Equals(track2.Position) && x.Parameter[7].Equals(track2.Begin) && x.Parameter[8].Equals(track2.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0} {1} '{2}' is used also by {3}({4},{5},{6},{7},{8}). Positions must be unique!" && x.Parameter != null && x.Parameter[2].Equals(track3.Position) && x.Parameter[7].Equals(track3.Begin) && x.Parameter[8].Equals(track3.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track1.Position) && x.Parameter[4].Equals(track1.Begin) && x.Parameter[5].Equals(track1.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track1.Position) && x.Parameter[9].Equals(track1.Begin) && x.Parameter[10].Equals(track1.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track3.Position) && x.Parameter[9].Equals(track3.Begin) && x.Parameter[10].Equals(track3.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track3.Position) && x.Parameter[4].Equals(track3.Begin) && x.Parameter[5].Equals(track3.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            track2.End = new TimeSpan(0, 5, 15);
            validationResult = cuesheet.Validate(x => x.Tracks);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track3.Position) && x.Parameter[9].Equals(track3.Begin) && x.Parameter[10].Equals(track3.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track3.Position) && x.Parameter[4].Equals(track3.Begin) && x.Parameter[5].Equals(track3.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            track1.Position = 1;
            track2.Position = 2;
            track3.Position = 3;
            validationResult = cuesheet.Validate(x => x.Tracks);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual(4, validationResult.ValidationMessages?.Count);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track1.Position) && x.Parameter[4].Equals(track1.Begin) && x.Parameter[5].Equals(track1.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track1.Position) && x.Parameter[9].Equals(track1.Begin) && x.Parameter[10].Equals(track1.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track3.Position) && x.Parameter[9].Equals(track3.Begin) && x.Parameter[10].Equals(track3.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track3.Position) && x.Parameter[4].Equals(track3.Begin) && x.Parameter[5].Equals(track3.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            track2.Begin = new TimeSpan(0, 2, 30);
            track3.Begin = new TimeSpan(0, 5, 15);
            validationResult = cuesheet.Validate(x => x.Tracks);
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);

        }

        [TestMethod()]
        public void RemoveTrackTest()
        {
            var testHelper = new TestHelper();
            testHelper.ApplicationOptions.LinkTracksWithPreviousOne = true;
            var cuesheet = new Cuesheet();
            var track1 = new Track() { Artist = "1", Title = "1" };
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
            Assert.AreEqual((uint)2, track3.Position);
            Assert.AreEqual((uint)3, track4.Position);
            Assert.AreEqual((uint)4, track5.Position);
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
            Assert.AreEqual(track2, cuesheet.Tracks.First());
            Assert.AreEqual(track1, cuesheet.Tracks.Last());
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
        [TestMethod()]
        public void ImportSamplesTest()
        {
            var textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes("../../../../AudioCuesheetEditor/wwwroot/samples/Sample_Inputfile.txt")));
            textImportFile.TextImportScheme.SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet;
            textImportFile.TextImportScheme.SchemeTracks = TextImportScheme.DefaultSchemeTracks;
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsNotNull(textImportFile.Cuesheet);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.IsValid);

            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet
            {
                Artist = "Testartist",
                Title = "Testtitle"
            };
            cuesheet.Import(textImportFile.Cuesheet, testHelper.ApplicationOptions);
            Assert.AreEqual("CuesheetArtist", cuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", cuesheet.Title);
            Assert.AreEqual(8, cuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(1, 15, 54), cuesheet.Tracks.Last().End);
        }

        [TestMethod()]
        public void ImportSamples2Test()
        {
            var textImportFile = new TextImportfile(new MemoryStream(File.ReadAllBytes("../../../../AudioCuesheetEditor/wwwroot/samples/Sample_Inputfile2.txt")));
            textImportFile.TextImportScheme.SchemeCuesheet = null;
            textImportFile.TextImportScheme.SchemeTracks = TextImportScheme.DefaultSchemeTracks;
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsNotNull(textImportFile.Cuesheet);
            Assert.IsTrue(textImportFile.Cuesheet.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.IsValid);

            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet
            {
                Artist = "Testartist",
                Title = "Testtitle"
            };
            cuesheet.Import(textImportFile.Cuesheet, testHelper.ApplicationOptions);
            Assert.IsNull(cuesheet.Artist);
            Assert.IsNull(cuesheet.Title);
            Assert.AreEqual(8, cuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(1, 15, 54), cuesheet.Tracks.Last().End);
        }
    }
}