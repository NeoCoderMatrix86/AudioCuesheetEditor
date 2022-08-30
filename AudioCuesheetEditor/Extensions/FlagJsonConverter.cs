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
using AudioCuesheetEditor.Model.AudioCuesheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Extensions
{
    public class FlagJsonConverter : JsonConverter<Flag>
    {
        public override Flag? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var json = reader.GetString();
            if (string.IsNullOrEmpty(json) == false)
            {
                return Flag.AvailableFlags.Single(x => x.CuesheetLabel.Equals(json));
            }
            else
            {
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, Flag value, JsonSerializerOptions options)
        {
            if ((value != null) && (String.IsNullOrEmpty(value.CuesheetLabel) == false))
            {
                writer.WriteStringValue(value.CuesheetLabel);
            }
        }
    }
}
