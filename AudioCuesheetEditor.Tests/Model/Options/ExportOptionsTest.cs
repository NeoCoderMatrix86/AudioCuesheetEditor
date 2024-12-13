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
using AudioCuesheetEditor.Model.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text.Json;

namespace AudioCuesheetEditorTests.Model.Options
{
    [TestClass()]
    public class ExportOptionsTest
    {
        [TestMethod()]
        public void SerializationTest()
        {
            var options = new ExportOptions();
            var serializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var optionsJson = JsonSerializer.Serialize<object>(options, serializerOptions);
            Assert.IsNotNull(optionsJson);
        }

        [TestMethod()]
        public void DeserializationTest()
        {
            var optionsJson = "{\"ExportProfiles\":[{\"Id\":\"ba65845a-f264-4ca1-b342-d99871e961da\",\"Name\":\"YouTube\",\"SchemeHead\":\"%Cuesheet.Artist% - %Cuesheet.Title%\",\"SchemeTracks\":\"%Track.Artist% - %Track.Title% %Track.Begin%\",\"SchemeFooter\":\"\",\"Filename\":\"YouTube.txt\"},{\"Id\":\"325cefed-ccdf-4694-9e0c-3fa28f38e9c6\",\"Name\":\"Mixcloud\",\"SchemeHead\":\"\",\"SchemeTracks\":\"%Track.Artist% - %Track.Title% %Track.Begin%\",\"SchemeFooter\":\"\",\"Filename\":\"Mixcloud.txt\"},{\"Id\":\"17c526a5-eec5-439a-b968-789975003600\",\"Name\":\"CSV Export\",\"SchemeHead\":\"%Cuesheet.Artist%;%Cuesheet.Title%;\",\"SchemeTracks\":\"%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%\",\"SchemeFooter\":\"Exported at %DateTime% using AudioCuesheetEditor (https://neocodermatrix86.github.io/AudioCuesheetEditor/)\",\"Filename\":\"Export.csv\"},{\"Id\":\"e85c8a42-a5a9-4df5-a580-48f0b85a19af\",\"Name\":\"Tracks only\",\"SchemeHead\":\"\",\"SchemeTracks\":\"%Track.Position% - %Track.Artist% - %Track.Title% - %Track.Begin% - %Track.End% - %Track.Length%\",\"SchemeFooter\":\"\",\"Filename\":\"Tracks.txt\"}],\"SelectedProfileId\":\"e85c8a42-a5a9-4df5-a580-48f0b85a19af\"}";
            var options = JsonSerializer.Deserialize<ExportOptions>(optionsJson);
            Assert.IsNotNull(options);
            Assert.AreEqual(options.ExportProfiles.Last(), options.SelectedExportProfile);
        }

        [TestMethod()]
        public void DeserializeBackwardCompabilityTest()
        {
            var optionsJson = "{\"ExportProfiles\":[{\"Name\":\"YouTube\",\"SchemeHead\":\"%Cuesheet.Artist% - %Cuesheet.Title%\",\"SchemeTracks\":\"%Track.Artist% - %Track.Title% %Track.Begin%\",\"SchemeFooter\":\"\",\"Filename\":\"YouTube.txt\"},{\"Name\":\"Mixcloud\",\"SchemeHead\":\"\",\"SchemeTracks\":\"%Track.Artist% - %Track.Title% %Track.Begin%\",\"SchemeFooter\":\"\",\"Filename\":\"Mixcloud.txt\"},{\"Name\":\"CSV Export\",\"SchemeHead\":\"%Cuesheet.Artist%;%Cuesheet.Title%;\",\"SchemeTracks\":\"%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%\",\"SchemeFooter\":\"Exported at %DateTime% using AudioCuesheetEditor (https://neocodermatrix86.github.io/AudioCuesheetEditor/)\",\"Filename\":\"Export.csv\"},{\"Name\":\"Tracks only\",\"SchemeHead\":\"\",\"SchemeTracks\":\"%Track.Position% - %Track.Artist% - %Track.Title% - %Track.Begin% - %Track.End% - %Track.Length%\",\"SchemeFooter\":\"\",\"Filename\":\"Tracks.txt\"}]}";
            var options = JsonSerializer.Deserialize<ExportOptions>(optionsJson);
            Assert.IsNotNull(options);
            Assert.AreEqual(4, options.ExportProfiles.Count);
        }
    }
}
