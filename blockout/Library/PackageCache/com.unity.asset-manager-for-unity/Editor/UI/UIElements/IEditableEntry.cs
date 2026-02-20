using System;

namespace Unity.AssetManager.UI.Editor
{
    interface IEditableEntry
    {
        string AssetId { get; }
        bool IsEditingEnabled { get; }
        bool AllowMultiSelection { get; }
        event Action<object> EntryEdited;
        event Func<string, object, bool> IsEntryEdited;
        void EnableEditing(bool enable);
    }
}
