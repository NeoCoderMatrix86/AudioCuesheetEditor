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
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Services.Options;
using AudioCuesheetEditor.Services.UI;
using Microsoft.JSInterop;
using Moq;

namespace AudioCuesheetEditor.Tests.Services.UI;

[TestClass]
public class SessionStateContainerTests : IDisposable
{
    readonly SessionStateContainer _sessionStateContainer;
    readonly Mock<ILocalStorageOptionsProvider> _localStorageOptionsProvider;
    readonly Mock<IJSRuntime> _jsRuntime;
    bool _disposedValue;

    public SessionStateContainerTests()
    {
        _localStorageOptionsProvider = new();
        _jsRuntime = new();
        _sessionStateContainer = new(_localStorageOptionsProvider.Object, _jsRuntime.Object);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _sessionStateContainer.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    [TestMethod]
    public async Task Cuesheet_SetNewValueInDetailView_ShouldTriggerActiveCuesheetChangedEventAsync()
    {
        // Arrange
        var newCuesheet = new Cuesheet();
        var viewOptions = new ViewOptions();
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        var activeCuesheetChangedEvent = false;
        var cuesheetChangedEvent = false;
        _sessionStateContainer.ActiveCuesheetChanged += (sender, args) => activeCuesheetChangedEvent = true;
        _sessionStateContainer.CuesheetChanged += (sender, args) => cuesheetChangedEvent = true;

        // Act
        _sessionStateContainer.Cuesheet = newCuesheet;

        // Assert
        Assert.IsTrue(activeCuesheetChangedEvent);
        Assert.IsTrue(cuesheetChangedEvent);
    }

    [TestMethod]
    public async Task Cuesheet_SetNewValueInImportView_ShouldNotTriggerActiveCuesheetChangedEventAsync()
    {
        // Arrange
        var newCuesheet = new Cuesheet();
        var viewOptions = new ViewOptions()
        {
            ActiveTab = ViewMode.ImportView
        };
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        var eventTriggered = false;
        var cuesheetChangedEvent = false;
        _sessionStateContainer.ActiveCuesheetChanged += (sender, args) => eventTriggered = true;
        _sessionStateContainer.CuesheetChanged += (sender, args) => cuesheetChangedEvent = true;
        // Act
        _sessionStateContainer.Cuesheet = newCuesheet;

        // Assert
        Assert.IsFalse(eventTriggered);
        Assert.IsTrue(cuesheetChangedEvent);
    }

    [TestMethod]
    public async Task ImportCuesheet_SetNewValueInImportView_ShouldTriggerActiveCuesheetChangedEventAsync()
    {
        // Arrange
        var newImportCuesheet = new Cuesheet();
        var viewOptions = new ViewOptions()
        {
            ActiveTab = ViewMode.ImportView
        };
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        var eventTriggered = false;
        _sessionStateContainer.ActiveCuesheetChanged += (sender, args) => eventTriggered = true;

        // Act
        _sessionStateContainer.ImportCuesheet = newImportCuesheet;

        // Assert
        Assert.IsTrue(eventTriggered);
    }

    [TestMethod]
    public async Task ImportCuesheet_SetNewValueInDetailView_ShouldTriggerActiveCuesheetChangedEventAsync()
    {
        // Arrange
        var newImportCuesheet = new Cuesheet();
        var viewOptions = new ViewOptions();
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        var eventTriggered = false;
        _sessionStateContainer.ActiveCuesheetChanged += (sender, args) => eventTriggered = true;

        // Act
        _sessionStateContainer.ImportCuesheet = newImportCuesheet;

        // Assert
        Assert.IsFalse(eventTriggered);
    }

    [TestMethod]
    public async Task ResetImportAsync_RevokeObjectUrls_ShouldClearPropertiesAsync()
    {
        // Arrange
        var viewOptions = new ViewOptions();
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        _sessionStateContainer.Importfile = Mock.Of<IImportfile>();
        _sessionStateContainer.ImportCuesheet = new Cuesheet();
        _sessionStateContainer.ImportIsAnalyzed = true;
        var importAudiofile1 = new Audiofile()
        {
            ObjectURL = "File 1"
        };
        var importAudiofile2 = new Audiofile()
        {
            ObjectURL = "File 2"
        };
        _sessionStateContainer.ImportAudiofiles.Add(importAudiofile1);
        _sessionStateContainer.ImportAudiofiles.Add(importAudiofile2);
        var importAudiofile3 = new Audiofile()
        {
            ObjectURL = "File 3"
        };
        var importAudiofile4 = new Audiofile()
        {
            ObjectURL = "File 4"
        };
        _sessionStateContainer.ImportAudiofileMapping.Add(new(), importAudiofile3);
        _sessionStateContainer.ImportAudiofileMapping.Add(new(), importAudiofile4);

        // Act
        await _sessionStateContainer.ResetImportAsync(true);

        // Assert
        Assert.IsNull(_sessionStateContainer.Importfile);
        Assert.IsEmpty(_sessionStateContainer.ImportAudiofiles);
        Assert.IsNull(_sessionStateContainer.ImportCuesheet);
        Assert.IsFalse(_sessionStateContainer.ImportIsAnalyzed);
        Assert.IsEmpty(_sessionStateContainer.ImportAudiofileMapping);
        _jsRuntime.Verify(js => js.InvokeAsync<object>("revokeAudioObjectURL", It.Is<object?[]>(args => args != null && args.Length > 0 && (args[0] as string) == importAudiofile1.ObjectURL)), Times.Once);
        _jsRuntime.Verify(js => js.InvokeAsync<object>("revokeAudioObjectURL", It.Is<object?[]>(args => args != null && args.Length > 0 && (args[0] as string) == importAudiofile2.ObjectURL)), Times.Once);
        _jsRuntime.Verify(js => js.InvokeAsync<object>("revokeAudioObjectURL", It.Is<object?[]>(args => args != null && args.Length > 0 && (args[0] as string) == importAudiofile3.ObjectURL)), Times.Once);
        _jsRuntime.Verify(js => js.InvokeAsync<object>("revokeAudioObjectURL", It.Is<object?[]>(args => args != null && args.Length > 0 && (args[0] as string) == importAudiofile4.ObjectURL)), Times.Once);
    }

    [TestMethod]
    public async Task ResetImportAsync_RevokeObjectUrlsNotSet_ShouldClearPropertiesAsync()
    {
        // Arrange
        var viewOptions = new ViewOptions();
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        _sessionStateContainer.Importfile = Mock.Of<IImportfile>();
        _sessionStateContainer.ImportCuesheet = new Cuesheet();
        _sessionStateContainer.ImportIsAnalyzed = true;
        var importAudiofile1 = new Audiofile()
        {
            ObjectURL = "File 1"
        };
        var importAudiofile2 = new Audiofile()
        {
            ObjectURL = "File 2"
        };
        _sessionStateContainer.ImportAudiofiles.Add(importAudiofile1);
        _sessionStateContainer.ImportAudiofiles.Add(importAudiofile2);
        var importAudiofile3 = new Audiofile()
        {
            ObjectURL = "File 3"
        };
        var importAudiofile4 = new Audiofile()
        {
            ObjectURL = "File 4"
        };
        _sessionStateContainer.ImportAudiofileMapping.Add(new(), importAudiofile3);
        _sessionStateContainer.ImportAudiofileMapping.Add(new(), importAudiofile4);

        // Act
        await _sessionStateContainer.ResetImportAsync(false);

        // Assert
        Assert.IsNull(_sessionStateContainer.Importfile);
        Assert.IsEmpty(_sessionStateContainer.ImportAudiofiles);
        Assert.IsNull(_sessionStateContainer.ImportCuesheet);
        Assert.IsFalse(_sessionStateContainer.ImportIsAnalyzed);
        Assert.IsEmpty(_sessionStateContainer.ImportAudiofileMapping);
        _jsRuntime.Verify(js => js.InvokeAsync<object>("revokeAudioObjectURL", It.Is<object?[]>(args => args != null && args.Length > 0 && (args[0] as string) == importAudiofile1.ObjectURL)), Times.Never);
        _jsRuntime.Verify(js => js.InvokeAsync<object>("revokeAudioObjectURL", It.Is<object?[]>(args => args != null && args.Length > 0 && (args[0] as string) == importAudiofile2.ObjectURL)), Times.Never);
        _jsRuntime.Verify(js => js.InvokeAsync<object>("revokeAudioObjectURL", It.Is<object?[]>(args => args != null && args.Length > 0 && (args[0] as string) == importAudiofile3.ObjectURL)), Times.Never);
        _jsRuntime.Verify(js => js.InvokeAsync<object>("revokeAudioObjectURL", It.Is<object?[]>(args => args != null && args.Length > 0 && (args[0] as string) == importAudiofile4.ObjectURL)), Times.Never);
    }

    [TestMethod]
    public async Task InitializeAsync_NotInitialized_ShouldInitializeAsync()
    {
        // Arrange
        var viewOptions = new ViewOptions();
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        // Act
        await _sessionStateContainer.InitializeAsync();
        // Assert
        _localStorageOptionsProvider.Verify(x => x.GetOptionsAsync<ViewOptions>(), Times.Once);
    }

    [TestMethod]
    public async Task InitializeAsync_Initialized_ShouldNotInitializeAsync()
    {
        // Arrange
        var viewOptions = new ViewOptions();
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        // Act
        await _sessionStateContainer.InitializeAsync();
        // Assert
        _localStorageOptionsProvider.Verify(x => x.GetOptionsAsync<ViewOptions>(), Times.Once);
    }

    [TestMethod]
    public async Task ActiveCuesheet_Initialized_ReturnsDetailViewCuesheet()
    {
        // Arrange
        var viewOptions = new ViewOptions();
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        // Act
        var cuesheet = _sessionStateContainer.ActiveCuesheet;
        // Assert
        Assert.AreEqual(_sessionStateContainer.Cuesheet, cuesheet);
    }

    [TestMethod]
    public async Task ActiveCuesheet_SwitchViewSignalesUpdate_ReturnsImportViewCuesheet()
    {
        // Arrange
        var viewOptions = new ViewOptions();
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        viewOptions.ActiveTab = ViewMode.ImportView;

        _ = _localStorageOptionsProvider.Setup(x => x.SaveOptionsAsync(viewOptions)).Callback(() => _localStorageOptionsProvider.Raise(x => x.OptionSaved += null, _localStorageOptionsProvider.Object, viewOptions));

        var activeCuesheetChangedEventTriggered = false;
        _sessionStateContainer.ActiveCuesheetChanged += delegate
        {
            activeCuesheetChangedEventTriggered = true;
        };
        _sessionStateContainer.Cuesheet.Title = "Cuesheet";
        _sessionStateContainer.ImportCuesheet = new()
        {
            Title = "ImportCuesheet"
        };
        _ = await _localStorageOptionsProvider.Object.SaveOptionsAsync(viewOptions);
        // Act
        var cuesheet = _sessionStateContainer.ActiveCuesheet;
        // Assert
        Assert.AreEqual(_sessionStateContainer.ImportCuesheet, cuesheet);
        Assert.IsTrue(activeCuesheetChangedEventTriggered);
    }

    [TestMethod]
    public async Task ActiveCuesheet_SaveOptionsSetsActiveCuesheetOnce_ReturnsDetailViewCuesheet()
    {
        // Arrange
        var viewOptions = new ViewOptions();
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        _ = _localStorageOptionsProvider.Setup(x => x.SaveOptionsAsync(viewOptions)).Callback(() => _localStorageOptionsProvider.Raise(x => x.OptionSaved += null, _localStorageOptionsProvider.Object, viewOptions));
        _sessionStateContainer.Cuesheet.Title = "Cuesheet";
        _sessionStateContainer.ImportCuesheet = new()
        {
            Title = "ImportCuesheet"
        };
        viewOptions.ActiveTab = ViewMode.ImportView;
        _ = await _localStorageOptionsProvider.Object.SaveOptionsAsync(viewOptions);
        var activeCuesheetChangedEventTriggered = false;
        _sessionStateContainer.ActiveCuesheetChanged += delegate
        {
            activeCuesheetChangedEventTriggered = true;
        };
        _ = await _localStorageOptionsProvider.Object.SaveOptionsAsync(viewOptions);
        // Act
        var cuesheet = _sessionStateContainer.ActiveCuesheet;
        // Assert
        Assert.AreEqual(_sessionStateContainer.ImportCuesheet, cuesheet);
        Assert.IsFalse(activeCuesheetChangedEventTriggered);
    }

    [TestMethod]
    public async Task ActiveCuesheet_InitializedWithImportView_ReturnsDetailViewCuesheet()
    {
        // Arrange
        var viewOptions = new ViewOptions()
        {
            ActiveTab = ViewMode.ImportView
        };
        _ = _localStorageOptionsProvider.Setup(x => x.GetOptionsAsync<ViewOptions>()).ReturnsAsync(viewOptions);
        await _sessionStateContainer.InitializeAsync();
        // Act
        var result = _sessionStateContainer.ActiveCuesheet;
        // Assert
        Assert.AreEqual(_sessionStateContainer.ImportCuesheet, result);
    }

    [TestMethod]
    public void ActiveCuesheet_NotInitialized_ReturnsNull()
    {
        // Arrange
        // Act
        var cuesheet = _sessionStateContainer.ActiveCuesheet;
        // Assert
        Assert.IsNull(cuesheet);
    }
}
