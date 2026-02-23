using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class EditableDropdownEntry : DetailsPageEntry, IEditableEntry
    {
        public string AssetId { get; }
        public bool IsEditingEnabled { get; private set; }
        public bool AllowMultiSelection { get; }

        DropdownField m_DropdownField;
        List<string> m_Options;
        string m_SelectedOption;

        public event Action<object> EntryEdited;
        public event Func<string, object, bool> IsEntryEdited;

        public EditableDropdownEntry(string assetId, string title, string selectedValue, IEnumerable<string> options, bool allowSelection = false, bool allowMultiSelection = false)
            : base(title, selectedValue, allowSelection)
        {
            AssetId = assetId;
            AllowMultiSelection = allowMultiSelection;
            SetupFields(selectedValue, options);
            EnableEditing(false);
        }

        void SetupFields(string selectedValue, IEnumerable<string> options)
        {
            m_Options = options.ToList();

            // We should always be able to re-select the currently assigned value
            if (!m_Options.Contains(selectedValue))
                m_Options.Insert(0, selectedValue);

            m_DropdownField = new DropdownField(m_Options, selectedValue);
            m_DropdownField.AddToClassList(UssStyle.DetailsPageEntryValue);
            m_DropdownField.AddToClassList(UssStyle.DetailsPageDropdown);
            m_DropdownField.RegisterValueChangedCallback(evt => OnEntryEdited(evt.newValue));
            m_DropdownField.RegisterCallback<GeometryChangedEvent>(OnDropdownGeometryChanged);
            m_DropdownField.style.display = DisplayStyle.None;

            hierarchy.Add(m_DropdownField);

            UpdateStyling(selectedValue);
        }

        void OnDropdownGeometryChanged(GeometryChangedEvent evt)
        {
            // Show tooltip only when the label text has been truncated
            var textElement = m_DropdownField.Q<TextElement>();
            var labelText = textElement.text ?? string.Empty;
            var measured = textElement.MeasureTextSize(
                labelText,
                float.PositiveInfinity,
                MeasureMode.Undefined,
                1,
                MeasureMode.Undefined
            );
            var available = textElement.contentRect.width;
            var isTruncated = measured.x > available;
            tooltip = isTruncated ? labelText : null;
        }

        public void EnableEditing(bool enable)
        {
            m_Text.style.display = enable ? DisplayStyle.None : DisplayStyle.Flex;
            m_DropdownField.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;

            if (!enable)
            {
                m_Text.text = m_DropdownField.value;
                style.display = string.IsNullOrWhiteSpace(m_DropdownField.value) ? DisplayStyle.None : DisplayStyle.Flex;
            }
            else
            {
                m_DropdownField.value = m_Text.text;
                UpdateStyling(m_DropdownField.value);
            }

            IsEditingEnabled = enable;
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
                m_DropdownField.AddToClassList(UssStyle.DetailsPageEntryValueEdited);
            }
            else
            {
                m_BorderLine.style.backgroundColor = Color.clear;
                m_DropdownField.RemoveFromClassList(UssStyle.DetailsPageEntryValueEdited);
            }
        }
    }
}
