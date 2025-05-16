using AudioCuesheetEditor.Model.UI;

namespace AudioCuesheetEditor.Services.UI
{
    /// <summary>
    /// Manager for Undo and Redo operations on objects.
    /// </summary>
    public interface ITraceChangeManager
    {
        public event EventHandler? TracedObjectHistoryChanged;
        public event EventHandler? UndoDone;
        public event EventHandler? RedoDone;
        /// <summary>
        /// Is Undo() currently possible (are there any changes)?
        /// </summary>
        public bool CanUndo { get; }
        /// <summary>
        /// Is Redo() currently possible (are there any changes)?
        /// </summary>
        public bool CanRedo { get; }
        public bool BulkEdit { get; set; }
        public void TraceChanges(ITraceable traceable);
        public void Reset();
        public void Undo();
        public void Redo();
        public void MergeLastEditWithEdit(Func<TracedChanges, bool> targetEdit);
    }
}
