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
using AudioCuesheetEditor.Services.AudioCuesheet;
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass]
    public class AudioImportServiceTests
    {
        private readonly AudioImportService _service;
        private readonly Mock<IDialogManager> _dialogManager;
        private readonly Mock<ISessionStateContainer> _sessionStateContainer;
        private readonly Mock<ICuesheetManager> _cuesheetManager;

        public AudioImportServiceTests()
        {
            _dialogManager = new();
            _sessionStateContainer = new();
            _cuesheetManager = new();
            _service = new(_dialogManager.Object, _sessionStateContainer.Object, _cuesheetManager.Object);
        }

        [TestMethod]
        public async Task MapAudioImportAsync_WhenMappingIsNotRequired_ReturnsTrueAsync()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            var importAudiofileMapping = new Dictionary<Audiofile, Audiofile?>();
            _dialogManager.Setup(x => x.ShowImportAudiofilesDialogAsync()).ReturnsAsync(true);
            _sessionStateContainer.SetupGet(x => x.ImportAudiofileMapping).Returns(importAudiofileMapping);
            _sessionStateContainer.SetupSet(x => x.ImportAudiofileMapping = It.IsAny<Dictionary<Audiofile, Audiofile?>>()).Callback<Dictionary<Audiofile, Audiofile?>>(cs => importAudiofileMapping = cs);
            _sessionStateContainer.SetupGet(x => x.ImportAudiofiles).Returns([]);
            _sessionStateContainer.SetupGet(x => x.ActiveCuesheet).Returns(cuesheet);
            // Act
            var result = await _service.MapAudioImportAsync();
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task MapAudioImportAsync_WhenMappingIsRequiredAndDoneAutomatically_ReturnsTrueAsync()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new()
                ]
            };
            var importAudiofileMapping = new Dictionary<Audiofile, Audiofile?>
            {
                { new(), null }
            };
            var importAudioFiles = new List<Audiofile>
            {
                new()
            };
            _dialogManager.Setup(x => x.ShowImportAudiofilesDialogAsync()).ReturnsAsync(true);
            _sessionStateContainer.SetupGet(x => x.ImportAudiofileMapping).Returns(importAudiofileMapping);
            _sessionStateContainer.SetupSet(x => x.ImportAudiofileMapping = It.IsAny<Dictionary<Audiofile, Audiofile?>>()).Callback<Dictionary<Audiofile, Audiofile?>>(cs => importAudiofileMapping = cs);
            _sessionStateContainer.SetupGet(x => x.ImportAudiofiles).Returns(importAudioFiles);
            _sessionStateContainer.SetupGet(x => x.ActiveCuesheet).Returns(cuesheet);
            // Act
            var result = await _service.MapAudioImportAsync();
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task MapAudioImportAsync_WhenMappingIsRequiredAndCannotBeDoneAutomatically_ReturnsDialogResultAsync()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Audiofiles = [
                    new()
                ]
            };
            var importAudiofileMapping = new Dictionary<Audiofile, Audiofile?>
            {
                { new(), null }
            };
            var importAudioFiles = new List<Audiofile>
            {
                new(), new()
            };
            var dialogResultAborted = true;
            _dialogManager.Setup(x => x.ShowImportAudiofilesDialogAsync()).ReturnsAsync(dialogResultAborted);
            _sessionStateContainer.SetupGet(x => x.ImportAudiofileMapping).Returns(importAudiofileMapping);
            _sessionStateContainer.SetupSet(x => x.ImportAudiofileMapping = It.IsAny<Dictionary<Audiofile, Audiofile?>>()).Callback<Dictionary<Audiofile, Audiofile?>>(cs => importAudiofileMapping = cs);
            _sessionStateContainer.SetupGet(x => x.ImportAudiofiles).Returns(importAudioFiles);
            _sessionStateContainer.SetupGet(x => x.ActiveCuesheet).Returns(cuesheet);
            // Act
            var result = await _service.MapAudioImportAsync();
            // Assert
            Assert.AreEqual(!dialogResultAborted, result);
        }
    }
}
