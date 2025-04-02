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
using AudioCuesheetEditor.Data.Options;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.Utility;
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using AudioCuesheetEditor.Tests.Properties;
using AudioCuesheetEditor.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AudioCuesheetEditor.Tests.Model.AudioCuesheet
{
    [TestClass()]
    public class CuesheetTests
    {
        [TestMethod()]
        public void AddTrackTest()
        {
            var cuesheet = new Cuesheet();
            Assert.AreEqual(0, cuesheet.Tracks.Count);
            cuesheet.AddTrack(new Track());
            Assert.AreEqual(1, cuesheet.Tracks.Count);
        }

        [TestMethod()]
        public void CuesheetTest()
        {
            var cuesheet = new Cuesheet();
            Assert.IsNull(cuesheet.Audiofile);
            var validationErrorAudioFile = cuesheet.Validate(nameof(Cuesheet.Audiofile));
            Assert.AreEqual(ValidationStatus.Error, validationErrorAudioFile.Status);
            cuesheet.Audiofile = new Audiofile("AudioFile01.ogg");
            validationErrorAudioFile = cuesheet.Validate(nameof(Cuesheet.Audiofile));
            Assert.AreEqual(ValidationStatus.Success, validationErrorAudioFile.Status);
        }

        [TestMethod()]
        public void EmptyCuesheetTracksValidationTest()
        {
            var cuesheet = new Cuesheet();
            Assert.AreEqual(0, cuesheet.Tracks.Count);
            var validationErrorTracks = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Error, validationErrorTracks.Status);
            cuesheet.AddTrack(new Track());
            validationErrorTracks = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Success, validationErrorTracks.Status);
        }

        [TestMethod()]
        public void ImportTest()
        {
            // Arrange
            var fileContent = new List<string>
            {
                "CuesheetArtist - CuesheetTitle				c:\\tmp\\Testfile.mp3",
                "Sample Artist 1 - Sample Title 1				00:05:00",
                "Sample Artist 2 - Sample Title 2				00:09:23",
                "Sample Artist 3 - Sample Title 3				00:15:54",
                "Sample Artist 4 - Sample Title 4				00:20:13",
                "Sample Artist 5 - Sample Title 5				00:24:54",
                "Sample Artist 6 - Sample Title 6				00:31:54",
                "Sample Artist 7 - Sample Title 7				00:45:54",
                "Sample Artist 8 - Sample Title 8				01:15:54"
            };

            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet,
                SchemeTracks = TextImportScheme.DefaultSchemeTracks
            };
            var timeSpanFormat = new TimeSpanFormat();
            var options = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptions<ApplicationOptions>()).ReturnsAsync(options);
            var importManager = new ImportManager(sessionStateContainer, localStorageOptionsProviderMock.Object, traceChangeManager);
            var testHelper = new TestHelper();
            // Act
            importManager.ImportText(fileContent, textImportScheme, timeSpanFormat);

            // Assert
            Assert.IsNull(sessionStateContainer.ImportCuesheet?.CDTextfile);
            Assert.AreEqual(ValidationStatus.Success, sessionStateContainer.ImportCuesheet?.Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, sessionStateContainer.ImportCuesheet?.Tracks.ElementAt(0).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, sessionStateContainer.ImportCuesheet?.Tracks.ElementAt(1).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, sessionStateContainer.ImportCuesheet?.Tracks.ElementAt(2).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, sessionStateContainer.ImportCuesheet?.Tracks.ElementAt(3).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, sessionStateContainer.ImportCuesheet?.Tracks.ElementAt(4).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, sessionStateContainer.ImportCuesheet?.Tracks.ElementAt(5).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, sessionStateContainer.ImportCuesheet?.Tracks.ElementAt(6).Validate().Status);
            Assert.AreEqual(ValidationStatus.Success, sessionStateContainer.ImportCuesheet?.Tracks.ElementAt(7).Validate().Status);
        }

        [TestMethod()]
        public void ImportTestCalculateEndCorrectly()
        {
            // Arrange
            var textImportMemoryStream = new MemoryStream(Resources.Textimport_Bug_54);
            using var reader = new StreamReader(textImportMemoryStream);
            List<string?> lines = [];
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            var fileContent = lines.AsReadOnly();
            var testHelper = new TestHelper();
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var options = new ApplicationOptions();
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = null,
                SchemeTracks = TextImportScheme.DefaultSchemeTracks
            };
            options.ImportScheme = textImportScheme;
            localStorageOptionsProviderMock.Setup(x => x.GetOptions<ApplicationOptions>()).ReturnsAsync(options);
            var importManager = new ImportManager(sessionStateContainer, localStorageOptionsProviderMock.Object, traceChangeManager);
            var timeSpanFormat = new TimeSpanFormat();
            // Act
            importManager.ImportText(fileContent, textImportScheme, timeSpanFormat);
            // Assert
            Assert.IsNull(sessionStateContainer.Importfile?.AnalyseException);
            Assert.IsNotNull(sessionStateContainer.ImportCuesheet);
            Assert.AreEqual(39, sessionStateContainer.ImportCuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(0, 5, 24), sessionStateContainer.ImportCuesheet.Tracks.ElementAt(0).End);
            Assert.AreEqual(new TimeSpan(3, 13, 13), sessionStateContainer.ImportCuesheet.Tracks.ElementAt(38).Begin);
        }

        [TestMethod()]
        public void RecordTest()
        {
            var cuesheet = new Cuesheet();
            Assert.IsFalse(cuesheet.IsRecording);
            Assert.IsNull(cuesheet.RecordingTime);
            cuesheet.StartRecording();
            Assert.IsTrue(cuesheet.IsRecording);
            Assert.IsNotNull(cuesheet.RecordingTime);
            var track = new Track();
            Assert.IsNull(track.Begin);
            Assert.IsNull(track.End);
            cuesheet.AddTrack(track);
            Assert.AreEqual(TimeSpan.Zero, track.Begin);
            Assert.IsNull(track.End);
            var track2 = new Track();
            cuesheet.AddTrack(track2);
            Assert.IsNotNull(track.End);
            Assert.AreNotEqual(TimeSpan.Zero, track.End);
            cuesheet = new Cuesheet();
            cuesheet.StartRecording();
            track = new Track();
            cuesheet.AddTrack(track);
            Assert.AreEqual(TimeSpan.Zero, track.Begin);
            Assert.IsNull(track.End);
            Thread.Sleep(3000);
            track2 = new Track();
            cuesheet.AddTrack(track2);
            Assert.IsNotNull(track.End);
            Assert.AreNotEqual(TimeSpan.Zero, track.End);
            cuesheet.StopRecording();
            Assert.IsNotNull(track.End);
            Assert.AreEqual(track.End, track2.Begin);
        }

        [TestMethod()]
        public void TrackRecalculationTest()
        {
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            var track3 = new Track();
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            Assert.AreEqual((uint)1, track1.Position);
            Assert.AreEqual((uint)2, track2.Position);
            Assert.AreEqual((uint)3, track3.Position);
            Assert.AreEqual(track1.Begin, TimeSpan.Zero);
            Assert.IsNull(track1.End);
            Assert.IsNull(track2.Begin);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.Begin);
            Assert.IsNull(track3.End);
            track1.Begin = TimeSpan.Zero;
            track2.Begin = new TimeSpan(0, 2, 43);
            Assert.IsNull(track1.End);
            Assert.IsNull(track2.End);
            track3.End = new TimeSpan(0, 12, 14);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.Begin);
            track3.Begin = new TimeSpan(0, 7, 56);
            Assert.IsNull(track2.End);
        }
        [TestMethod()]
        public void TrackOverlappingTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            var track3 = new Track();
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            Assert.AreEqual((uint)1, track1.Position);
            Assert.AreEqual((uint)2, track2.Position);
            Assert.AreEqual((uint)3, track3.Position);
            var validationResult = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
            track1.Position = 1;
            track2.Position = 1;
            track3.Position = 1;
            track1.End = new TimeSpan(0, 2, 30);
            track2.Begin = new TimeSpan(0, 2, 0);
            track2.End = new TimeSpan(0, 5, 30);
            track3.Begin = new TimeSpan(0, 4, 54);
            track3.End = new TimeSpan(0, 8, 12);
            validationResult = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual(7, validationResult.ValidationMessages?.Count);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0} {1} '{2}' is used also by {3}({4},{5},{6},{7},{8}). Positions must be unique!" && x.Parameter != null && x.Parameter[2].Equals(track1.Position) && x.Parameter[7].Equals(track1.Begin) && x.Parameter[8].Equals(track1.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0} {1} '{2}' is used also by {3}({4},{5},{6},{7},{8}). Positions must be unique!" && x.Parameter != null && x.Parameter[2].Equals(track2.Position) && x.Parameter[7].Equals(track2.Begin) && x.Parameter[8].Equals(track2.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0} {1} '{2}' is used also by {3}({4},{5},{6},{7},{8}). Positions must be unique!" && x.Parameter != null && x.Parameter[2].Equals(track3.Position) && x.Parameter[7].Equals(track3.Begin) && x.Parameter[8].Equals(track3.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track1.Position) && x.Parameter[4].Equals(track1.Begin) && x.Parameter[5].Equals(track1.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track1.Position) && x.Parameter[9].Equals(track1.Begin) && x.Parameter[10].Equals(track1.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track3.Position) && x.Parameter[9].Equals(track3.Begin) && x.Parameter[10].Equals(track3.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track3.Position) && x.Parameter[4].Equals(track3.Begin) && x.Parameter[5].Equals(track3.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            track2.End = new TimeSpan(0, 5, 15);
            validationResult = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track3.Position) && x.Parameter[9].Equals(track3.Begin) && x.Parameter[10].Equals(track3.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track3.Position) && x.Parameter[4].Equals(track3.Begin) && x.Parameter[5].Equals(track3.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            track1.Position = 1;
            track2.Position = 2;
            track3.Position = 3;
            validationResult = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual(4, validationResult.ValidationMessages?.Count);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track1.Position) && x.Parameter[4].Equals(track1.Begin) && x.Parameter[5].Equals(track1.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track1.Position) && x.Parameter[9].Equals(track1.Begin) && x.Parameter[10].Equals(track1.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track2.Position) && x.Parameter[4].Equals(track2.Begin) && x.Parameter[5].Equals(track2.End) && x.Parameter[6].Equals(track3.Position) && x.Parameter[9].Equals(track3.Begin) && x.Parameter[10].Equals(track3.End)));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Message == "{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!" && x.Parameter != null && x.Parameter[1].Equals(track3.Position) && x.Parameter[4].Equals(track3.Begin) && x.Parameter[5].Equals(track3.End) && x.Parameter[6].Equals(track2.Position) && x.Parameter[9].Equals(track2.Begin) && x.Parameter[10].Equals(track2.End)));
            track2.Begin = new TimeSpan(0, 2, 30);
            track3.Begin = new TimeSpan(0, 5, 15);
            validationResult = cuesheet.Validate(nameof(Cuesheet.Tracks));
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
        }

        [TestMethod()]
        public void RemoveTrack_ShouldReCalculate_WithPreviouslyAddedTracks()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track() { Artist = "1", Title = "1", IsLinkedToPreviousTrack = true };
            var track2 = new Track() { Artist = "2", Title = "2", IsLinkedToPreviousTrack = true };
            var track3 = new Track() { Artist = "3", Title = "3" , IsLinkedToPreviousTrack = true };
            var track4 = new Track() { Artist = "4", Title = "4" , IsLinkedToPreviousTrack = true };
            var track5 = new Track() { Artist = "5", Title = "5" , IsLinkedToPreviousTrack = true };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            cuesheet.AddTrack(track4);
            cuesheet.AddTrack(track5);
            track1.End = new TimeSpan(0, 5, 0);
            track2.End = new TimeSpan(0, 10, 0);
            track3.End = new TimeSpan(0, 15, 0);
            track4.End = new TimeSpan(0, 20, 0);
            track5.End = new TimeSpan(0, 25, 0);
            // Act
            cuesheet.RemoveTrack(track2);
            // Assert
            Assert.AreEqual((uint)2, track3.Position);
            Assert.AreEqual((uint)3, track4.Position);
            Assert.AreEqual((uint)4, track5.Position);
        }

        [TestMethod()]
        public void RemoveTracks_ShouldRemoveTracks_WithPreviouslyAddedTracks()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track
            {
                Artist = "Track 1",
                Title = "Track 1",
                IsLinkedToPreviousTrack = true
            };
            cuesheet.AddTrack(track1);
            var track2 = new Track
            {
                Title = "Track 2",
                Artist = "Track 2",
                Begin = new TimeSpan(0, 5, 0),
                IsLinkedToPreviousTrack = true
            };
            cuesheet.AddTrack(track2);
            var track3 = new Track
            {
                Artist = "Track 3",
                Title = "Track 3",
                Begin = new TimeSpan(0, 10, 0),
                IsLinkedToPreviousTrack = true
            };
            cuesheet.AddTrack(track3);
            var track4 = new Track
            {
                Artist = "Track 4",
                Title = "Track 4",
                Begin = new TimeSpan(0, 15, 0),
                IsLinkedToPreviousTrack = true
            };
            cuesheet.AddTrack(track4);
            var track5 = new Track
            {
                Artist = "Track 5",
                Title = "Track 5",
                Begin = new TimeSpan(0, 20, 0), IsLinkedToPreviousTrack = true
            };
            cuesheet.AddTrack(track5);
            var list = new List<Track>() { track2, track4 };
            // Act
            cuesheet.RemoveTracks(list.AsReadOnly());
            // Assert
            Assert.AreEqual(3, cuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(0, 5, 0), track3.Begin);
            Assert.AreEqual(new TimeSpan(0, 15, 0), track5.Begin);
            Assert.AreEqual((uint)1, track1.Position);
            Assert.AreEqual((uint)2, track3.Position);
            Assert.AreEqual((uint)3, track5.Position);
        }

        [TestMethod()]
        public void UnlickedTrackTest()
        {
            var cuesheet = new Cuesheet();
            var track1 = new Track
            {
                Position = 1,
                End = new TimeSpan(0, 3, 23)
            };
            var track2 = new Track
            {
                Position = 2
            };
            var track3 = new Track
            {
                Position = 3
            };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track3);
            Assert.AreEqual(track1.End, track2.Begin);
            Assert.IsNull(track2.End);
            Assert.IsNull(track3.Begin);
            track3.Begin = new TimeSpan(0, 7, 32);
            Assert.IsNull(track2.End);
        }

        [TestMethod()]
        public void TrackPositionChangedTest()
        {
            var cuesheet = new Cuesheet();
            var track1 = new Track
            {
                Position = 3,
                End = new TimeSpan(0, 5, 0)
            };
            var track2 = new Track
            {
                Position = 2,
                Begin = track1.End,
                End = new TimeSpan(0, 7, 30)
            };
            cuesheet.AddTrack(track2);
            cuesheet.AddTrack(track1);
            Assert.AreEqual(track2, cuesheet.Tracks.First());
            Assert.AreEqual(track1, cuesheet.Tracks.Last());
            track1.Position = 1;
            Assert.AreEqual(track2, cuesheet.Tracks.First());
            Assert.AreEqual(track1, cuesheet.Tracks.Last());
        }

        [TestMethod()]
        public void TrackLengthChangedWithIsLinkedToPreivousTest()
        {
            var cuesheet = new Cuesheet();
            var track1 = new Track();
            var track2 = new Track();
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            track1.IsLinkedToPreviousTrack = true;
            track2.IsLinkedToPreviousTrack = true;
            Assert.IsNull(track2.Begin);
            Assert.IsNotNull(track1.Begin);
            var editedTrack = new Track() { Begin = TimeSpan.Zero, Length = new TimeSpan(0, 2, 23) };
            track1.CopyValues(editedTrack);
            Assert.IsNotNull(track1.End);
            Assert.AreEqual(track1.End, track2.Begin);
            Assert.AreEqual(editedTrack.End, track2.Begin);
        }
        [TestMethod()]
        public void ImportSamplesTest()
        {
            // Arrange
            var fileContent = File.ReadAllLines("../../../../AudioCuesheetEditor/wwwroot/samples/Sample_Inputfile.txt");
            var testHelper = new TestHelper();
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = TextImportScheme.DefaultSchemeCuesheet,
                SchemeTracks = TextImportScheme.DefaultSchemeTracks
            };
            var timeSpanFormat = new TimeSpanFormat();
            var options = new ApplicationOptions();
            localStorageOptionsProviderMock.Setup(x => x.GetOptions<ApplicationOptions>()).ReturnsAsync(options);
            var importManager = new ImportManager(sessionStateContainer, localStorageOptionsProviderMock.Object, traceChangeManager);
            // Act
            importManager.ImportText(fileContent, textImportScheme, timeSpanFormat);
            // Assert
            Assert.IsNull(sessionStateContainer.Importfile?.AnalyseException);
            Assert.IsNotNull(sessionStateContainer.ImportCuesheet);
            Assert.AreEqual("CuesheetArtist", sessionStateContainer.ImportCuesheet.Artist);
            Assert.AreEqual("CuesheetTitle", sessionStateContainer.ImportCuesheet.Title);
            Assert.AreEqual(8, sessionStateContainer.ImportCuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(1, 15, 54), sessionStateContainer.ImportCuesheet.Tracks.Last().End);
        }

        [TestMethod()]
        public void ImportSamples2Test()
        {
            // Arrange
            var fileContent = File.ReadAllLines("../../../../AudioCuesheetEditor/wwwroot/samples/Sample_Inputfile2.txt");
            var testHelper = new TestHelper();
            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var textImportScheme = new TextImportScheme()
            {
                SchemeCuesheet = null,
                SchemeTracks = TextImportScheme.DefaultSchemeTracks
            };
            var timeSpanFormat = new TimeSpanFormat();
            var options = new ApplicationOptions
            {
                ImportScheme = textImportScheme
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptions<ApplicationOptions>()).ReturnsAsync(options);
            var importManager = new ImportManager(sessionStateContainer, localStorageOptionsProviderMock.Object, traceChangeManager);
            // Act
            importManager.ImportText(fileContent, textImportScheme, timeSpanFormat);
            // Assert
            Assert.IsNull(sessionStateContainer.Importfile?.AnalyseException);
            Assert.IsNotNull(sessionStateContainer.ImportCuesheet);
            Assert.IsNull(sessionStateContainer.ImportCuesheet.Artist);
            Assert.IsNull(sessionStateContainer.ImportCuesheet.Title);
            Assert.AreEqual(8, sessionStateContainer.ImportCuesheet.Tracks.Count);
            Assert.AreEqual(new TimeSpan(1, 15, 54), sessionStateContainer.ImportCuesheet.Tracks.Last().End);
        }

        [TestMethod()]
        public void ValidateTest()
        {
            var cuesheet = new Cuesheet();
            Assert.AreEqual(ValidationStatus.Error, cuesheet.Validate(nameof(Cuesheet.Artist)).Status);
            cuesheet.Artist = "Testartist";
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Validate(nameof(Cuesheet.Artist)).Status);
            Assert.AreEqual(ValidationStatus.Error, cuesheet.Validate(nameof(Cuesheet.Title)).Status);
            cuesheet.Title = "Testtitle";
            Assert.AreEqual(ValidationStatus.Success, cuesheet.Validate(nameof(Cuesheet.Title)).Status);
        }

        [TestMethod()]
        public void IsLinkedToPreviousTrack_ChangedLastTrackBegin_SetsTrackProperties()
        {
            //Arrange
            var cuesheet = new Cuesheet();
            var track1 = new Track()
            {
                Artist = "Track1 Artist",
                Title = "Track1 Title"
            };
            var track2 = new Track()
            {
                Artist = "Track2 Artist",
                Title = "Track2 Title",
                End = new TimeSpan(0, 9, 12)
            };
            cuesheet.AddTrack(track1);
            cuesheet.AddTrack(track2);
            track2.Begin = new TimeSpan(0, 4, 23);
            //Act
            track2.IsLinkedToPreviousTrack = true;
            //Assert
            Assert.AreEqual((uint)2, track2.Position);
            Assert.AreEqual(track2.Begin, track1.End);
        }

        [TestMethod()]
        public void IsRecordingPossible_WhenRecordingIsAlreadyRunning_ReturnsError()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.StartRecording();

            // Act
            var errors = cuesheet.IsRecordingPossible.ToList();

            // Assert
            Assert.Contains("Record is already running!", errors);
        }

        [TestMethod()]
        public void IsRecordingPossible_WhenCuesheetContainsTracks_ReturnsError()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.AddTrack(new Track());

            // Act
            var errors = cuesheet.IsRecordingPossible.ToList();

            // Assert
            Assert.Contains("Cuesheet already contains tracks!", errors);
        }

        [TestMethod()]
        public void IsRecordingPossible_WhenRecordingIsAlreadyAvailable_ReturnsError()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.Audiofile = new Audiofile("test", isRecorded: true);

            // Act
            var errors = cuesheet.IsRecordingPossible.ToList();

            // Assert
            Assert.Contains("A recording is already available!", errors);
        }

        [TestMethod()]
        public void IsRecordingPossible_WhenNoErrors_ReturnsEmpty()
        {
            // Arrange
            var cuesheet = new Cuesheet();

            // Act
            var errors = cuesheet.IsRecordingPossible.ToList();

            // Assert
            Assert.IsEmpty(errors);
        }

        [TestMethod]
        public void AddSection_WithValidData_FiresEvents()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            bool eventFired = false;
            cuesheet.TraceablePropertyChanged += (sender, args) =>
            {
                if (args.TraceableChange.PropertyName == nameof(Cuesheet.Sections))
                {
                    eventFired = true;
                }
            };

            // Act
            var section = cuesheet.AddSection();

            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(section);
            Assert.AreEqual(1, cuesheet.Sections.Count);
            Assert.AreEqual(cuesheet, section.Cuesheet);
        }
    }
}