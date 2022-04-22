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
using AudioCuesheetEditor.Extensions;
using AudioCuesheetEditor.Model.AudioCuesheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO
{
    public class Projectfile
    {
        public const String MimeType = "text/*";
        public const String FileExtension = ".ace";

        public static readonly String DefaultFileName = "Project.ace";

        public static readonly JsonSerializerOptions Options = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public static Cuesheet? ImportFile(byte[] fileContent)
        {
            var json = Encoding.UTF8.GetString(fileContent);
            return JsonSerializer.Deserialize<Cuesheet>(json, Options);
        }

        public Projectfile(Cuesheet cuesheet)
        {
            if (cuesheet == null)
            {
                throw new ArgumentNullException(nameof(cuesheet));
            }
            Cuesheet = cuesheet;
        }

        public Cuesheet Cuesheet { get; private set; }

        /// <summary>
        /// Generate a ProjectFile
        /// </summary>
        /// <returns>Byte array with project file content</returns>
        public byte[] GenerateFile()
        {
            var json = JsonSerializer.Serialize<Cuesheet>(Cuesheet, Options);    
            return Encoding.UTF8.GetBytes(json);        
        }
    }
}
