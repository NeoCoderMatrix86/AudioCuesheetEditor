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
using AudioCuesheetEditor.Data.Options;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.Options;
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
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Services.AudioCuesheet
{
    [TestClass]
    public class CuesheetManagerTests
    {
        private readonly CuesheetManager _cuesheetManager;
        private readonly Mock<ITraceChangeManager> _traceChangeManager;
        private readonly Mock<ISessionStateContainer> _sessionStateContainer;
        private readonly Mock<ITrackManager> _trackManager;
        private readonly Mock<ILocalStorageOptionsProvider> _localStorageOptionsProvider;

        public CuesheetManagerTests()
        {
            _traceChangeManager = new();
            _sessionStateContainer = new();
            _trackManager = new();
            _localStorageOptionsProvider = new();
            _trackManager.Setup(x => x.SetProperty(It.IsAny<Track>(),It.IsAny<Expression<Func<Track, It.IsAnyType>>>(),It.IsAny<It.IsAnyType>()))
            .Callback((Track track, LambdaExpression propExpr, object value) =>
            {
                var memberExpression = propExpr.Body as MemberExpression;
                if (memberExpression?.Member is PropertyInfo propertyInfo)
                {
                    propertyInfo.SetValue(track, value);
                }
            });
            _trackManager.Setup(x => x.GetPreviousLinkedTrack(It.IsAny<Track>())).Returns(delegate (Track track)
            {
                Track? previousLinkedTrack = null;
                if (track.IsLinkedToPreviousTrack)
                {
                    if (track.Position.HasValue)
                    {
                        previousLinkedTrack = track.Cuesheet?.Tracks.LastOrDefault(x => x.Position == track.Position - 1 && Equals(x, track) == false);
                    }
                    else
                    {
                        if (track.Begin.HasValue)
                        {
                            previousLinkedTrack = track.Cuesheet?.Tracks.OrderBy(x => x.End).LastOrDefault(x => x.End <= track.Begin);
                        }
                    }
                }
                return previousLinkedTrack;
            });
            _cuesheetManager = new(_traceChangeManager.Object, _sessionStateContainer.Object, _trackManager.Object, _localStorageOptionsProvider.Object);
        }

        [TestMethod]
        public async Task SetPropertyAsync_NewValue_ChangesPropertyAndSetsTracedChangeAsync()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            // Act
            await _cuesheetManager.SetPropertyAsync(x => x.Artist, "Artist");
            // Assert
            Assert.AreEqual("Artist", cuesheet.Artist);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Cuesheet.Artist))), Times.Once);
        }

        [TestMethod]
        public async Task SetPropertyAsync_EqualValue_DoesntChangePropertyAsync()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Artist = "Artist"
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            // Act
            await _cuesheetManager.SetPropertyAsync(x => x.Artist, cuesheet.Artist);
            // Assert
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Cuesheet.Artist))), Times.Never);
        }

        [TestMethod]
        public async Task SetPropertyAsync_AudiofileWithDuration_SetsLastTrackEndAlsoAsync()
        {
            // Arrange
            var track = new Track()
            {
                Position = 1
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track]
            };
            track.Cuesheet = cuesheet;
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var duration = new TimeSpan(0, 5, 23);
            var audiofile = new Audiofile("Test.mp3", nameof(SetPropertyAsync_AudiofileWithDuration_SetsLastTrackEndAlsoAsync), Audiofile.AudioCodecs.First(), duration);
            // Act
            await _cuesheetManager.SetPropertyAsync(x => x.Audiofile, audiofile);
            // Assert
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Cuesheet.Audiofile))), Times.Once);
            Assert.AreEqual(duration, track.End);
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
        public async Task AddTrackAsync_FirstTrack_AddsNewTrackWithCalulatedTrackPropertiesAsync()
        {
            // Arrange
            var duration = new TimeSpan(0, 27, 56);
            var cuesheet = new Cuesheet()
            {
                Audiofile = new("Audio.mp3", nameof(AddTrackAsync_FirstTrack_AddsNewTrackWithCalulatedTrackPropertiesAsync), Audiofile.AudioCodecs.First(), duration)
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var viewOptions = new ViewOptions()
            {
                ActiveTab = ViewMode.DetailView
            };
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            var track = new Track();
            // Act
            await _cuesheetManager.AddTrackAsync(track);
            // Assert
            Assert.HasCount(1, cuesheet.Tracks);
            Assert.AreEqual((ushort)1, cuesheet.Tracks.First().Position);
            Assert.AreEqual(TimeSpan.Zero, cuesheet.Tracks.First().Begin);
            Assert.AreEqual(duration, cuesheet.Tracks.First().End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == Enumerable.Empty<Track>() && y.TraceableChange.PropertyName == nameof(Cuesheet.Tracks))), Times.Once);
        }

        [TestMethod]
        public async Task AddTrackAsync_AddToPreviousTracks_AddsNewTrackWithCalulatedTrackPropertiesAsync()
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
                Audiofile = new("Audio.mp3", nameof(AddTrackAsync_AddToPreviousTracks_AddsNewTrackWithCalulatedTrackPropertiesAsync), Audiofile.AudioCodecs.First(), duration)
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var viewOptions = new ViewOptions()
            {
                ActiveTab = ViewMode.DetailView
            };
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            var track = new Track();
            // Act
            await _cuesheetManager.AddTrackAsync(track);
            // Assert
            Assert.HasCount(2, cuesheet.Tracks);
            Assert.AreEqual((ushort)2, cuesheet.Tracks.Last().Position);
            Assert.AreEqual(cuesheet.Tracks.First().End, cuesheet.Tracks.Last().Begin);
            Assert.AreEqual(duration, cuesheet.Tracks.Last().End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == tracks && y.TraceableChange.PropertyName == nameof(Cuesheet.Tracks))), Times.Once);
        }

        [TestMethod]
        public async Task AddTrackAsync_DuringRecording_AddsNewTrackWithCalulatedTrackPropertiesAsync()
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
            var viewOptions = new ViewOptions()
            {
                ActiveTab = ViewMode.RecordView
            };
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            var track = new Track();
            // Act
            await _cuesheetManager.AddTrackAsync(track);
            // Assert
            Assert.HasCount(2, cuesheet.Tracks);
            Assert.AreEqual((ushort)2, cuesheet.Tracks.Last().Position);
            Assert.IsNotNull(cuesheet.Tracks.First().End);
            Assert.IsNotNull(cuesheet.Tracks.Last().Begin);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == tracks && y.TraceableChange.PropertyName == nameof(Cuesheet.Tracks))), Times.Once);
        }

        [TestMethod]
        public async Task AddTrackAsync_FirstTrackImporting_AddsNewTrackWithCalulatedTrackPropertiesAsync()
        {
            // Arrange
            var duration = new TimeSpan(0, 27, 56);
            var importCuesheet = new Cuesheet()
            {
                Audiofile = new("Audio.mp3", nameof(AddTrackAsync_FirstTrack_AddsNewTrackWithCalulatedTrackPropertiesAsync), Audiofile.AudioCodecs.First(), duration)
            };
            _sessionStateContainer.SetupProperty(x => x.ImportCuesheet, importCuesheet);
            var viewOptions = new ViewOptions()
            {
                ActiveTab = ViewMode.ImportView
            };
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            var track = new Track();
            // Act
            await _cuesheetManager.AddTrackAsync(track);
            // Assert
            Assert.HasCount(1, importCuesheet.Tracks);
            Assert.AreEqual((ushort)1, importCuesheet.Tracks.First().Position);
            Assert.AreEqual(TimeSpan.Zero, importCuesheet.Tracks.First().Begin);
            Assert.AreEqual(duration, importCuesheet.Tracks.First().End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == importCuesheet && y.TraceableChange.PreviousValue == Enumerable.Empty<Track>() && y.TraceableChange.PropertyName == nameof(Cuesheet.Tracks))), Times.Once);
        }

        [TestMethod]
        public async Task RemoveTracksAsync_WithAttachedTracks_RemovesTracksAndRecalculatesRemainingTrackPropertiesAsync()
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
                Audiofile = new("Audio.mp3", nameof(RemoveTracksAsync_WithAttachedTracks_RemovesTracksAndRecalculatesRemainingTrackPropertiesAsync), Audiofile.AudioCodecs.First(), duration)
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            track4.Cuesheet = cuesheet;
            track5.Cuesheet = cuesheet;
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            var viewOptions = new ViewOptions()
            {
                ActiveTab = ViewMode.DetailView
            };
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            // Act
            await _cuesheetManager.RemoveTracksAsync([track2, track4]);
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

        [TestMethod]
        public async Task RemoveTracksAsync_DuringImport_RemovesTracksAndRecalculatesRemainingTrackPropertiesAsync()
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
            var importCuesheet = new Cuesheet()
            {
                Tracks = previousValue,
                Audiofile = new("Audio.mp3", nameof(RemoveTracksAsync_WithAttachedTracks_RemovesTracksAndRecalculatesRemainingTrackPropertiesAsync), Audiofile.AudioCodecs.First(), duration)
            };
            track1.Cuesheet = importCuesheet;
            track2.Cuesheet = importCuesheet;
            track3.Cuesheet = importCuesheet;
            track4.Cuesheet = importCuesheet;
            track5.Cuesheet = importCuesheet;
            _sessionStateContainer.SetupProperty(x => x.ImportCuesheet, importCuesheet);
            var viewOptions = new ViewOptions()
            {
                ActiveTab = ViewMode.ImportView
            };
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            // Act
            await _cuesheetManager.RemoveTracksAsync([track2, track4]);
            // Assert
            Assert.HasCount(3, importCuesheet.Tracks);
            Assert.IsTrue(importCuesheet.Tracks.Contains(track1));
            Assert.IsTrue(importCuesheet.Tracks.Contains(track3));
            Assert.IsTrue(importCuesheet.Tracks.Contains(track5));
            Assert.AreEqual((ushort)1, track1.Position);
            Assert.AreEqual(TimeSpan.Zero, track1.Begin);
            Assert.AreEqual(track1.End, track3.Begin);
            Assert.AreEqual((ushort)2, track3.Position);
            Assert.AreEqual(track5.Begin, track3.End);
            Assert.AreEqual((ushort)3, track5.Position);
            Assert.AreEqual(duration, track5.End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == importCuesheet && y.TraceableChange.PreviousValue == previousValue && y.TraceableChange.PropertyName == nameof(Cuesheet.Tracks))), Times.Once);
        }
    }
}
