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
    /// <summary>
    /// Class for tracing changes on an object
    /// </summary>
    public class TracedChange
    {
        private readonly WeakReference<ITraceable> _tracedObject;
        public TracedChange(ITraceable traceableObject, object propertyValue, string propertyName)
        {
            if (traceableObject == null)
            {
                throw new ArgumentNullException(nameof(traceableObject));
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            _tracedObject = new WeakReference<ITraceable>(traceableObject, false);
            PropertyValue = propertyValue;
            PropertyName = propertyName;
        }
        public ITraceable TraceableObject
        {
            get
            {
                if (_tracedObject.TryGetTarget(out var traceable))
                {
                    return traceable;
                }
                return null;
            }
        }
        public object PropertyValue { get; }
        public string PropertyName { get; }
    }

    /// <summary>
    /// Manager for Undo and Redo operations on objects.
    /// </summary>
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

        private readonly Stack<TracedChange> undoStack = new();
        private readonly Stack<TracedChange> redoStack = new();

        public Boolean CurrentlyHandlingRedoOrUndoChanges { get; private set; }
        /// <summary>
        /// Is Undo() currently possible (are there any changes)?
        /// </summary>
        public Boolean CanUndo
        {
            get
            {
                //TODO: Maybe also check if the undoStack references deleted objects?
                return undoStack.Count > 0;
            }
        }

        /// <summary>
        /// Is Redo() currently possible (are there any changes)?
        /// </summary>
        public Boolean CanRedo
        {
            get
            {
                //TODO: Maybe also check if the redoStack references deleted objects?
                return redoStack.Count > 0;
            }
        }

        private TraceChangeManager() 
        {
            CurrentlyHandlingRedoOrUndoChanges = false;
        }

        private void Traceable_TraceablePropertyChanged(object sender, TraceablePropertyChangedEventArgs e)
        {
            if (CurrentlyHandlingRedoOrUndoChanges == false)
            {
                undoStack.Push(new TracedChange((ITraceable) sender, e.PreviousValue, e.PropertyName));
                redoStack.Clear();
            }
        }

        public void Reset()
        {
            //Disconnect all
            foreach (var tracedObject in undoStack.Select(x => x.TraceableObject))
            {
                if (tracedObject != null)
                {
                    tracedObject.TraceablePropertyChanged -= Traceable_TraceablePropertyChanged;
                }
            }
            foreach (var tracedObject in redoStack.Select(x => x.TraceableObject))
            {
                if (tracedObject != null)
                {
                    tracedObject.TraceablePropertyChanged -= Traceable_TraceablePropertyChanged;
                }
            }
            undoStack.Clear();
            redoStack.Clear();
        }

        public void TraceChanges(ITraceable traceable)
        {
            if (traceable == null)
            {
                throw new ArgumentNullException(nameof(traceable));
            }
            traceable.TraceablePropertyChanged += Traceable_TraceablePropertyChanged;
        }

        public void Undo()
        {
            CurrentlyHandlingRedoOrUndoChanges = true;
            TracedChange changes;
            do
            {
                changes = undoStack.Pop();
            } while ((undoStack.Count > 0) && (changes.TraceableObject == null));
            if ((changes != null) && (changes.TraceableObject != null))
            {
                redoStack.Push(changes);
                var propertyInfo = changes.TraceableObject.GetType().GetProperty(changes.PropertyName);
                propertyInfo.SetValue(changes.TraceableObject, changes.PropertyValue);
            }
            CurrentlyHandlingRedoOrUndoChanges = false;
        }

        public void Redo()
        {
            //TODO
        }
    }
}
