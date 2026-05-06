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
using AudioCuesheetEditor.Model.AudioCuesheet.Import;
using AudioCuesheetEditor.Model.UI;
using AudioCuesheetEditor.Services.AudioCuesheet;
using AudioCuesheetEditor.Services.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace AudioCuesheetEditor.Tests.Services.AudioCuesheet
{
    [TestClass]
    public class TrackManagerTests
    {
        private readonly TrackManager _trackManager;
        private readonly Mock<ITraceChangeManager> _traceChangeManager;

        public TrackManagerTests()
        {
            _traceChangeManager = new Mock<ITraceChangeManager>();
            _trackManager = new(_traceChangeManager.Object);
        }

        [TestMethod]
        public void SetProperty_NewValue_ChangesPropertyAndSetsTracedChange()
        {
            // Arrange
            var track = new Track();
            var artist = "Artist 1";
            // Act
            _trackManager.SetProperty(track, x => x.Artist, artist);
            // Assert
            Assert.AreEqual(artist, track.Artist);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == track && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Track.Artist))), Times.Once);
        }

        [TestMethod]
        public void SetProperty_EqualValue_DoesntChangeProperty()
        {
            // Arrange
            var artist = "Artist 1";
            var track = new Track()
            {
                Artist = artist,
                Position = 2
            };
            // Act
            _trackManager.SetProperty(track, x => x.Artist, artist);
            // Assert
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == track && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Track.Artist))), Times.Never);
        }

        [TestMethod]
        public void Clone_ImportTrack_ReturnsTrackClone()
        {
            // Arrange
            var importTrack = new ImportTrack()
            {
                Artist = "Artist 1",
                Title = "Title 1",
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 2, 54),
                IsLinkedToPreviousTrack = true,
                Position = 1,
                PostGap = new TimeSpan(0, 0, 2),
                PreGap = new TimeSpan(0, 0, 4),
                Flags = [Flag.FourCH, Flag.DCP],
                StartDateTime = new DateTime(2024, 2, 28, 19, 23, 54)
            };
            // Act
            var track = _trackManager.Clone(importTrack);
            // Assert
            Assert.AreEqual(importTrack.Artist, track.Artist);
            Assert.AreEqual(importTrack.Title, track.Title);
            Assert.AreEqual(importTrack.Begin, track.Begin);
            Assert.AreEqual(importTrack.End, track.End);
            Assert.AreEqual(importTrack.IsLinkedToPreviousTrack, track.IsLinkedToPreviousTrack);
            Assert.AreEqual(importTrack.Position, track.Position);
            Assert.AreEqual(importTrack.PostGap, track.PostGap);
            Assert.AreEqual(importTrack.PreGap, track.PreGap);
            Assert.AreEqual(importTrack.Flags, track.Flags);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == track)), Times.Never);
        }

        [TestMethod]
        public void Clone_ImportTrackWithLength_ReturnsTrackCloneWithLengthSet()
        {
            // Arrange
            var importTrack = new ImportTrack()
            {
                Artist = "Artist 1",
                Title = "Title 1",
                Begin = TimeSpan.Zero,
                Length = new TimeSpan(0, 2, 54),
                IsLinkedToPreviousTrack = true,
                Position = 1,
                PostGap = new TimeSpan(0, 0, 2),
                PreGap = new TimeSpan(0, 0, 4),
                Flags = [Flag.FourCH, Flag.DCP],
                StartDateTime = new DateTime(2024, 2, 28, 19, 23, 54)
            };
            // Act
            var track = _trackManager.Clone(importTrack);
            // Assert
            Assert.AreEqual(importTrack.Artist, track.Artist);
            Assert.AreEqual(importTrack.Title, track.Title);
            Assert.AreEqual(importTrack.Begin, track.Begin);
            Assert.AreEqual(importTrack.Length, track.End);
            Assert.AreEqual(importTrack.IsLinkedToPreviousTrack, track.IsLinkedToPreviousTrack);
            Assert.AreEqual(importTrack.Position, track.Position);
            Assert.AreEqual(importTrack.PostGap, track.PostGap);
            Assert.AreEqual(importTrack.PreGap, track.PreGap);
            Assert.AreEqual(importTrack.Flags, track.Flags);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == track)), Times.Never);
        }

        [TestMethod]
        public void CopyValues_AllDefaultValues_CopiesAllValues()
        {
            // Arrange
            var source = new Track()
            {
                Artist = "Artist 1",
                Title = "Title 1",
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 2, 54),
                IsLinkedToPreviousTrack = true,
                Position = 1,
                PostGap = new TimeSpan(0, 0, 2),
                PreGap = new TimeSpan(0, 0, 4),
                Flags = [Flag.FourCH, Flag.DCP]
            };
            var target = new Track();
            // Act
            _trackManager.CopyValues(source, target);
            // Assert
            Assert.AreEqual(source.Artist, target.Artist);
            Assert.AreEqual(source.Title, target.Title);
            Assert.AreEqual(source.Begin, target.Begin);
            Assert.AreEqual(source.End, target.End);
            Assert.AreEqual(source.IsLinkedToPreviousTrack, target.IsLinkedToPreviousTrack);
            Assert.AreEqual(source.Position, target.Position);
            Assert.AreEqual(source.PostGap, target.PostGap);
            Assert.AreEqual(source.PreGap, target.PreGap);
            Assert.AreEqual(source.Flags, target.Flags);
            Assert.AreEqual(source.Length, target.Length);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == target)), Times.Exactly(9));
        }

        [TestMethod]
        public void CopyValues_ValuesSet_CopiesOnlySetValues()
        {
            // Arrange
            var source = new Track()
            {
                Artist = "Artist 1",
                Title = "Title 1",
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 2, 54),
                IsLinkedToPreviousTrack = true,
                Position = 1,
                PostGap = new TimeSpan(0, 0, 2),
                PreGap = new TimeSpan(0, 0, 4),
                Flags = [Flag.FourCH, Flag.DCP]
            };
            var target = new Track();
            // Act
            _trackManager.CopyValues(source, target, setBegin: false, setPostGap:false, setLength: false, setFlags: false);
            // Assert
            Assert.AreEqual(source.Artist, target.Artist);
            Assert.AreEqual(source.Title, target.Title);
            Assert.AreNotEqual(source.Begin, target.Begin);
            Assert.AreEqual(source.End, target.End);
            Assert.AreEqual(source.IsLinkedToPreviousTrack, target.IsLinkedToPreviousTrack);
            Assert.AreEqual(source.Position, target.Position);
            Assert.AreNotEqual(source.PostGap, target.PostGap);
            Assert.AreEqual(source.PreGap, target.PreGap);
            Assert.AreNotEqual(source.Flags, target.Flags);
            Assert.AreNotEqual(source.Length, target.Length);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == target)), Times.Exactly(6));
        }

        [TestMethod]
        public void GetPreviousLinkedTrack_LinkedTracks_ReturnsTrack()
        {
            // Arrange
            var track1 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Position = 1
            };
            var track2 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Position = 2
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1, track2]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            var linkedTrack = _trackManager.GetPreviousLinkedTrack(track2);
            // Assert
            Assert.IsNotNull(linkedTrack);
            Assert.AreEqual(track1, linkedTrack);
        }

        [TestMethod]
        public void GetPreviousLinkedTrack_NotLinkedTracks_ReturnsNull()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1
            };
            var track2 = new Track()
            {
                Position = 2
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1, track2]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            var linkedTrack = _trackManager.GetPreviousLinkedTrack(track2);
            // Assert
            Assert.IsNull(linkedTrack);
        }

        [TestMethod]
        public void GetPreviousLinkedTrack_WithoutPosition_ReturnsNull()
        {
            // Arrange
            var track1 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Position = 1
            };
            var track2 = new Track()
            {
                IsLinkedToPreviousTrack = true
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1, track2]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            var linkedTrack = _trackManager.GetPreviousLinkedTrack(track2);
            // Assert
            Assert.IsNull(linkedTrack);
        }

        [TestMethod]
        public void GetPreviousLinkedTrack_WithBegin_ReturnsTrack()
        {
            // Arrange
            var track1 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Position = 1,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 6, 37)
            };
            var track2 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Begin = track1.End
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1, track2]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            var linkedTrack = _trackManager.GetPreviousLinkedTrack(track2);
            // Assert
            Assert.IsNotNull(linkedTrack);
            Assert.AreEqual(track1, linkedTrack);
        }

        [TestMethod]
        public void GetNextLinkedTrack_LinkedTracks_ReturnsTrack()
        {
            // Arrange
            var track1 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Position = 1
            };
            var track2 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Position = 2
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1, track2]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            var linkedTrack = _trackManager.GetNextLinkedTrack(track1);
            // Assert
            Assert.IsNotNull(linkedTrack);
            Assert.AreEqual(track2, linkedTrack);
        }

        [TestMethod]
        public void GetNextLinkedTrack_NotLinkedTracks_ReturnsNull()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1
            };
            var track2 = new Track()
            {
                Position = 2
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1, track2]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            var linkedTrack = _trackManager.GetNextLinkedTrack(track1);
            // Assert
            Assert.IsNull(linkedTrack);
        }

        [TestMethod]
        public void GetNextLinkedTrack_WithoutPosition_ReturnsNull()
        {
            // Arrange
            var track1 = new Track()
            {
                IsLinkedToPreviousTrack = true
            };
            var track2 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Position = 2
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1, track2]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            var linkedTrack = _trackManager.GetNextLinkedTrack(track1);
            // Assert
            Assert.IsNull(linkedTrack);
        }

        [TestMethod]
        public void GetNextLinkedTrack_WithBegin_ReturnsTrack()
        {
            // Arrange
            var track1 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 8, 32)
            };
            var track2 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Position = 2,
                Begin = track1.End,
                End = new TimeSpan(0, 13, 54)
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1, track2]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            var linkedTrack = _trackManager.GetNextLinkedTrack(track1);
            // Assert
            Assert.IsNotNull(linkedTrack);
            Assert.AreEqual(track2, linkedTrack);
        }

        [TestMethod]
        public void GetNextLinkedTrack_OnlyOneTrack_ReturnsNull()
        {
            // Arrange
            var track1 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 8, 32)
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1]
            };
            track1.Cuesheet = cuesheet;
            // Act
            var linkedTrack = _trackManager.GetNextLinkedTrack(track1);
            // Assert
            Assert.IsNull(linkedTrack);
        }

        [TestMethod]
        public void GetNextLinkedTrack_MultiplePositions_ReturnsTrack()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1,
                IsLinkedToPreviousTrack = true,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 8, 32)
            };
            var track2 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Position = 2,
                Begin = track1.End,
                End = new TimeSpan(0, 13, 54)
            };
            var track3 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Position = 2,
                Begin = track2.End,
                End = new TimeSpan(0, 17, 32)
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1, track2, track3]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            track3.Cuesheet = cuesheet;
            // Act
            var linkedTrack = _trackManager.GetNextLinkedTrack(track1);
            // Assert
            Assert.IsNotNull(linkedTrack);
            Assert.AreEqual(track2, linkedTrack);
        }

        [TestMethod]
        public void RecalculateLinkedTracksProperties_TrackHasNoBegin_SetsBegin()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 3, 12)
            };
            var track2 = new Track()
            {
                Position = 2,
                IsLinkedToPreviousTrack = true
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track2, track1]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            _trackManager.RecalculateLinkedTracksProperties(track2);
            // Assert
            Assert.AreEqual(track1.End, track2.Begin);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == track2 && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Track.Begin))), Times.Once);
        }

        [TestMethod]
        public void RecalculateLinkedTracksProperties_TrackHasNoPosition_SetsPosition()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 3, 12)
            };
            var track2 = new Track()
            {
                IsLinkedToPreviousTrack = true,
                Begin = track1.End
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track2, track1]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            _trackManager.RecalculateLinkedTracksProperties(track2);
            // Assert
            Assert.AreEqual((ushort?)2, track2.Position);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == track2 && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Track.Position))), Times.Once);
        }

        [TestMethod]
        public void RecalculateLinkedTracksProperties_PreviousTrackHasNoEnd_SetsEnd()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
            };
            var track2 = new Track()
            {
                Position = 2,
                IsLinkedToPreviousTrack = true,
                Begin = new TimeSpan(0, 3, 12)
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track2, track1]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            _trackManager.RecalculateLinkedTracksProperties(track2);
            // Assert
            Assert.AreEqual(track2.Begin, track1.End);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == track1 && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Track.End))), Times.Once);
        }

        [TestMethod]
        public void RecalculateLinkedTracksProperties_NextTrackNoPosition_SetsPosition()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 3, 12)
            };
            var track2 = new Track()
            {
                Begin = track1.End,
                IsLinkedToPreviousTrack = true
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track2, track1]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            _trackManager.RecalculateLinkedTracksProperties(track1);
            // Assert
            Assert.AreEqual((ushort?)2, track2.Position);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == track2 && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Track.Position))), Times.Once);
        }

        [TestMethod]
        public void RecalculateLinkedTracksProperties_NextTrackNoBegin_SetsBegin()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 3, 12)
            };
            var track2 = new Track()
            {
                Position = 2,
                IsLinkedToPreviousTrack = true
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track2, track1]
            };
            track1.Cuesheet = cuesheet;
            track2.Cuesheet = cuesheet;
            // Act
            _trackManager.RecalculateLinkedTracksProperties(track1);
            // Assert
            Assert.AreEqual(track1.End, track2.Begin);
            _traceChangeManager.Verify(x => x.AddChange(It.Is<TracedChange>(y => y.TraceableObject == track2 && y.TraceableChange.PreviousValue == null && y.TraceableChange.PropertyName == nameof(Track.Begin))), Times.Once);
        }
    }
}
