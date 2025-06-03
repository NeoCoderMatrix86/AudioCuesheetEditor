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
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Services.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Services.UI
{
    [TestClass()]
    public class ApplicationOptionsTimeSpanParserTests
    {
        [TestMethod]
        public async Task TimespanTextChanged_ValidInput_SetsPropertyCorrectly()
        {
            // Arrange
            var options = new ApplicationOptions()
            {
                TimeSpanFormat = new()
                {
                    Scheme = "Minutes:Seconds"
                }
            };
            var mockOptionsProvider = new Mock<ILocalStorageOptionsProvider>();
            mockOptionsProvider
                .Setup(p => p.GetOptionsAsync<ApplicationOptions>())
                .ReturnsAsync(options);
            var parser = new ApplicationOptionsTimeSpanParser(mockOptionsProvider.Object);
            await Task.Delay(50);
            var track = new Track();
            // Act
            await parser.TimespanTextChanged(track, x => x.Begin, "92:12");
            // Assert
            Assert.AreEqual(new TimeSpan(1, 32, 12), track.Begin);
        }

        [TestMethod]
        public async Task TimespanTextChanged_InvalidInput_SetsNull()
        {
            var options = new ApplicationOptions()
            {
                TimeSpanFormat = new()
                {
                    Scheme = "Minutes:Seconds"
                }
            };
            var mockOptionsProvider = new Mock<ILocalStorageOptionsProvider>();
            mockOptionsProvider
                .Setup(p => p.GetOptionsAsync<ApplicationOptions>())
                .ReturnsAsync(options);
            var parser = new ApplicationOptionsTimeSpanParser(mockOptionsProvider.Object);
            await Task.Delay(50);
            var track = new Track();
            // Act
            await parser.TimespanTextChanged(track, x => x.End, "not a time");
            // Assert
            Assert.IsNull(track.End);
        }

        [TestMethod()]
        public async Task GetTimespanFormatted_ValidFormat_ReturnsCorrectString()
        {
            // Arrange
            var options = new ApplicationOptions()
            {
                DisplayTimeSpanFormat = @"hh\:mm\:ss"
            };
            var mockOptionsProvider = new Mock<ILocalStorageOptionsProvider>();
            mockOptionsProvider
                .Setup(p => p.GetOptionsAsync<ApplicationOptions>())
                .ReturnsAsync(options);
            var parser = new ApplicationOptionsTimeSpanParser(mockOptionsProvider.Object);
            await Task.Delay(50);
            // Act
            var result = parser.GetTimespanFormatted(new TimeSpan(0, 1, 30, 27, 200, 103));
            // Assert
            Assert.AreEqual("01:30:27", result);
        }

        [TestMethod()]
        public async Task GetTimespanFormatted_InvalidFormat_FallbackToDefault()
        {
            // Arrange
            var options = new ApplicationOptions()
            {
                DisplayTimeSpanFormat = "INVALID_FORMAT"
            };
            var mockOptionsProvider = new Mock<ILocalStorageOptionsProvider>();
            mockOptionsProvider
                .Setup(p => p.GetOptionsAsync<ApplicationOptions>())
                .ReturnsAsync(options);
            var parser = new ApplicationOptionsTimeSpanParser(mockOptionsProvider.Object);
            await Task.Delay(50);
            // Act
            var result = parser.GetTimespanFormatted(new TimeSpan(0, 1, 30, 27, 200, 103));
            // Assert
            Assert.AreEqual("01:30:27.2001030", result);
        }
    }
}