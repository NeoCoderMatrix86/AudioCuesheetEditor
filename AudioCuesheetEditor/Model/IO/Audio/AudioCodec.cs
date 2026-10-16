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

namespace AudioCuesheetEditor.Model.IO.Audio;

public class AudioCodec
{
    public string MimeType { get; private set; }
    public string FileExtension { get; private set; }
    public string Name { get; private set; }
    
    public AudioCodec(string mimeType, string fileExtension, string name)
    {
        if (string.IsNullOrEmpty(mimeType))
        {
            throw new ArgumentNullException(nameof(mimeType));
        }
        if (string.IsNullOrEmpty(fileExtension))
        {
            throw new ArgumentNullException(nameof(fileExtension));
        }
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        MimeType = mimeType;
        FileExtension = fileExtension;
        Name = name;
    }
}
