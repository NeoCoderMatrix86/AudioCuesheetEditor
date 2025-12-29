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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Services.UI;
using AudioCuesheetEditor.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioCuesheetEditor.Tests.Services.UI
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
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var cuesheet = new Cuesheet();
            manager.TraceChanges(cuesheet);
            Assert.IsFalse(manager.CanUndo);
            cuesheet.AddTrack(new Track());
            Assert.IsTrue(manager.CanUndo);
            manager.Undo();
            Assert.IsEmpty(cuesheet.Tracks);
            Assert.IsTrue(manager.CanRedo);
            manager.Redo();
            Assert.HasCount(1, cuesheet.Tracks);
            Assert.IsFalse(manager.CanRedo);
        }

        [TestMethod()]
        public void RemoveTracksTest()
        {
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
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            cuesheet.AddTrack(track4);
            Assert.AreEqual(ValidationStatus.Success, track1.Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, track2.Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, track3.Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, track4.Validate().Status);
            var tracksToRemove = new List<Track>
            {
                track2,
                track4
            };
            cuesheet.RemoveTracks(tracksToRemove);
            Assert.IsTrue(manager.CanUndo);
            manager.Undo();
            Assert.HasCount(4, cuesheet.Tracks);
            //We need to set object references back to cue sheet tracks since TraceChangeManager creates new objects
            track1 = cuesheet.Tracks.ElementAt(0);
            track2 = cuesheet.Tracks.ElementAt(1);
            track3 = cuesheet.Tracks.ElementAt(2);
            track4 = cuesheet.Tracks.ElementAt(3);
            Assert.AreEqual(ValidationStatus.Success, track1.Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, track2.Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, track3.Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, track4.Validate().Status);
        }

        [TestMethod()]
        public void ResetTest()
        {
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
            cuesheet.AddTrack(track1);
            Assert.IsTrue(manager.CanUndo);
            manager.Reset();
            Assert.IsFalse(manager.CanUndo);
            Assert.IsFalse(manager.CanRedo);
        }

        [TestMethod()]
        public void BulkEditTracksTest()
        {
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
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            cuesheet.AddTrack(track4);
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
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            cuesheet.AddTrack(track4);
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
        
        [TestMethod()]
        public void RemoveTracedChanges_RemovesChanges_WhenChangesAvailable()
        {
            // Arrange
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var cuesheet1 = new Cuesheet();
            var cuesheet2 = new Cuesheet();
            manager.TraceChanges(cuesheet1);
            manager.TraceChanges(cuesheet2);
            cuesheet1.Artist = "Test Artist Cuesheet 1";
            cuesheet1.Title = "Test Title Cuesheet 1";
            cuesheet2.CDTextfile = new("CD Testfile.cdt");
            cuesheet2.AddSection();
            var tracedObjectHistoryChangedFired = false;
            manager.TracedObjectHistoryChanged += delegate
            {
                tracedObjectHistoryChangedFired = true;
            };
            // Act
            manager.RemoveTracedChanges(cuesheet1);
            // Assert
            Assert.IsTrue(tracedObjectHistoryChangedFired);
            Assert.IsTrue(manager.CanUndo);
            manager.Undo();
            manager.Undo();
            Assert.IsFalse(manager.CanUndo);
        }

        [TestMethod()]
        public void RemoveTracedChanges_RemovesNoChanges_WhenNoChangesAvailable()
        {
            // Arrange
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var cuesheet1 = new Cuesheet();
            var cuesheet2 = new Cuesheet();
            manager.TraceChanges(cuesheet1);
            manager.TraceChanges(cuesheet2);
            cuesheet2.CDTextfile = new("CD Testfile.cdt");
            cuesheet2.AddSection();
            var tracedObjectHistoryChangedFired = false;
            manager.TracedObjectHistoryChanged += delegate
            {
                tracedObjectHistoryChangedFired = true;
            };
            // Act
            manager.RemoveTracedChanges(cuesheet1);
            // Assert
            Assert.IsTrue(tracedObjectHistoryChangedFired);
            Assert.IsTrue(manager.CanUndo);
            manager.Undo();
            manager.Undo();
            Assert.IsFalse(manager.CanUndo);
        }
    }
}