using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class EditableTextEntry : DetailsPageEntry, IEditableEntry
    {
        public string AssetId { get; }
        public bool IsEditingEnabled { get; private set; }
        public bool AllowMultiSelection { get; }

        TextField m_TextField;

        public event Action<object> EntryEdited;
        public event Func<string, object, bool> IsEntryEdited;

        public EditableTextEntry(string assetId, string title, string details, bool allowSelection = false)
            : base(title, details, allowSelection)
        {
            AssetId = assetId;
            SetupEditField(details);
            EnableEditing(false);
        }

        public EditableTextEntry(string assetId, string title, string details, bool allowSelection = false, bool allowMultiSelection = false)
            : this(assetId, title, details, allowSelection)
        {
            AllowMultiSelection = allowMultiSelection;
        }

        void SetupEditField(string details)
        {
            m_TextField = new TextField()
            {
                value = details,
            };
            m_TextField.AddToClassList(UssStyle.DetailsPageEntryValue);
            m_TextField.RegisterCallback<KeyUpEvent>(OnKeyUpEvent);
            m_TextField.RegisterCallback<FocusOutEvent>(_ => OnEntryEdited(m_TextField.value));
            m_TextField.style.display = DisplayStyle.None;

            UpdateStyling(details);

            hierarchy.Add(m_TextField);
        }

        public void EnableEditing(bool enable)
        {
            if (enable == IsEditingEnabled)
                return;

            m_TextField.value = m_Text.text;

            m_TextField.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
            m_Text.style.display = enable ? DisplayStyle.None : DisplayStyle.Flex;

            // Disable the entire entry if the value is empty
            if (!enable)
                style.display = string.IsNullOrWhiteSpace(m_Text.text) ? DisplayStyle.None : DisplayStyle.Flex;
            else
                UpdateStyling(m_Text.text);

            IsEditingEnabled = enable;
        }

        void OnKeyUpEvent(KeyUpEvent evt)
        {
            if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                OnEntryEdited(m_TextField.value);
        }

        void OnEntryEdited(string newValue)
        {
            if (newValue == m_Text.text)
                return;

            m_Text.text = newValue;
            EntryEdited?.Invoke(newValue);
            UpdateStyling(newValue);
        }

        void UpdateStyling(string value)
        {
            var isEdited = IsEntryEdited?.Invoke(AssetId, value) ?? false;
            if (isEdited)
            {
                m_BorderLine.style.backgroundColor = UssStyle.EditedBorderColor;
                m_TextField.AddToClassList(UssStyle.DetailsPageEntryValueEdited);
            }
            else
            {
                m_BorderLine.style.backgroundColor = Color.clear;
                m_TextField.RemoveFromClassList(UssStyle.DetailsPageEntryValueEdited);
            }
        }
    }
}
