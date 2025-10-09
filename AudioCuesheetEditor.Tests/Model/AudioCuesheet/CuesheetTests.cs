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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AudioCuesheetEditor.Tests.Model.AudioCuesheet
{
    [TestClass()]
    public class CuesheetTests
    {
        [TestMethod()]
        public void AddTrackTest()
        {
            var cuesheet = new Cuesheet();
            Assert.IsEmpty(cuesheet.Tracks);
            cuesheet.AddTrack(new Track());
            Assert.HasCount(1, cuesheet.Tracks);
        }

        [TestMethod()]
        public void CuesheetTest()
        {
            var cuesheet = new Cuesheet();
            Assert.IsNull(cuesheet.Audiofile);
            var validationErrorAudioFile = cuesheet.Validate(nameof(Cuesheet.Audiofile));
            Assert.AreEqual(ValidationStatus.Error, validationErrorAudioFile.Status);
            cuesheet.Audiofile = new Audiofile("AudioFile01.ogg");
            validationErrorAudioFile = cuesheet.Validate(nameof(Cuesheet.Audiofile));
            Assert.AreEqual(ValidationStatus.Success, validationErrorAudioFile.Status);
        }

        [TestMethod()]
        public void EmptyCuesheetTracksValidationTest()
        {
            var cuesheet = new Cuesheet();
            Assert.IsEmpty(cuesheet.Tracks);
            var validationErrorTracks = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Error, validationErrorTracks.Status);
            cuesheet.AddTrack(new Track());
            validationErrorTracks = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Success, validationErrorTracks.Status);
        }

        [TestMethod()]
        public void RecordTest()
        {
            var cuesheet = new Cuesheet();
            Assert.IsFalse(cuesheet.IsRecording);
            Assert.IsNull(cuesheet.RecordingStart);
            cuesheet.StartRecording();
            Assert.IsTrue(cuesheet.IsRecording);
            Assert.IsNotNull(cuesheet.RecordingStart);
            var track = new Track();
            Assert.IsNull(track.Begin);
            Assert.IsNull(track.End);
            cuesheet.AddTrack(track);
            Assert.AreEqual(TimeSpan.Zero, track.Begin);
            Assert.IsNull(track.End);
            var track2 = new Track();
            cuesheet.AddTrack(track2);
            Assert.IsNotNull(track.End);
            Assert.AreNotEqual(TimeSpan.Zero, track.End);
            cuesheet = new Cuesheet();
            cuesheet.StartRecording();
            track = new Track();
            cuesheet.AddTrack(track);
            Assert.AreEqual(TimeSpan.Zero, track.Begin);
            Assert.IsNull(track.End);
            Thread.Sleep(3000);
            track2 = new Track();
            cuesheet.AddTrack(track2);
            Assert.IsNotNull(track.End);
            Assert.AreNotEqual(TimeSpan.Zero, track.End);
            cuesheet.StopRecording();
            Assert.IsNotNull(track.End);
            Assert.AreEqual(track.End, track2.Begin);
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
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            Assert.AreEqual((uint)1, track1.Position);
            Assert.AreEqual((uint)2, track2.Position);
            Assert.AreEqual((uint)3, track3.Position);
            var validationResult = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
            track1.Position = 1;
            track2.Position = 1;
            track3.Position = 1;
            track1.End = new TimeSpan(0, 2, 30);
            track2.Begin = new TimeSpan(0, 2, 0);
            track2.End = new TimeSpan(0, 5, 30);
            track3.Begin = new TimeSpan(0, 4, 54);
            track3.End = new TimeSpan(0, 8, 12);
            validationResult = cuesheet.Validate(nameof(Cuesheet.Tracks));
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
            validationResult = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track3.Position) && x.Parameter[9].Equals(track3.Begin) && x.Parameter[10].Equals(track3.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track3.Position) && x.Parameter[4].Equals(track3.Begin) && x.Parameter[5].Equals(track3.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            track1.Position = 1;
            track2.Position = 2;
            track3.Position = 3;
            validationResult = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual(4, validationResult.ValidationMessages?.Count);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track1.Position) && x.Parameter[4].Equals(track1.Begin) && x.Parameter[5].Equals(track1.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track1.Position) && x.Parameter[9].Equals(track1.Begin) && x.Parameter[10].Equals(track1.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track3.Position) && x.Parameter[9].Equals(track3.Begin) && x.Parameter[10].Equals(track3.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track3.Position) && x.Parameter[4].Equals(track3.Begin) && x.Parameter[5].Equals(track3.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            track2.Begin = new TimeSpan(0, 2, 30);
            track3.Begin = new TimeSpan(0, 5, 15);
            validationResult = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
        }

        [TestMethod()]
        public void RemoveTrack_ShouldReCalculate_WithPreviouslyAddedTracks()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track() { Artist = "1", Title = "1", IsLinkedToPreviousTrack = true };
            var track2 = new Track() { Artist = "2", Title = "2", IsLinkedToPreviousTrack = true };
            var track3 = new Track() { Artist = "3", Title = "3" , IsLinkedToPreviousTrack = true };
            var track4 = new Track() { Artist = "4", Title = "4" , IsLinkedToPreviousTrack = true };
            var track5 = new Track() { Artist = "5", Title = "5" , IsLinkedToPreviousTrack = true };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            cuesheet.AddTrack(track4);
            cuesheet.AddTrack(track5);
            track1.End = new TimeSpan(0, 5, 0);
            track2.End = new TimeSpan(0, 10, 0);
            track3.End = new TimeSpan(0, 15, 0);
            track4.End = new TimeSpan(0, 20, 0);
            track5.End = new TimeSpan(0, 25, 0);
            // Act
            cuesheet.RemoveTrack(track2);
            // Assert
            Assert.AreEqual((uint)2, track3.Position);
            Assert.AreEqual((uint)3, track4.Position);
            Assert.AreEqual((uint)4, track5.Position);
        }

        [TestMethod()]
        public void RemoveTracks_ShouldRemoveTracks_WithPreviouslyAddedTracks()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track
            {
                Artist = "Track 1",
                Title = "Track 1",
                IsLinkedToPreviousTrack = true
            };
            cuesheet.AddTrack(track1);
            var track2 = new Track
            {
                Title = "Track 2",
                Artist = "Track 2",
                Begin = new TimeSpan(0, 5, 0),
                IsLinkedToPreviousTrack = true
            };
            cuesheet.AddTrack(track2);
            var track3 = new Track
            {
                Artist = "Track 3",
                Title = "Track 3",
                Begin = new TimeSpan(0, 10, 0),
                IsLinkedToPreviousTrack = true
            };
            cuesheet.AddTrack(track3);
            var track4 = new Track
            {
                Artist = "Track 4",
                Title = "Track 4",
                Begin = new TimeSpan(0, 15, 0),
                IsLinkedToPreviousTrack = true
            };
            cuesheet.AddTrack(track4);
            var track5 = new Track
            {
                Artist = "Track 5",
                Title = "Track 5",
                Begin = new TimeSpan(0, 20, 0), IsLinkedToPreviousTrack = true
            };
            cuesheet.AddTrack(track5);
            var list = new List<Track>() { track2, track4 };
            // Act
            cuesheet.RemoveTracks(list.AsReadOnly());
            // Assert
            Assert.HasCount(3, cuesheet.Tracks);
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
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            Assert.AreEqual(track1.End, track2.Begin);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.Begin);
            track3.Begin = new TimeSpan(0, 7, 32);
            Assert.IsNull(track2.End);
        }

        [TestMethod()]
        public void TrackPositionChangedTest()
        {
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
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track1);
            Assert.AreEqual(track2, cuesheet.Tracks.First());
            Assert.AreEqual(track1, cuesheet.Tracks.Last());
            track1.Position = 1;
            Assert.AreEqual(track2, cuesheet.Tracks.First());
            Assert.AreEqual(track1, cuesheet.Tracks.Last());
        }

        [TestMethod()]
        public void TrackLengthChangedWithIsLinkedToPreivousTest()
        {
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
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
        public void ValidateTest()
        {
            var cuesheet = new Cuesheet();
            Assert.AreEqual(ValidationStatus.Error, cuesheet.Validate(nameof(Cuesheet.Artist)).Status);
            cuesheet.Artist = "Testartist";
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Validate(nameof(Cuesheet.Artist)).Status);
            Assert.AreEqual(ValidationStatus.Error, cuesheet.Validate(nameof(Cuesheet.Title)).Status);
            cuesheet.Title = "Testtitle";
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Validate(nameof(Cuesheet.Title)).Status);
        }

        [TestMethod()]
        public void IsLinkedToPreviousTrack_ChangedLastTrackBegin_SetsTrackProperties()
        {
            //Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track()
            {
                Artist = "Track1 Artist",
                Title = "Track1 Title"
            };
            var track2 = new Track()
            {
                Artist = "Track2 Artist",
                Title = "Track2 Title",
                End = new TimeSpan(0, 9, 12)
            };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            track2.Begin = new TimeSpan(0, 4, 23);
            //Act
            track2.IsLinkedToPreviousTrack = true;
            //Assert
            Assert.AreEqual((uint)2, track2.Position);
            Assert.AreEqual(track2.Begin, track1.End);
        }

        [TestMethod()]
        public void IsRecordingPossible_WhenRecordingIsAlreadyRunning_ReturnsError()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.StartRecording();

            // Act
            var errors = cuesheet.IsRecordingPossible.ToList();

            // Assert
            Assert.Contains("Record is already running!", errors);
        }

        [TestMethod()]
        public void IsRecordingPossible_WhenCuesheetContainsTracks_ReturnsError()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.AddTrack(new Track());

            // Act
            var errors = cuesheet.IsRecordingPossible.ToList();

            // Assert
            Assert.Contains("Cuesheet already contains tracks!", errors);
        }

        [TestMethod()]
        public void IsRecordingPossible_WhenNoErrors_ReturnsEmpty()
        {
            // Arrange
            var cuesheet = new Cuesheet();

            // Act
            var errors = cuesheet.IsRecordingPossible.ToList();

            // Assert
            Assert.IsEmpty(errors);
        }

        [TestMethod]
        public void AddSection_WithValidData_FiresEvents()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            bool eventFired = false;
            cuesheet.TraceablePropertyChanged += (sender, args) =>
            {
                if (args.TraceableChange.PropertyName == nameof(Cuesheet.Sections))
                {
                    eventFired = true;
                }
            };

            // Act
            var section = cuesheet.AddSection();

            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(section);
            Assert.HasCount(1, cuesheet.Sections);
            Assert.AreEqual(cuesheet, section.Cuesheet);
        }

        [TestMethod()]
        public void RemoveSections_RemovesSpecifiedSections()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var section1 = cuesheet.AddSection();
            var section2 = cuesheet.AddSection();
            var section3 = cuesheet.AddSection();
            var sectionsToRemove = new List<CuesheetSection> { section1, section3 };
            bool eventFired = false;
            cuesheet.TraceablePropertyChanged += (sender, args) =>
            {
                if (args.TraceableChange.PropertyName == nameof(Cuesheet.Sections))
                {
                    eventFired = true;
                }
            };

            // Act
            cuesheet.RemoveSections(sectionsToRemove);

            // Assert
            Assert.IsTrue(eventFired);
            Assert.HasCount(1, cuesheet.Sections);
            Assert.IsTrue(cuesheet.Sections.Contains(section2));
            Assert.IsFalse(cuesheet.Sections.Contains(section1));
            Assert.IsFalse(cuesheet.Sections.Contains(section3));
        }

        [TestMethod()]
        public void GetSection_ReturnsCorrectSection()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var section1 = cuesheet.AddSection();
            section1.Begin = TimeSpan.Zero;
            section1.End = TimeSpan.FromSeconds(120);
            cuesheet.AddSection();
            var track = new Track { Begin = TimeSpan.Zero, End = TimeSpan.FromSeconds(83) };
            cuesheet.AddTrack(track);

            // Act
            var result = cuesheet.GetSection(track);

            // Assert
            Assert.AreEqual(section1, result);
        }

        [TestMethod()]
        public void GetSection_ReturnsNullIfNoMatchingSection()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var section1 = cuesheet.AddSection();
            section1.Begin = TimeSpan.Zero;
            section1.End = TimeSpan.FromHours(1.5);
            var track = new Track { Begin = section1.End + TimeSpan.FromSeconds(1), End = section1.End + TimeSpan.FromSeconds(2) };
            cuesheet.AddTrack(track);

            // Act
            var result = cuesheet.GetSection(track);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void MoveTracksPossible_ShouldReturnFalse_WhenNoTracksToMove()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var tracksToMove = new List<Track>();

            // Act
            var result = cuesheet.MoveTracksPossible(tracksToMove, MoveDirection.Up);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MoveTracksPossible_ShouldReturnFalse_WhenMovingUpAndFirstTrack()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track = new Track();
            cuesheet.AddTrack(track);
            var tracksToMove = new List<Track> { track };

            // Act
            var result = cuesheet.MoveTracksPossible(tracksToMove, MoveDirection.Up);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MoveTracksPossible_ShouldReturnTrue_WhenMovingUpAndNotFirstTrack()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            var tracksToMove = new List<Track> { track2 };

            // Act
            var result = cuesheet.MoveTracksPossible(tracksToMove, MoveDirection.Up);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MoveTracksPossible_ShouldReturnFalse_WhenMovingDownAndLastTrack()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            var tracksToMove = new List<Track> { track2 };

            // Act
            var result = cuesheet.MoveTracksPossible(tracksToMove, MoveDirection.Down);

            // Assert
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void MoveTracksPossible_ShouldReturnTrue_WhenMovingDownAndNotLastTrack()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            var track3 = new Track();
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            var tracksToMove = new List<Track> { track2 };

            // Act
            var result = cuesheet.MoveTracksPossible(tracksToMove, MoveDirection.Down);

            // Assert
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void MoveTracks_Up_Success()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track { Position = 1 };
            var track2 = new Track { Position = 2 };
            var track3 = new Track { Position = 3 };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);

            // Act
            cuesheet.MoveTracks([track2], MoveDirection.Up);

            // Assert
            Assert.AreEqual((uint)2, cuesheet.Tracks.ElementAt(0).Position);
            Assert.AreEqual((uint)1, cuesheet.Tracks.ElementAt(1).Position);
            Assert.AreEqual((uint)3, cuesheet.Tracks.ElementAt(2).Position);
            Assert.AreEqual(track2, cuesheet.Tracks.ElementAt(0));
            Assert.AreEqual(track1, cuesheet.Tracks.ElementAt(1));
            Assert.AreEqual(track3, cuesheet.Tracks.ElementAt(2));
        }

        [TestMethod]
        public void MoveTracks_Down_Success()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track { Position = 1 };
            var track2 = new Track { Position = 2 };
            var track3 = new Track { Position = 3 };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);

            // Act
            cuesheet.MoveTracks([track2], MoveDirection.Down);

            // Assert
            Assert.AreEqual((uint)1, cuesheet.Tracks.ElementAt(0).Position);
            Assert.AreEqual((uint)3, cuesheet.Tracks.ElementAt(1).Position);
            Assert.AreEqual((uint)2, cuesheet.Tracks.ElementAt(2).Position);
            Assert.AreEqual(track1, cuesheet.Tracks.ElementAt(0));
            Assert.AreEqual(track3, cuesheet.Tracks.ElementAt(1));
            Assert.AreEqual(track2, cuesheet.Tracks.ElementAt(2));
        }

        [TestMethod]
        public void MoveTracks_Up_NotPossible()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track { Position = 1 };
            var track2 = new Track { Position = 2 };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);

            // Act
            cuesheet.MoveTracks([track1], MoveDirection.Up);

            // Assert
            Assert.AreEqual((uint)1, cuesheet.Tracks.ElementAt(0).Position);
            Assert.AreEqual((uint)2, cuesheet.Tracks.ElementAt(1).Position);
            Assert.AreEqual(track1, cuesheet.Tracks.ElementAt(0));
            Assert.AreEqual(track2, cuesheet.Tracks.ElementAt(1));
        }

        [TestMethod]
        public void MoveTracks_Down_NotPossible()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track { Position = 1 };
            var track2 = new Track { Position = 2 };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);

            // Act
            cuesheet.MoveTracks([track2], MoveDirection.Down);

            // Assert
            Assert.AreEqual((uint)1, cuesheet.Tracks.ElementAt(0).Position);
            Assert.AreEqual((uint)2, cuesheet.Tracks.ElementAt(1).Position);
            Assert.AreEqual(track1, cuesheet.Tracks.ElementAt(0));
            Assert.AreEqual(track2, cuesheet.Tracks.ElementAt(1));
        }

        [TestMethod]
        public void StartRecording_WithValidData_ShouldStartRecording()
        {
            // Arrange
            var cuesheet = new Cuesheet();

            // Act
            cuesheet.StartRecording();

            // Assert
            Assert.IsTrue(cuesheet.IsRecording);
            Assert.IsNotNull(cuesheet.RecordingStart);
        }

        [TestMethod]
        public void StopRecording_WithRecordingRunning_ShouldStopRecording()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.StartRecording();
            var track = new Track();
            cuesheet.AddTrack(track);
            bool isRecordingChangedEventFired = false;
            cuesheet.IsRecordingChanged += (sender, args) => isRecordingChangedEventFired = true;

            // Act
            cuesheet.StopRecording();

            // Assert
            Assert.IsFalse(cuesheet.IsRecording);
            Assert.IsNull(cuesheet.RecordingStart);
            Assert.IsTrue(isRecordingChangedEventFired);
            Assert.IsNotNull(track.End);
        }

        [TestMethod]
        public void StartRecording_ShouldNotStartIfAlreadyRecording()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.StartRecording();

            // Act
            cuesheet.StartRecording();

            // Assert
            Assert.IsTrue(cuesheet.IsRecording);
            Assert.IsNotNull(cuesheet.RecordingStart);
        }

        [TestMethod]
        public void StartRecording_WithTrackAlreadyAdded_ShouldNotStartRecording()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.AddTrack(new Track());

            // Act
            cuesheet.StartRecording();

            // Assert
            Assert.IsFalse(cuesheet.IsRecording);
        }

        [TestMethod]
        public void StartRecording_WithAudiofile_ShouldStartRecording()
        {
            // Arrange
            var cuesheet = new Cuesheet
            {
                Audiofile = new Audiofile("test")
            };

            // Act
            cuesheet.StartRecording();

            // Assert
            Assert.IsTrue(cuesheet.IsRecording);
        }

        [TestMethod]
        public void RecalculateLastTrackEnd_SingleTrackWithAudiofile_EndSetToAudiofileDuration()
        {
            // Arrange
            var audiofileMock = new Mock<IAudiofile>();
            audiofileMock.SetupGet(a => a.Duration).Returns(TimeSpan.FromMinutes(5));

            var cuesheet = new Cuesheet
            {
                Audiofile = audiofileMock.Object
            };
            var track = new Track { Position = 1, Begin = TimeSpan.Zero };
            cuesheet.AddTrack(track);

            // Act
            cuesheet.RecalculateLastTrackEnd();

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(5), track.End);
        }

        [TestMethod]
        public void RecalculateLastTrackEnd_MultipleTracks_EndSetCorrectly()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track { Position = 1, Begin = TimeSpan.Zero, End = TimeSpan.FromMinutes(2) };
            var track2 = new Track { Position = 2, Begin = TimeSpan.FromMinutes(2) };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);

            // Act
            cuesheet.RecalculateLastTrackEnd();

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(2), track1.End);
            Assert.IsNull(track2.End);
        }

        [TestMethod]
        public void RecalculateLastTrackEnd_MultipleTracksWithAudiofile_LastTrackEndSetToAudiofileDuration()
        {
            // Arrange
            var audiofileMock = new Mock<IAudiofile>();
            audiofileMock.SetupGet(a => a.Duration).Returns(TimeSpan.FromMinutes(5));

            var cuesheet = new Cuesheet
            {
                Audiofile = audiofileMock.Object
            };
            var track1 = new Track { Position = 1, Begin = TimeSpan.Zero, End = TimeSpan.FromMinutes(2) };
            var track2 = new Track { Position = 2, Begin = TimeSpan.FromMinutes(2) };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);

            // Act
            cuesheet.RecalculateLastTrackEnd();

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(2), track1.End);
            Assert.AreEqual(TimeSpan.FromMinutes(5), track2.End);
        }

        [TestMethod]
        public void RecalculateLastTrackEnd_TracksWithLinkedPreviousTrack_PropertiesRecalculatedCorrectly()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track { Position = 1, Begin = TimeSpan.Zero, End = TimeSpan.FromMinutes(2) };
            var track2 = new Track { Position = 2, IsLinkedToPreviousTrack = true };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);

            // Act
            cuesheet.RecalculateLastTrackEnd();

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(2), track1.End);
            Assert.AreEqual(TimeSpan.FromMinutes(2), track2.Begin);
        }
    }
}