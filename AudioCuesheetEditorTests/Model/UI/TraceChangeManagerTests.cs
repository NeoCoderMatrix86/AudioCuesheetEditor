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
        private static T CallInItsOwnScope<T>(Func<T> getter)
        {
            return getter();
        }

        [TestMethod()]
        public void TraceChangesTest()
        {
            TraceChangeManager.Instance.Reset();
            var callResult = CallInItsOwnScope(() =>
            {
                var track = new Track();
                TraceChangeManager.Instance.TraceChanges(track);
                var track2 = new Track();
                TraceChangeManager.Instance.TraceChanges(track2);
                track2.Title = "Title 2";
                track.Artist = "Artist 1";
                track = null;
                GC.Collect();
                return track2;
            });
            GC.Collect();
            Assert.IsTrue(TraceChangeManager.Instance.CanUndo);
            TraceChangeManager.Instance.Undo();
            Assert.IsNull(callResult.Title);
            Assert.IsFalse(TraceChangeManager.Instance.CanUndo);
        }

        [TestMethod()]
        public void UndoTest()
        {
            TraceChangeManager.Instance.Reset();
            var track = new Track();
            TraceChangeManager.Instance.TraceChanges(track);
            Assert.IsFalse(TraceChangeManager.Instance.CanUndo);
            TraceChangeManager.Instance.Undo();
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
            Assert.IsFalse(TraceChangeManager.Instance.CanRedo);
            TraceChangeManager.Instance.Redo();
            TraceChangeManager.Instance.Undo();
            Assert.IsTrue(TraceChangeManager.Instance.CanRedo);
            Assert.IsNull(track.End);
            TraceChangeManager.Instance.Redo();
            Assert.AreEqual(new TimeSpan(0, 3, 23), track.End);
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
        
        [TestMethod()]
        public void UndoRedoCombinationTest()
        {
            TraceChangeManager.Instance.Reset();
            var track = new Track();
            TraceChangeManager.Instance.TraceChanges(track);
            Assert.IsFalse(TraceChangeManager.Instance.CanUndo);
            track.Position = 3;
            Assert.IsTrue(TraceChangeManager.Instance.CanUndo);
            track.Artist = "Artist";
            track.End = new TimeSpan(0, 3, 23);
            Assert.IsFalse(TraceChangeManager.Instance.CanRedo);
            TraceChangeManager.Instance.Redo();
            TraceChangeManager.Instance.Undo();
            Assert.IsTrue(TraceChangeManager.Instance.CanRedo);
            Assert.IsNull(track.End);
            TraceChangeManager.Instance.Redo();
            Assert.AreEqual(new TimeSpan(0, 3, 23), track.End);
            Assert.IsTrue(TraceChangeManager.Instance.CanUndo);
            TraceChangeManager.Instance.Undo();
            Assert.IsNull(track.End);
            TraceChangeManager.Instance.Redo();
            Assert.AreEqual(new TimeSpan(0, 3, 23), track.End);
        }
    }
}