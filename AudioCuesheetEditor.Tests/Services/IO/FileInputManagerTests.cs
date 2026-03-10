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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Tests.Services.IO
{
    [TestClass()]
    public class FileInputManagerTests
    {
        [TestMethod()]
        public void CheckFileMimeType_ReturnsTrue_WhenContentTypeDoesNotMatchButExtension()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.mp3";
            var contentType = "audio/wav";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.CheckFileMimeType(contentType, fileName, "audio/mpeg", [".mp3"]);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void CheckFileMimeType_ReturnsTrue_WhenContentTypeDoesMatchButNotExtension()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.mpeg";
            var contentType = "audio/mpeg";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.CheckFileMimeType(contentType, fileName, "audio/mpeg", [".mp3", ".txt"]);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void CheckFileMimeType_ReturnsFalse_WhenExtensionDoesNotMatchAndContentTypeIsEmpty()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.flac";
            var contentType = string.Empty;
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.CheckFileMimeType(contentType, fileName, "audio/flac", [".mp3"]);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void CheckFileMimeType_ReturnsTrue_WhenContentTypeAndExtensionMatch()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.wav";
            var contentType = "audio/wave";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.CheckFileMimeType(contentType, fileName, "audio/wave", [".wav"]);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void CheckFileMimeType_ReturnsTrue_WhenContentMainTypeMatch()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "history.txt";
            var contentType = "text/plain";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.CheckFileMimeType(contentType, fileName, "text/*", [".txt", ".text"]);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidAudiofile_ReturnsTrue_WithValidAudiocodec()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.wav";
            var contentType = "audio/wav";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.IsValidAudiofile(contentType, fileName);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidAudiofile_ReturnsFalse_WithInvalidAudiocodecAndExtension()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.mock";
            var contentType = "just a fantasy";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.IsValidAudiofile(contentType, fileName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void GetAudioCodec_ReturnsAudiocodec_WhenContentTypeMatches()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.wbem";
            var contentType = "audio/webm";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.GetAudioCodec(contentType, fileName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(Audiofile.AudioCodecWEBM, result);
        }

        [TestMethod()]
        public void GetAudioCodec_ReturnsAudiocodec_WhenContentTypeAndFileExtensionMatches()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.wbem";
            var contentType = "audio/webm";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.GetAudioCodec(contentType, fileName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(Audiofile.AudioCodecWEBM, result);
        }

        [TestMethod()]
        public void GetAudioCodec_ReturnsNull_WhenContentTypeAndFileExtensionNotMatch()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.acx";
            var contentType = "fantasy stuff";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.GetAudioCodec(contentType, fileName);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod()]
        public void IsValidForImportView_ReturnsTrue_WhenFileIsHtml()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.html";
            var contentType = "text/html";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.IsValidForImportView(contentType, fileName);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidForImportView_ReturnsFalse_WhenFileIsBinary()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var fileName = "test.dat";
            var contentType = "application/octet-stream";
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);

            // Act
            var result = manager.IsValidForImportView(contentType, fileName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CreateFileUploadsAsync_ReturnsFileUploads_WhenFileHasTextContentAsync()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);
            var firstFile = CreateBrowserFile("Test.txt", "text/plain", "Just a test!");
            var secondFile = CreateBrowserFile("Test.mp3", "audio/mpeg");
            var fileInputId = nameof(CreateFileUploadsAsync_ReturnsFileUploads_WhenFileHasTextContentAsync);
            IReadOnlyList<IBrowserFile> browserfiles = [
                firstFile,
                secondFile
            ];
            var objectUrl = "Some object url!";
            jsRuntimeMock.Setup(js => js.InvokeAsync<String>(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(objectUrl);
            // Act
            var result = await manager.CreateFileUploadsAsync(browserfiles, fileInputId);
            // Assert
            Assert.AreEqual(2, result.Count());
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
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientMock = new Mock<HttpClient>();
            var loggerMock = new Mock<ILogger<FileInputManager>>();
            var manager = new FileInputManager(jsRuntimeMock.Object, httpClientMock.Object, loggerMock.Object);
            var firstFile = CreateBrowserFile("Test.bin", "binary", "Just a test!");
            var secondFile = CreateBrowserFile("Test.bin", "octet/stream");
            IReadOnlyList<IBrowserFile> browserfiles = [
                firstFile,
                secondFile
            ];
            // Act
            var result = await manager.CreateFileUploadsAsync(browserfiles);
            // Assert
            Assert.AreEqual(0, result.Count());
        }

        private static IBrowserFile CreateBrowserFile(string name, string contentType, string? content = null)
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