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
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
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
    public class AudiofileManagerTests
    {
        private readonly AudiofileManager _audiofileManager;
        private readonly Mock<IFileInputManager> _fileInputManager;
        private readonly Mock<ITraceChangeManager> _traceChangeManager;
        private readonly Mock<IJSRuntime> _jsRuntime;
        private readonly Mock<ITrackManager> _trackManager;
        private readonly Mock<ISessionStateContainer> _sessionStateContainer;

        public AudiofileManagerTests()
        {
            _fileInputManager = new Mock<IFileInputManager>();
            _traceChangeManager = new Mock<ITraceChangeManager>();
            _jsRuntime = new Mock<IJSRuntime>();
            _trackManager = new Mock<ITrackManager>();
            _trackManager.Setup(x => x.SetProperty(It.IsAny<Track>(), It.IsAny<Expression<Func<Track, It.IsAnyType>>>(), It.IsAny<It.IsAnyType>()))
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
                if (track.IsLinkedToPreviousTrack == false)
                {
                    return null;
                }
                if (track.Position.HasValue && (track.Cuesheet?.Audiofiles.SelectMany(x => x.Tracks).All(x => x.Position.HasValue) == true))
                {
                    return track.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).LastOrDefault(x => x.Position == track.Position - 1 && Equals(x, track) == false);
                }
                if (track.Begin.HasValue)
                {
                    return track.Cuesheet?.Audiofiles.SelectMany(x => x.Tracks).OrderBy(x => x.End).LastOrDefault(x => x.End <= track.Begin && Equals(x, track) == false);
                }
                return null;
            });
            _sessionStateContainer = new Mock<ISessionStateContainer>();
            _audiofileManager = new AudiofileManager(_fileInputManager.Object, _traceChangeManager.Object, _jsRuntime.Object, _trackManager.Object, _sessionStateContainer.Object);
        }

        [TestMethod]
        public async Task SetPropertiesAsync_EmptyBrowserFile_ShouldClearPropertiesAsync()
        {
            // Arrange
            var audiofile = new Audiofile
            {
                AudioCodec = Audiofile.AudioCodecs.First(x => x.FileExtension == ".mp3"),
                Name = "test.mp3",
                ObjectURL = "Just a test",
                Duration = TimeSpan.FromSeconds(120)
            };
            var expectedUrl = audiofile.ObjectURL;
            // Act
            await _audiofileManager.SetPropertiesAsync(audiofile, null, string.Empty);
            // Assert
            Assert.IsNull(audiofile.AudioCodec);
            Assert.IsNull(audiofile.Name);
            Assert.IsNull(audiofile.ObjectURL);
            Assert.IsNull(audiofile.Duration);
            _jsRuntime.Verify(js => js.InvokeAsync<object>("revokeAudioObjectURL", It.Is<object?[]>(args => args != null && args.Length > 0 && (args[0] as string) == expectedUrl)), Times.Once);
        }

        [TestMethod]
        public async Task SetPropertiesAsync_WithBrowserFile_ShouldSetPropertiesAsync()
        {
            // Arrange
            var objectUrl = "blob:123";
            var filename = "song.mp3";
            var inputId = "inputId";
            var browserFile = new Mock<IBrowserFile>();
            browserFile.SetupGet(b => b.ContentType).Returns("audio/mpeg");
            browserFile.SetupGet(b => b.Name).Returns(filename);
            
            var codec = Audiofile.AudioCodecs.First(x => x.FileExtension == ".mp3");
            _fileInputManager.Setup(f => f.GetAudioCodec("audio/mpeg", filename)).Returns(codec);
            _fileInputManager.Setup(f => f.GetObjectUrlAsync(inputId)).ReturnsAsync(objectUrl);
            _jsRuntime.Setup(js => js.InvokeAsync<double>("getAudioDurationFromFile", It.IsAny<object?[]>())).Returns(new ValueTask<double>(90.0));

            var audiofile = new Audiofile();

            // Act
            await _audiofileManager.SetPropertiesAsync(audiofile, browserFile.Object, inputId);

            // Assert
            Assert.AreEqual(codec, audiofile.AudioCodec);
            Assert.AreEqual(filename, audiofile.Name);
            Assert.AreEqual(objectUrl, audiofile.ObjectURL);
            Assert.AreEqual(TimeSpan.FromSeconds(90), audiofile.Duration);

            _fileInputManager.Verify(f => f.GetAudioCodec("audio/mpeg", filename), Times.Once);
            _fileInputManager.Verify(f => f.GetObjectUrlAsync(inputId), Times.Once);
            _jsRuntime.Verify(js => js.InvokeAsync<double>("getAudioDurationFromFile", It.Is<object?[]>(o => o[0] as string == audiofile.ObjectURL)), Times.Once);
        }

        [TestMethod]
        public async Task SetProperty_AudiofileWithDuration_SetsLastTrackEndAlsoAsync()
        {
            // Arrange
            var objectUrl = "blob:123";
            var filename = "song.mp3";
            var inputId = "inputId";
            var browserFile = new Mock<IBrowserFile>();
            browserFile.SetupGet(b => b.ContentType).Returns("audio/mpeg");
            browserFile.SetupGet(b => b.Name).Returns(filename);

            var codec = Audiofile.AudioCodecs.First(x => x.FileExtension == ".mp3");
            _fileInputManager.Setup(f => f.GetAudioCodec("audio/mpeg", filename)).Returns(codec);
            _fileInputManager.Setup(f => f.GetObjectUrlAsync(inputId)).ReturnsAsync(objectUrl);
            _jsRuntime.Setup(js => js.InvokeAsync<double>("getAudioDurationFromFile", It.IsAny<object?[]>())).Returns(new ValueTask<double>(90.0));
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.FromSeconds(0),
                End = TimeSpan.FromSeconds(30)
            };
            var track2 = new Track()
            {
                Position = 2,
                Begin = track1.End
            };
            var audiofile = new Audiofile()
            {
                Tracks = [track1, track2]
            };

            // Act
            await _audiofileManager.SetPropertiesAsync(audiofile, browserFile.Object, inputId);

            // Assert
            Assert.AreEqual(codec, audiofile.AudioCodec);
            Assert.AreEqual(filename, audiofile.Name);
            Assert.AreEqual(objectUrl, audiofile.ObjectURL);
            Assert.AreEqual(TimeSpan.FromSeconds(90), audiofile.Duration);
            Assert.AreEqual(TimeSpan.FromSeconds(90), track2.End);

            _fileInputManager.Verify(f => f.GetAudioCodec("audio/mpeg", filename), Times.Once);
            _fileInputManager.Verify(f => f.GetObjectUrlAsync(inputId), Times.Once);
            _jsRuntime.Verify(js => js.InvokeAsync<double>("getAudioDurationFromFile", It.Is<object?[]>(o => o[0] as string == audiofile.ObjectURL)), Times.Once);
        }

        [TestMethod]
        public async Task SetPropertiesAsync_NullBrowserFile_WithExistingObjectUrl_ShouldRevokeObjectUrl()
        {
            // Arrange
            var audiofile = new Audiofile
            {
                ObjectURL = "just a test"
            };
            var expectedUrl = audiofile.ObjectURL;

            // Act
            await _audiofileManager.SetPropertiesAsync(audiofile, null, string.Empty);

            // Assert
            _jsRuntime.Verify(js =>js.InvokeAsync<object>("revokeAudioObjectURL", It.Is<object?[]>(args => args != null && args.Length > 0 && (args[0] as string) == expectedUrl)),Times.Once);
            Assert.IsNull(audiofile.ObjectURL);
        }

        [TestMethod]
        public void SetProperty_ChangedName_UpdatesPropertyAndAddsTrace()
        {
            // Arrange
            var audiofile = new Audiofile
            {
                Name = "oldname.mp3"
            };

            // Act
            _audiofileManager.SetProperty(audiofile, x => x.Name, "newname.mp3");

            // Assert
            Assert.AreEqual("newname.mp3", audiofile.Name);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == audiofile && y.TraceableChange.PreviousValue!.Equals("oldname.mp3") && y.TraceableChange.PropertyName == nameof(Audiofile.Name))), Times.Once);
        }

        [TestMethod]
        public void SetProperty_EqualValue_DoesntChangeProperty()
        {
            // Arrange
            var audiofile = new Audiofile
            {
                Name = "oldname.mp3"
            };

            // Act
            _audiofileManager.SetProperty(audiofile, x => x.Name, "oldname.mp3");

            // Assert
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == audiofile && y.TraceableChange.PreviousValue!.Equals("oldname.mp3") && y.TraceableChange.PropertyName == nameof(Audiofile.Name))), Times.Never);
        }

        [TestMethod]
        public void SetProperty_DurationChanged_SetsLastTrackEndAlso()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.FromSeconds(0),
                End = TimeSpan.FromSeconds(30)
            };
            var track2 = new Track()
            {
                Position = 2,
                Begin = track1.End
            };
            var audiofile = new Audiofile
            {
                Tracks = [track1, track2],
            };
            var duration = new TimeSpan(0, 3, 37, 12);

            // Act
            _audiofileManager.SetProperty(audiofile, x => x.Duration, duration);

            // Assert
            Assert.AreEqual(duration, audiofile.Duration);
            Assert.AreEqual(duration, track2.End);
        }

        [TestMethod]
        public void AddTrack_FirstTrack_AddsNewTrackWithCalulatedTrackProperties()
        {
            // Arrange
            var duration = new TimeSpan(0, 27, 56);
            var audiofile = new Audiofile("Audio.mp3", nameof(AddTrack_FirstTrack_AddsNewTrackWithCalulatedTrackProperties), Audiofile.AudioCodecs.First(), duration);
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [audiofile]
            };
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(cuesheet);
            var track = new Track();
            // Act
            _audiofileManager.AddTrack(audiofile, track);
            // Assert
            Assert.HasCount(1, audiofile.Tracks);
            Assert.AreEqual((ushort)1, audiofile.Tracks.First().Position);
            Assert.AreEqual(TimeSpan.Zero, audiofile.Tracks.First().Begin);
            Assert.AreEqual(duration, audiofile.Tracks.First().End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == audiofile && ((ICollection<Track>)y.TraceableChange.PreviousValue!).Count == 0 && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Once);
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
                    End = new TimeSpan(3, 12, 0),
                    IsLinkedToPreviousTrack = true
                }
            };
            var audiofile = new Audiofile("Audio.mp3", nameof(AddTrack_AddToPreviousTracks_AddsNewTrackWithCalulatedTrackProperties), Audiofile.AudioCodecs.First(), duration)
            {
                Tracks = tracks
            };
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [audiofile]
            };
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(cuesheet);
            var track = new Track()
            {
                IsLinkedToPreviousTrack = true
            };
            // Act
            _audiofileManager.AddTrack(audiofile, track);
            // Assert
            Assert.HasCount(2, audiofile.Tracks);
            Assert.AreEqual((ushort)2, audiofile.Tracks.Last().Position);
            Assert.AreEqual(audiofile.Tracks.First().End, audiofile.Tracks.Last().Begin);
            Assert.AreEqual(duration, audiofile.Tracks.Last().End);
            Assert.AreEqual(cuesheet, track.Cuesheet);
            Assert.AreEqual(audiofile, track.Audiofile);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == audiofile && y.TraceableChange.PreviousValue == tracks && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Once);
            _traceChangeManager.VerifySet(t => t.BulkEdit = true, Times.Once);
            _traceChangeManager.VerifySet(t => t.BulkEdit = false, Times.Once);
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
                    Begin = TimeSpan.Zero,
                    IsLinkedToPreviousTrack = true
                }
            };
            var audiofile = new Audiofile()
            {
                Tracks = tracks
            };
            var cuesheet = new Cuesheet()
            {
                RecordingStart = DateTime.UtcNow,
                Audiofiles = [audiofile]
            };
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(cuesheet);
            var track = new Track()
            {
                IsLinkedToPreviousTrack = true
            };
            // Act
            _audiofileManager.AddTrack(audiofile, track);
            // Assert
            Assert.HasCount(2, audiofile.Tracks);
            Assert.AreEqual((ushort)2, audiofile.Tracks.Last().Position);
            Assert.IsNotNull(audiofile.Tracks.First().End);
            Assert.IsNotNull(audiofile.Tracks.Last().Begin);
            Assert.AreEqual(cuesheet, track.Cuesheet);
            Assert.AreEqual(audiofile, track.Audiofile);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == audiofile && y.TraceableChange.PreviousValue == tracks && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Once);
            _traceChangeManager.VerifySet(t => t.BulkEdit = true, Times.Once);
            _traceChangeManager.VerifySet(t => t.BulkEdit = false, Times.Once);
        }

        [TestMethod]
        public void AddTrack_FirstTrackImporting_AddsNewTrackWithCalulatedTrackProperties()
        {
            // Arrange
            var duration = new TimeSpan(0, 27, 56);
            var audiofile = new Audiofile("Audio.mp3", nameof(AddTrack_FirstTrackImporting_AddsNewTrackWithCalulatedTrackProperties), Audiofile.AudioCodecs.First(), duration);
            var importCuesheet = new Cuesheet()
            {
                Audiofiles = [audiofile]
            };
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(importCuesheet);
            var track = new Track();
            // Act
            _audiofileManager.AddTrack(audiofile, track);
            // Assert
            Assert.HasCount(1, audiofile.Tracks);
            Assert.AreEqual((ushort)1, audiofile.Tracks.First().Position);
            Assert.AreEqual(TimeSpan.Zero, audiofile.Tracks.First().Begin);
            Assert.AreEqual(duration, audiofile.Tracks.First().End);
            Assert.AreEqual(importCuesheet, track.Cuesheet);
            Assert.AreEqual(audiofile, track.Audiofile);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == audiofile && ((ICollection<Track>) y.TraceableChange.PreviousValue!).Count == 0 && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Once);
            _traceChangeManager.VerifySet(t => t.BulkEdit = true, Times.Once);
            _traceChangeManager.VerifySet(t => t.BulkEdit = false, Times.Once);
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
            var audiofile = new Audiofile("Audio.mp3", nameof(RemoveTracks_WithAttachedTracks_RemovesTracksAndRecalculatesRemainingTrackProperties), Audiofile.AudioCodecs.First(), duration)
            {
                Tracks = previousValue
            };
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [audiofile]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            track4.Cuesheet = cuesheet;
            track5.Cuesheet = cuesheet;
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(cuesheet);
            // Act
            _audiofileManager.RemoveTracks(audiofile,  [track2, track4]);
            // Assert
            Assert.HasCount(3, audiofile.Tracks);
            Assert.Contains(track1, audiofile.Tracks);
            Assert.Contains(track3, audiofile.Tracks);
            Assert.Contains(track5, audiofile.Tracks);
            Assert.AreEqual((ushort)1, track1.Position);
            Assert.AreEqual(TimeSpan.Zero, track1.Begin);
            Assert.AreEqual(track1.End, track3.Begin);
            Assert.AreEqual((ushort)2, track3.Position);
            Assert.AreEqual(track5.Begin, track3.End);
            Assert.AreEqual((ushort)3, track5.Position);
            Assert.AreEqual(duration, track5.End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == audiofile && y.TraceableChange.PreviousValue == previousValue && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Once);
            _traceChangeManager.VerifySet(t => t.BulkEdit = true, Times.Once);
            _traceChangeManager.VerifySet(t => t.BulkEdit = false, Times.Once);
        }

        [TestMethod]
        public void RemoveTracks_DuringImport_RemovesTracksAndRecalculatesRemainingTrackProperties()
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
            var audiofile = new Audiofile("Audio.mp3", nameof(RemoveTracks_DuringImport_RemovesTracksAndRecalculatesRemainingTrackProperties), Audiofile.AudioCodecs.First(), duration)
            {
                Tracks = previousValue
            };
            var importCuesheet = new Cuesheet()
            {
                Audiofiles = [audiofile]
            };
            track1.Cuesheet = importCuesheet;
            track2.Cuesheet = importCuesheet;
            track3.Cuesheet = importCuesheet;
            track4.Cuesheet = importCuesheet;
            track5.Cuesheet = importCuesheet;
            track1.Audiofile = audiofile;
            track2.Audiofile = audiofile;
            track3.Audiofile = audiofile;
            track4.Audiofile = audiofile;
            track5.Audiofile = audiofile;
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(importCuesheet);
            // Act
            _audiofileManager.RemoveTracks(audiofile, [track2, track4]);
            // Assert
            Assert.HasCount(3, audiofile.Tracks);
            Assert.Contains(track1, audiofile.Tracks);
            Assert.Contains(track3, audiofile.Tracks);
            Assert.Contains(track5, audiofile.Tracks);
            Assert.AreEqual((ushort)1, track1.Position);
            Assert.AreEqual(TimeSpan.Zero, track1.Begin);
            Assert.AreEqual(track1.End, track3.Begin);
            Assert.AreEqual((ushort)2, track3.Position);
            Assert.AreEqual(track5.Begin, track3.End);
            Assert.AreEqual((ushort)3, track5.Position);
            Assert.AreEqual(duration, track5.End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == audiofile && y.TraceableChange.PreviousValue == previousValue && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Once);
            _traceChangeManager.VerifySet(t => t.BulkEdit = true, Times.Once);
            _traceChangeManager.VerifySet(t => t.BulkEdit = false, Times.Once);
        }
    }
}
