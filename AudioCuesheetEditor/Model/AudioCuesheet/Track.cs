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
using AudioCuesheetEditor.Controller;
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.Reflection;
using AudioCuesheetEditor.Shared.ResourceFiles;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class Track : Validateable, ITrack
    {
        private uint? position;
        private String artist;
        private String title;
        private TimeSpan? begin;
        private TimeSpan? end;
        public Track()
        {
            Validate();
        }

        public Track(ImportTrack importTrack)
        {
            //We use the internal properties because we only want to set the values, everything around like validation or automatic calculation doesn't need to be fired
            position = importTrack.Position;
            artist = importTrack.Artist;
            title = importTrack.Title;
            begin = importTrack.Begin;
            end = importTrack.End;
        }

        public uint? Position 
        {
            get { return position; }
            set { position = value; OnValidateablePropertyChanged(); }
        }
        public String Artist 
        {
            get { return artist; }
            set { artist = value; OnValidateablePropertyChanged(); }
        }
        public String Title 
        {
            get { return title; }
            set { title = value; OnValidateablePropertyChanged(); }
        }
        public TimeSpan? Begin 
        {
            get { return begin; }
            set { begin = value; OnValidateablePropertyChanged(); }
        }
        public TimeSpan? End 
        {
            get { return end; }
            set { end = value; OnValidateablePropertyChanged(); }
        }
        public TimeSpan? Length 
        {
            get
            {
                if ((Begin.HasValue == true) && (End.HasValue == true) && (Begin.Value <= End.Value))
                {
                    return End - Begin;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if ((Begin.HasValue == false) && (End.HasValue == false))
                {
                    Begin = TimeSpan.Zero;
                    End = Begin.Value + value;
                }
                else
                {
                    if ((Begin.HasValue == true) && (End.HasValue == true))
                    {
                        if (Begin.Value > End.Value)
                        {
                            End = Begin.Value + value;
                        }
                    }
                    else
                    {
                        if (End.HasValue == false)
                        {
                            End = Begin.Value + value;
                        }
                        if (Begin.HasValue == false)
                        {
                            Begin = End.Value - value;
                        }
                    }
                }
                OnValidateablePropertyChanged();
            }
        }

        public String GetDisplayNameLocalized(IStringLocalizer<Localization> localizer)
        {
            String identifierString = null;
            if (Position != null)
            {
                identifierString += String.Format("{0}", Position);
            }
            if (identifierString == null)
            {
                if (String.IsNullOrEmpty(Artist) == false)
                {
                    identifierString += String.Format("{0}", Artist);
                }
                if (String.IsNullOrEmpty(Title) == false)
                {
                    if (identifierString != null)
                    {
                        identifierString += ",";
                    }
                    identifierString += String.Format("{0}", Title);
                }
            }
            return String.Format("{0} ({1})", localizer[nameof(Track)], identifierString);
        }

        protected override void Validate()
        {
            if (Position == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "HasNoValue", nameof(Position)));
            }
            if ((Position != null) && (Position == 0))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "HasInvalidValue", nameof(Position)));
            }
            if (String.IsNullOrEmpty(Artist) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Artist)), ValidationErrorType.Warning, "HasNoValue", nameof(Artist)));
            }
            if (String.IsNullOrEmpty(Title) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Title)), ValidationErrorType.Warning, "HasNoValue", nameof(Title)));
            }
            if (Begin == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Error, "HasNoValue", nameof(Begin)));
            }
            else
            {
                if (Begin < TimeSpan.Zero)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Error, "HasInvalidBegin"));
                }
            }
            if (End == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Error, "HasNoValue", nameof(End)));
            }
            else
            {
                if (End < TimeSpan.Zero)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Error, "HasInvalidEnd"));
                }
            }
            if (Length == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Length)), ValidationErrorType.Error, "LengthHasNoValue"));
            }
        }
    }
}
