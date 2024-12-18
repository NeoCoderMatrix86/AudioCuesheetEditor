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
using AudioCuesheetEditor.Data.Options;
using AudioCuesheetEditor.Extensions;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.UI;
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Tests.Properties;
using AudioCuesheetEditor.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Model.UI
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
        public async Task Import_ValidTextfile_IsUndoable()
        {
            // Arrange
            var testhelper = new TestHelper();
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_with_Cuesheetdata);
            using var reader = new StreamReader(textImportMemoryStream);
            List<string?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var importOptions = new ImportOptions();
            importOptions.TextImportScheme.SchemeCuesheet = "(?'Artist'\\A.*) - (?'Title'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Cataloguenumber'.{1,})";
            importOptions.TextImportScheme.SchemeTracks = TextImportScheme.DefaultSchemeTracks;
            localStorageOptionsProviderMock.Setup(x => x.GetOptions<ImportOptions>()).ReturnsAsync(importOptions);
            var textImportService = new TextImportService();
            var importManager = new ImportManager(sessionStateContainer, localStorageOptionsProviderMock.Object, textImportService, traceChangeManager);
            bool eventFired = false;
            sessionStateContainer.Cuesheet.TracksAdded += delegate
            {
                eventFired = true;
            };
            // Act
            await importManager.ImportTextAsync(fileContent);
            // Assert
            Assert.IsFalse(traceChangeManager.CanUndo);
            Assert.IsFalse(traceChangeManager.CanRedo);
            Assert.IsNotNull(sessionStateContainer.ImportCuesheet);
            Assert.AreEqual("DJFreezeT", sessionStateContainer.ImportCuesheet.Artist);
            Assert.AreEqual("0123456789123", sessionStateContainer.ImportCuesheet.Cataloguenumber.Value);
            Assert.AreNotEqual(0, sessionStateContainer.ImportCuesheet.Tracks.Count);
            Assert.IsFalse(eventFired);
        }

        [TestMethod()]
        public async Task UndoImport_ValidTextfile_ResetsToEmptyCuesheet()
        {
            // Arrange
            var testhelper = new TestHelper();
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_with_Cuesheetdata);
            using var reader = new StreamReader(textImportMemoryStream);
            List<string?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var importOptions = new ImportOptions();
            importOptions.TextImportScheme.SchemeCuesheet = "(?'Artist'\\A.*) - (?'Title'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Cataloguenumber'.{1,})";
            importOptions.TextImportScheme.SchemeTracks = TextImportScheme.DefaultSchemeTracks;
            localStorageOptionsProviderMock.Setup(x => x.GetOptions<ImportOptions>()).ReturnsAsync(importOptions);
            var textImportService = new TextImportService();
            var importManager = new ImportManager(sessionStateContainer, localStorageOptionsProviderMock.Object, textImportService, traceChangeManager);
            bool eventFired = false;
            sessionStateContainer.Cuesheet.TracksAdded += delegate
            {
                eventFired = true;
            };
            await importManager.ImportTextAsync(fileContent);
            await importManager.ImportCuesheetAsync();
            // Act
            traceChangeManager.Undo();
            // Assert
            Assert.AreEqual(0, sessionStateContainer.Cuesheet.Tracks.Count);
            Assert.IsTrue(string.IsNullOrEmpty(sessionStateContainer.Cuesheet.Artist));
            Assert.IsTrue(string.IsNullOrEmpty(sessionStateContainer.Cuesheet.Cataloguenumber.Value));
            Assert.IsFalse(traceChangeManager.CanUndo);
            Assert.IsTrue(traceChangeManager.CanRedo);
            Assert.IsFalse(eventFired);
        }

        [TestMethod()]
        public async Task UndoAndRedoImport_ValidTextfile_ResetsTextfileValues()
        {
            // Arrange
            var testhelper = new TestHelper();
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_with_Cuesheetdata);
            using var reader = new StreamReader(textImportMemoryStream);
            List<string?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var importOptions = new ImportOptions();
            importOptions.TextImportScheme.SchemeCuesheet = "(?'Artist'\\A.*) - (?'Title'[a-zA-Z0-9_ .();äöü&:,]{1,}) - (?'Cataloguenumber'.{1,})";
            importOptions.TextImportScheme.SchemeTracks = TextImportScheme.DefaultSchemeTracks;
            localStorageOptionsProviderMock.Setup(x => x.GetOptions<ImportOptions>()).ReturnsAsync(importOptions);
            var textImportService = new TextImportService();
            var importManager = new ImportManager(sessionStateContainer, localStorageOptionsProviderMock.Object, textImportService, traceChangeManager);
            bool eventFired = false;
            sessionStateContainer.Cuesheet.TracksAdded += delegate
            {
                eventFired = true;
            };
            await importManager.ImportTextAsync(fileContent);
            traceChangeManager.Undo();
            // Act
            traceChangeManager.Redo();
            // Assert
            Assert.AreEqual("DJFreezeT", sessionStateContainer.ImportCuesheet?.Artist);
            Assert.AreEqual("0123456789123", sessionStateContainer.ImportCuesheet?.Cataloguenumber.Value);
            Assert.AreEqual(39, sessionStateContainer.ImportCuesheet?.Tracks.Count);
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
            Assert.AreEqual(4, cuesheet.Tracks.Count);
            //We need to set object references back to cuesheet tracks since TraceChangeManager creates new objects
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