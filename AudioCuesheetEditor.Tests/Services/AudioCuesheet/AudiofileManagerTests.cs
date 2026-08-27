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
        public void AddTrack_ShouldAddTrackAndSetCuesheet()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var audiofile = new Audiofile();
            cuesheet.Audiofiles.Add(audiofile);

            _sessionStateContainer.Setup(s => s.GetActiveCuesheet()).Returns(cuesheet);

            var track = new Track();

            // Act
            _audiofileManager.AddTrack(audiofile, track);

            // Assert
            Assert.Contains(track, audiofile.Tracks);
            Assert.AreEqual(cuesheet, track.Cuesheet);
            Assert.AreEqual(audiofile, track.Audiofile);
            _traceChangeManager.VerifySet(t => t.BulkEdit = true, Times.AtLeastOnce);
            _traceChangeManager.VerifySet(t => t.BulkEdit = false, Times.AtLeastOnce);
        }

        [TestMethod]
        public void RemoveTracks_ShouldRemoveGivenTracks()
        {
            // Arrange
            var cuesheet = new Cuesheet();

            var track1 = new Track { Position = 1 };
            var track2 = new Track { Position = 2 };

            var audiofile = new Audiofile
            {
                Tracks = [track1, track2]
            };
            cuesheet.Audiofiles.Add(audiofile);

            _sessionStateContainer.Setup(s => s.GetActiveCuesheet()).Returns(cuesheet);

            // Act
            _audiofileManager.RemoveTracks(audiofile, [track1]);

            // Assert
            Assert.DoesNotContain(track1, audiofile.Tracks);
            Assert.Contains(track2, audiofile.Tracks);
            Assert.IsNull(track1.Audiofile);
            _traceChangeManager.VerifySet(t => t.BulkEdit = true, Times.AtLeastOnce);
            _traceChangeManager.VerifySet(t => t.BulkEdit = false, Times.AtLeastOnce);
        }
    }
}
