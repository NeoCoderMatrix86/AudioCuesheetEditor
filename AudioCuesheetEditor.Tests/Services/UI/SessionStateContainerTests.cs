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
using AudioCuesheetEditor.Model.UI;
using AudioCuesheetEditor.Services.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AudioCuesheetEditor.Tests.Services.UI
{
    [TestClass]
    public class SessionStateContainerTests
    {
        private readonly Mock<ITraceChangeManager> _traceChangeManagerMock;
        private readonly SessionStateContainer _sessionStateContainer;

        public SessionStateContainerTests()
        {
            _traceChangeManagerMock = new Mock<ITraceChangeManager>();
            _sessionStateContainer = new SessionStateContainer(_traceChangeManagerMock.Object);
        }
        
        [TestMethod]
        public void Cuesheet_SetNewValue_ShouldTriggerCuesheetChangedEvent()
        {
            // Arrange
            var newCuesheet = new Cuesheet();
            bool eventTriggered = false;
            _sessionStateContainer.CuesheetChanged += (sender, args) => eventTriggered = true;

            // Act
            _sessionStateContainer.Cuesheet = newCuesheet;

            // Assert
            Assert.IsTrue(eventTriggered);
            _traceChangeManagerMock.Verify(m => m.AddChange(It.Is<TracedChange>(x => x.TraceableObject == _sessionStateContainer && x.TraceableChange.PropertyName == nameof(SessionStateContainer.Cuesheet))), Times.Once);
        }

        [TestMethod]
        public void ImportCuesheet_SetNewValue_ShouldTriggerImportCuesheetChangedEvent()
        {
            // Arrange
            var newImportCuesheet = new Cuesheet();
            bool eventTriggered = false;
            _sessionStateContainer.ImportCuesheetChanged += (sender, args) => eventTriggered = true;

            // Act
            _sessionStateContainer.ImportCuesheet = newImportCuesheet;

            // Assert
            Assert.IsTrue(eventTriggered);
        }

        [TestMethod]
        public void ImportAudiofile_SetNewValue_ShouldUpdateImportCuesheetAndTriggerEvent()
        {
            // Arrange
            var importCuesheet = new Cuesheet();
            var audioFile = new Audiofile("Test audio file.mp3");
            _sessionStateContainer.ImportCuesheet = importCuesheet;
            bool eventTriggered = false;
            _sessionStateContainer.ImportCuesheetChanged += (sender, args) => eventTriggered = true;

            // Act
            _sessionStateContainer.ImportAudiofile = audioFile;

            // Assert
            Assert.AreEqual(audioFile, importCuesheet.Audiofile);
            Assert.IsTrue(eventTriggered);
        }

        [TestMethod]
        public void ResetImport_ShouldClearImportProperties()
        {
            // Arrange
            _sessionStateContainer.Importfile = Mock.Of<IImportfile>();
            _sessionStateContainer.ImportCuesheet = new Cuesheet();

            // Act
            _sessionStateContainer.ResetImport();

            // Assert
            Assert.IsNull(_sessionStateContainer.Importfile);
            Assert.IsNull(_sessionStateContainer.ImportAudiofile);
            Assert.IsNull(_sessionStateContainer.ImportCuesheet);
        }
    }
}