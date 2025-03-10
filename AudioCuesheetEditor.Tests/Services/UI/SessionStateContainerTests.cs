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
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Services.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AudioCuesheetEditor.Tests.Services.UI
{
    [TestClass]
    public class SessionStateContainerTests
    {
        private readonly Mock<ITraceChangeManager> traceChangeManagerMock;
        private readonly SessionStateContainer sessionStateContainer;

        public SessionStateContainerTests()
        {
            traceChangeManagerMock = new Mock<ITraceChangeManager>();
            sessionStateContainer = new SessionStateContainer(traceChangeManagerMock.Object);
        }

        [TestMethod]
        public void Cuesheet_SetNewValue_ShouldTriggerCuesheetChangedEvent()
        {
            // Arrange
            var newCuesheet = new Cuesheet();
            bool eventTriggered = false;
            sessionStateContainer.CuesheetChanged += (sender, args) => eventTriggered = true;

            // Act
            sessionStateContainer.Cuesheet = newCuesheet;

            // Assert
            Assert.IsTrue(eventTriggered);
            traceChangeManagerMock.Verify(m => m.Reset(), Times.Exactly(2));
            traceChangeManagerMock.Verify(m => m.TraceChanges(newCuesheet), Times.Once);
        }

        [TestMethod]
        public void ImportCuesheet_SetNewValue_ShouldTriggerImportCuesheetChangedEvent()
        {
            // Arrange
            var newImportCuesheet = new Cuesheet();
            bool eventTriggered = false;
            sessionStateContainer.ImportCuesheetChanged += (sender, args) => eventTriggered = true;

            // Act
            sessionStateContainer.ImportCuesheet = newImportCuesheet;

            // Assert
            Assert.IsTrue(eventTriggered);
        }

        [TestMethod]
        public void ImportAudiofile_SetNewValue_ShouldUpdateImportCuesheetAndTriggerEvent()
        {
            // Arrange
            var importCuesheet = new Cuesheet();
            var audioFile = new Audiofile("Test audio file.mp3");
            sessionStateContainer.ImportCuesheet = importCuesheet;
            bool eventTriggered = false;
            sessionStateContainer.ImportCuesheetChanged += (sender, args) => eventTriggered = true;

            // Act
            sessionStateContainer.ImportAudiofile = audioFile;

            // Assert
            Assert.AreEqual(audioFile, importCuesheet.Audiofile);
            Assert.IsTrue(eventTriggered);
        }

        [TestMethod]
        public void ResetImport_ShouldClearImportProperties()
        {
            // Arrange
            sessionStateContainer.Importfile = Mock.Of<IImportfile>();
            sessionStateContainer.ImportCuesheet = new Cuesheet();

            // Act
            sessionStateContainer.ResetImport();

            // Assert
            Assert.IsNull(sessionStateContainer.Importfile);
            Assert.IsNull(sessionStateContainer.ImportAudiofile);
            Assert.IsNull(sessionStateContainer.ImportCuesheet);
        }
    }
}