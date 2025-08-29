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
using AudioCuesheetEditor.Model.IO.Audio;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.IO
{
    public class Projectfile(Cuesheet cuesheet)
    {
        public static readonly String DefaultFilename = "Project.ace";

        public static readonly JsonSerializerOptions Options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters =
            {
                new JsonStringEnumConverter(),
                new InterfaceConverter<IAudiofile, Audiofile>() // Add a custom converter for IAudiofile  
            }
        };

        public static Cuesheet? ImportFile(string fileContent)
        {
            return JsonSerializer.Deserialize<Cuesheet>(fileContent, Options);
        }

        public Cuesheet Cuesheet { get; private set; } = cuesheet;

        /// <summary>
        /// Generate a ProjectFile
        /// </summary>
        /// <returns>Byte array with project file content</returns>
        public byte[] GenerateFile()
        {
            var json = JsonSerializer.Serialize(Cuesheet, Options);
            return Encoding.UTF8.GetBytes(json);        
        }
    }
}
