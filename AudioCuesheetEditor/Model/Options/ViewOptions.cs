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

namespace AudioCuesheetEditor.Model.Options
{
    public enum ViewMode
    {
        DetailView = 0,
        RecordView = 1,
        ImportView = 2
    }
    public enum ImportTab
    {
        Edit = 0,
        Analyze = 1
    }
    public class ViewOptions : IOptions
    {
        [JsonIgnore]
        public ViewMode ActiveTab { get; set; }
        public String? ActiveTabName
        {
            get => Enum.GetName(ActiveTab);
            set
            {
                if (value != null)
                {
                    ActiveTab = Enum.Parse<ViewMode>(value);
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
        }

        [JsonIgnore]
        public ImportTab ActiveImportTab { get; set; }

        public String? ActiveImportTabName
        {
            get => Enum.GetName(ActiveImportTab);
            set
            {
                if (value != null)
                {
                    ActiveImportTab = Enum.Parse<ImportTab>(value);
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
        }
    }
}
