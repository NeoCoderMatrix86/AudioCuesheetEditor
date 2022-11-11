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
using AudioCuesheetEditor.Model.Entity;

namespace AudioCuesheetEditor.Model.UI
{
    public class BootstrapUtility
    {
        public static String GetCSSClassAlert(Validateable? validateable, String property)
        {
            String cssClass = String.Empty;
            if (validateable != null)
            {
                var validationErrors = validateable.GetValidationErrorsFiltered(property);
                if (validationErrors.Count >= 1)
                {
                    if (validationErrors.Count > 1)
                    {
                        if (validateable.GetValidationErrorsFiltered(property, ValidationErrorFilterType.ErrorOnly).Count >= 1)
                        {
                            cssClass = "alert-danger";
                        }
                        else
                        {
                            cssClass = "alert-warning";
                        }
                    }
                    else
                    {
                        if (validationErrors.First().Type == ValidationErrorType.Error)
                        {
                            cssClass = "alert-danger";
                        }
                        if (validationErrors.First().Type == ValidationErrorType.Warning)
                        {
                            cssClass = "alert-warning";
                        }
                    }
                }
            }
            return cssClass;
        }
    }
}
