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
            Assert.AreEqual(fieldReference.DisplayName, "FieldReferenceTestClass.Property1");
            fieldReference = FieldReference.Create(testObject, nameof(FieldReferenceTestClass.Property2));
            Assert.AreEqual(fieldReference.DisplayName, "FieldReferenceTestClass.Property2");
        }
    }
}