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
using AudioCuesheetEditor.Extensions;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.UI;
using AudioCuesheetEditor.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Services.IO.Tests
{
    [TestClass()]
    public class ImportManagerTests
    {
        [TestMethod()]
        public async Task ImportTextAsync_TextfileWithStartDateTime_CreatesValidCuesheetAsync()
        {
            // Arrange
            var fileContent = new List<String>
            {
                "Innellea~The Golden Fort~02.08.2024 20:10:48",
                "Nora En Pure~Diving with Whales (Daniel Portman Remix)~02.08.2024 20:15:21",
                "WhoMadeWho & Adriatique~Miracle (RÜFÜS DU SOL Remix)~02.08.2024 20:20:42",
                "Ella Wild~Poison D'araignee (Original Mix)~02.08.2024 20:28:03",
                "Stil & Bense~On The Edge (Original Mix)~02.08.2024 20:32:42",
                "Nebula~Clairvoyant Dreams~02.08.2024 20:39:01",
                "Valentina Black~I'm a Tree (Extended Mix)~02.08.2024 20:47:08",
                "Nebula~Clairvoyant Dreams~02.08.2024 20:53:20",
                "Kiko & Dave Davis feat. Phoebe~Living in Space (Dub Mix)~02.08.2024 20:58:11",
                "Lilly Palmer~Before Acid~02.08.2024 21:03:53",
                "Sofi Tukker~Drinkee (Vintage Culture & John Summit Extended Mix)~02.08.2024 21:09:52",
                "CID & Truth x Lies~Caroline (Extended Mix)~02.08.2024 21:14:09",
                "Moby~Why Does My Heart Feel So Bad? (Oxia Remix)~02.08.2024 21:17:15",
                "Ammo Avenue~Little Gurl (Extended Mix)~02.08.2024 21:22:46",
                "James Hurr & Smokin Jo & Stealth~Beggin' For Change~02.08.2024 21:28:37",
                "Kristine Blond~Love Shy (Sam Divine & CASSIMM Extended Remix)~02.08.2024 21:30:47",
                "Vanilla Ace~Work On You (Original Mix)~02.08.2024 21:36:28",
                "Truth X Lies~Like This~02.08.2024 21:42:05",
                "Terri-Anne~Round Round~02.08.2024 21:44:07",
                "Joanna Magik~Maneater~02.08.2024 21:46:32",
                "Jen Payne & Kevin McKay~Feed Your Soul~02.08.2024 21:48:45",
                "Kevin McKay & Eppers & Notelle~On My Own~02.08.2024 21:51:37",
                "Nader Razdar & Kevin McKay~Get Ur Freak On (Kevin McKay Extended Mix)~02.08.2024 21:53:49",
                "Philip Z~Yala (Extended Mix)~02.08.2024 21:59:40",
                "Kyle Kinch & Kevin McKay~Hella~02.08.2024 22:05:53",
                "Roze Wild~B-O-D-Y~02.08.2024 22:08:26",
                "Jey Kurmis~Snoop~02.08.2024 22:11:09",
                "Bootie Brown & Tame Impala & Gorillaz~New Gold (Dom Dolla Remix Extended)~02.08.2024 22:16:23",
                "Eli Brown & Love Regenerator~Don't You Want Me (Original Mix)~02.08.2024 22:21:23",
                "Local Singles~Voices~02.08.2024 22:25:59"
            };

            var traceChangeManager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var sessionStateContainer = new SessionStateContainer(traceChangeManager);
            var localStorageOptionsProviderMock = new Mock<ILocalStorageOptionsProvider>();
            var importOptions = new ImportOptions
            {
                TextImportScheme = new TextImportScheme()
                {
                    SchemeCuesheet = null,
                    SchemeTracks = @"(?'Track.Artist'[a-zA-Z0-9_ .();äöü&:,'*-?:]{1,})~(?'Track.Title'[a-zA-Z0-9_ .();äöü&'*-?:Ü]{1,})~(?'Track.StartDateTime'.{1,})"
                }
            };
            localStorageOptionsProviderMock.Setup(x => x.GetOptions<ImportOptions>()).ReturnsAsync(importOptions);
            var textImportService = new TextImportService();
            var importManager = new ImportManager(sessionStateContainer, localStorageOptionsProviderMock.Object, textImportService, traceChangeManager);
            var testHelper = new TestHelper();
            // Act
            await importManager.ImportTextAsync(fileContent);
            // Assert
            Assert.IsNull(sessionStateContainer.Importfile?.AnalyseException);
            Assert.IsNotNull(sessionStateContainer.ImportCuesheet);
            Assert.AreEqual(30, sessionStateContainer.ImportCuesheet.Tracks.Count);
            Assert.AreEqual("Innellea", sessionStateContainer.ImportCuesheet.Tracks.ElementAt(0).Artist);
            Assert.AreEqual("The Golden Fort", sessionStateContainer.ImportCuesheet.Tracks.ElementAt(0).Title);
            Assert.AreEqual(TimeSpan.Zero, sessionStateContainer.ImportCuesheet.Tracks.ElementAt(0).Begin);
            Assert.AreEqual(new TimeSpan(0, 4, 33), sessionStateContainer.ImportCuesheet.Tracks.ElementAt(0).End);
            Assert.AreEqual(new TimeSpan(0, 4, 33), sessionStateContainer.ImportCuesheet.Tracks.ElementAt(0).Length);
            Assert.AreEqual("Nora En Pure", sessionStateContainer.ImportCuesheet.Tracks.ElementAt(1).Artist);
            Assert.AreEqual("Diving with Whales (Daniel Portman Remix)", sessionStateContainer.ImportCuesheet.Tracks.ElementAt(1).Title);
            Assert.AreEqual(new TimeSpan(0, 4, 33), sessionStateContainer.ImportCuesheet.Tracks.ElementAt(1).Begin);
            Assert.AreEqual(new TimeSpan(0, 9, 54), sessionStateContainer.ImportCuesheet.Tracks.ElementAt(1).End);
            Assert.AreEqual(new TimeSpan(0, 5, 21), sessionStateContainer.ImportCuesheet.Tracks.ElementAt(1).Length);
            Assert.AreEqual("Local Singles", sessionStateContainer.ImportCuesheet.Tracks.ElementAt(29).Artist);
            Assert.AreEqual("Voices", sessionStateContainer.ImportCuesheet.Tracks.ElementAt(29).Title);
            Assert.AreEqual(new TimeSpan(2, 15, 11), sessionStateContainer.ImportCuesheet.Tracks.ElementAt(29).Begin);
            Assert.IsNull(sessionStateContainer.ImportCuesheet.Tracks.ElementAt(29).End);
            Assert.IsNull(sessionStateContainer.ImportCuesheet.Tracks.ElementAt(29).Length);
        }
    }
}