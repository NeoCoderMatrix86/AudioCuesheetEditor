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
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Services.IO;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass()]
    public class FileInputManagerTests
    {
        private readonly FileInputManager _service;
        private readonly Mock<IJSRuntime> _jsRuntimeMock = new();

        public FileInputManagerTests()
        {
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            _service = new FileInputManager(_jsRuntimeMock.Object, loggerMock.Object);
        }

        [TestMethod()]
        public void CheckFileMimeType_ReturnsTrue_WhenContentTypeDoesNotMatchButExtension()
        {
            // Arrange
            var fileName = "test.mp3";
            var contentType = "audio/wav";

            // Act
            var result = _service.CheckFileMimeType(contentType, fileName, "audio/mpeg", [".mp3"]);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void CheckFileMimeType_ReturnsTrue_WhenContentTypeDoesMatchButNotExtension()
        {
            // Arrange
            var fileName = "test.mpeg";
            var contentType = "audio/mpeg";

            // Act
            var result = _service.CheckFileMimeType(contentType, fileName, "audio/mpeg", [".mp3", ".txt"]);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void CheckFileMimeType_ReturnsFalse_WhenExtensionDoesNotMatchAndContentTypeIsEmpty()
        {
            // Arrange
            var fileName = "test.flac";
            var contentType = string.Empty;

            // Act
            var result = _service.CheckFileMimeType(contentType, fileName, "audio/flac", [".mp3"]);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void CheckFileMimeType_ReturnsTrue_WhenContentTypeAndExtensionMatch()
        {
            // Arrange
            var fileName = "test.wav";
            var contentType = "audio/wave";

            // Act
            var result = _service.CheckFileMimeType(contentType, fileName, "audio/wave", [".wav"]);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void CheckFileMimeType_ReturnsTrue_WhenContentMainTypeMatch()
        {
            // Arrange

            var fileName = "history.txt";
            var contentType = "text/plain";

            // Act
            var result = _service.CheckFileMimeType(contentType, fileName, "text/*", [".txt", ".text"]);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidAudiofile_ReturnsTrue_WithValidAudiocodec()
        {
            // Arrange
            var fileName = "test.wav";
            var contentType = "audio/wav";

            // Act
            var result = _service.IsValidAudiofile(contentType, fileName);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidAudiofile_ReturnsFalse_WithInvalidAudiocodecAndExtension()
        {
            // Arrange
            var fileName = "test.mock";
            var contentType = "just a fantasy";

            // Act
            var result = _service.IsValidAudiofile(contentType, fileName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void GetAudioCodec_ReturnsAudiocodec_WhenContentTypeMatches()
        {
            // Arrange
            var fileName = "test.wbem";
            var contentType = "audio/webm";

            // Act
            var result = _service.GetAudioCodec(contentType, fileName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(Audiofile.AudioCodecWEBM, result);
        }

        [TestMethod()]
        public void GetAudioCodec_ReturnsAudiocodec_WhenContentTypeAndFileExtensionMatches()
        {
            // Arrange
            var fileName = "test.wbem";
            var contentType = "audio/webm";

            // Act
            var result = _service.GetAudioCodec(contentType, fileName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(Audiofile.AudioCodecWEBM, result);
        }

        [TestMethod()]
        public void GetAudioCodec_ReturnsNull_WhenContentTypeAndFileExtensionNotMatch()
        {
            // Arrange
            var fileName = "test.acx";
            var contentType = "fantasy stuff";

            // Act
            var result = _service.GetAudioCodec(contentType, fileName);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod()]
        public void IsValidForImportView_ReturnsTrue_WhenFileIsHtml()
        {
            // Arrange
            var fileName = "test.html";
            var contentType = "text/html";

            // Act
            var result = _service.IsValidForImportView(contentType, fileName);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidForImportView_ReturnsFalse_WhenFileIsBinary()
        {
            // Arrange
            var fileName = "test.dat";
            var contentType = "application/octet-stream";

            // Act
            var result = _service.IsValidForImportView(contentType, fileName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CreateFileUploadsAsync_ReturnsFileUploads_WhenFileHasTextContentAsync()
        {
            // Arrange
            var firstFile = CreateBrowserFile("Test.txt", "text/plain", "Just a test!");
            var secondFile = CreateBrowserFile("Test.mp3", "audio/mpeg");
            var fileInputId = nameof(CreateFileUploadsAsync_ReturnsFileUploads_WhenFileHasTextContentAsync);
            IReadOnlyList<IBrowserFile> browserfiles = [
                firstFile,
                secondFile
            ];
            var objectUrl = "Some object url!";
            _jsRuntimeMock.Setup(js => js.InvokeAsync<String>(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(objectUrl);
            // Act
            var result = await _service.CreateFileUploadsAsync(browserfiles, fileInputId);
            // Assert
            Assert.HasCount(2, result);
            Assert.AreEqual(firstFile.Name, result.First().Name);
            Assert.AreEqual(firstFile.ContentType, result.First().ContentType);
            Assert.AreEqual("Just a test!", result.First().Content);
            Assert.AreEqual(secondFile.Name, result.Last().Name);
            Assert.AreEqual(secondFile.ContentType, result.Last().ContentType);
            Assert.IsNull(result.Last().Content);
            Assert.AreEqual(objectUrl, result.Last().ObjectUrl);
        }

        [TestMethod]
        public async Task CreateFileUploadsAsync_ReturnsEmpty_WhenFilesHaveInvalidMimeTypeAsync()
        {
            // Arrange
            var firstFile = CreateBrowserFile("Test.bin", "binary", "Just a test!");
            var secondFile = CreateBrowserFile("Test.bin", "octet/stream");
            IReadOnlyList<IBrowserFile> browserfiles = [
                firstFile,
                secondFile
            ];
            // Act
            var result = await _service.CreateFileUploadsAsync(browserfiles);
            // Assert
            Assert.HasCount(0, result);
        }

        static IBrowserFile CreateBrowserFile(string name, string contentType, string? content = null)
        {
            var fileMock = new Mock<IBrowserFile>();
            fileMock.Setup(f => f.Name).Returns(name);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            if (content != null)
            { 
                fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            }
            return fileMock.Object;
        }
    }
}