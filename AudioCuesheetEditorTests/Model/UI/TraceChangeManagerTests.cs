using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditorTests.Utility;
using AudioCuesheetEditor.Model.IO.Import;
using System.IO;
using AudioCuesheetEditorTests.Properties;

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
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var callResult = CallInItsOwnScope(() =>
            {
                var track = new Track();
                manager.TraceChanges(track);
                var track2 = new Track();
                manager.TraceChanges(track2);
                track2.Title = "Title 2";
                track.Artist = "Artist 1";
                track = null;
                GC.Collect();
                return track2;
            });
            GC.Collect();
            Assert.IsTrue(manager.CanUndo);
            manager.Undo();
            Assert.IsNull(callResult.Title);
            Assert.IsFalse(manager.CanUndo);
        }

        [TestMethod()]
        public void UndoTest()
        {
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var track = new Track();
            manager.TraceChanges(track);
            Assert.IsFalse(manager.CanUndo);
            manager.Undo();
            track.Position = 3;
            Assert.IsTrue(manager.CanUndo);
            track.Artist = "Artist";
            track.End = new TimeSpan(0, 3, 23);
            manager.Undo();
            Assert.IsTrue(manager.CanRedo);
            Assert.IsNull(track.End);
            var track2 = new Track();
            manager.TraceChanges(track2);
            track2.Position = 5;
            Assert.IsFalse(manager.CanRedo);
            track2.Artist = "Artist 2";
            track2.Title = "Title 2";
            track2.Length = new TimeSpan(0, 4, 12);
            Assert.AreEqual(new TimeSpan(0, 4, 12), track2.Length);
            manager.Undo();
            Assert.IsNull(track2.Length);
        }

        [TestMethod()]
        public void RedoTest()
        {
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var track = new Track();
            manager.TraceChanges(track);
            Assert.IsFalse(manager.CanUndo);
            track.Position = 3;
            Assert.IsTrue(manager.CanUndo);
            track.Artist = "Artist";
            track.End = new TimeSpan(0, 3, 23);
            Assert.IsFalse(manager.CanRedo);
            manager.Redo();
            manager.Undo();
            Assert.IsTrue(manager.CanRedo);
            Assert.IsNull(track.End);
            manager.Redo();
            Assert.AreEqual(new TimeSpan(0, 3, 23), track.End);
            var track2 = new Track();
            manager.TraceChanges(track2);
            track2.Position = 5;
            Assert.IsFalse(manager.CanRedo);
            track2.Artist = "Artist 2";
            track2.Title = "Title 2";
            track2.Length = new TimeSpan(0, 4, 12);
            Assert.AreEqual(new TimeSpan(0, 4, 12), track2.Length);
            manager.Undo();
            Assert.IsNull(track2.Length);
            manager.Redo();
            Assert.AreEqual(new TimeSpan(0, 4, 12), track2.Length);
        }

        [TestMethod()]
        public void UndoRedoCombinationTest()
        {
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var track = new Track();
            manager.TraceChanges(track);
            Assert.IsFalse(manager.CanUndo);
            track.Position = 3;
            Assert.IsTrue(manager.CanUndo);
            track.Artist = "Artist";
            track.End = new TimeSpan(0, 3, 23);
            Assert.IsFalse(manager.CanRedo);
            manager.Redo();
            manager.Undo();
            Assert.IsTrue(manager.CanRedo);
            Assert.IsNull(track.End);
            manager.Redo();
            Assert.AreEqual(new TimeSpan(0, 3, 23), track.End);
            Assert.IsTrue(manager.CanUndo);
            manager.Undo();
            Assert.IsNull(track.End);
            manager.Redo();
            Assert.AreEqual(new TimeSpan(0, 3, 23), track.End);
        }

        [TestMethod()]
        public void TrackListTest()
        {
            var testhelper = new TestHelper();
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var cuesheet = new Cuesheet();
            manager.TraceChanges(cuesheet);
            Assert.IsFalse(manager.CanUndo);
            cuesheet.AddTrack(new Track(), testhelper.ApplicationOptions);
            Assert.IsTrue(manager.CanUndo);
            manager.Undo();
            Assert.AreEqual(0, cuesheet.Tracks.Count);
            Assert.IsTrue(manager.CanRedo);
            manager.Redo();
            Assert.AreEqual(1, cuesheet.Tracks.Count);
            Assert.IsFalse(manager.CanRedo);
        }

        [TestMethod()]
        public void ImportCuesheetTest()
        {
            var testhelper = new TestHelper();
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var textImportFile = new TextImportfile(new MemoryStream(Resources.Textimport_with_Cuesheetdata));
            textImportFile.TextImportScheme.SchemeCuesheet = "(?'Artist'\\A.*) - (?'Title'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Cataloguenumber'.{1,})";
            var cuesheet = new Cuesheet();
            Boolean eventFired = false;
            cuesheet.TrackAdded += delegate
            {
                eventFired = true;
            };
            manager.TraceChanges(cuesheet);
            Assert.IsFalse(manager.CanUndo);
            Assert.IsFalse(manager.CanRedo);
            Assert.IsNotNull(textImportFile.Cuesheet);
            cuesheet.Import(textImportFile.Cuesheet, testhelper.ApplicationOptions, manager);
            Assert.AreEqual("DJFreezeT", cuesheet.Artist);
            Assert.AreEqual("0123456789123", cuesheet.Cataloguenumber.Value);
            Assert.AreNotEqual(0, cuesheet.Tracks.Count);
            Assert.IsTrue(manager.CanUndo);
            manager.Undo();
            Assert.AreEqual(0, cuesheet.Tracks.Count);
            Assert.IsTrue(String.IsNullOrEmpty(cuesheet.Artist));
            Assert.IsTrue(String.IsNullOrEmpty(cuesheet.Cataloguenumber.Value));
            Assert.IsFalse(manager.CanUndo);
            Assert.IsTrue(manager.CanRedo);
            manager.Redo();
            Assert.AreEqual("DJFreezeT", cuesheet.Artist);
            Assert.AreEqual("0123456789123", cuesheet.Cataloguenumber.Value);
            Assert.AreNotEqual(0, cuesheet.Tracks.Count);
            Assert.IsFalse(eventFired);
        }

        [TestMethod()]
        public void RemoveTracksTest()
        {
            var testhelper = new TestHelper();
            testhelper.ApplicationOptions.LinkTracksWithPreviousOne = true;
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var cuesheet = new Cuesheet();
            manager.TraceChanges(cuesheet);
            var track1 = new Track()
            {
                Artist = "Track 1 Artist",
                Title = "Track 1 Title",
                End = new TimeSpan(0, 2, 30)
            };
            var track2 = new Track()
            {
                Artist = "Track 2 Artist",
                Title = "Track 2 Title",
                End = new TimeSpan(0, 4, 20)
            };
            var track3 = new Track()
            {
                Artist = "Track 3 Artist",
                Title = "Track 3 Title",
                End = new TimeSpan(0, 7, 12)
            };
            var track4 = new Track()
            {
                Artist = "Track 4 Artist",
                Title = "Track 4 Title",
                End = new TimeSpan(0, 9, 12)
            };
            cuesheet.AddTrack(track1, testhelper.ApplicationOptions);
            cuesheet.AddTrack(track2, testhelper.ApplicationOptions);
            cuesheet.AddTrack(track3, testhelper.ApplicationOptions);
            cuesheet.AddTrack(track4, testhelper.ApplicationOptions);
            Assert.IsTrue(track1.IsValid);
            Assert.IsTrue(track2.IsValid);
            Assert.IsTrue(track3.IsValid);
            Assert.IsTrue(track4.IsValid);
            var tracksToRemove = new List<Track>
            {
                track2,
                track4
            };
            cuesheet.RemoveTracks(tracksToRemove);
            Assert.IsTrue(manager.CanUndo);
            manager.Undo();
            Assert.AreEqual(4, cuesheet.Tracks.Count);
            Assert.IsTrue(track1.IsValid);
            Assert.IsTrue(track2.IsValid);
            Assert.IsTrue(track3.IsValid);
            Assert.IsTrue(track4.IsValid);
        }

        [TestMethod()]
        public void MoveTracksTest()
        {
            var testhelper = new TestHelper();
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var cuesheet = new Cuesheet();
            manager.TraceChanges(cuesheet);
            var track1 = new Track()
            {
                Artist = "Track 1 Artist",
                Title = "Track 1 Title",
                End = new TimeSpan(0, 2, 30)
            };
            var track2 = new Track()
            {
                Artist = "Track 2 Artist",
                Title = "Track 2 Title",
                End = new TimeSpan(0, 4, 20)
            };
            var track3 = new Track()
            {
                Artist = "Track 3 Artist",
                Title = "Track 3 Title",
                End = new TimeSpan(0, 7, 12)
            };
            var track4 = new Track()
            {
                Artist = "Track 4 Artist",
                Title = "Track 4 Title",
                End = new TimeSpan(0, 9, 12)
            };
            cuesheet.AddTrack(track1, testhelper.ApplicationOptions);
            cuesheet.AddTrack(track2, testhelper.ApplicationOptions);
            cuesheet.AddTrack(track3, testhelper.ApplicationOptions);
            cuesheet.AddTrack(track4, testhelper.ApplicationOptions);
            cuesheet.MoveTrack(track2, MoveDirection.Up);
            Assert.AreEqual(track2, cuesheet.Tracks.First());
            manager.Undo();
            Assert.AreEqual(track1, cuesheet.Tracks.First());
        }

        [TestMethod()]
        public void ResetTest()
        {
            var testhelper = new TestHelper();
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            Assert.IsFalse(manager.CanUndo);
            Assert.IsFalse(manager.CanRedo);
            var cuesheet = new Cuesheet();
            manager.TraceChanges(cuesheet);
            var track1 = new Track()
            {
                Artist = "Track 1 Artist",
                Title = "Track 1 Title",
                End = new TimeSpan(0, 2, 30)
            };
            cuesheet.AddTrack(track1, testhelper.ApplicationOptions);
            Assert.IsTrue(manager.CanUndo);
            manager.Reset();
            Assert.IsFalse(manager.CanUndo);
            Assert.IsFalse(manager.CanRedo);
        }

        [TestMethod()]
        public void BulkEditTracksTest()
        {
            var helper = new TestHelper();
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var cuesheet = new Cuesheet();
            manager.TraceChanges(cuesheet);
            var tracedObjectHistoryChangedFired = false;
            manager.TracedObjectHistoryChanged += delegate
            {
                tracedObjectHistoryChangedFired = true;
            };
            var track1 = new Track();
            var track2 = new Track();
            var track3 = new Track();
            var track4 = new Track();
            manager.TraceChanges(track1);
            manager.TraceChanges(track2);
            manager.TraceChanges(track3);
            manager.TraceChanges(track4);
            cuesheet.AddTrack(track1, helper.ApplicationOptions);
            cuesheet.AddTrack(track2, helper.ApplicationOptions);
            cuesheet.AddTrack(track3, helper.ApplicationOptions);
            cuesheet.AddTrack(track4, helper.ApplicationOptions);
            track1.IsLinkedToPreviousTrack = true;
            track2.IsLinkedToPreviousTrack = true;
            track3.IsLinkedToPreviousTrack = true;
            track4.IsLinkedToPreviousTrack = true;
            var editedTrack = new Track() { Artist = "Test", Title = "Testtitle", Length = new TimeSpan(0, 2, 0) };
            manager.BulkEdit = true;
            track1.CopyValues(editedTrack, false, setIsLinkedToPreviousTrack: false, setBegin: false, setEnd: false, setLength: true);
            track2.CopyValues(editedTrack, false, setIsLinkedToPreviousTrack: false, setBegin: false, setEnd: false, setLength: true);
            track3.CopyValues(editedTrack, false, setIsLinkedToPreviousTrack: false, setBegin: false, setEnd: false, setLength: true);
            track4.CopyValues(editedTrack, false, setIsLinkedToPreviousTrack: false, setBegin: false, setEnd: false, setLength: true);
            manager.BulkEdit = false;
            Assert.IsTrue(manager.CanUndo);
            Assert.AreEqual(editedTrack.Length, track1.End);
            Assert.AreEqual(track1.End, track2.Begin);
            Assert.AreEqual(track2.End, track3.Begin);
            Assert.AreEqual(track3.End, track4.Begin);
            Assert.IsNotNull(track4.End);
            Assert.AreEqual(editedTrack.Artist, track1.Artist);
            Assert.AreEqual(editedTrack.Artist, track2.Artist);
            Assert.AreEqual(editedTrack.Artist, track3.Artist);
            Assert.AreEqual(editedTrack.Artist, track4.Artist);
            Assert.AreEqual(editedTrack.Title, track1.Title);
            Assert.AreEqual(editedTrack.Title, track2.Title);
            Assert.AreEqual(editedTrack.Title, track3.Title);
            Assert.AreEqual(editedTrack.Title, track4.Title);
            Assert.IsTrue(tracedObjectHistoryChangedFired);
            manager.Undo();
            Assert.IsNull(track1.End);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.End);
            Assert.IsNull(track4.End);
            Assert.IsNull(track1.Artist);
            Assert.IsNull(track2.Artist);
            Assert.IsNull(track3.Artist);
            Assert.IsNull(track4.Artist);
            Assert.IsNull(track1.Title);
            Assert.IsNull(track2.Title);
            Assert.IsNull(track3.Title);
            Assert.IsNull(track4.Title);
        }

        [TestMethod()]
        public void BulkEditTracksLengthTest()
        {
            //We do an edit on 4 tracks to set the length, undo, redo and again undo.
            var helper = new TestHelper();
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var cuesheet = new Cuesheet();
            manager.TraceChanges(cuesheet);
            var track1 = new Track();
            var track2 = new Track();
            var track3 = new Track();
            var track4 = new Track();
            manager.TraceChanges(track1);
            manager.TraceChanges(track2);
            manager.TraceChanges(track3);
            manager.TraceChanges(track4);
            cuesheet.AddTrack(track1, helper.ApplicationOptions);
            cuesheet.AddTrack(track2, helper.ApplicationOptions);
            cuesheet.AddTrack(track3, helper.ApplicationOptions);
            cuesheet.AddTrack(track4, helper.ApplicationOptions);
            track1.IsLinkedToPreviousTrack = true;
            track2.IsLinkedToPreviousTrack = true;
            track3.IsLinkedToPreviousTrack = true;
            track4.IsLinkedToPreviousTrack = true;
            var editedTrack = new Track() { Length = new TimeSpan(0, 2, 0) };
            manager.BulkEdit = true;
            track1.CopyValues(editedTrack, false, setIsLinkedToPreviousTrack: false, setBegin: false, setEnd: false, setLength: true);
            track2.CopyValues(editedTrack, false, setIsLinkedToPreviousTrack: false, setBegin: false, setEnd: false, setLength: true);
            track3.CopyValues(editedTrack, false, setIsLinkedToPreviousTrack: false, setBegin: false, setEnd: false, setLength: true);
            track4.CopyValues(editedTrack, false, setIsLinkedToPreviousTrack: false, setBegin: false, setEnd: false, setLength: true);
            manager.BulkEdit = false;
            Assert.IsTrue(manager.CanUndo);
            Assert.AreEqual(editedTrack.Length, track1.End);
            Assert.AreEqual(track1.End, track2.Begin);
            Assert.AreEqual(track2.End, track3.Begin);
            Assert.AreEqual(track3.End, track4.Begin);
            Assert.IsNotNull(track4.End);
            manager.Undo();
            Assert.IsNull(track1.End);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.End);
            Assert.IsNull(track4.End);
            Assert.IsTrue(manager.CanRedo);
            manager.Redo();
            Assert.AreEqual(editedTrack.Length, track1.End);
            Assert.AreEqual(track1.End, track2.Begin);
            Assert.AreEqual(track2.End, track3.Begin);
            Assert.AreEqual(track3.End, track4.Begin);
            Assert.IsNotNull(track4.End);
            Assert.IsTrue(manager.CanUndo);
            manager.Undo();
            Assert.IsNull(track1.End);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.End);
            Assert.IsNull(track4.End);
        }
    }
}