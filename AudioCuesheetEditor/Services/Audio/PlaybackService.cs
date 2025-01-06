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
using AudioCuesheetEditor.Extensions;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.IO.Audio;
using Howler.Blazor.Components;

namespace AudioCuesheetEditor.Services.Audio
{
    public class PlaybackService : IDisposable
    {
        private readonly SessionStateContainer _sessionStateContainer;
        private readonly IHowl _howl;

        private int? soundId;
        private Audiofile? currentlyPlayingAudiofile;
        private Timer? updateTimer;
        private bool disposedValue;
        private readonly Lock timerLock = new();

        public TimeSpan? CurrentPosition { get; private set; }
        public Track? CurrentlyPlayingTrack => _sessionStateContainer.Cuesheet.Tracks.SingleOrDefault(x => x.Begin.HasValue == true && x.End.HasValue == true && x.Begin <= CurrentPosition && x.End > CurrentPosition);
        public TimeSpan? TotalTime => _sessionStateContainer.Cuesheet.Audiofile?.Duration;
        public Boolean IsPlaying { get; private set; } = false;
        //Refer to Cuesheet (not ImportCuesheet) since playback will always be done on the imported cuesheet
        public Boolean PlaybackPossible => _sessionStateContainer.Cuesheet.Audiofile != null && _sessionStateContainer.Cuesheet.Audiofile.PlaybackPossible;
        public Boolean PreviousPossible => (CurrentlyPlayingTrack != null) && _sessionStateContainer.Cuesheet.Tracks.ToList().IndexOf(CurrentlyPlayingTrack) >= 1;
        public Boolean NextPossible => (CurrentlyPlayingTrack != null) && _sessionStateContainer.Cuesheet.Tracks.ToList().IndexOf(CurrentlyPlayingTrack) < _sessionStateContainer.Cuesheet.Tracks.Count - 1;

        public PlaybackService(SessionStateContainer sessionStateContainer, IHowl howl)
        {
            _sessionStateContainer = sessionStateContainer;
            _howl = howl;
            _howl.OnPlay += Howl_OnPlay;
            _howl.OnPause += Howl_OnPause;
            _howl.OnEnd += Howl_OnEnd;
            _howl.OnStop += Howl_OnStop;
        }

        public async Task PlayOrPauseAsync(Track? trackToPlay = null)
        {
            //Reset if the last played audiofile is not the current one
            if (currentlyPlayingAudiofile != _sessionStateContainer.Cuesheet.Audiofile)
            {
                soundId = null;
            }
            //If the current audiofile already started, we just pause
            if (soundId != null)
            {
                await _howl.Pause(soundId.Value);
            }
            else
            {
                if (PlaybackPossible)
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
                    soundId = await _howl.Play(options);
                    currentlyPlayingAudiofile = _sessionStateContainer.Cuesheet.Audiofile;
                    if (trackToPlay?.Begin.HasValue == true)
                    {
                        await _howl.Seek(soundId.Value, trackToPlay.Begin.Value);
                    }
                }
            }
        }
        
        public async Task StopAsync()
        {
            if (soundId != null)
            {
                await _howl.Stop(soundId.Value);
            }
        }

        public async Task PlayNextTrackAsync()
        {
            if (CurrentlyPlayingTrack != null)
            {
                var index = _sessionStateContainer.Cuesheet.Tracks.ToList().IndexOf(CurrentlyPlayingTrack);
                var trackToPlay = _sessionStateContainer.Cuesheet.Tracks.ElementAtOrDefault(index + 1);
                await PlayOrPauseAsync(trackToPlay);
            }
        }

        public async Task PlayPreviousTrackAsync()
        {
            if (CurrentlyPlayingTrack != null)
            {
                var index = _sessionStateContainer.Cuesheet.Tracks.ToList().IndexOf(CurrentlyPlayingTrack);
                var trackToPlay = _sessionStateContainer.Cuesheet.Tracks.ElementAtOrDefault(index - 1);
                await PlayOrPauseAsync(trackToPlay);
            }
        }

        public async Task SeekAsync(TimeSpan time)
        {
            if (soundId.HasValue)
            {
                if (IsPlaying == false)
                {
                    await PlayOrPauseAsync();
                }
                await _howl.Seek(soundId.Value, time);
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
            if (!disposedValue)
            {
                if (disposing)
                {
                    _howl.OnPlay -= Howl_OnPlay;
                    _howl.OnPause -= Howl_OnPause;
                    _howl.OnEnd -= Howl_OnEnd;
                    _howl.OnStop -= Howl_OnStop;
                }
                disposedValue = true;
            }
        }

        private void Howl_OnStop(Howler.Blazor.Components.Events.HowlEventArgs obj)
        {
            IsPlaying = false;
            soundId = null;
            StopTimer();
            CurrentPosition = null;
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
            updateTimer ??= new Timer(UpdateCurrentPosition, null, 0, 500);
        }

        private void StopTimer()
        {
            updateTimer?.Dispose();
            updateTimer = null;
        }

        private async void UpdateCurrentPosition(object? state)
        {
            // Thread-safe access
            lock (timerLock)
            {
                if (soundId == null || !IsPlaying) return;
            }
            CurrentPosition = await _howl.GetCurrentTime(soundId.Value);
        }

        
    }
}
