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
using Howler.Blazor.Components;

namespace AudioCuesheetEditor.Services.Audio
{
    public class PlaybackService : IDisposable
    {
        private readonly ISessionStateContainer _sessionStateContainer;
        private readonly IHowl _howl;

        private int? _currentPlayingSoundId;
        private Audiofile? _currentlyPlayingAudiofile;
        private Timer? _updateTimer;
        private bool _disposedValue;
        private readonly Lock _timerLock = new();
        private TimeSpan? _currentPosition;

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
        public Track? CurrentlyPlayingTrack => _sessionStateContainer.Cuesheet.Tracks.SingleOrDefault(x => x.Begin.HasValue == true && x.End.HasValue == true && x.Begin <= CurrentPosition && x.End > CurrentPosition);
        public TimeSpan? TotalTime => _sessionStateContainer.Cuesheet.Audiofile?.Duration;
        public Boolean IsPlaying { get; private set; } = false;
        public Boolean IsPlaybackPossible
        {
            get
            {
                var audiofile = _sessionStateContainer.Cuesheet.Audiofile;
                return String.IsNullOrEmpty(audiofile?.ObjectURL) == false && String.IsNullOrEmpty(audiofile?.AudioFileType) == false;
            }
        }
        public Boolean IsPreviousPossible => (CurrentlyPlayingTrack != null) && _sessionStateContainer.Cuesheet.Tracks.FirstOrDefault(x => x.End <= CurrentlyPlayingTrack.Begin) != null;
        public Boolean IsNextPossible => (CurrentlyPlayingTrack != null) && _sessionStateContainer.Cuesheet.Tracks.FirstOrDefault(x => x.Begin >= CurrentlyPlayingTrack.End) != null;

        public PlaybackService(ISessionStateContainer sessionStateContainer, IHowl howl)
        {
            _sessionStateContainer = sessionStateContainer;
            _sessionStateContainer.CuesheetChanged += SessionStateContainer_CuesheetChanged;
            _howl = howl;
            _howl.OnPlay += Howl_OnPlay;
            _howl.OnPause += Howl_OnPause;
            _howl.OnEnd += Howl_OnEnd;
            _howl.OnStop += Howl_OnStop;
        }

        public async Task PlayOrPauseAsync()
        {
            //Reset if the last played audiofile is not the current one
            if (_currentlyPlayingAudiofile != _sessionStateContainer.Cuesheet.Audiofile)
            {
                _currentPlayingSoundId = null;
            }
            //If the current audiofile already started, we just pause
            if (_currentPlayingSoundId != null)
            {
                await _howl.Pause(_currentPlayingSoundId.Value);
            }
            else
            {
                if (IsPlaybackPossible)
                {
                    string[]? sources = null;
                    string[]? formats = null;
                    if (_sessionStateContainer.Cuesheet.Audiofile?.ObjectURL != null)
                    {
                        sources = [_sessionStateContainer.Cuesheet.Audiofile.ObjectURL];
                    }
                    if (_sessionStateContainer.Cuesheet.Audiofile?.AudioFileType != null)
                    {
                        formats = [_sessionStateContainer.Cuesheet.Audiofile.AudioFileType.ToLower()];
                    }
                    var options = new HowlOptions
                    {
                        Sources = sources,
                        Formats = formats,
                        Html5 = true
                    };
                    _currentPlayingSoundId = await _howl.Play(options);
                    _currentlyPlayingAudiofile = _sessionStateContainer.Cuesheet.Audiofile;
                }
            }
        }

        public async Task PlayAsync(Track trackToPlay)
        {
            if (trackToPlay?.Begin.HasValue == true)
            {
                if (IsPlaying == false)
                {
                    await PlayOrPauseAsync();
                }
                if (_currentPlayingSoundId.HasValue)
                {
                    await _howl.Seek(_currentPlayingSoundId.Value, trackToPlay.Begin.Value);
                }
            }
        }

        public async Task StopAsync()
        {
            if (_currentPlayingSoundId != null)
            {
                await _howl.Stop(_currentPlayingSoundId.Value);
            }
        }

        public async Task PlayNextTrackAsync()
        {
            if (CurrentlyPlayingTrack != null)
            {
                var trackToPlay = _sessionStateContainer.Cuesheet.Tracks.FirstOrDefault(x => x.Begin >= CurrentlyPlayingTrack.End);
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
                var trackToPlay = _sessionStateContainer.Cuesheet.Tracks.LastOrDefault(x => x.End <= CurrentlyPlayingTrack.Begin);
                if (trackToPlay != null)
                {
                    await PlayAsync(trackToPlay);
                }
            }
        }

        public async Task SeekAsync(TimeSpan time)
        {
            if (_currentPlayingSoundId.HasValue == false)
            {
                await PlayOrPauseAsync();
            }
            if (_currentPlayingSoundId.HasValue)
            {
                if (IsPlaying == false)
                {
                    await PlayOrPauseAsync();
                }
                await _howl.Seek(_currentPlayingSoundId.Value, time);
            }
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _sessionStateContainer.CuesheetChanged -= SessionStateContainer_CuesheetChanged;
                    _howl.OnPlay -= Howl_OnPlay;
                    _howl.OnPause -= Howl_OnPause;
                    _howl.OnEnd -= Howl_OnEnd;
                    _howl.OnStop -= Howl_OnStop;
                }
                _disposedValue = true;
            }
        }

        private void Howl_OnStop(Howler.Blazor.Components.Events.HowlEventArgs obj)
        {
            IsPlaying = false;
            _currentPlayingSoundId = null;
            StopTimer();
            CurrentPosition = null;
            _currentlyPlayingAudiofile = _sessionStateContainer.Cuesheet.Audiofile;
        }

        private void Howl_OnEnd(Howler.Blazor.Components.Events.HowlEventArgs obj)
        {
            IsPlaying = false;
            StopTimer();
            CurrentPosition = null;
        }

        private void Howl_OnPause(Howler.Blazor.Components.Events.HowlEventArgs obj)
        {
            IsPlaying = false;
            StopTimer();
        }

        private void Howl_OnPlay(Howler.Blazor.Components.Events.HowlPlayEventArgs obj)
        {
            IsPlaying = true;
            StartTimer();
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
                if (_currentPlayingSoundId == null || !IsPlaying) return;
            }
            CurrentPosition = await _howl.GetCurrentTime(_currentPlayingSoundId.Value);
            if (_sessionStateContainer.Cuesheet.Audiofile != _currentlyPlayingAudiofile)
            {
                await _howl.Stop(_currentPlayingSoundId.Value);
            }
        }

        private void SessionStateContainer_CuesheetChanged(object? sender, EventArgs e)
        {
            _ = StopAsync();
        }
    }
}
