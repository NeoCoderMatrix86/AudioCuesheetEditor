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
using Blazorise.Localization;

namespace AudioCuesheetEditor.Model.Entity
{
    public class ValidationMessage
    {
        public ValidationMessage(String message, params object[]? args)
        {
            if (String.IsNullOrEmpty(message) == true)
            {
                throw new ArgumentNullException(nameof(message));
            }
            Message = message;
            Parameter = args;
        }
        public String Message { get; private set; }
        public object[]? Parameter { get; private set; }
        
        public String GetMessageLocalized(ITextLocalizer<ValidationMessage> localizer)
        {
            object[]? arguments = null;
            if (Parameter != null)
            {
                arguments = new object[Parameter.Length];
                Parameter.CopyTo(arguments, 0);
            }
            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    if ((arguments[i] != null) && (arguments[i].GetType() == typeof(String)))
                    {
                        arguments[i] = localizer[(String)arguments[i]];
                    }
                }
            }
            return localizer[Message, arguments];
        }
    }
}
