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
using AudioCuesheetEditor.Services;
using AudioCuesheetEditor.Services.AudioCuesheet;
using AudioCuesheetEditor.Services.UI;
using Microsoft.JSInterop;
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
        private readonly Mock<IAudiofileManager> _audiofileManager;
        private readonly Mock<IJSRuntime> _jsRuntime;

        public CuesheetManagerTests()
        {
            _traceChangeManager = new();
            _sessionStateContainer = new();
            _audiofileManager = new();
            _trackManager = new();
            _jsRuntime = new Mock<IJSRuntime>();
            _trackManager.Setup(x => x.SetProperty(It.IsAny<Track>(),It.IsAny<Expression<Func<Track, It.IsAnyType>>>(),It.IsAny<It.IsAnyType>()))
                .Callback((Track track, LambdaExpression propExpr, object value) =>
                {
                    var memberExpression = propExpr.Body as MemberExpression;
                    if (memberExpression?.Member is PropertyInfo propertyInfo)
                    {
                        propertyInfo.SetValue(track, value);
                    }
                });
            _cuesheetManager = new(_traceChangeManager.Object, _sessionStateContainer.Object, _trackManager.Object, _audiofileManager.Object, _jsRuntime.Object);
        }

        [TestMethod]
        public void SetProperty_NewValue_ChangesPropertyAndSetsTracedChange()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(cuesheet);
            // Act
            _cuesheetManager.SetProperty(x => x.Artist, "Artist");
            // Assert
            Assert.AreEqual("Artist", cuesheet.Artist);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Cuesheet.Artist))), Times.Once);
        }

        [TestMethod]
        public void SetProperty_EqualValue_DoesntChangeProperty()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Artist = "Artist"
            };
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(cuesheet);
            // Act
            _cuesheetManager.SetProperty(x => x.Artist, cuesheet.Artist);
            // Assert
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Cuesheet.Artist))), Times.Never);
        }

        [TestMethod]
        public void SetProperty_DeletedAudiofile_RevokesObjectURLs()
        {
            // Arrange
            var audiofile1 = new Audiofile()
            {
                Name = "Test 1.mp3",
                AudioCodec = Audiofile.AudioCodecs.First(x => x.FileExtension == ".mp3"),
                ObjectURL = "objecturl 1"
            };
            var audiofile2 = new Audiofile()
            {
                Name = "Test 2.mp3",
                AudioCodec = Audiofile.AudioCodecs.First(x => x.FileExtension == ".mp3"),
                ObjectURL = "objecturl 2"
            };
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [audiofile1, audiofile2]
            };
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(cuesheet);
            var files = new List<Audiofile>(cuesheet.Audiofiles);
            files.Remove(audiofile2);
            // Act
            _cuesheetManager.SetProperty(x => x.Audiofiles, files);
            // Assert
            _jsRuntime.Verify(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>("revokeAudioObjectURL", It.Is<object[]>(args => args.Length == 1 && args[0].Equals(audiofile2.ObjectURL))), Times.Once());
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
                Audiofiles = [
                    new() {
                        Tracks = [
                            new()
                        ]
                    }
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
        public void IsRecordingPossible_RecordingAlreadyRunning_ReturnsFailure()
        {
            // Arrange
            var cuesheet = new Cuesheet
            {
                Audiofiles = [
                    new() {
                    }
                ],
                RecordingStart = DateTime.UtcNow
            };
            _sessionStateContainer.SetupProperty(x => x.Cuesheet, cuesheet);
            // Act
            var result = _cuesheetManager.IsRecordingPossible;
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Record is already running!", result.Error!.Message);
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
            Assert.HasCount(1, cuesheet.Audiofiles);
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
                Audiofiles = [
                    new() {
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
            Assert.IsNotNull(cuesheet.Audiofiles.First().Tracks.Last().End);
        }

        [TestMethod]
        public void StopRecording_WithoutActiveRecording_ChangesNothing()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
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
            Assert.IsNull(cuesheet.Audiofiles.First().Tracks.Last().End);
        }

        [TestMethod]
        public void IsMoveUpPossible_TracksAbove_ReturnsTrue()
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
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
                        Tracks = [track3, track2, track1]
                    }
                ],
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            // Act
            var result = _cuesheetManager.IsMoveUpPossible([track2, track3]);
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsMoveUpPossible_NoTracksAbove_ReturnsFalse()
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
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
                        Tracks = [track2, track1, track3]
                    }
                ],
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            // Act
            var result = _cuesheetManager.IsMoveUpPossible([track1, track2]);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsMoveUpPossible_EmptyTracksCollection_ReturnsFalse()
        {
            // Arrange
            // Act
            var result = _cuesheetManager.IsMoveUpPossible(new HashSet<Track>());
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsMoveDownPossible_TracksBelow_ReturnsTrue()
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
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
                        Tracks = [track3, track2, track1]
                    }
                ],
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(cuesheet);
            // Act
            var result = _cuesheetManager.IsMoveDownPossible([track2, track1]);
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsMoveDownPossible_NoTracksBelow_ReturnsFalse()
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
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
                        Tracks = [track3, track2, track1]
                    }
                ],
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            // Act
            var result = _cuesheetManager.IsMoveDownPossible([track3, track2]);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsMoveDownPossible_EmptyTracksCollection_ReturnsFalse()
        {
            // Arrange
            // Act
            var result = _cuesheetManager.IsMoveDownPossible([]);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MoveUp_TracksAbove_ReturnsSuccess()
        {
            // Arrange
            var track1End = new TimeSpan(0, 3, 12);
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = track1End,
                IsLinkedToPreviousTrack = true
            };
            var track2End = new TimeSpan(0, 7, 34);
            var track2 = new Track()
            {
                Position = 2,
                Begin = track1End,
                End = track2End,
                IsLinkedToPreviousTrack = true
            };
            var track3 = new Track()
            {
                Position = 3,
                Begin = track2End,
                End = new TimeSpan(0, 10, 4),
                IsLinkedToPreviousTrack = true
            };
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
                        Tracks = [track1]
                    },
                    new() {
                        Tracks = [track3, track2]
                    }
                ],
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            track1.Audiofile = cuesheet.Audiofiles.First();
            track2.Audiofile = cuesheet.Audiofiles.Last();
            track3.Audiofile = cuesheet.Audiofiles.Last();
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(cuesheet);
            // Act
            var result = _cuesheetManager.MoveUp([track2, track3]);
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual((ushort?)1, track2.Position);
            Assert.AreEqual(TimeSpan.Zero, track2.Begin);
            Assert.AreEqual(track1End, track2.End);
            Assert.AreEqual((ushort?)2, track3.Position);
            Assert.AreEqual(track1End, track3.Begin);
            Assert.AreEqual(track2End, track3.End);
            Assert.AreEqual((ushort?)3, track1.Position);
            Assert.AreEqual(track2End, track1.Begin);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet.Audiofiles.First() && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Exactly(2));
            _traceChangeManager.VerifySet(x => x.BulkEdit = true, Times.Once);
            _traceChangeManager.VerifySet(x => x.BulkEdit = false, Times.Once);
        }

        [TestMethod]
        public void MoveUp_NoTracksAbove_ReturnsFailure()
        {
            // Arrange
            var track1End = new TimeSpan(0, 3, 12);
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = track1End,
                IsLinkedToPreviousTrack = true
            };
            var track2End = new TimeSpan(0, 7, 34);
            var track2 = new Track()
            {
                Position = 2,
                Begin = track1End,
                End = track2End,
                IsLinkedToPreviousTrack = true
            };
            var track3 = new Track()
            {
                Position = 3,
                Begin = track2End,
                End = new TimeSpan(0, 10, 4),
                IsLinkedToPreviousTrack = true
            };
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
                        Tracks = [track3, track2, track1]
                    }
                ]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            track1.Audiofile = cuesheet.Audiofiles.First();
            track2.Audiofile = cuesheet.Audiofiles.First();
            track3.Audiofile = cuesheet.Audiofiles.First();
            _sessionStateContainer.Setup(x => x.Cuesheet).Returns(cuesheet);
            // Act
            var result = _cuesheetManager.MoveUp([track2, track1]);
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorType.NotPossible, result.Error!.Type);
            Assert.AreEqual((ushort?)1, track1.Position);
            Assert.AreEqual(TimeSpan.Zero, track1.Begin);
            Assert.AreEqual(track1End, track1.End);
            Assert.AreEqual((ushort?)2, track2.Position);
            Assert.AreEqual(track1End, track2.Begin);
            Assert.AreEqual(track2End, track2.End);
            Assert.AreEqual((ushort?)3, track3.Position);
            Assert.AreEqual(track2End, track3.Begin);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet.Audiofiles.First() && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Never);
            _traceChangeManager.VerifySet(x => x.BulkEdit = true, Times.Never);
            _traceChangeManager.VerifySet(x => x.BulkEdit = false, Times.Never);
        }

        [TestMethod]
        public void MoveUp_NoTracksSelected_ReturnsFailure()
        {
            // Arrange
            var track1End = new TimeSpan(0, 3, 12);
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = track1End,
                IsLinkedToPreviousTrack = true
            };
            var track2End = new TimeSpan(0, 7, 34);
            var track2 = new Track()
            {
                Position = 2,
                Begin = track1End,
                End = track2End,
                IsLinkedToPreviousTrack = true
            };
            var track3 = new Track()
            {
                Position = 3,
                Begin = track2End,
                End = new TimeSpan(0, 10, 4),
                IsLinkedToPreviousTrack = true
            };
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
                        Tracks = [track3, track2, track1]
                    }
                ],
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            track1.Audiofile = cuesheet.Audiofiles.First();
            track2.Audiofile = cuesheet.Audiofiles.First();
            track3.Audiofile = cuesheet.Audiofiles.First();
            _sessionStateContainer.Setup(x => x.Cuesheet).Returns(cuesheet);
            // Act
            var result = _cuesheetManager.MoveUp([]);
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorType.NotPossible, result.Error!.Type);
            Assert.AreEqual((ushort?)1, track1.Position);
            Assert.AreEqual(TimeSpan.Zero, track1.Begin);
            Assert.AreEqual(track1End, track1.End);
            Assert.AreEqual((ushort?)2, track2.Position);
            Assert.AreEqual(track1End, track2.Begin);
            Assert.AreEqual(track2End, track2.End);
            Assert.AreEqual((ushort?)3, track3.Position);
            Assert.AreEqual(track2End, track3.Begin);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet.Audiofiles.First() && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Never);
            _traceChangeManager.VerifySet(x => x.BulkEdit = true, Times.Never);
            _traceChangeManager.VerifySet(x => x.BulkEdit = false, Times.Never);
        }

        [TestMethod]
        public void MoveDown_TracksBelow_ReturnsSucess()
        {
            // Arrange
            var track1End = new TimeSpan(0, 3, 12);
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = track1End,
                IsLinkedToPreviousTrack = true
            };
            var track2End = new TimeSpan(0, 7, 34);
            var track2 = new Track()
            {
                Position = 2,
                Begin = track1End,
                End = track2End,
                IsLinkedToPreviousTrack = true
            };
            var track3 = new Track()
            {
                Position = 3,
                Begin = track2End,
                End = new TimeSpan(0, 10, 4),
                IsLinkedToPreviousTrack = true
            };
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
                        Tracks = [track2, track1]
                    },
                    new() {
                        Tracks = [track3]
                    }
                ]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            track1.Audiofile = cuesheet.Audiofiles.First();
            track2.Audiofile = cuesheet.Audiofiles.First();
            track3.Audiofile = cuesheet.Audiofiles.Last();
            _sessionStateContainer.Setup(x => x.GetActiveCuesheet()).Returns(cuesheet);
            // Act
            var result = _cuesheetManager.MoveDown([track2, track1]);
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual((ushort?)1, track3.Position);
            Assert.AreEqual(TimeSpan.Zero, track3.Begin);
            Assert.AreEqual(track1End, track3.End);
            Assert.AreEqual((ushort?)2, track1.Position);
            Assert.AreEqual(track1End, track1.Begin);
            Assert.AreEqual(track2End, track1.End);
            Assert.AreEqual((ushort?)3, track2.Position);
            Assert.AreEqual(track2End, track2.Begin);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet.Audiofiles.First() && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Exactly(2));
            _traceChangeManager.VerifySet(x => x.BulkEdit = true, Times.Once);
            _traceChangeManager.VerifySet(x => x.BulkEdit = false, Times.Once);
        }

        [TestMethod]
        public void MoveDown_NoTracksBelow_ReturnsFailure()
        {
            // Arrange
            var track1End = new TimeSpan(0, 3, 12);
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = track1End,
                IsLinkedToPreviousTrack = true
            };
            var track2End = new TimeSpan(0, 7, 34);
            var track2 = new Track()
            {
                Position = 2,
                Begin = track1End,
                End = track2End,
                IsLinkedToPreviousTrack = true
            };
            var track3 = new Track()
            {
                Position = 3,
                Begin = track2End,
                End = new TimeSpan(0, 10, 4),
                IsLinkedToPreviousTrack = true
            };
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
                        Tracks = [track3, track2, track1]
                    }
                ]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            track1.Audiofile = cuesheet.Audiofiles.First();
            track2.Audiofile = cuesheet.Audiofiles.First();
            track3.Audiofile = cuesheet.Audiofiles.First();
            _sessionStateContainer.Setup(x => x.Cuesheet).Returns(cuesheet);
            // Act
            var result = _cuesheetManager.MoveDown([track2, track3]);
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorType.NotPossible, result.Error!.Type);
            Assert.AreEqual((ushort?)1, track1.Position);
            Assert.AreEqual(TimeSpan.Zero, track1.Begin);
            Assert.AreEqual(track1End, track1.End);
            Assert.AreEqual((ushort?)2, track2.Position);
            Assert.AreEqual(track1End, track2.Begin);
            Assert.AreEqual(track2End, track2.End);
            Assert.AreEqual((ushort?)3, track3.Position);
            Assert.AreEqual(track2End, track3.Begin);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet.Audiofiles.First() && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Never);
            _traceChangeManager.VerifySet(x => x.BulkEdit = true, Times.Never);
            _traceChangeManager.VerifySet(x => x.BulkEdit = false, Times.Never);
        }

        [TestMethod]
        public void MoveDown_NoTracksSelected_ReturnsFailure()
        {
            // Arrange
            var track1End = new TimeSpan(0, 3, 12);
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = track1End,
                IsLinkedToPreviousTrack = true
            };
            var track2End = new TimeSpan(0, 7, 34);
            var track2 = new Track()
            {
                Position = 2,
                Begin = track1End,
                End = track2End,
                IsLinkedToPreviousTrack = true
            };
            var track3 = new Track()
            {
                Position = 3,
                Begin = track2End,
                End = new TimeSpan(0, 10, 4),
                IsLinkedToPreviousTrack = true
            };
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new() {
                        Tracks = [track3, track2, track1]
                    }
                ]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            track1.Audiofile = cuesheet.Audiofiles.First();
            track2.Audiofile = cuesheet.Audiofiles.First();
            track3.Audiofile = cuesheet.Audiofiles.First();
            _sessionStateContainer.Setup(x => x.Cuesheet).Returns(cuesheet);
            // Act
            var result = _cuesheetManager.MoveDown([]);
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorType.NotPossible, result.Error!.Type);
            Assert.AreEqual((ushort?)1, track1.Position);
            Assert.AreEqual(TimeSpan.Zero, track1.Begin);
            Assert.AreEqual(track1End, track1.End);
            Assert.AreEqual((ushort?)2, track2.Position);
            Assert.AreEqual(track1End, track2.Begin);
            Assert.AreEqual(track2End, track2.End);
            Assert.AreEqual((ushort?)3, track3.Position);
            Assert.AreEqual(track2End, track3.Begin);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == cuesheet.Audiofiles && y.TraceableChange.PropertyName == nameof(Audiofile.Tracks))), Times.Never);
            _traceChangeManager.VerifySet(x => x.BulkEdit = true, Times.Never);
            _traceChangeManager.VerifySet(x => x.BulkEdit = false, Times.Never);
        }
    }
}
