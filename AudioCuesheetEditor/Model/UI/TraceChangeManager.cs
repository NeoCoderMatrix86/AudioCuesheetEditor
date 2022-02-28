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
        public TracedChange(ITraceable traceableObject, Stack<TraceableChange> traceableChanges)
        {
            if (traceableObject == null)
            {
                throw new ArgumentNullException(nameof(traceableObject));
            }
            if (traceableChanges == null)
            {
                throw new ArgumentNullException(nameof(traceableChanges));
            }
            _tracedObject = new WeakReference<ITraceable>(traceableObject, false);
            TraceableChanges = traceableChanges;
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

        public Stack<TraceableChange> TraceableChanges { get; }
    }

    /// <summary>
    /// Manager for Undo and Redo operations on objects.
    /// </summary>
    public class TraceChangeManager
    {
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
                return redoStack.Count > 0;
            }
        }

        public TraceChangeManager() 
        {
            CurrentlyHandlingRedoOrUndoChanges = false;
        }

        public void TraceChanges(ITraceable traceable)
        {
            if (traceable == null)
            {
                throw new ArgumentNullException(nameof(traceable));
            }
            traceable.TraceablePropertyChanged += Traceable_TraceablePropertyChanged;
        }

        public void Reset()
        {
            undoStack.ToList().Where(x => x.TraceableObject != null).ToList().ForEach(x => x.TraceableObject.TraceablePropertyChanged -= Traceable_TraceablePropertyChanged);
            redoStack.ToList().Where(x => x.TraceableObject != null).ToList().ForEach(x => x.TraceableObject.TraceablePropertyChanged -= Traceable_TraceablePropertyChanged);
            undoStack.Clear();
            redoStack.Clear();
        }

        public void Undo()
        {
            CurrentlyHandlingRedoOrUndoChanges = true;
            TracedChange changes = null;
            while ((undoStack.Count > 0) && (changes == null))
            {
                changes = undoStack.Pop();
                if (changes.TraceableObject == null)
                {
                    changes = null;
                }    
            }
            if ((changes != null) && (changes.TraceableObject != null))
            {
                var redoChanges = new Stack<TraceableChange>();
                do
                {
                    var change = changes.TraceableChanges.Pop();
                    var propertyInfo = changes.TraceableObject.GetType().GetProperty(change.PropertyName);
                    var currentValue = propertyInfo.GetValue(changes.TraceableObject);
                    redoChanges.Push(new TraceableChange(currentValue, change.PropertyName));
                    propertyInfo.SetValue(changes.TraceableObject, change.PreviousValue);
                } while (changes.TraceableChanges.Count > 0);
                //Push the old value to redo stack
                redoStack.Push(new TracedChange(changes.TraceableObject, redoChanges));
            }
            CurrentlyHandlingRedoOrUndoChanges = false;
        }

        public void Redo()
        {
            CurrentlyHandlingRedoOrUndoChanges = true;
            TracedChange changes = null;
            while ((redoStack.Count > 0) && (changes == null))
            {
                changes = redoStack.Pop();
                if (changes.TraceableObject == null)
                {
                    changes = null;
                }
            }
            if ((changes != null) && (changes.TraceableObject != null))
            {
                var undoChanges = new Stack<TraceableChange>();
                do
                {
                    var change = changes.TraceableChanges.Pop();
                    var propertyInfo = changes.TraceableObject.GetType().GetProperty(change.PropertyName);
                    var currentValue = propertyInfo.GetValue(changes.TraceableObject);
                    undoChanges.Push(new TraceableChange(currentValue, change.PropertyName));
                    propertyInfo.SetValue(changes.TraceableObject, change.PreviousValue);
                } while (changes.TraceableChanges.Count > 0);
                //Push the old value to undo stack
                undoStack.Push(new TracedChange(changes.TraceableObject, undoChanges));
            }
            CurrentlyHandlingRedoOrUndoChanges = false;
        }

        private void Traceable_TraceablePropertyChanged(object sender, TraceablePropertiesChangedEventArgs e)
        {
            if (CurrentlyHandlingRedoOrUndoChanges == false)
            {
                undoStack.Push(new TracedChange((ITraceable)sender, e.TraceableChanges));
                redoStack.Clear();
            }
        }
    }
}
