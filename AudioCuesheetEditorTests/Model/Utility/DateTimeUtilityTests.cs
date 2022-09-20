using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditorTests.Utility;

namespace AudioCuesheetEditor.Model.Utility.Tests
{
    [TestClass()]
    public class DateTimeUtilityTests
    {
        [TestMethod()]
        public void ParseTimeSpanTest()
        {
            var timespan = DateTimeUtility.ParseTimeSpan("01:23:45");
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(1, 23, 45), timespan);
            var format = new Timespanformat() { Scheme = "(?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})" };
            timespan = DateTimeUtility.ParseTimeSpan("3:12", format);
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(0, 3, 12), timespan);
            timespan = DateTimeUtility.ParseTimeSpan("63:12", format);
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(1, 3, 12), timespan);
            format.Scheme = "(?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})";
            timespan = DateTimeUtility.ParseTimeSpan("23:12", format);
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(23, 12, 0), timespan);
            format.Scheme = "(?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})";
            timespan = DateTimeUtility.ParseTimeSpan("23:45:56", format);
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(0, 23, 45, 56), timespan);
            format.Scheme = "(?'Days'\\d{1,})[.](?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})";
            timespan = DateTimeUtility.ParseTimeSpan("2.23:45:56", format);
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(2, 23, 45, 56), timespan);
            format.Scheme = "(?'Hours'\\d{1,})[:](?'Minutes'\\d{1,})[:](?'Seconds'\\d{1,})[.](?'Milliseconds'\\d{1,})";
            timespan = DateTimeUtility.ParseTimeSpan("23:45:56.599", format);
            Assert.IsNotNull(timespan);
            Assert.AreEqual(new TimeSpan(0, 23, 45, 56, 599), timespan);
            timespan = DateTimeUtility.ParseTimeSpan("1.2e:45:87.h3a", format);
            Assert.IsNull(timespan);
            timespan = DateTimeUtility.ParseTimeSpan("Test", format);
            Assert.IsNull(timespan);
            format.Scheme = "this is a test";
            timespan = DateTimeUtility.ParseTimeSpan("Test", format);
            Assert.IsNull(timespan);
        }
    }
}