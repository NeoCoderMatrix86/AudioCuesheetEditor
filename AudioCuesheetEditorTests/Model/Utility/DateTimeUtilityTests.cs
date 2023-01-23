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
using System;

namespace AudioCuesheetEditor.Model.Utility.Tests
{
    [TestClass()]
    public class DateTimeUtilityTests
    {
        [TestMethod()]
        public void ParseTimeSpanTest()
        {
            var utility = new DateTimeUtility(new TimeSpanFormat());
            var timespan = utility.ParseTimeSpan("01:23:45");
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(1, 23, 45), timespan);
            var format = new TimeSpanFormat() { Scheme = "(?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})" };
            utility = new DateTimeUtility(format);
            timespan = utility.ParseTimeSpan("3:12");
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(0, 3, 12), timespan);
            timespan = utility.ParseTimeSpan("63:12");
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(1, 3, 12), timespan);
            format.Scheme = "(?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})";
            timespan = utility.ParseTimeSpan("23:12");
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(23, 12, 0), timespan);
            format.Scheme = "(?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})";
            timespan = utility.ParseTimeSpan("23:45:56");
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(0, 23, 45, 56), timespan);
            format.Scheme = "(?'Days'\\d{1,})[.](?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})";
            timespan = utility.ParseTimeSpan("2.23:45:56");
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(2, 23, 45, 56), timespan);
            format.Scheme = "(?'Hours'\\d{1,})[:](?'TimeSpanFormat.Minutes'\\d{1,})[:](?'TimeSpanFormat.Seconds'\\d{1,})[.](?'TimeSpanFormat.Milliseconds'\\d{1,})";
            timespan = utility.ParseTimeSpan("23:45:56.599");
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(0, 23, 45, 56, 599), timespan);
            timespan = utility.ParseTimeSpan("1.2e:45:87.h3a");
            Assert.IsNull(timespan);
            timespan = utility.ParseTimeSpan("Test");
            Assert.IsNull(timespan);
            format.Scheme = "this is a test";
            timespan = utility.ParseTimeSpan("Test");
            Assert.IsNull(timespan);
        }
    }
}