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
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Shared.ResourceFiles;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.Options
{
    public class ApplicationOptions
    {
        public ApplicationOptions()
        {
            SetDefaultValues();
        }

        public void SetDefaultValues()
        {
            //TODO Declare defaults
            if (String.IsNullOrEmpty(CuesheetFileName) == true)
            {
                CuesheetFileName = CuesheetFile.DefaultFileName;
            }
            if (String.IsNullOrEmpty(CultureName) == true)
            {
                CultureName = CultureInfo.CurrentCulture.DisplayName;
            }
        }

        public String CuesheetFileName { get; set; }
        public String CultureName { get; set; }
        [JsonIgnore]
        public CultureInfo Culture
        {
            get
            {
                if (String.IsNullOrEmpty(CultureName) == false)
                {
                    return new CultureInfo(CultureName);
                }
                else
                {
                    return CultureInfo.CurrentCulture;
                }
            }
        }
    }
}
