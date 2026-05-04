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
using AudioCuesheetEditor.Services.UI;
using AudioCuesheetEditor.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AudioCuesheetEditor.Tests.Services.UI
{
    [TestClass()]
    public class TraceChangeManagerTests
    {
        private readonly TraceChangeManager _traceChangeManager = new(TestHelper.CreateLogger<TraceChangeManager>());

        [TestMethod()]
        public void AddChange_SingleChange_EnablesUndo()
        {
            // Arrange
            var cuesheet = new Cuesheet
            {
                Artist = "Test Artist 1"
            };
            var tracedObjectHistoryChangedFired = false;
            _traceChangeManager.TracedObjectHistoryChanged += delegate
            {
                tracedObjectHistoryChangedFired = true;
            };
            // Act
            _traceChangeManager.AddChange(new(cuesheet, new(null, nameof(Cuesheet.Artist))));
            // Assert
            Assert.IsTrue(_traceChangeManager.CanUndo);
            Assert.IsFalse(_traceChangeManager.CanRedo);
            Assert.IsTrue(tracedObjectHistoryChangedFired);
        }

        [TestMethod()]
        public void AddChange_BulkChange_EnablesUndo()
        {
            // Arrange
            var cuesheet = new Cuesheet
            {
                Artist = "Test Artist 1",
                Title = "Test Title 1",
                Cataloguenumber = "1234567890"
            };
            var tracedObjectHistoryChangedFired = false;
            _traceChangeManager.TracedObjectHistoryChanged += delegate
            {
                tracedObjectHistoryChangedFired = true;
            };
            _traceChangeManager.BulkEdit = true;
            _traceChangeManager.AddChange(new(cuesheet, new(null, nameof(Cuesheet.Artist))));
            _traceChangeManager.AddChange(new(cuesheet, new(null, nameof(Cuesheet.Title))));
            _traceChangeManager.AddChange(new(cuesheet, new(null, nameof(Cuesheet.Cataloguenumber))));
            // Act
            _traceChangeManager.BulkEdit = false;
            // Assert
            Assert.IsTrue(_traceChangeManager.CanUndo);
            Assert.IsFalse(_traceChangeManager.CanRedo);
            Assert.IsTrue(tracedObjectHistoryChangedFired);
        }

        [TestMethod()]
        public void BulkEdit_WithoutChange_DoesntAddChange()
        {
            // Arrange
            _traceChangeManager.BulkEdit = true;
            // Act
            _traceChangeManager.BulkEdit = false;
            // Assert
            Assert.IsFalse(_traceChangeManager.CanUndo);
        }

        [TestMethod()]
        public void BulkEdit_AlreadyEnabledBulkEdit_DoesntDropPreviousEdits()
        {
            // Arrange
            var cuesheet = new Cuesheet
            {
                Artist = "Test Artist 1",
                Title = "Test Title 1",
                Cataloguenumber = "1234567890"
            };
            _traceChangeManager.BulkEdit = true;
            _traceChangeManager.AddChange(new(cuesheet, new(null, nameof(Cuesheet.Artist))));
            _traceChangeManager.AddChange(new(cuesheet, new(null, nameof(Cuesheet.Title))));
            _traceChangeManager.AddChange(new(cuesheet, new(null, nameof(Cuesheet.Cataloguenumber))));
            _traceChangeManager.BulkEdit = true;
            _traceChangeManager.BulkEdit = false;
            // Act
            _traceChangeManager.Undo();
            // Assert
            Assert.IsNull(cuesheet.Artist);
            Assert.IsNull(cuesheet.Title);
            Assert.IsNull(cuesheet.Cataloguenumber);
            Assert.IsTrue(_traceChangeManager.CanRedo);
        }

        [TestMethod()]
        public void Undo_SingleChange_UndoesChange()
        {
            // Arrange
            var track = new Track
            {
                Position = 3
            };
            var undoDoneEventFired = false;
            _traceChangeManager.UndoDone += delegate
            {
                undoDoneEventFired = true;
            };
            _traceChangeManager.AddChange(new(track, new(null, nameof(Track.Position))));
            // Act
            _traceChangeManager.Undo();
            // Assert
            Assert.IsNull(track.Position);
            Assert.IsTrue(undoDoneEventFired);
            Assert.IsFalse(_traceChangeManager.CanUndo);
            Assert.IsTrue(_traceChangeManager.CanRedo);
        }

        [TestMethod()]
        public void Undo_BulkChange_UndoesChanges()
        {
            // Arrange
            var track = new Track
            {
                Position = 3,
                Artist = "Artist"
            };
            var undoDoneEventFired = false;
            _traceChangeManager.UndoDone += delegate
            {
                undoDoneEventFired = true;
            };
            _traceChangeManager.BulkEdit = true;
            _traceChangeManager.AddChange(new(track, new(null, nameof(Track.Position))));
            _traceChangeManager.AddChange(new(track, new(null, nameof(Track.Artist))));
            _traceChangeManager.BulkEdit = false;
            // Act
            _traceChangeManager.Undo();
            // Assert
            Assert.IsNull(track.Position);
            Assert.IsNull(track.Artist);
            Assert.IsTrue(undoDoneEventFired);
            Assert.IsFalse(_traceChangeManager.CanUndo);
            Assert.IsTrue(_traceChangeManager.CanRedo);
        }

        [TestMethod()]
        public void Redo_SingleChange_UndoesChange()
        {
            // Arrange
            var track = new Track
            {
                Position = 3
            };
            var redoDoneEventFired = false;
            _traceChangeManager.RedoDone += delegate
            {
                redoDoneEventFired = true;
            };
            _traceChangeManager.AddChange(new(track, new(null, nameof(Track.Position))));
            _traceChangeManager.Undo();
            // Act
            _traceChangeManager.Redo();
            // Assert
            Assert.AreEqual((ushort)3, track.Position);
            Assert.IsTrue(redoDoneEventFired);
            Assert.IsTrue(_traceChangeManager.CanUndo);
            Assert.IsFalse(_traceChangeManager.CanRedo);
        }

        [TestMethod()]
        public void Redo_BulkChange_UndoesChanges()
        {
            // Arrange
            var track = new Track
            {
                Position = 3,
                Artist = "Artist"
            };
            var redoDoneEventFired = false;
            _traceChangeManager.RedoDone += delegate
            {
                redoDoneEventFired = true;
            };
            _traceChangeManager.BulkEdit = true;
            _traceChangeManager.AddChange(new(track, new(null, nameof(Track.Position))));
            _traceChangeManager.AddChange(new(track, new(null, nameof(Track.Artist))));
            _traceChangeManager.BulkEdit = false;
            _traceChangeManager.Undo();
            // Act
            _traceChangeManager.Redo();
            // Assert
            Assert.AreEqual((ushort)3, track.Position);
            Assert.AreEqual("Artist", track.Artist);
            Assert.IsTrue(redoDoneEventFired);
            Assert.IsTrue(_traceChangeManager.CanUndo);
            Assert.IsFalse(_traceChangeManager.CanRedo);
        }

        [TestMethod]
        public void Undo_ObjectReferences_UndoesChanges()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Tracks = [new()]
            };
            var undoDoneEventFired = false;
            _traceChangeManager.UndoDone += delegate
            {
                undoDoneEventFired = true;
            };
            _traceChangeManager.AddChange(new(cuesheet, new(Enumerable.Empty<Track>(), nameof(Cuesheet.Tracks))));
            // Act
            _traceChangeManager.Undo();
            // Assert
            Assert.IsEmpty(cuesheet.Tracks);
            Assert.IsFalse(_traceChangeManager.CanUndo);
            Assert.IsTrue(_traceChangeManager.CanRedo);
            Assert.IsTrue(undoDoneEventFired);
        }

        [TestMethod]
        public void Redo_ObjectReferences_UndoesChanges()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Tracks = [new()]
            };
            var redoDoneEventFired = false;
            _traceChangeManager.RedoDone += delegate
            {
                redoDoneEventFired = true;
            };
            _traceChangeManager.AddChange(new(cuesheet, new(Enumerable.Empty<Track>(), nameof(Cuesheet.Tracks))));
            _traceChangeManager.Undo();
            // Act
            _traceChangeManager.Redo();
            // Assert
            Assert.HasCount(1, cuesheet.Tracks);
            Assert.IsTrue(_traceChangeManager.CanUndo);
            Assert.IsFalse(_traceChangeManager.CanRedo);
            Assert.IsTrue(redoDoneEventFired);
        }

        [TestMethod]
        public void Reset_HasChanges_ResetsAllChanges()
        {
            // Arrange
            var track = new Track()
            {
                Artist = "Track 1 Artist",
                Title = "Track 1 Title",
                End = new TimeSpan(0, 2, 30)
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track]
            };
            _traceChangeManager.AddChange(new(cuesheet, new(Enumerable.Empty<Track>(), nameof(Cuesheet.Tracks))));
            _traceChangeManager.AddChange(new(track, new(null, nameof(Track.End))));
            _traceChangeManager.AddChange(new(track, new(null, nameof(Track.Title))));
            _traceChangeManager.AddChange(new(track, new(null, nameof(Track.Artist))));
            _traceChangeManager.Undo();
            // Act
            _traceChangeManager.Reset();
            // Assert
            Assert.IsFalse(_traceChangeManager.CanUndo);
            Assert.IsFalse(_traceChangeManager.CanRedo);
        }

        [TestMethod()]
        public void RemoveTracedChanges_RemovesChanges_WhenChangesAvailable()
        {
            // Arrange
            var cuesheet1 = new Cuesheet()
            {
                Artist = "Test Artist Cuesheet 1",
                Title = "Test Title Cuesheet 1"
            };
            var cuesheet2 = new Cuesheet()
            {
                CDTextfile = new("CD Testfile.cdt"),
                Title = "Test Title Cuesheet 2"
            };
            _traceChangeManager.AddChange(new(cuesheet1, new(null, nameof(Cuesheet.Artist))));
            _traceChangeManager.AddChange(new(cuesheet1, new(null, nameof(Cuesheet.Title))));
            _traceChangeManager.AddChange(new(cuesheet2, new(null, nameof(Cuesheet.CDTextfile))));
            _traceChangeManager.AddChange(new(cuesheet2, new(null, nameof(Cuesheet.Title))));
            var tracedObjectHistoryChangedFired = false;
            _traceChangeManager.TracedObjectHistoryChanged += delegate
            {
                tracedObjectHistoryChangedFired = true;
            };
            // Act
            _traceChangeManager.RemoveTracedChanges([cuesheet1]);
            // Assert
            Assert.IsTrue(tracedObjectHistoryChangedFired);
            Assert.IsTrue(_traceChangeManager.CanUndo);
            _traceChangeManager.Undo();
            _traceChangeManager.Undo();
            Assert.IsFalse(_traceChangeManager.CanUndo);
            Assert.IsNull(cuesheet2.CDTextfile);
            Assert.IsNull(cuesheet2.Title);
        }

        [TestMethod()]
        public void RemoveTracedChanges_RemovesNoChanges_WhenNoChangesAvailable()
        {
            // Arrange
            var cuesheet1 = new Cuesheet();
            var cuesheet2 = new Cuesheet()
            {
                CDTextfile = new("CD Testfile.cdt"),
                Title = "Test Title Cuesheet 2"
            };
            _traceChangeManager.AddChange(new(cuesheet2, new(null, nameof(Cuesheet.CDTextfile))));
            _traceChangeManager.AddChange(new(cuesheet2, new(null, nameof(Cuesheet.Title))));
            var tracedObjectHistoryChangedFired = false;
            _traceChangeManager.TracedObjectHistoryChanged += delegate
            {
                tracedObjectHistoryChangedFired = true;
            };
            // Act
            _traceChangeManager.RemoveTracedChanges([cuesheet1]);
            // Assert
            Assert.IsTrue(tracedObjectHistoryChangedFired);
            Assert.IsTrue(_traceChangeManager.CanUndo);
            _traceChangeManager.Undo();
            _traceChangeManager.Undo();
            Assert.IsFalse(_traceChangeManager.CanUndo);
            Assert.IsNull(cuesheet2.CDTextfile);
            Assert.IsNull(cuesheet2.Title);
        }
    }
}