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
using AudioCuesheetEditorTests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace AudioCuesheetEditor.Model.Entity.Tests
{
    public class ValidateableTestClass : Validateable<ValidateableTestClass>
    {
        public String TestProperty { get; set; }
        protected override ValidationResult Validate(string property)
        {
            var result = new ValidationResult() { Status = ValidationStatus.NoValidation };
            List<String>? errors = null;
            switch (property)
            {
                case nameof(TestProperty):
                    if (String.IsNullOrEmpty(TestProperty))
                    {
                        errors ??= new();
                        errors.Add(String.Format("{0} has no value!", nameof(TestProperty)));
                    }
                    else
                    {
                        result.Status = ValidationStatus.Success;
                    }
                    break;
            }
            result.ErrorMessages = errors;
            return result;
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
            Assert.AreEqual(ValidationStatus.Error, testObject.Validate().Status);
            Assert.IsNotNull(testObject.Validate().ErrorMessages);
            Assert.IsTrue(testObject.Validate().ErrorMessages.Count == 1);
        }
    }
}