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
using AudioCuesheetEditor.Model.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AudioCuesheetEditor.Tests.Model.Utility
{
    [TestClass()]
    public class TimeSpanUtilityTests
    {
        [TestMethod()]
        public void ParseTimeSpan_Valid_HoursMinutesSeconds()
        {
            // Arrange
            string input = "01:23:45";

            // Act
            var timespan = TimeSpanUtility.ParseTimeSpan(input);

            // Assert
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(1, 23, 45), timespan);
        }

        [TestMethod()]
        public void ParseTimeSpan_Valid_MinutesSeconds()
        {
            // Arrange
            string input = "3:12";
            var format = new TimeSpanFormat() { Scheme = "(?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})" };

            // Act
            var timespan = TimeSpanUtility.ParseTimeSpan(input, format);

            // Assert
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(0, 3, 12), timespan);
        }

        [TestMethod()]
        public void ParseTimeSpan_Valid_OverflowMinutes()
        {
            // Arrange
            string input = "63:12";
            var format = new TimeSpanFormat() { Scheme = "(?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})" };

            // Act
            var timespan = TimeSpanUtility.ParseTimeSpan(input, format);

            // Assert
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(1, 3, 12), timespan);
        }

        [TestMethod()]
        public void ParseTimeSpan_Valid_HoursMinutes()
        {
            // Arrange
            string input = "23:12";
            var format = new TimeSpanFormat() { Scheme = "(?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})" };

            // Act
            var timespan = TimeSpanUtility.ParseTimeSpan(input, format);

            // Assert
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(23, 12, 0), timespan);
        }

        [TestMethod()]
        public void ParseTimeSpan_Valid_FullFormat()
        {
            // Arrange
            string input = "23:45:56";
            var format = new TimeSpanFormat() { Scheme = "(?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})" };

            // Act
            var timespan = TimeSpanUtility.ParseTimeSpan(input, format);

            // Assert
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(0, 23, 45, 56), timespan);
        }

        [TestMethod()]
        public void ParseTimeSpan_Valid_WithDays()
        {
            // Arrange
            string input = "2.23:45:56";
            var format = new TimeSpanFormat() { Scheme = "(?'Days'\\d{1,})[.](?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})" };

            // Act
            var timespan = TimeSpanUtility.ParseTimeSpan(input, format);

            // Assert
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(2, 23, 45, 56), timespan);
        }

        [TestMethod()]
        public void ParseTimeSpan_Valid_WithMilliseconds()
        {
            // Arrange
            string input = "23:45:56.599";
            var format = new TimeSpanFormat() { Scheme = "(?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})[.](?'Milliseconds'\\d{1,})" };

            // Act
            var timespan = TimeSpanUtility.ParseTimeSpan(input, format);

            // Assert
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(0, 23, 45, 56, 599), timespan);
        }

        [TestMethod()]
        public void ParseTimeSpan_Invalid_Format()
        {
            // Arrange
            string input = "1.2e:45:87.h3a";
            var format = new TimeSpanFormat() { Scheme = "(?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})[.](?'Milliseconds'\\d{1,})" };

            // Act
            var timespan = TimeSpanUtility.ParseTimeSpan(input, format);

            // Assert
            Assert.IsNull(timespan);
        }

        [TestMethod()]
        public void ParseTimeSpan_Invalid_Text()
        {
            // Arrange
            string input = "Test";
            var format = new TimeSpanFormat() { Scheme = "(?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})[.](?'Milliseconds'\\d{1,})" };

            // Act
            var timespan = TimeSpanUtility.ParseTimeSpan(input, format);

            // Assert
            Assert.IsNull(timespan);
        }

        [TestMethod()]
        public void ParseTimeSpan_Invalid_Scheme()
        {
            // Arrange
            string input = "Test";
            var format = new TimeSpanFormat() { Scheme = "this is a test" };

            // Act
            var timespan = TimeSpanUtility.ParseTimeSpan(input, format);

            // Assert
            Assert.IsNull(timespan);
        }
    }

}