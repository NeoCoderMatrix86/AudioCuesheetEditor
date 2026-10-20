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
using AudioCuesheetEditor.Model.Entity;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.IO.Audio;

public class Audiofile() : Validateable, IAudiofile
{
    public static readonly AudioCodec AudioCodecWEBM = new("audio/webm", ".webm", "AudioCodec WEBM");

    public static readonly List<AudioCodec> AudioCodecs =
    [
        AudioCodecWEBM,
        new AudioCodec("audio/mpeg", ".mp3", "AudioCodec MP3"),
        new AudioCodec("audio/ogg", ".oga", "AudioCodec OGA"),
        new AudioCodec("audio/ogg", ".ogg", "AudioCodec OGG"),
        new AudioCodec("audio/ogg; codecs=opus", ".opus", "AudioCodec OPUS"),
        new AudioCodec("audio/wav", ".wav", "AudioCodec WAV"),
        new AudioCodec("audio/wav", ".wave", "AudioCodec WAVE"),
        new AudioCodec("audio/flac", ".flac", "AudioCodec FLAC")
    ];

    public Audiofile(string? name, string? objectURL, AudioCodec? audioCodec, TimeSpan? duration = null) : this()
    {
        Name = name;
        ObjectURL = objectURL;
        AudioCodec = audioCodec;
        Duration = duration;
    }

    public string? Name
    {
        get;
        set
        {
            var extension = Path.GetExtension(value);
            if (extension?.Equals(AudioCodec?.FileExtension, StringComparison.OrdinalIgnoreCase) == false)
            {
                value = $"{value}{AudioCodec?.FileExtension}";
            }
            field = value;
        }
    }

    [JsonIgnore]
    public string? ObjectURL { get; set; }

    /// <summary>
    /// Duration of the audio file
    /// </summary>
    public TimeSpan? Duration { get; set; }

    public AudioCodec? AudioCodec
    {
        get;
        set
        {
            field = value;
            if ((field != null) && (Name?.EndsWith(field.FileExtension, StringComparison.CurrentCultureIgnoreCase) == false))
            {
                //Replace file ending
                Name = $"{Path.GetFileNameWithoutExtension(Name)}{field.FileExtension}";
            }
        }
    }

    public ICollection<Track> Tracks { get; set; } = [];

    public override ValidationResult Validate(string propertyName)
    {
        var validationStatus = ValidationStatus.NoValidation;
        List<ValidationMessage>? validationMessages = null;
        switch (propertyName)
        {
            case nameof(Tracks):
                validationStatus = ValidationStatus.Success;
                if (Tracks.Count == 0)
                {
                    validationMessages ??= [];
                    validationMessages.Add(new ValidationMessage("{0} has invalid count ({1})!", nameof(Tracks), 0));
                }
                else
                {
                    //Check track overlapping
                    var tracksWithSamePosition = Tracks
                        .GroupBy(x => x.Position)
                        .Where(grp => grp.Count() > 1);
                    if (tracksWithSamePosition.Any())
                    {
                        validationMessages ??= [];
                        foreach (var track in tracksWithSamePosition)
                        {
                            foreach (var trackWithSamePosition in track)
                            {
                                validationMessages.Add(new ValidationMessage("{0} {1} '{2}' is used also by {3}({4},{5},{6},{7},{8}). Positions must be unique!", nameof(Track), nameof(Track.Position), track.Key != null ? track.Key : string.Empty, nameof(Track), trackWithSamePosition.Position != null ? trackWithSamePosition.Position : string.Empty, trackWithSamePosition.Artist ?? string.Empty, trackWithSamePosition.Title ?? string.Empty, trackWithSamePosition.Begin != null ? trackWithSamePosition.Begin : string.Empty, trackWithSamePosition.Finish != null ? trackWithSamePosition.Finish : string.Empty));
                            }
                        }
                    }
                    foreach (var track in Tracks.OrderBy(x => x.Position))
                    {
                        var tracksBetween = Tracks.Where(x => ((track.Begin >= x.Begin && track.Begin < x.Finish)
                                                    || (x.Begin < track.Finish && track.Finish <= x.Finish))
                                                    && (x.Equals(track) == false));
                        if (tracksBetween.Any())
                        {
                            validationMessages ??= [];
                            foreach (var trackBetween in tracksBetween)
                            {
                                validationMessages.Add(new ValidationMessage("{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!", nameof(Track), track.Position != null ? track.Position : string.Empty, track.Artist ?? string.Empty, track.Title ?? string.Empty, track.Begin != null ? track.Begin : string.Empty, track.Finish != null ? track.Finish : string.Empty, trackBetween.Position != null ? trackBetween.Position : string.Empty, trackBetween.Artist ?? string.Empty, trackBetween.Title ?? string.Empty, trackBetween.Begin != null ? trackBetween.Begin : string.Empty, trackBetween.Finish != null ? trackBetween.Finish : string.Empty));
                            }
                        }
                    }
                }
                break;
            case nameof(Name):
                validationStatus = ValidationStatus.Success;
                if (string.IsNullOrEmpty(Name))
                {
                    validationMessages ??= [];
                    validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Name)));
                }
                break;
            case nameof(AudioCodec):
                validationStatus = ValidationStatus.Success;
                if (AudioCodec == null)
                {
                    validationMessages ??= [];
                    validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(AudioCodec)));
                }
                break;
        }
        return ValidationResult.Create(validationStatus, validationMessages);
    }
}
