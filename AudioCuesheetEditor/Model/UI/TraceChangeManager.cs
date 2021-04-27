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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.UI
{
    public class TracedChange
    {
        public TracedChange(object traceableObject, object propertyValue, string propertyName)
        {
            if (traceableObject == null)
            {
                throw new ArgumentNullException(nameof(traceableObject));
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            TraceableObject = traceableObject;
            PropertyValue = propertyValue;
            PropertyName = propertyName;
        }
        public object TraceableObject { get; init; }
        public object PropertyValue { get; init; }
        public string PropertyName { get; init; }
    }
    public class TraceChangeManager
    {
        private static TraceChangeManager instance;
        public static TraceChangeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TraceChangeManager();
                }
                return instance;
            }
        }

        private readonly List<ITraceable> tracedObjects = new();

        private readonly Stack<TracedChange> undoStack = new();
        private readonly Stack<TracedChange> redoStack = new();

        public Boolean CurrentlyHandlingChanges { get; private set; }
        /// <summary>
        /// Is Undo() currently possible (are there any changes)?
        /// </summary>
        public Boolean CanUndo
        {
            get
            {
                //TODO
                return false;
            }
        }

        /// <summary>
        /// Is Redo() currently possible (are there any changes)?
        /// </summary>
        public Boolean CanRedo
        {
            get
            {
                //TODO
                return false;
            }
        }

        private TraceChangeManager() 
        {
            CurrentlyHandlingChanges = false;
        }

        private void Traceable_TraceablePropertyChanged(object sender, TraceablePropertyChangedEventArgs e)
        {
            if (CurrentlyHandlingChanges == false)
            {
                undoStack.Push(new TracedChange(sender, e.PreviousValue, e.PropertyName));
            }
        }

        public void TraceChanges(ITraceable traceable)
        {
            if (traceable == null)
            {
                throw new ArgumentNullException(nameof(traceable));
            }
            traceable.TraceablePropertyChanged += Traceable_TraceablePropertyChanged;
            tracedObjects.Add(traceable);
        }

        public void Undo()
        {
            CurrentlyHandlingChanges = true;
            var changes = undoStack.Pop();
            redoStack.Push(changes);
            var propertyInfo = changes.TraceableObject.GetType().GetProperty(changes.PropertyName);
            propertyInfo.SetValue(changes.TraceableObject, changes.PropertyValue);
            CurrentlyHandlingChanges = false;
        }

        public void Redo()
        {
            //TODO
        }        
    }
}
