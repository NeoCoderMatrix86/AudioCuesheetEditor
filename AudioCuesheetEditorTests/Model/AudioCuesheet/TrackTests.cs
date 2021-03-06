﻿//This file is part of AudioCuesheetEditor.

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

namespace AudioCuesheetEditor.Model.AudioCuesheet.Tests
{
    [TestClass()]
    public class TrackTests
    {
        [TestMethod()]
        public void TrackTest()
        {
            //Tests for Length
            var track = new Track();
            Assert.IsNull(track.Length);
            track.Length = new TimeSpan(0, 2, 30);
            Assert.IsNotNull(track.Length);
            Assert.IsNotNull(track.Begin);
            Assert.IsNotNull(track.End);
            Assert.AreEqual(TimeSpan.Zero, track.Begin);
            Assert.AreEqual(new TimeSpan(0, 2, 30), track.End);
            Assert.AreEqual(new TimeSpan(0, 2, 30), track.Length);
            track.Begin = new TimeSpan(0, 25, 0);
            Assert.IsNull(track.Length);
            track.Length = new TimeSpan(0, 3, 30);
            Assert.IsNotNull(track.Length);
            Assert.AreEqual(new TimeSpan(0, 3, 30), track.Length);
            Assert.AreEqual(new TimeSpan(0, 28, 30), track.End);
            track.Begin = null;
            Assert.IsNull(track.Length);
            track.Length = new TimeSpan(0, 5, 15);
            Assert.AreEqual(new TimeSpan(0, 28, 30), track.End);
            Assert.AreEqual(new TimeSpan(0, 5, 15), track.Length);
            Assert.AreEqual(new TimeSpan(0, 23, 15), track.Begin);
        }

        [TestMethod()]
        public void SetLengthTest()
        {
            var track = new Track();
            Assert.IsNull(track.Length);
            Assert.IsNull(track.Begin);
            Assert.IsNull(track.End);
            track.Begin = new TimeSpan(0, 2, 43);
            track.End = new TimeSpan(0, 5, 23);
            Assert.IsNotNull(track.Length);
            track.Length = new TimeSpan(0, 2, 0);
            Assert.AreEqual(new TimeSpan(0, 4, 43), track.End);
        }

        [TestMethod()]
        public void LinkTrackTest()
        {
            //Test LinkedPreviousTrack
            var track = new Track
            {
                Begin = TimeSpan.Zero,
                Position = 1,
                End = new TimeSpan(0, 3, 23)
            };
            Assert.IsNull(track.LinkedPreviousTrack);
            var track2 = new Track
            {
                Begin = track.End,
                End = new TimeSpan(0, 5, 45),
                Position = 2
            };
            Assert.IsNull(track2.LinkedPreviousTrack);
            track2.LinkedPreviousTrack = track;
            Assert.AreEqual(track.End, track2.Begin);
            track.End = new TimeSpan(0, 3, 45);
            Assert.AreEqual(track.End, track2.Begin);
            var track3 = new Track
            {
                Begin = track2.End,
                Position = 7,
                End = new TimeSpan(0, 10, 12)
            };
            track3.LinkedPreviousTrack = track2;
            track3.End = new TimeSpan(0, 15, 2);
            track3.Position = 3;
            Assert.AreEqual(track2.End, track3.Begin);
            Assert.AreEqual((uint)1, track.Position);
            Assert.AreEqual((uint)2, track2.Position);
            Assert.AreEqual((uint)3, track3.Position);
        }
    }
}