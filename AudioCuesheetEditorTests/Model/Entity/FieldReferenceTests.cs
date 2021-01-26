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
using AudioCuesheetEditor.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioCuesheetEditor.Model.Reflection.Tests
{
    [TestClass()]
    public class FieldReferenceTests
    {
        public class FieldReferenceTestClass
        {
            public String Property1 { get; set; }
            public int Property2 { get; set; }
        }

        [TestMethod()]
        public void CreateTest()
        {
            var testObject = new FieldReferenceTestClass();
            var fieldReference = FieldReference.Create(testObject, nameof(FieldReferenceTestClass.Property1));
            Assert.AreEqual(fieldReference.CompleteName, "FieldReferenceTestClass.Property1");
            fieldReference = FieldReference.Create(testObject, nameof(FieldReferenceTestClass.Property2));
            Assert.AreEqual(fieldReference.CompleteName, "FieldReferenceTestClass.Property2");
        }
    }
}