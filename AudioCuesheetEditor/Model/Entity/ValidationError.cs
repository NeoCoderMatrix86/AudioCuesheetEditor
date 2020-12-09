using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.Entity
{
    public enum ValidationErrorType
    {
        Warning,
        Error
    }
    public class ValidationError
    {
        public String Message { get; private set; }
        public ValidationErrorType Type { get; private set; }

        public ValidationError(String message, ValidationErrorType validationErrorType)
        {
            Message = message;
            Type = validationErrorType;
        }
    }
}
