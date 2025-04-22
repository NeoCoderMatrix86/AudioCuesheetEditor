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
    public class TimeSpanFormatTests
    {
        [TestMethod()]
        public void ParseTimeSpan_WithScheme_ReturnsTimeSpan()
        {
            //Arrange
            var format = new TimeSpanFormat()
            {
                Scheme = $"{TimeSpanFormat.Minutes}:{TimeSpanFormat.Seconds}"
            };
            //Act
            var timespan = format.ParseTimeSpan("63:12");
            //Assert
            Assert.AreEqual(new TimeSpan(1,3,12), timespan);
        }
    }
}