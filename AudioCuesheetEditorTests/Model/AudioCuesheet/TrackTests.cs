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
            var testHelper = new TestHelper();
            var track = new Track(testHelper.CuesheetController);
            Assert.IsNull(track.Length);
            track.Length = new TimeSpan(0, 2, 30);
            Assert.IsNotNull(track.Length);
            Assert.IsNotNull(track.Begin);
            Assert.IsNotNull(track.End);
            Assert.AreEqual(track.Begin, TimeSpan.Zero);
            Assert.AreEqual(track.End, new TimeSpan(0, 2, 30));
            Assert.AreEqual(track.Length, new TimeSpan(0, 2, 30));
            track.Begin = new TimeSpan(0, 25, 0);
            Assert.IsNull(track.Length);
            track.Length = new TimeSpan(0, 3, 30);
            Assert.IsNotNull(track.Length);
            Assert.AreEqual(track.Length, new TimeSpan(0, 3, 30));
            Assert.AreEqual(track.End, new TimeSpan(0, 28, 30));
            track.Begin = null;
            Assert.IsNull(track.Length);
            track.Length = new TimeSpan(0, 5, 15);
            Assert.AreEqual(track.End, new TimeSpan(0, 28, 30));
            Assert.AreEqual(track.Length, new TimeSpan(0, 5, 15));
            Assert.AreEqual(track.Begin, new TimeSpan(0, 23, 15));
        }
    }
}