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
using AudioCuesheetEditor.Model.UI;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.IO.Export
{
    public class CuesheetSection : Validateable<CuesheetSection>, ITraceable
    {
        private Cuesheet? cuesheet;
        private TimeSpan? begin;
        private TimeSpan? end;
        private String? artist;
        private String? title;
        private String? audiofileName;

        public event EventHandler<TraceablePropertiesChangedEventArgs>? TraceablePropertyChanged;

        public CuesheetSection(Cuesheet cuesheet)
        {
            Cuesheet = cuesheet;
            artist = Cuesheet.Artist;
            title = Cuesheet.Title;
            audiofileName = Cuesheet.Audiofile?.Name;
            //Try to set begin
            begin = Cuesheet.Sections?.LastOrDefault()?.End;
            if (Begin.HasValue == false)
            {
                begin = Cuesheet.Tracks.Min(x => x.Begin);
            }
            end = Cuesheet.Tracks.Max(x => x.End);
        }

        [JsonConstructor]
        public CuesheetSection() { }
        public Cuesheet? Cuesheet 
        {
            get => cuesheet;
            set
            {
                if (cuesheet == null)
                {
                    cuesheet = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public String? Artist
        {
            get => artist;
            set
            {
                var previousValue = artist;
                artist = value;
                OnValidateablePropertyChanged(nameof(Artist));
                OnTraceablePropertyChanged(previousValue, nameof(Artist));
            }
        }

        public String? Title
        {
            get => title;
            set
            {
                var previousValue = title;
                title = value;
                OnValidateablePropertyChanged(nameof(Title));
                OnTraceablePropertyChanged(previousValue, nameof(Title));
            }
        }

        public TimeSpan? Begin 
        { 
            get => begin;
            set
            {
                var previousValue = begin;
                begin = value;
                OnValidateablePropertyChanged(nameof(Begin));
                OnTraceablePropertyChanged(previousValue, nameof(Begin));
            }
        }

        public TimeSpan? End
        {
            get => end;
            set
            {
                var previousValue = end;
                end = value;
                OnValidateablePropertyChanged(nameof(End));
                OnTraceablePropertyChanged(previousValue, nameof(End));
            }
        }

        public String? AudiofileName
        {
            get => audiofileName;
            set
            {
                var previousValue = audiofileName;
                audiofileName = value;
                OnValidateablePropertyChanged(nameof(AudiofileName));
                OnTraceablePropertyChanged(previousValue, nameof(AudiofileName));
            }
        }

        public void CopyValues(CuesheetSection splitPoint)
        {
            Artist = splitPoint.Artist;
            Title = splitPoint.Title;
            Begin = splitPoint.Begin;
        }

        protected override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(Begin):
                    validationStatus = ValidationStatus.Success;
                    if (Begin == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Begin)));
                    }
                    else
                    {
                        var minBegin = Cuesheet?.Tracks.Min(x => x.Begin);
                        if (Begin < minBegin)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} should be greater than or equal '{1}'!", nameof(Begin), minBegin));
                        }
                        if (Begin > End)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} should be less than or equal '{1}'!", nameof(Begin), End));
                        }
                    }
                    break;
                case nameof(End):
                    validationStatus = ValidationStatus.Success;
                    if (End == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(End)));
                    }
                    else
                    {
                        var maxEnd = Cuesheet?.Tracks.Max(x => x.End);
                        if (End > maxEnd)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} should be less than or equal '{1}'!", nameof(End), maxEnd));
                        }
                        if (End < Begin)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} should be greater than or equal '{1}'!", nameof(End), Begin));
                        }
                    }
                    break;
                case nameof(Artist):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Artist))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Artist)));
                    }
                    break;
                case nameof(Title):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Title))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Title)));
                    }
                    break;
                case nameof(AudiofileName):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(AudiofileName))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(AudiofileName)));
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }

        private void OnTraceablePropertyChanged(object? previousValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            TraceablePropertyChanged?.Invoke(this, new TraceablePropertiesChangedEventArgs(new TraceableChange(previousValue, propertyName)));
        }
    }
}
