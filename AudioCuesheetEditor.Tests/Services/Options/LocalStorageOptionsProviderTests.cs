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
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Services.IO;
using AudioCuesheetEditor.Services.Options;
using AudioCuesheetEditor.Services.UI;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Services.Options
{
    [TestClass()]
    public class LocalStorageOptionsProviderTests
    {
        private readonly LocalStorageOptionsProvider _service;
        private readonly Mock<IJSRuntime> _jsRuntime = new();
        private readonly Mock<IStringLocalizer<ValidationMessage>> _localizer = new();

        public LocalStorageOptionsProviderTests()
        {
            _service = new(_jsRuntime.Object, _localizer.Object);
        }

        [TestMethod]
        public async Task SaveOptionsAsync_NonValidatableOptions_ReturnsSuccessAsync()
        {
            // Arrange
            var options = new ExportOptions();
            Boolean eventRaised = false;
            _service.OptionSaved += delegate
            {
                eventRaised = true;
            };
            // Act
            var result = await _service.SaveOptionsAsync(options);
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public async Task SaveOptionsAsync_ValidOptions_ReturnsSuccessAsync()
        {
            // Arrange
            var options = new DownloadOptions()
            {
                ProjectFilename = "Test.ace",
                CuesheetFilename = "Test.cue"
            };
            Boolean eventRaised = false;
            _service.OptionSaved += delegate
            {
                eventRaised = true;
            };
            // Act
            var result = await _service.SaveOptionsAsync(options);
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public async Task SaveOptionsAsync_InvalidOptions_ReturnsFailureAsync()
        {
            // Arrange
            var options = new DownloadOptions()
            {
                CuesheetFilename = null,
                ProjectFilename = null
            };
            Boolean eventRaised = false;
            _service.OptionSaved += delegate
            {
                eventRaised = true;
            };
            // Act
            var result = await _service.SaveOptionsAsync(options);
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(eventRaised);
        }
    }
}
