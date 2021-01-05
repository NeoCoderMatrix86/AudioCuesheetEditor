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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioCuesheetEditor.Model.IO.Tests
{
    [TestClass()]
    public class AudioFileTests
    {
        [TestMethod()]
        public void AudioFileTest()
        {
            var audioFile = new AudioFile("test.mp3");
            Assert.IsNotNull(audioFile.FileName);
            Assert.AreEqual(audioFile.AudioFileType, "MP3");
            audioFile = new AudioFile("Test");
            Assert.AreEqual(audioFile.AudioFileType, String.Empty);
            Assert.IsNotNull(audioFile.FileName);
            audioFile = new AudioFile("test.ogg", "TestobjectURL", "contentType");
            Assert.IsNotNull(audioFile.FileName);
            Assert.AreEqual(audioFile.AudioFileType, "OGG");
            Assert.IsNotNull(audioFile.ObjectURL);
            Assert.IsFalse(audioFile.PlaybackPossible);
        }
    }
}