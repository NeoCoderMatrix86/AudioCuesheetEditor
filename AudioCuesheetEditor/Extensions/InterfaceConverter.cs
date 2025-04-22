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
using System.Text.Json.Serialization;
using System.Text.Json;

namespace AudioCuesheetEditor.Extensions
{
    public class InterfaceConverter<TInterface, TImplementation> : JsonConverter<TInterface> where TImplementation : TInterface
    {
        public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TImplementation>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                JsonSerializer.Serialize(writer, (TImplementation)value, options);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
