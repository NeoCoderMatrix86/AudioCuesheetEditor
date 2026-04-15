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
using AudioCuesheetEditor.Model.UI;
using AudioCuesheetEditor.Services.AudioCuesheet;
using AudioCuesheetEditor.Services.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AudioCuesheetEditor.Tests.Services.AudioCuesheet
{
    [TestClass]
    public class CuesheetManagerTests
    {
        private readonly CuesheetManager _cuesheetManager;
        private readonly Mock<ITraceChangeManager> _traceChangeManager;
        private readonly Mock<ISessionStateContainer> _sessionStateContainer;
        private readonly Mock<ITrackManager> _trackManager;

        public CuesheetManagerTests()
        {
            _traceChangeManager = new();
            _sessionStateContainer = new();
            _trackManager = new();
            _cuesheetManager = new(_traceChangeManager.Object, _sessionStateContainer.Object, _trackManager.Object);
        }

        [TestMethod]
        public void SetProperty_NewValue_ChangesPropertyAndSetsTracedChange()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            // Act
            _cuesheetManager.SetProperty(x => x.Artist, "Artist");
            // Assert
            Assert.AreEqual("Artist", cuesheet.Artist);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Cuesheet.Artist))), Times.Once);
        }

        [TestMethod]
        public void SetProperty_EqualValue_ChangesPropertyAndSetsTracedChange()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Artist = "Artist"
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            // Act
            _cuesheetManager.SetProperty(x => x.Artist, cuesheet.Artist);
            // Assert
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Cuesheet.Artist))), Times.Never);
        }

        [TestMethod]
        public void IsRecordingPossible_WithoutTracks_ReturnsSuccess()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            // Act
            var result = _cuesheetManager.IsRecordingPossible;
            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void IsRecordingPossible_WithTracks_ReturnsFailure()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Tracks = [
                    new()
                ]
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            // Act
            var result = _cuesheetManager.IsRecordingPossible;
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Cuesheet already contains tracks!", result.Error!.Message);
        }

        [TestMethod]
        public void StartRecording_RecordingPossible_ReturnsSuccess()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var isRecordingChanged = false;
            _cuesheetManager.IsRecordingChanged += delegate
            {
                isRecordingChanged = true;
            };
            // Act
            var result = _cuesheetManager.StartRecording();
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(isRecordingChanged);
            Assert.IsNotNull(cuesheet.RecordingStart);
        }

        [TestMethod]
        public void StartRecording_RecordingNotPossible_ReturnsFailure()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                RecordingStart = DateTime.UtcNow.AddDays(-1)
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var isRecordingChanged = false;
            _cuesheetManager.IsRecordingChanged += delegate
            {
                isRecordingChanged = true;
            };
            // Act
            var result = _cuesheetManager.StartRecording();
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(isRecordingChanged);
            Assert.AreEqual("Record is already running!", result.Error!.Message);
        }

        [TestMethod]
        public void StopRecording_WithActiveRecording_StopsRecordAndSetsTrackDetails()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                RecordingStart = DateTime.UtcNow.AddDays(-1),
                Tracks = [
                    new() {
                        Position = 1,
                        Begin = TimeSpan.Zero,
                        End = new TimeSpan(0, 3, 12)
                    },
                    new() {
                        Position = 2,
                        Begin = new TimeSpan(0, 3, 12)
                    }
                ]
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var isRecordingChanged = false;
            _cuesheetManager.IsRecordingChanged += delegate
            {
                isRecordingChanged = true;
            };
            // Act
            _cuesheetManager.StopRecording();
            // Assert
            Assert.IsFalse(cuesheet.IsRecording);
            Assert.IsNull(cuesheet.RecordingStart);
            Assert.IsTrue(isRecordingChanged);
            Assert.IsNotNull(cuesheet.Tracks.Last().End);
        }

        [TestMethod]
        public void StopRecording_WithoutActiveRecording_ChangesNothing()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Tracks = [
                    new() {
                        Position = 1,
                        Begin = TimeSpan.Zero,
                        End = new TimeSpan(0, 3, 12)
                    },
                    new() {
                        Position = 2,
                        Begin = new TimeSpan(0, 3, 12)
                    }
                ]
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var isRecordingChanged = false;
            _cuesheetManager.IsRecordingChanged += delegate
            {
                isRecordingChanged = true;
            };
            // Act
            _cuesheetManager.StopRecording();
            // Assert
            Assert.IsFalse(isRecordingChanged);
            Assert.IsNull(cuesheet.Tracks.Last().End);
        }

        [TestMethod]
        public void AddTrack_FirstTrack_AddsNewTrackWithCalulatedTrackProperties()
        {
            // Arrange
            var duration = new TimeSpan(0, 27, 56);
            var cuesheet = new Cuesheet()
            {
                Audiofile = new("Audio.mp3", nameof(AddTrack_FirstTrack_AddsNewTrackWithCalulatedTrackProperties), Audiofile.AudioCodecs.First(), duration)
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var track = new Track();
            _trackManager.Setup(x => x.SetProperty(track, x => x.End, It.IsAny<TimeSpan>())).Callback<Track, Expression<Func<Track, TimeSpan?>>, TimeSpan?>((t, propExpression, value) =>
            {
                var memberExpression = propExpression.Body as MemberExpression;
                if (memberExpression?.Member is PropertyInfo propInfo)
                {
                    propInfo.SetValue(t, value);
                }
            });
            // Act
            _cuesheetManager.AddTrack(track);
            // Assert
            Assert.HasCount(1, cuesheet.Tracks);
            Assert.AreEqual((ushort)1, cuesheet.Tracks.First().Position);
            Assert.AreEqual(TimeSpan.Zero, cuesheet.Tracks.First().Begin);
            Assert.AreEqual(duration, cuesheet.Tracks.First().End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == Enumerable.Empty<Track>() && y.TraceableChange.PropertyName == nameof(Cuesheet.Tracks))), Times.Once);
        }

        [TestMethod]
        public void AddTrack_AddToPreviousTracks_AddsNewTrackWithCalulatedTrackProperties()
        {
            // Arrange
            var duration = new TimeSpan(0, 27, 56);
            var tracks = new List<Track>
            {
                new()
                {
                    Position = 1,
                    Begin = TimeSpan.Zero,
                    End = new TimeSpan(3, 12, 0)
                }
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = tracks,
                Audiofile = new("Audio.mp3", nameof(AddTrack_FirstTrack_AddsNewTrackWithCalulatedTrackProperties), Audiofile.AudioCodecs.First(), duration)
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var track = new Track();
            _trackManager.Setup(x => x.SetProperty(track, x => x.End, It.IsAny<TimeSpan>())).Callback<Track, Expression<Func<Track, TimeSpan?>>, TimeSpan?>((t, propExpression, value) =>
            {
                var memberExpression = propExpression.Body as MemberExpression;
                if (memberExpression?.Member is PropertyInfo propInfo)
                {
                    propInfo.SetValue(t, value);
                }
            });
            // Act
            _cuesheetManager.AddTrack(track);
            // Assert
            Assert.HasCount(2, cuesheet.Tracks);
            Assert.AreEqual((ushort)2, cuesheet.Tracks.Last().Position);
            Assert.AreEqual(cuesheet.Tracks.First().End, cuesheet.Tracks.Last().Begin);
            Assert.AreEqual(duration, cuesheet.Tracks.Last().End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == tracks && y.TraceableChange.PropertyName == nameof(Cuesheet.Tracks))), Times.Once);
        }

        [TestMethod]
        public void AddTrack_DuringRecording_AddsNewTrackWithCalulatedTrackProperties()
        {
            // Arrange
            var tracks = new List<Track>
            {
                new()
                {
                    Position = 1,
                    Begin = TimeSpan.Zero
                }
            };
            var cuesheet = new Cuesheet()
            {
                RecordingStart = DateTime.UtcNow,
                Tracks = tracks
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var track = new Track();
            // Act
            _cuesheetManager.AddTrack(track);
            // Assert
            Assert.HasCount(2, cuesheet.Tracks);
            Assert.AreEqual((ushort)2, cuesheet.Tracks.Last().Position);
            Assert.IsNotNull(cuesheet.Tracks.First().End);
            Assert.IsNotNull(cuesheet.Tracks.Last().Begin);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == tracks && y.TraceableChange.PropertyName == nameof(Cuesheet.Tracks))), Times.Once);
        }

        [TestMethod]
        public void RemoveTracks_WithAttachedTracks_RemovesTracksAndRecalculatesRemainingTrackProperties()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 3, 12),
                IsLinkedToPreviousTrack = true
            };
            var track2 = new Track()
            {
                Position = 2,
                Begin = new TimeSpan(0, 3, 12),
                End = new TimeSpan(0, 7, 34),
                IsLinkedToPreviousTrack = true
            };
            var track3 = new Track()
            {
                Position = 3,
                Begin = new TimeSpan(0, 7, 34),
                End = new TimeSpan(0, 10, 4),
                IsLinkedToPreviousTrack = true
            };
            var track4 = new Track()
            {
                Position = 4,
                Begin = new TimeSpan(0, 10, 4),
                End = new TimeSpan(0, 14, 54),
                IsLinkedToPreviousTrack = true
            };
            var track5 = new Track()
            {
                Position = 5,
                Begin = new TimeSpan(0, 14, 54),
                IsLinkedToPreviousTrack = true
            };
            var previousValue = new List<Track>() { track1, track2, track3, track4, track5 };
            var duration = new TimeSpan(0, 19, 38);
            var cuesheet = new Cuesheet()
            {
                Tracks = previousValue,
                Audiofile = new("Audio.mp3", nameof(RemoveTracks_WithAttachedTracks_RemovesTracksAndRecalculatesRemainingTrackProperties), Audiofile.AudioCodecs.First(), duration)
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            track4.Cuesheet = cuesheet;
            track5.Cuesheet = cuesheet;
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            _trackManager.Setup(x => x.GetPreviousLinkedTrack(It.IsAny<Track>())).Returns(delegate(Track track)
            {
                Track? previousLinkedTrack = null;
                if (track.IsLinkedToPreviousTrack)
                {
                    previousLinkedTrack = track.Cuesheet?.Tracks.LastOrDefault(x => x.Position == track.Position - 1);
                }
                return previousLinkedTrack;
            });
            _trackManager.Setup(x => x.SetProperty(It.IsAny<Track>(), x => x.End, It.IsAny<TimeSpan>())).Callback<Track, Expression<Func<Track, TimeSpan?>>, TimeSpan?>((track, propExpression, value) =>
            {
                var memberExpression = propExpression.Body as MemberExpression;
                if (memberExpression?.Member is PropertyInfo propInfo)
                {
                    propInfo.SetValue(track, value);
                }
            });
            // Act
            _cuesheetManager.RemoveTracks([track2, track4]);
            // Assert
            Assert.HasCount(3, cuesheet.Tracks);
            Assert.IsTrue(cuesheet.Tracks.Contains(track1));
            Assert.IsTrue(cuesheet.Tracks.Contains(track3));
            Assert.IsTrue(cuesheet.Tracks.Contains(track5));
            Assert.AreEqual((ushort)1, track1.Position);
            Assert.AreEqual(TimeSpan.Zero, track1.Begin);
            Assert.AreEqual(track1.End, track3.Begin);
            Assert.AreEqual((ushort)2, track3.Position);
            Assert.AreEqual(track5.Begin, track3.End);
            Assert.AreEqual((ushort)3, track5.Position);
            Assert.AreEqual(duration, track5.End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == previousValue && y.TraceableChange.PropertyName == nameof(Cuesheet.Tracks))), Times.Once);
        }
    }
}
