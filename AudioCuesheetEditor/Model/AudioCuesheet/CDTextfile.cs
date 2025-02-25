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

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class CDTextfile
    {
        private string name;
        public CDTextfile(String name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            this.name = name;
        }
        public String Name 
        {
            get => name;
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }
                var extension = Path.GetExtension(value);
                if (extension.Equals(FileExtensions.CDTextfile, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    value = $"{value}{FileExtensions.CDTextfile}";
                }
                name = value;
            }
        }
    }
}
