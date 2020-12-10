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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class Track : Validateable
    {
        private readonly CuesheetController _cuesheetController;

        private String artist;
        private String title;
        private TimeSpan? begin;
        private TimeSpan? end;
        public Track(CuesheetController cuesheetController)
        {
            _cuesheetController = cuesheetController;
            Position = _cuesheetController.GetNextFreePosition();
            Validate();
        }
        public uint Position { get; private set; }
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

        protected override void Validate()
        {
            if (String.IsNullOrEmpty(Artist) == true)
            {
                validationErrors.Add(new ValidationError(String.Format(_cuesheetController.GetLocalizedString("HasNoValue"), _cuesheetController.GetLocalizedString("Artist")), nameof(Artist), ValidationErrorType.Warning));
            }
            if (String.IsNullOrEmpty(Title) == true)
            {
                validationErrors.Add(new ValidationError(String.Format(_cuesheetController.GetLocalizedString("HasNoValue"), _cuesheetController.GetLocalizedString("Title")), nameof(Title), ValidationErrorType.Warning));
            }
            if (Begin == null)
            {
                validationErrors.Add(new ValidationError(String.Format(_cuesheetController.GetLocalizedString("HasNoValue"), _cuesheetController.GetLocalizedString("Begin")), nameof(Begin), ValidationErrorType.Error));
            }
            if (End == null)
            {
                validationErrors.Add(new ValidationError(String.Format(_cuesheetController.GetLocalizedString("HasNoValue"), _cuesheetController.GetLocalizedString("End")), nameof(End), ValidationErrorType.Error));
            }
            //TODO: more Validation
        }
    }
}
