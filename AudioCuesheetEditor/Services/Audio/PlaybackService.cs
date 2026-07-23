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
    public class PlaybackService(IJSRuntime jsRuntime, ISessionStateContainer sessionStateContainer)
    {
        private readonly ISessionStateContainer _sessionStateContainer = sessionStateContainer;
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        private Audiofile? _currentlyPlayingAudiofile;
        private Timer? _updateTimer;
        private readonly Lock _timerLock = new();
        private TimeSpan? _currentPosition;
        private ElementReference? _audioElement;

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
        public Boolean IsPlaying { get; private set; } = false;
        public Boolean IsPlaybackPossible => _sessionStateContainer.Cuesheet.Audiofiles.Any(x => string.IsNullOrEmpty(x.ObjectURL) == false);
        public Boolean IsPreviousPossible => (CurrentlyPlayingTrack != null) && _sessionStateContainer.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).FirstOrDefault(x => x.End <= CurrentlyPlayingTrack.Begin) != null;
        public Boolean IsNextPossible => (CurrentlyPlayingTrack != null) && _sessionStateContainer.Cuesheet.Audiofiles.SelectMany(x => x.Tracks).FirstOrDefault(x => x.Begin >= CurrentlyPlayingTrack.End) != null;

        public void Initialize(ElementReference audioElement)
        {
            _audioElement = audioElement;
        }

        public async Task PlayOrPauseAsync()
        {
            if (IsPlaying)
            {
                await _jsRuntime.InvokeVoidAsync("audioInterop.pauseAudio", _audioElement);
                IsPlaying = false;
            }
            else
            {
                if (_currentlyPlayingAudiofile != null)
                {
                    //TODO: Check if we need to switch to the next audio file or if we can resume the current one
                    //TODO: Resume current playback
                }
                else
                {
                    var audiofileToPlay = _sessionStateContainer.Cuesheet.Audiofiles.FirstOrDefault(x => string.IsNullOrEmpty(x.ObjectURL) == false);
                    if (audiofileToPlay != null)
                    {
                        await _jsRuntime.InvokeVoidAsync("audioInterop.playAudio", _audioElement, audiofileToPlay.ObjectURL);
                        IsPlaying = true;
                        StartTimer();
                    }
                    _currentlyPlayingAudiofile = audiofileToPlay;
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
            await _jsRuntime.InvokeVoidAsync("audioInterop.stopAudio", _audioElement);
            _currentlyPlayingAudiofile = null;
            IsPlaying = false; 
            StopTimer();    
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
            if (IsPlaying == false)
            {
                await PlayOrPauseAsync();
            }
            var seconds = time.TotalSeconds;
            await _jsRuntime.InvokeVoidAsync("audioInterop.seekAudio", _audioElement, seconds);
        }

        private void StartTimer()
        {
            _updateTimer ??= new Timer(UpdateCurrentPosition, null, 0, 500);
        }

        private void StopTimer()
        {
            _updateTimer?.Dispose();
            _updateTimer = null;
        }

        private async void UpdateCurrentPosition(object? state)
        {
            // Thread-safe access
            lock (_timerLock)
            {
                if (_currentlyPlayingAudiofile == null || !IsPlaying)
                {
                    StopTimer();
                }
            }
            var currentTime = await _jsRuntime.InvokeAsync<double>("audioInterop.getAudioCurrentTime", _audioElement);
            CurrentPosition = TimeSpan.FromSeconds(currentTime);
        }
    }
}
