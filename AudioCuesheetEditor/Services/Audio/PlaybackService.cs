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
using AudioCuesheetEditor.Services.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AudioCuesheetEditor.Services.Audio
{
    public class PlaybackService(IJSRuntime jsRuntime, ISessionStateContainer sessionStateContainer) : IAsyncDisposable
    {
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        private Audiofile? _currentlyPlayingAudiofile;
        private Timer? _updateTimer;
        private readonly Lock _timerLock = new();
        private TimeSpan? _currentPosition;
        private ElementReference? _audioElement;
        private DotNetObjectReference<PlaybackService>? _dotNetObjectReference;
        private TimeSpan? _audiofileDurationsBeforeCurrentlyPlayingAudiofile;

        public event Action? CurrentPositionChanged;

        public TimeSpan? CurrentPosition
        {
            get => _currentPosition;
            private set
            {
                if (_currentPosition != value)
                {
                    _currentPosition = value;
                    CurrentPositionChanged?.Invoke();
                }
            }
        }
        public Track? CurrentlyPlayingTrack => _sessionStateContainer.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).SingleOrDefault(x => x.Begin.HasValue == true && x.End.HasValue == true && x.Begin <= CurrentPosition && x.End > CurrentPosition);
        public TimeSpan? TotalTime
        {
            get
            {
                var durations = _sessionStateContainer.Cuesheet.Audiofiles.Where(a => a.Duration.HasValue).Select(x => x.Duration);
                if (durations.Any())
                {
                    return durations.Aggregate((sum, a) => sum + a);
                }
                return null;
            }
        }
        public Boolean IsPaused { get; private set; } = false;
        public Boolean IsPlaybackPossible => _sessionStateContainer.Cuesheet.Audiofiles.Any(x => string.IsNullOrEmpty(x.ObjectURL) == false);
        public Boolean IsPreviousPossible => (CurrentlyPlayingTrack != null) && _sessionStateContainer.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).FirstOrDefault(x => x.End <= CurrentlyPlayingTrack.Begin) != null;
        public Boolean IsNextPossible => (CurrentlyPlayingTrack != null) && _sessionStateContainer.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).FirstOrDefault(x => x.Begin >= CurrentlyPlayingTrack.End) != null;

        public async Task InitializeAsync(ElementReference audioElement)
        {
            if (audioElement.Equals(_audioElement) == false)
            {
                _audioElement = audioElement;
                _dotNetObjectReference = DotNetObjectReference.Create(this);
                await _jsRuntime.InvokeVoidAsync("audioInterop.register", _dotNetObjectReference, audioElement);
            }
        }

        public async Task PlayOrPauseAsync()
        {
            if (_currentlyPlayingAudiofile != null)
            {
                if (IsPaused == false)
                {
                    await _jsRuntime.InvokeVoidAsync("audioInterop.pauseAudio");
                }
                else
                {
                    await _jsRuntime.InvokeVoidAsync("audioInterop.playAudio");
                }
            }
            else
            {
                var audiofileToPlay = _sessionStateContainer.Cuesheet.Audiofiles.FirstOrDefault(x => string.IsNullOrEmpty(x.ObjectURL) == false);
                if (audiofileToPlay != null)
                {
                    await PlayAsync(audiofileToPlay);
                }
            }
        }

        public async Task PlayAsync(Track trackToPlay)
        {
            if (trackToPlay?.Begin.HasValue == true)
            {
                await SeekAsync(trackToPlay.Begin.Value);
            }
        }

        public async Task StopAsync()
        {
            Reset();
            await _jsRuntime.InvokeVoidAsync("audioInterop.stopAudio");
        }

        public async Task PlayNextTrackAsync()
        {
            if (CurrentlyPlayingTrack != null)
            {
                var trackToPlay = _sessionStateContainer.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).FirstOrDefault(x => x.Begin >= CurrentlyPlayingTrack.End);
                if (trackToPlay != null)
                {
                    await PlayAsync(trackToPlay);
                }
            }
        }

        public async Task PlayPreviousTrackAsync()
        {
            if (CurrentlyPlayingTrack != null)
            {
                var trackToPlay = _sessionStateContainer.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).LastOrDefault(x => x.End <= CurrentlyPlayingTrack.Begin);
                if (trackToPlay != null)
                {
                    await PlayAsync(trackToPlay);
                }
            }
        }

        public async Task SeekAsync(TimeSpan time)
        {
            TimeSpan cumulativeDuration = TimeSpan.Zero;
            Audiofile? targetAudiofile = null;
            TimeSpan targetPositionInAudiofile = TimeSpan.Zero;

            foreach (var audiofile in _sessionStateContainer.Cuesheet.Audiofiles)
            {
                if (string.IsNullOrEmpty(audiofile.ObjectURL))
                {
                    continue;
                }

                if (audiofile.Duration.HasValue)
                {
                    TimeSpan nextCumulativeDuration = cumulativeDuration + audiofile.Duration.Value;

                    if (time >= cumulativeDuration && time < nextCumulativeDuration)
                    {
                        targetAudiofile = audiofile;
                        targetPositionInAudiofile = time - cumulativeDuration;
                        break;
                    }

                    cumulativeDuration = nextCumulativeDuration;
                }
            }

            if (targetAudiofile == null)
            {
                targetAudiofile = _sessionStateContainer.Cuesheet.Audiofiles.FirstOrDefault(x => string.IsNullOrEmpty(x.ObjectURL) == false);

                if (targetAudiofile == null)
                {
                    return;
                }
            }

            if (_currentlyPlayingAudiofile != targetAudiofile)
            {
                await PlayAsync(targetAudiofile);
            }

            if (IsPaused)
            {
                await PlayOrPauseAsync();
            }

            await _jsRuntime.InvokeVoidAsync("audioInterop.seekAudio", targetPositionInAudiofile.TotalSeconds);
        }

        [JSInvokable]
        public void OnPlaybackStarted(string objectUrl)
        {
            IsPaused = false;
            StartTimer();
        }

        [JSInvokable]
        public void OnPlaybackEnded(string objectUrlEnded)
        {
            var audioFileEnded = _sessionStateContainer.Cuesheet.Audiofiles.Single(x => x.ObjectURL == objectUrlEnded);
            var index = _sessionStateContainer.Cuesheet.Audiofiles.IndexOf(audioFileEnded);
            if (index < _sessionStateContainer.Cuesheet.Audiofiles.Count - 1)
            {
                var nextAudioFile = _sessionStateContainer.Cuesheet.Audiofiles.Where(x => string.IsNullOrEmpty(x.ObjectURL) == false).Skip(index + 1).FirstOrDefault();
                if (nextAudioFile != null)
                {
                    _ = PlayAsync(nextAudioFile);
                }
                else
                {
                    // No more audio files to play, we stop playback
                    Reset();
                }
            }
            else
            {
                // No more audio files to play, we stop playback
                Reset();
            }
        }

        [JSInvokable]
        public void OnPlaybackPaused(string objectUrl)
        {
            IsPaused = true;
            StopTimer();
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await _jsRuntime.InvokeVoidAsync("audioInterop.unregister");
            _dotNetObjectReference?.Dispose();
            _audioElement = null;
        }

        async Task PlayAsync(Audiofile audiofileToPlay)
        {
            await _jsRuntime.InvokeVoidAsync("audioInterop.setAudioSource", audiofileToPlay.ObjectURL);
            await _jsRuntime.InvokeVoidAsync("audioInterop.playAudio");            
            _currentlyPlayingAudiofile = audiofileToPlay;
            _audiofileDurationsBeforeCurrentlyPlayingAudiofile = null;
        }

        void Reset()
        {
            StopTimer();
            _currentlyPlayingAudiofile = null;
            _audiofileDurationsBeforeCurrentlyPlayingAudiofile = null;
            CurrentPosition = null;
            IsPaused = false;
        }

        void StartTimer()
        {
            _updateTimer ??= new Timer(UpdateCurrentPosition, null, 0, 500);
        }

        void StopTimer()
        {
            _updateTimer?.Dispose();
            _updateTimer = null;
        }

        async void UpdateCurrentPosition(object? state)
        {
            // Thread-safe access
            lock (_timerLock)
            {
                if (_currentlyPlayingAudiofile == null || IsPaused) return;
            }
            CalculateDurationsBeforeCurrentlyPlayingAudiofile();
            var currentSecondsInCurrentlyPlayingAudiofile = await _jsRuntime.InvokeAsync<double>("audioInterop.getAudioCurrentTime");
            if (_audiofileDurationsBeforeCurrentlyPlayingAudiofile.HasValue)
            {
                CurrentPosition = _audiofileDurationsBeforeCurrentlyPlayingAudiofile + TimeSpan.FromSeconds(currentSecondsInCurrentlyPlayingAudiofile);
            }
            else
            {
                CurrentPosition = TimeSpan.FromSeconds(currentSecondsInCurrentlyPlayingAudiofile);
            }
        }

        void CalculateDurationsBeforeCurrentlyPlayingAudiofile()
        {
            if ((_audiofileDurationsBeforeCurrentlyPlayingAudiofile != null) || (_currentlyPlayingAudiofile == null))
            {
                return;
            }
            _audiofileDurationsBeforeCurrentlyPlayingAudiofile = TimeSpan.Zero;
            var index = _sessionStateContainer.Cuesheet.Audiofiles.IndexOf(_currentlyPlayingAudiofile);
            for (int i = 0; i < index; i++)
            {
                var audiofile = _sessionStateContainer.Cuesheet.Audiofiles[i];
                _audiofileDurationsBeforeCurrentlyPlayingAudiofile += audiofile.Duration;
            }
        }
    }
}
