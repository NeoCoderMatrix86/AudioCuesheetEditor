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
        }
    }
}