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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditor.Model.Reflection;
using AudioCuesheetEditorTests.Utility;

namespace AudioCuesheetEditor.Model.Entity.Tests
{
    public class ValidateableTestClass : Validateable
    {
        private String testProperty;
        public String TestProperty
        {
            get { return testProperty; }
            set { testProperty = value; OnValidateablePropertyChanged(); }
        }
        protected override void Validate()
        {
            if (String.IsNullOrEmpty(TestProperty))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(TestProperty)), ValidationErrorType.Warning, "Testmessage"));
            }
        }
    }
    [TestClass()]
    public class ValidateableTests
    {
        [TestMethod()]
        public void GetValidationErrorsTest()
        {
            var testObject = new ValidateableTestClass
            {
                TestProperty = String.Empty
            };
            var testhelper = new TestHelper();
            Assert.IsNull(testObject.GetValidationErrors(testhelper.Localizer, validationErrorFilterType: ValidationErrorFilterType.ErrorOnly));
            Assert.IsTrue(testObject.ValidationErrors.Count > 0);
        }
    }
}