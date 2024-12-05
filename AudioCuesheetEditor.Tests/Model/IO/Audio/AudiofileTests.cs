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
using System;
using System.Collections.Generic;
using System.Text;
using AudioCuesheetEditor.Model.IO.Audio;
using System.Linq;

namespace AudioCuesheetEditor.Model.IO.Audio.Tests
{
    [TestClass()]
    public class AudiofileTests
    {
        [TestMethod()]
        public void AudioFileTest()
        {
            var audioFile = new Audiofile("test.mp3");
            Assert.IsNull(audioFile.ContentStream);
            Assert.IsFalse(audioFile.IsContentStreamLoaded);
            Assert.IsNotNull(audioFile.Name);
            Assert.AreEqual(audioFile.AudioFileType, "MP3");
            audioFile = new Audiofile("Test");
            Assert.AreEqual(audioFile.AudioFileType, String.Empty);
            Assert.IsNotNull(audioFile.Name);
            var codec = Audiofile.AudioCodecs.Single(x => x.FileExtension == ".ogg");
            var httpClient = new System.Net.Http.HttpClient();
            audioFile = new Audiofile("test", "TestobjectURL", codec, httpClient);
            Assert.IsNotNull(audioFile.Name);
            Assert.AreEqual("test.ogg", audioFile.Name);
            Assert.AreEqual(audioFile.AudioFileType, "OGG");
            Assert.IsNotNull(audioFile.ObjectURL);
            Assert.IsTrue(audioFile.PlaybackPossible);
            codec = Audiofile.AudioCodecs.Single(x => x.FileExtension == ".mp3");
            var audioFile2 = new Audiofile(audioFile.Name, "TestObjectURL2", codec, httpClient);
            Assert.AreEqual("test.mp3", audioFile2.Name);
        }
    }
}