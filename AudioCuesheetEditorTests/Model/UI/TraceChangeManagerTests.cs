using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditor.Model.AudioCuesheet;

namespace AudioCuesheetEditor.Model.UI.Tests
{
    [TestClass()]
    public class TraceChangeManagerTests
    {
        [TestMethod()]
        public void TraceChangesTest()
        {
            TraceChangeManager.Instance.Reset();
            var track = new Track();
            TraceChangeManager.Instance.TraceChanges(track);
            var track2 = new Track();
            TraceChangeManager.Instance.TraceChanges(track2);
            track2.Title = "Title 2";
            track.Artist = "Artist 1";
            track = null;
            GC.WaitForFullGCComplete();
            TraceChangeManager.Instance.Undo();
            //TODO: Doesn't work!
            Assert.IsNull(track2.Title);
        }

        [TestMethod()]
        public void UndoTest()
        {
            TraceChangeManager.Instance.Reset();
            var track = new Track();
            TraceChangeManager.Instance.TraceChanges(track);
            Assert.IsFalse(TraceChangeManager.Instance.CanUndo);
            track.Position = 3;
            Assert.IsTrue(TraceChangeManager.Instance.CanUndo);
            track.Artist = "Artist";
            track.End = new TimeSpan(0, 3, 23);
            TraceChangeManager.Instance.Undo();
            Assert.IsTrue(TraceChangeManager.Instance.CanRedo);
            Assert.IsNull(track.End);
            var track2 = new Track();
            TraceChangeManager.Instance.TraceChanges(track2);
            track2.Position = 5;
            Assert.IsFalse(TraceChangeManager.Instance.CanRedo);
            track2.Artist = "Artist 2";
            track2.Title = "Title 2";
            track2.Length = new TimeSpan(0, 4, 12);
            Assert.AreEqual(new TimeSpan(0, 4, 12), track2.Length);
            TraceChangeManager.Instance.Undo();
            Assert.IsNull(track2.Length);
        }

        [TestMethod()]
        public void RedoTest()
        {
            TraceChangeManager.Instance.Reset();
            var track = new Track();
            TraceChangeManager.Instance.TraceChanges(track);
            Assert.IsFalse(TraceChangeManager.Instance.CanUndo);
            track.Position = 3;
            Assert.IsTrue(TraceChangeManager.Instance.CanUndo);
            track.Artist = "Artist";
            track.End = new TimeSpan(0, 3, 23);
            TraceChangeManager.Instance.Undo();
            Assert.IsTrue(TraceChangeManager.Instance.CanRedo);
            Assert.IsNull(track.End);
            var track2 = new Track();
            TraceChangeManager.Instance.TraceChanges(track2);
            track2.Position = 5;
            Assert.IsFalse(TraceChangeManager.Instance.CanRedo);
            track2.Artist = "Artist 2";
            track2.Title = "Title 2";
            track2.Length = new TimeSpan(0, 4, 12);
            Assert.AreEqual(new TimeSpan(0, 4, 12), track2.Length);
            TraceChangeManager.Instance.Undo();
            Assert.IsNull(track2.Length);
            TraceChangeManager.Instance.Redo();
            Assert.AreEqual(new TimeSpan(0, 4, 12), track2.Length);
        }
    }
}