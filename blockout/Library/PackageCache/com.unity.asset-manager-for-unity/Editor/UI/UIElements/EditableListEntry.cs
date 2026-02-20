using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    static partial class UssStyle
    {
        public const string DetailsPageChipField = "details-page-chip-field";
    }

    class EditableListEntry : DetailsPageEntry, IEditableEntry
    {
        List<VisualElement> m_ReadOnlyFields = new();
        List<VisualElement> m_EditFields = new();

        public string AssetId { get; }
        public bool IsEditingEnabled { get; private set; }
        public bool AllowMultiSelection { get; }
        public event Action<object> EntryEdited;
        public event Func<string, object, bool> IsEntryEdited;

        ChipListField m_TagField;
        Func<string, Chip> m_ChipCreator;

        HashSet<string> m_Values;

        public EditableListEntry(string assetId, string title, IEnumerable<string> values, Func<string, Chip> chipCreator)
            : base(title)
        {
            AssetId = assetId;
            m_Values = values.ToHashSet();
            m_ChipCreator = chipCreator;

            m_ChipContainer = AddChipContainer();
            m_ChipContainer.AddToClassList(UssStyle.FlexWrap);
            m_ReadOnlyFields.Add(m_ChipContainer);

            m_TagField = new ChipListField(m_Values);
            m_TagField.ChipAdded += OnChipAdded;
            m_TagField.ChipRemoved += OnChipRemoved;
            m_TagField.AddToClassList(UssStyle.DetailsPageChipField);
            m_TagField.AddToClassList(UssStyle.DetailsPageEntryValue);
            m_TagField.style.display = DisplayStyle.None;
            hierarchy.Add(m_TagField);

            m_EditFields.Add(m_TagField);

            EnableEditing(false);
        }

        public EditableListEntry(string assetId, string title, IEnumerable<string> values, Func<string, Chip> chipCreator, bool allowMultiSelection = false)
            : this(assetId, title, values, chipCreator)
        {
            AllowMultiSelection = allowMultiSelection;
        }

        public void EnableEditing(bool enable)
        {
            IsEditingEnabled = enable;

            if (enable)
            {
                ToggleEditField();
            }
            else
            {
                ToggleReadonlyField();
            }

        }

        void ToggleEditField()
        {
            m_EditFields.ForEach(x => x.style.display = DisplayStyle.Flex);
            m_ReadOnlyFields.ForEach(x => x.style.display = DisplayStyle.None);

            UpdateEditableChipContainer();
            UpdateStyling();
        }

        void ToggleReadonlyField()
        {
            m_EditFields.ForEach(x => x.style.display = DisplayStyle.None);
            m_ReadOnlyFields.ForEach(x => x.style.display = DisplayStyle.Flex);

            UpdateReadonlyChipContainer();
            UpdateStyling();
        }

        void UpdateEditableChipContainer()
            => m_TagField.UpdateChips(m_Values);

        void UpdateReadonlyChipContainer()
        {
            m_ChipContainer.Clear();
            foreach (var value in m_Values)
            {
                var chip = m_ChipCreator.Invoke(value);
                if (chip != null)
                    m_ChipContainer.Add(chip);
            }
        }

        void OnChipAdded(string chip)
        {
            m_Values.Add(chip);
            EntryEdited?.Invoke(m_Values);
        }

        void OnChipRemoved(string chip)
        {
            m_Values.Remove(chip);
            EntryEdited?.Invoke(m_Values);
        }

        void UpdateStyling()
        {
            var isEdited = IsEditingEnabled && (IsEntryEdited?.Invoke(AssetId, m_Values) ?? false);
            if (isEdited)
            {
                m_BorderLine.style.backgroundColor = UssStyle.EditedBorderColor;
                m_TagField.AddToClassList(UssStyle.DetailsPageEntryValueEdited);
            }
            else
            {
                m_BorderLine.style.backgroundColor = Color.clear;
                m_TagField.RemoveFromClassList(UssStyle.DetailsPageEntryValueEdited);
            }
        }
    }
}
