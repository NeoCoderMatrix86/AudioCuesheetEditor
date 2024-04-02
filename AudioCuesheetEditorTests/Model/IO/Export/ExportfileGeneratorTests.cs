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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.IO.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditorTests.Utility;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.IO.Audio;
using System.IO;
using AudioCuesheetEditor.Model.Entity;
using System.Diagnostics.Metrics;

namespace AudioCuesheetEditor.Model.IO.Export.Tests
{
    [TestClass()]
    public class ExportfileGeneratorTests
    {

        [TestMethod()]
        public void GenerateCuesheetFilesTest()
        {
            var testHelper = new TestHelper();
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            }
            var generator = new ExportfileGenerator(ExportType.Cuesheet, cuesheet, applicationOptions: testHelper.ApplicationOptions);
            var generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(1, generatedFiles.Count);
            Assert.AreEqual(Exportfile.DefaultCuesheetFilename, generatedFiles.First().Name);
            var content = generatedFiles.First().Content;
            Assert.IsNotNull(content);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, content);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, cuesheet.Audiofile.Name, cuesheet.Audiofile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 4)
            {
                var track = cuesheet.Tracks.Single(x => x.Position == position);
                position++;
                Assert.AreEqual(fileContent[i], String.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, track.Position, CuesheetConstants.CuesheetTrackAudio));
                Assert.AreEqual(fileContent[i + 1], String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title));
                Assert.AreEqual(fileContent[i + 2], String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist));
                var trackBegin = track.Begin;
                Assert.IsNotNull(trackBegin);
                Assert.AreEqual(fileContent[i + 3], String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(trackBegin.Value.TotalMinutes), trackBegin.Value.Seconds, trackBegin.Value.Milliseconds / 75));
            }
            File.Delete(fileName);
            cuesheet.CDTextfile = new CDTextfile("Testfile.cdt");
            cuesheet.Cataloguenumber.Value = "0123456789123";
            generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(1, generatedFiles.Count);
            content = generatedFiles.First().Content;
            Assert.IsNotNull(content);
            fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, content);
            fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} {1}", CuesheetConstants.CuesheetCatalogueNumber, cuesheet.Cataloguenumber.Value));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetCDTextfile, cuesheet.CDTextfile.Name));
            File.Delete(fileName);
            cuesheet.CDTextfile = new CDTextfile("Testfile.cdt");
            cuesheet.Cataloguenumber.Value = "Testvalue";
            Assert.AreEqual(ValidationStatus.Error, generator.Validate().Status);
            generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(0, generatedFiles.Count);
        }

        [TestMethod()]
        public void GenerateCuesheetFilesWithPreGapAndPostGapTest()
        {
            var testHelper = new TestHelper();
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                var rand = new Random();
                var flagsToAdd = rand.Next(1, 3);
                for (int x = 0; x < flagsToAdd; x++)
                {
                    track.SetFlag(Flag.AvailableFlags.ElementAt(x), SetFlagMode.Add);
                }
                track.PostGap = new TimeSpan(0, 0, 2);
                track.PreGap = new TimeSpan(0, 0, 3);
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            }
            var generator = new ExportfileGenerator(ExportType.Cuesheet, cuesheet, applicationOptions: testHelper.ApplicationOptions);
            var generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(1, generatedFiles.Count);
            var content = generatedFiles.First().Content;
            Assert.IsNotNull(content);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, content);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, cuesheet.Audiofile.Name, cuesheet.Audiofile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 7)
            {
                var track = cuesheet.Tracks.Single(x => x.Position == position);
                position++;
                Assert.AreEqual(String.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, track.Position, CuesheetConstants.CuesheetTrackAudio), fileContent[i]);
                Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title), fileContent[i + 1]);
                Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist), fileContent[i + 2]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackFlags, String.Join(" ", track.Flags.Select(x => x.CuesheetLabel))), fileContent[i + 3]);
                var preGap = track.PreGap;
                var trackBegin = track.Begin;
                var postGap = track.PostGap;
                Assert.IsNotNull(preGap);
                Assert.IsNotNull(trackBegin);
                Assert.IsNotNull(postGap);
                Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackPreGap, Math.Floor(preGap.Value.TotalMinutes), preGap.Value.Seconds, preGap.Value.Milliseconds / 75), fileContent[i + 4]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(trackBegin.Value.TotalMinutes), trackBegin.Value.Seconds, trackBegin.Value.Milliseconds / 75), fileContent[i + 5]);
                Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackPostGap, Math.Floor(postGap.Value.TotalMinutes), postGap.Value.Seconds, postGap.Value.Milliseconds / 75), fileContent[i + 6]);
            }
            File.Delete(fileName);
        }

        [TestMethod()]
        public void GenerateCuesheetFilesWithTrackFlagsTest()
        {
            var testHelper = new TestHelper();
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                var rand = new Random();
                var flagsToAdd = rand.Next(1, 3);
                for (int x = 0; x < flagsToAdd; x++)
                {
                    track.SetFlag(Flag.AvailableFlags.ElementAt(x), SetFlagMode.Add);
                }
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            }
            var generator = new ExportfileGenerator(ExportType.Cuesheet, cuesheet, applicationOptions: testHelper.ApplicationOptions);
            var generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(1, generatedFiles.Count);
            var content = generatedFiles.First().Content;
            Assert.IsNotNull(content);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, content);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, cuesheet.Audiofile.Name, cuesheet.Audiofile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 5)
            {
                var track = cuesheet.Tracks.Single(x => x.Position == position);
                position++;
                Assert.AreEqual(fileContent[i], String.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, track.Position, CuesheetConstants.CuesheetTrackAudio));
                Assert.AreEqual(fileContent[i + 1], String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title));
                Assert.AreEqual(fileContent[i + 2], String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist));
                Assert.AreEqual(fileContent[i + 3], String.Format("{0}{1}{2} {3}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackFlags, String.Join(" ", track.Flags.Select(x => x.CuesheetLabel))));
                var trackBegin = track.Begin;
                Assert.IsNotNull(trackBegin);
                Assert.AreEqual(fileContent[i + 4], String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(trackBegin.Value.TotalMinutes), trackBegin.Value.Seconds, trackBegin.Value.Milliseconds / 75));
            }
            File.Delete(fileName);
        }

        [TestMethod()]
        public void GenerateCuesheetFilesWithIncorrectTrackPositionsTest()
        {
            var testHelper = new TestHelper();
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            var random = new Random();
            for (int i = 1; i < 6; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin,
                    Position = (uint)(i + random.Next(1, 10))
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            }
            var generator = new ExportfileGenerator(ExportType.Cuesheet, cuesheet, applicationOptions: testHelper.ApplicationOptions);
            Assert.AreEqual(ValidationStatus.Error, generator.Validate().Status);
            //Rearrange positions
            cuesheet.Tracks.ElementAt(0).Position = 1;
            cuesheet.Tracks.ElementAt(1).Position = 2;
            cuesheet.Tracks.ElementAt(2).Position = 3;
            cuesheet.Tracks.ElementAt(3).Position = 4;
            cuesheet.Tracks.ElementAt(4).Position = 5;
            Assert.AreEqual(ValidationStatus.Success, generator.Validate().Status);
            var generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(1, generatedFiles.Count);
            var content = generatedFiles.First().Content;
            Assert.IsNotNull(content);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, content);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetConstants.CuesheetFileName, cuesheet.Audiofile.Name, cuesheet.Audiofile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 4)
            {
                var track = cuesheet.Tracks.ElementAt(position - 1);
                Assert.AreEqual(String.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, position, CuesheetConstants.CuesheetTrackAudio), fileContent[i]);
                Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title), fileContent[i + 1]);
                Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist), fileContent[i + 2]);
                var trackBegin = track.Begin;
                Assert.IsNotNull(trackBegin);
                Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(trackBegin.Value.TotalMinutes), trackBegin.Value.Seconds, trackBegin.Value.Milliseconds / 75), fileContent[i + 3]);
                position++;
            }
            File.Delete(fileName);
        }

        [TestMethod()]
        public void GenerateCuesheetFilesWithSplitPointsTest()
        {
            var testHelper = new TestHelper();
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
            }
            var splitPoint = cuesheet.AddSplitPoint();
            splitPoint.Moment = new TimeSpan(2, 0, 0);
            splitPoint.Title = "Last part";
            splitPoint = cuesheet.AddSplitPoint();
            splitPoint.Moment = new TimeSpan(0, 30, 0);
            splitPoint = cuesheet.AddSplitPoint();
            splitPoint.Moment = new TimeSpan(1, 0, 0);
            splitPoint.Artist = "Demo Artist Part2";
            splitPoint = cuesheet.AddSplitPoint();
            splitPoint.Artist = "Artist 3";
            splitPoint.Title = "Title 3";
            splitPoint.Moment = new TimeSpan(1, 30, 0);
            testHelper.ApplicationOptions.CuesheetFilename = "Unit test.cue";
            var generator = new ExportfileGenerator(ExportType.Cuesheet, cuesheet, applicationOptions: testHelper.ApplicationOptions);
            Assert.AreEqual(ValidationStatus.Success, generator.Validate().Status);
            var generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(5, generatedFiles.Count);
            var position = 1;
            var counter = 1;
            //Check split according to split points
            Assert.IsNull(generatedFiles.First().Begin);
            Assert.AreEqual(new TimeSpan(0, 30, 0), generatedFiles.First().End);
            Assert.AreEqual(new TimeSpan(0, 30, 0), generatedFiles.ElementAt(1).Begin);
            Assert.AreEqual(new TimeSpan(1, 0, 0), generatedFiles.ElementAt(1).End);
            Assert.AreEqual(new TimeSpan(1, 0, 0), generatedFiles.ElementAt(2).Begin);
            Assert.AreEqual(new TimeSpan(1, 30, 0), generatedFiles.ElementAt(2).End);
            Assert.AreEqual(new TimeSpan(1, 30, 0), generatedFiles.ElementAt(3).Begin);
            Assert.AreEqual(new TimeSpan(2, 0, 0), generatedFiles.ElementAt(3).End);
            Assert.AreEqual(new TimeSpan(2, 0, 0), generatedFiles.ElementAt(4).Begin);
            Assert.IsNull(generatedFiles.Last().End);
            foreach (var generatedFile in generatedFiles)
            {
                Assert.AreEqual(String.Format("Unit test({0}).cue", counter), generatedFile.Name);
                counter++;
                var content = generatedFile.Content;
                Assert.IsNotNull(content);
                var fileName = Path.GetTempFileName();
                File.WriteAllBytes(fileName, content);
                var fileContent = File.ReadAllLines(fileName);
                File.Delete(fileName);
                int positionDifference = 1 - position;
                // Check cuesheet header for splitpoint artist and title
                var splitPointForThisFile = cuesheet.SplitPoints.FirstOrDefault(x => x.Moment == generatedFile.End);
                if (splitPointForThisFile != null)
                {
                    Assert.AreEqual(String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, splitPointForThisFile.Title), fileContent.First());
                    Assert.AreEqual(String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, splitPointForThisFile.Artist), fileContent[1]);
                }
                else
                {
                    Assert.AreEqual(String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetTitle, cuesheet.Title), fileContent.First());
                    Assert.AreEqual(String.Format("{0} \"{1}\"", CuesheetConstants.CuesheetArtist, cuesheet.Artist), fileContent[1]);
                }
                //Check for start from position 1 and begin = 00:00:00
                for (int i = 3; i < fileContent.Length; i += 4)
                {
                    var track = cuesheet.Tracks.Single(x => x.Position == position);
                    position++;
                    Assert.AreEqual(String.Format("{0}{1} {2:00} {3}", CuesheetConstants.Tab, CuesheetConstants.CuesheetTrack, track.Position + positionDifference, CuesheetConstants.CuesheetTrackAudio), fileContent[i]);
                    Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackTitle, track.Title), fileContent[i + 1]);
                    Assert.AreEqual(String.Format("{0}{1}{2} \"{3}\"", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackArtist, track.Artist), fileContent[i + 2]);
                    var trackBegin = track.Begin;
                    if (generatedFile.Begin != null)
                    {
                        if (generatedFile.Begin >= track.Begin)
                        {
                            trackBegin = TimeSpan.Zero;
                        }
                        else
                        {
                            trackBegin = track.Begin - generatedFile.Begin;
                        }
                    }
                    Assert.IsNotNull(trackBegin);
                    Assert.AreEqual(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetConstants.Tab, CuesheetConstants.Tab, CuesheetConstants.TrackIndex01, Math.Floor(trackBegin.Value.TotalMinutes), trackBegin.Value.Seconds, trackBegin.Value.Milliseconds / 75), fileContent[i + 3]);
                }
                position--;
            }
        }

        [TestMethod()]
        public void GenerateExportfilesTest()
        {
            var testHelper = new TestHelper();
            //Prepare cuesheet
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
                var rand = new Random();
                var flagsToAdd = rand.Next(0, 3);
                if (flagsToAdd > 0)
                {
                    for (int x = 0; x < flagsToAdd; x++)
                    {
                        track.SetFlag(Flag.AvailableFlags.ElementAt(x), SetFlagMode.Add);
                    }
                }
            }

            cuesheet.Cataloguenumber.Value = "0123456789123";
            cuesheet.CDTextfile = new CDTextfile("Testfile.cdt");

            //Test class
            var exportProfile = new Exportprofile
            {
                SchemeHead = "%Cuesheet.Artist%;%Cuesheet.Title%;%Cuesheet.Cataloguenumber%;%Cuesheet.CDTextfile%",
                SchemeTracks = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%",
                SchemeFooter = "Exported %Cuesheet.Title% from %Cuesheet.Artist% using AudioCuesheetEditor"
            };
            Assert.AreEqual(ValidationStatus.Success, exportProfile.Validate().Status);
            var generator = new ExportfileGenerator(ExportType.Exportprofile, cuesheet, exportprofile: exportProfile);
            var generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(1, generatedFiles.Count);
            Assert.AreEqual(exportProfile.Filename, generatedFiles.First().Name);
            var fileContent = generatedFiles.First().Content;
            Assert.IsNotNull(fileContent);
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, fileContent);
            var content = File.ReadAllLines(tempFile);
            Assert.AreEqual("Demo Artist;Demo Title;0123456789123;Testfile.cdt", content[0]);
            for (int i = 1; i < content.Length - 1; i++)
            {
                Assert.IsFalse(String.IsNullOrEmpty(content[i]));
                Assert.AreNotEqual(content[i], ";;;;;");
                Assert.IsTrue(content[i].StartsWith(cuesheet.Tracks.ToList()[i - 1].Position + ";"));
            }
            Assert.AreEqual(content[^1], "Exported Demo Title from Demo Artist using AudioCuesheetEditor");

            File.Delete(tempFile);

            exportProfile.SchemeHead = "%Track.Position%;%Cuesheet.Artist%;";
            var validationResult = exportProfile.Validate(x => x.SchemeHead);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Parameter != null && x.Parameter.Contains("%Track.Position%")));

            //Check multiline export
            exportProfile = new Exportprofile
            {
                SchemeHead = "%Cuesheet.Artist%;%Cuesheet.Title%",
                SchemeTracks = String.Format("%Track.Position%{0}%Track.Artist%{1}%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%", Environment.NewLine, Environment.NewLine),
                SchemeFooter = "Exported %Cuesheet.Title% from %Cuesheet.Artist% using AudioCuesheetEditor"
            };
            Assert.AreEqual(ValidationStatus.Success, exportProfile.Validate().Status);
            generator.Exportprofile = exportProfile;
            generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(1, generatedFiles.Count);
            Assert.AreEqual(exportProfile.Filename, generatedFiles.First().Name);
            fileContent = generatedFiles.First().Content;
            Assert.IsNotNull(fileContent);
            tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, fileContent);
            content = File.ReadAllLines(tempFile);
            Assert.AreEqual(content[0], "Demo Artist;Demo Title");
            var trackPosition = 0;
            for (int i = 1; i < content.Length - 1; i += 3)
            {
                Assert.IsFalse(String.IsNullOrEmpty(content[i]));
                Assert.AreNotEqual(content[i], ";;;;;");
                var track = cuesheet.Tracks.ToList()[trackPosition];
                Assert.IsNotNull(track.Position);
                var position = track.Position.ToString();
                Assert.IsNotNull(position);
                Assert.IsTrue(content[i].StartsWith(position));
                trackPosition++;
            }
            Assert.AreEqual(content[^1], "Exported Demo Title from Demo Artist using AudioCuesheetEditor");

            File.Delete(tempFile);

            //Test flags
            exportProfile = new Exportprofile
            {
                SchemeHead = "%Cuesheet.Artist%;%Cuesheet.Title%;%Cuesheet.Cataloguenumber%;%Cuesheet.CDTextfile%",
                SchemeTracks = "%Track.Position%;%Track.Flags%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%",
                SchemeFooter = "Exported %Cuesheet.Title% from %Cuesheet.Artist% using AudioCuesheetEditor"
            };
            Assert.AreEqual(ValidationStatus.Success, exportProfile.Validate().Status);
            generator.Exportprofile = exportProfile;
            generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(1, generatedFiles.Count);
            Assert.AreEqual(exportProfile.Filename, generatedFiles.First().Name);
            fileContent = generatedFiles.First().Content;
            Assert.IsNotNull(fileContent);
            tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, fileContent);
            content = File.ReadAllLines(tempFile);
            Assert.AreEqual("Demo Artist;Demo Title;0123456789123;Testfile.cdt", content[0]);
            for (int i = 1; i < content.Length - 1; i++)
            {
                Assert.IsFalse(String.IsNullOrEmpty(content[i]));
                Assert.AreNotEqual(content[i], ";;;;;");
                Assert.IsTrue(content[i].StartsWith(cuesheet.Tracks.ToList()[i - 1].Position + ";"));
                if (cuesheet.Tracks.ElementAt(i - 1).Flags.Count > 0)
                {
                    var flags = cuesheet.Tracks.ElementAt(i - 1).Flags;
                    Assert.IsTrue(content[i].Contains(String.Join(" ", flags.Select(x => x.CuesheetLabel))));
                }
            }
            Assert.AreEqual(content[^1], "Exported Demo Title from Demo Artist using AudioCuesheetEditor");
            File.Delete(tempFile);
        }

        [TestMethod()]
        public void GenerateExportfilesWithPregapAndPostgapTest()
        {
            var testHelper = new TestHelper();
            //Prepare cuesheet
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin,
                    PostGap = new TimeSpan(0, 0, 1),
                    PreGap = new TimeSpan(0, 0, 3)
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
                var rand = new Random();
                var flagsToAdd = rand.Next(0, 3);
                if (flagsToAdd > 0)
                {
                    for (int x = 0; x < flagsToAdd; x++)
                    {
                        track.SetFlag(Flag.AvailableFlags.ElementAt(x), SetFlagMode.Add);
                    }
                }
            }

            cuesheet.Cataloguenumber.Value = "0123456789123";
            cuesheet.CDTextfile = new CDTextfile("Testfile.cdt");

            var exportProfile = new Exportprofile
            {
                SchemeHead = "%Cuesheet.Artist%;%Cuesheet.Title%;%Cuesheet.Cataloguenumber%;%Cuesheet.CDTextfile%",
                SchemeTracks = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%;%Track.PreGap%;%Track.PostGap%",
                SchemeFooter = "Exported %Cuesheet.Title% from %Cuesheet.Artist% using AudioCuesheetEditor at %Date%"
            };
            Assert.AreEqual(ValidationStatus.Success, exportProfile.Validate().Status);
            var generator = new ExportfileGenerator(ExportType.Exportprofile, cuesheet, exportprofile: exportProfile);
            var generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(1, generatedFiles.Count);
            Assert.AreEqual(exportProfile.Filename, generatedFiles.First().Name);
            var fileContent = generatedFiles.First().Content;
            Assert.IsNotNull(fileContent);
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, fileContent);
            var content = File.ReadAllLines(tempFile);
            Assert.AreEqual("Demo Artist;Demo Title;0123456789123;Testfile.cdt", content[0]);
            for (int i = 1; i < content.Length - 1; i++)
            {
                Assert.IsFalse(String.IsNullOrEmpty(content[i]));
                Assert.AreNotEqual(content[i], ";;;;;");
                Assert.IsTrue(content[i].StartsWith(cuesheet.Tracks.ToList()[i - 1].Position + ";"));
            }
            Assert.AreEqual(content[^1], String.Format("Exported Demo Title from Demo Artist using AudioCuesheetEditor at {0}", DateTime.Now.ToShortDateString()));
            File.Delete(tempFile);
        }

        [TestMethod()]
        public void GenerateExportfilesWithSplitPointsTest()
        {
            var testHelper = new TestHelper();
            //Prepare cuesheet
            Cuesheet cuesheet = new()
            {
                Artist = "Demo Artist",
                Title = "Demo Title",
                Audiofile = new Audiofile("Testfile.mp3")
            };
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = new Track
                {
                    Artist = String.Format("Demo Track Artist {0}", i),
                    Title = String.Format("Demo Track Title {0}", i),
                    Begin = begin
                };
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track, testHelper.ApplicationOptions);
                var rand = new Random();
                var flagsToAdd = rand.Next(0, 3);
                if (flagsToAdd > 0)
                {
                    for (int x = 0; x < flagsToAdd; x++)
                    {
                        track.SetFlag(Flag.AvailableFlags.ElementAt(x), SetFlagMode.Add);
                    }
                }
            }

            cuesheet.Cataloguenumber.Value = "0123456789123";
            cuesheet.CDTextfile = new CDTextfile("Testfile.cdt");

            var splitPoint = cuesheet.AddSplitPoint();
            splitPoint.Moment = new TimeSpan(2, 0, 0);
            splitPoint.Title = "Last part";
            splitPoint = cuesheet.AddSplitPoint();
            splitPoint.Moment = new TimeSpan(0, 30, 0);
            splitPoint = cuesheet.AddSplitPoint();
            splitPoint.Moment = new TimeSpan(1, 0, 0);
            splitPoint.Artist = "Demo Artist Part2";
            splitPoint = cuesheet.AddSplitPoint();
            splitPoint.Artist = "Artist 3";
            splitPoint.Title = "Title 3";
            splitPoint.Moment = new TimeSpan(1, 30, 0);
            //Test export
            var exportProfile = new Exportprofile
            {
                SchemeHead = "%Cuesheet.Artist%;%Cuesheet.Title%;%Cuesheet.Cataloguenumber%;%Cuesheet.CDTextfile%",
                SchemeTracks = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%",
                SchemeFooter = "Exported %Cuesheet.Title% from %Cuesheet.Artist% using AudioCuesheetEditor"
            };
            var generator = new ExportfileGenerator(ExportType.Exportprofile, cuesheet, exportProfile);
            Assert.AreEqual(ValidationStatus.Success, generator.Validate().Status);
            var generatedFiles = generator.GenerateExportfiles();
            Assert.AreEqual(5, generatedFiles.Count);

            //Check split according to split points
            Assert.IsNull(generatedFiles.First().Begin);
            Assert.AreEqual(new TimeSpan(0, 30, 0), generatedFiles.First().End);
            Assert.AreEqual(new TimeSpan(0, 30, 0), generatedFiles.ElementAt(1).Begin);
            Assert.AreEqual(new TimeSpan(1, 0, 0), generatedFiles.ElementAt(1).End);
            Assert.AreEqual(new TimeSpan(1, 0, 0), generatedFiles.ElementAt(2).Begin);
            Assert.AreEqual(new TimeSpan(1, 30, 0), generatedFiles.ElementAt(2).End);
            Assert.AreEqual(new TimeSpan(1, 30, 0), generatedFiles.ElementAt(3).Begin);
            Assert.AreEqual(new TimeSpan(2, 0, 0), generatedFiles.ElementAt(3).End);
            Assert.AreEqual(new TimeSpan(2, 0, 0), generatedFiles.ElementAt(4).Begin);
            Assert.IsNull(generatedFiles.Last().End);
            var counter = 1;
            var position = 1;
            foreach (var generatedFile in generatedFiles)
            {
                Assert.AreEqual(String.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(exportProfile.Filename), counter, Path.GetExtension(exportProfile.Filename)), generatedFile.Name);
                counter++;
                var content = generatedFile.Content;
                Assert.IsNotNull(content);
                var fileName = Path.GetTempFileName();
                File.WriteAllBytes(fileName, content);
                var fileContent = File.ReadAllLines(fileName);
                File.Delete(fileName);
                int positionDifference = 1 - position;
                //Check cuesheet header for splitpoint artist and title
                var splitPointForThisFile = cuesheet.SplitPoints.FirstOrDefault(x => x.Moment == generatedFile.End);
                if (splitPointForThisFile != null)
                {
                    Assert.AreEqual(String.Format("{0};{1};0123456789123;Testfile.cdt", splitPointForThisFile.Artist, splitPointForThisFile.Title), fileContent[0]);
                }
                else
                {
                    Assert.AreEqual(String.Format("{0};{1};0123456789123;Testfile.cdt", cuesheet.Artist, cuesheet.Title), fileContent[0]);
                }
                //Check for start from position 1 and begin = 00:00:00
                for (int i = 1; i < fileContent.Length - 1; i++)
                {
                    var track = cuesheet.Tracks.Single(x => x.Position == position);
                    position++;
                    var trackBegin = track.Begin;
                    var trackEnd = track.End;
                    if (generatedFile.Begin != null)
                    {
                        if (generatedFile.Begin >= track.Begin)
                        {
                            trackBegin = TimeSpan.Zero;
                        }
                        else
                        {
                            trackBegin = track.Begin - generatedFile.Begin;
                        }
                        trackEnd = track.End - generatedFile.Begin;
                    }
                    Assert.AreEqual(String.Format("{0};{1};{2};{3};{4};{5}", track.Position + positionDifference, track.Artist, track.Title, trackBegin, trackEnd, trackEnd - trackBegin), fileContent[i]);
                }
                if (splitPointForThisFile != null)
                {
                    Assert.AreEqual(String.Format("Exported {0} from {1} using AudioCuesheetEditor", splitPointForThisFile.Title, splitPointForThisFile.Artist), fileContent.Last());
                }
                else
                {
                    Assert.AreEqual(String.Format("Exported {0} from {1} using AudioCuesheetEditor", cuesheet.Title, cuesheet.Artist), fileContent.Last());
                }
                position--;
            }
        }
    }
}