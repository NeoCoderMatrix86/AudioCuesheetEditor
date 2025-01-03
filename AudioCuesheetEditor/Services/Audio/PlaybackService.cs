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
using AudioCuesheetEditor.Model.IO.Audio;
using Howler.Blazor.Components;

namespace AudioCuesheetEditor.Services.Audio
{
    public class PlaybackService
    {
        //TODO: here comes all playback related stuff
        //TODO: IDisposable
        private readonly SessionStateContainer _sessionStateContainer;
        private readonly IHowl _howl;

        private int? soundId;
        private Audiofile? currentlyPlayingAudiofile;

        public PlaybackService(SessionStateContainer sessionStateContainer, IHowl howl)
        {
            _sessionStateContainer = sessionStateContainer;
            _howl = howl;
            _howl.OnPlay += Howl_OnPlay;
            _howl.OnPause += Howl_OnPause;
            _howl.OnEnd += Howl_OnEnd;
            _howl.OnStop += Howl_OnStop;
        }

        private void Howl_OnStop(Howler.Blazor.Components.Events.HowlEventArgs obj)
        {
            IsPlaying = false;
            soundId = null;
        }

        private void Howl_OnEnd(Howler.Blazor.Components.Events.HowlEventArgs obj)
        {
            IsPlaying = false;
        }

        private void Howl_OnPause(Howler.Blazor.Components.Events.HowlEventArgs obj)
        {
            IsPlaying = false;
        }

        private void Howl_OnPlay(Howler.Blazor.Components.Events.HowlPlayEventArgs obj)
        {
            IsPlaying = true;
        }

        public TimeSpan? CurrentPosition { get; private set; }
        public TimeSpan? TotalTime => _sessionStateContainer.Cuesheet.Audiofile?.Duration;
        public Boolean IsPlaying { get; private set; } = false;
        //Refer to Cuesheet (not ImportCuesheet) since playback will always be done on the imported cuesheet
        public Boolean PlaybackPossible => _sessionStateContainer.Cuesheet.Audiofile != null && _sessionStateContainer.Cuesheet.Audiofile.PlaybackPossible;
        public async Task PlayOrPauseAsync()
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
    }
}
