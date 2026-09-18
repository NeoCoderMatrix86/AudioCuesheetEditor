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
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Services.Options;
using AudioCuesheetEditor.Services.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Services.UI
{
    [TestClass]
    public class SessionStateContainerTests
    {
        private readonly SessionStateContainer _sessionStateContainer;
        private readonly Mock<ILocalStorageOptionsProvider> _localStorageOptionsProvider;

        public SessionStateContainerTests()
        {
            _localStorageOptionsProvider = new();
            _sessionStateContainer = new(_localStorageOptionsProvider.Object);
        }

        [TestMethod]
        public async Task Cuesheet_SetNewValueInDetailView_ShouldTriggerActiveCuesheetChangedEventAsync()
        {
            // Arrange
            var newCuesheet = new Cuesheet();
            var viewOptions = new ViewOptions();
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            await _sessionStateContainer.InitializeAsync();
            bool eventTriggered = false;
            _sessionStateContainer.ActiveCuesheetChanged += (sender, args) => eventTriggered = true;

            // Act
            _sessionStateContainer.Cuesheet = newCuesheet;

            // Assert
            Assert.IsTrue(eventTriggered);
        }

        [TestMethod]
        public async Task Cuesheet_SetNewValueInImportView_ShouldNotTriggerActiveCuesheetChangedEventAsync()
        {
            // Arrange
            var newCuesheet = new Cuesheet();
            var viewOptions = new ViewOptions()
            {
                ActiveTab = ViewMode.ImportView
            };
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            await _sessionStateContainer.InitializeAsync();
            bool eventTriggered = false;
            _sessionStateContainer.ActiveCuesheetChanged += (sender, args) => eventTriggered = true;

            // Act
            _sessionStateContainer.Cuesheet = newCuesheet;

            // Assert
            Assert.IsFalse(eventTriggered);
        }

        [TestMethod]
        public async Task ImportCuesheet_SetNewValueInImportView_ShouldTriggerActiveCuesheetChangedEventAsync()
        {
            // Arrange
            var newImportCuesheet = new Cuesheet();
            var viewOptions = new ViewOptions()
            {
                ActiveTab = ViewMode.ImportView
            };
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            await _sessionStateContainer.InitializeAsync();
            bool eventTriggered = false;
            _sessionStateContainer.ActiveCuesheetChanged += (sender, args) => eventTriggered = true;

            // Act
            _sessionStateContainer.ImportCuesheet = newImportCuesheet;

            // Assert
            Assert.IsTrue(eventTriggered);
        }

        [TestMethod]
        public async Task ImportCuesheet_SetNewValueInDetailView_ShouldTriggerActiveCuesheetChangedEventAsync()
        {
            // Arrange
            var newImportCuesheet = new Cuesheet();
            var viewOptions = new ViewOptions();
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            await _sessionStateContainer.InitializeAsync();
            bool eventTriggered = false;
            _sessionStateContainer.ActiveCuesheetChanged += (sender, args) => eventTriggered = true;

            // Act
            _sessionStateContainer.ImportCuesheet = newImportCuesheet;

            // Assert
            Assert.IsFalse(eventTriggered);
        }

        [TestMethod]
        public async Task ResetImport_InitializedService_ShouldClearPropertiesAsync()
        {
            // Arrange
            var viewOptions = new ViewOptions();
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            await _sessionStateContainer.InitializeAsync();
            _sessionStateContainer.Importfile = Mock.Of<IImportfile>();
            _sessionStateContainer.ImportCuesheet = new Cuesheet();

            // Act
            _sessionStateContainer.ResetImport();

            // Assert
            Assert.IsNull(_sessionStateContainer.Importfile);
            Assert.IsEmpty(_sessionStateContainer.ImportAudiofiles);
            Assert.IsNull(_sessionStateContainer.ImportCuesheet);
        }

        [TestMethod]
        public async Task InitializeAsync_NotInitialized_ShouldInitializeAsync()
        {
            // Arrange
            var viewOptions = new ViewOptions();
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            // Act
            await _sessionStateContainer.InitializeAsync();
            // Assert
            _localStorageOptionsProvider.Verify(x => x.GetOptionsAsync<ViewOptions>(), Times.Once);
        }

        [TestMethod]
        public async Task InitializeAsync_Initialized_ShouldNotInitializeAsync()
        {
            // Arrange
            var viewOptions = new ViewOptions();
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            await _sessionStateContainer.InitializeAsync();
            // Act
            await _sessionStateContainer.InitializeAsync();
            // Assert
            _localStorageOptionsProvider.Verify(x => x.GetOptionsAsync<ViewOptions>(), Times.Once);
        }

        [TestMethod]
        public async Task ActiveCuesheet_Initialized_ReturnsDetailViewCuesheet()
        {
            // Arrange
            var viewOptions = new ViewOptions();
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            await _sessionStateContainer.InitializeAsync();
            // Act
            var cuesheet = _sessionStateContainer.ActiveCuesheet;
            // Assert
            Assert.AreEqual(_sessionStateContainer.Cuesheet, cuesheet);
        }

        [TestMethod]
        public async Task ActiveCuesheet_SwitchViewSignalesUpdate_ReturnsImportViewCuesheet()
        {
            // Arrange
            var viewOptions = new ViewOptions();
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            await _sessionStateContainer.InitializeAsync();
            viewOptions.ActiveTab = ViewMode.ImportView;

            _localStorageOptionsProvider.Setup(x => x.SaveOptionsAsync(viewOptions)).Callback(() =>
            {
                _localStorageOptionsProvider.Raise(x => x.OptionSaved += null, _localStorageOptionsProvider.Object, viewOptions);
            });
            
            Boolean activeCuesheetChangedEventTriggered = false;
            _sessionStateContainer.ActiveCuesheetChanged += delegate
            {
                activeCuesheetChangedEventTriggered = true;
            };
            _sessionStateContainer.Cuesheet.Title = "Cuesheet";
            _sessionStateContainer.ImportCuesheet = new()
            {
                Title = "ImportCuesheet"
            };
            await _localStorageOptionsProvider.Object.SaveOptionsAsync(viewOptions);
            // Act
            var cuesheet = _sessionStateContainer.ActiveCuesheet;
            // Assert
            Assert.AreEqual(_sessionStateContainer.ImportCuesheet, cuesheet);
            Assert.IsTrue(activeCuesheetChangedEventTriggered);
        }

        [TestMethod]
        public async Task ActiveCuesheet_SaveOptionsSetsActiveCuesheetOnce_ReturnsDetailViewCuesheet()
        {
            // Arrange
            var viewOptions = new ViewOptions();
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            await _sessionStateContainer.InitializeAsync();
            _localStorageOptionsProvider.Setup(x => x.SaveOptionsAsync(viewOptions)).Callback(() =>
            {
                _localStorageOptionsProvider.Raise(x => x.OptionSaved += null, _localStorageOptionsProvider.Object, viewOptions);
            });
            _sessionStateContainer.Cuesheet.Title = "Cuesheet";
            _sessionStateContainer.ImportCuesheet = new()
            {
                Title = "ImportCuesheet"
            };
            viewOptions.ActiveTab = ViewMode.ImportView;
            await _localStorageOptionsProvider.Object.SaveOptionsAsync(viewOptions);
            Boolean activeCuesheetChangedEventTriggered = false;
            _sessionStateContainer.ActiveCuesheetChanged += delegate
            {
                activeCuesheetChangedEventTriggered = true;
            };
            await _localStorageOptionsProvider.Object.SaveOptionsAsync(viewOptions);
            // Act
            var cuesheet = _sessionStateContainer.ActiveCuesheet;
            // Assert
            Assert.AreEqual(_sessionStateContainer.ImportCuesheet, cuesheet);
            Assert.IsFalse(activeCuesheetChangedEventTriggered);
        }

        [TestMethod]
        public async Task ActiveCuesheet_InitializedWithImportView_ReturnsDetailViewCuesheet()
        {
            // Arrange
            var viewOptions = new ViewOptions()
            {
                ActiveTab = ViewMode.ImportView
            };
            _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
            await _sessionStateContainer.InitializeAsync();
            // Act
            var result = _sessionStateContainer.ActiveCuesheet;
            // Assert
            Assert.AreEqual(_sessionStateContainer.ImportCuesheet, result);
        }

        [TestMethod]
        public void ActiveCuesheet_NotInitialized_ReturnsNull()
        {
            // Arrange
            // Act
            var cuesheet = _sessionStateContainer.ActiveCuesheet;
            // Assert
            Assert.IsNull(cuesheet);
        }
    }
}