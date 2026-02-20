using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class ChipListField : VisualElement
    {
        TextField m_TextField { get; }
        VisualElement m_ChipContainer;
        VisualElement m_TextInput;
        TextElement m_InputTextElement;

        HashSet<string> m_Values;

        public event Action<string> ChipAdded;
        public event Action<string> ChipRemoved;

        public ChipListField(HashSet<string> values, string label = null)
        {
            m_Values = values;

            m_TextField = new TextField();
            if (label != null)
            {
                m_TextField.label = label;
                m_TextField.tooltip = label;
            }
            m_TextField.RegisterCallback<KeyUpEvent>(OnKeyUpEvent);
            m_TextField.RegisterCallback<FocusOutEvent>(_ => OnEntryAdded(m_TextField.value));

            m_ChipContainer = new VisualElement();
            m_ChipContainer.AddToClassList(UssStyle.DetailsPageChipContainer);
            m_ChipContainer.AddToClassList(UssStyle.FlexWrap);
            m_ChipContainer.focusable = false;
            m_ChipContainer.pickingMode = PickingMode.Ignore;

            m_TextInput = m_TextField.Q("unity-text-input");
            if (m_TextInput != null)
            {
                m_InputTextElement = m_TextInput.Q<TextElement>();
                m_TextInput.style.flexDirection = FlexDirection.Column;
                m_TextInput.Insert(0, m_ChipContainer);
            }
            else
            {
                m_TextField.Add(m_ChipContainer);
            }

            Add(m_TextField);
            RegisterCallback<PointerUpEvent>(OnParentFieldClicked);
        }

        public void UpdateChips(IEnumerable<string> values, bool insertMultiValueChip = false)
        {
            m_Values = values.ToHashSet();

            m_ChipContainer.Clear();

            if (insertMultiValueChip)
                m_ChipContainer.Add(CreateMixedValueChip());

            foreach (var chipText in values)
            {
                var chip = EditChipCreator(chipText);
                if (chip != null)
                    m_ChipContainer.Add(chip);
            }
        }

        void OnKeyUpEvent(KeyUpEvent evt)
        {
            if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                OnEntryAdded(m_TextField.value);
        }

        void OnParentFieldClicked(PointerUpEvent evt)
        {
            evt.StopImmediatePropagation();
            FocusOnTextInput();
        }

        void FocusOnTextInput()
        {
            // This method is meant to forward focus to the actual text input element.
            // Necessary because the chip container exists within the text field
            // and was otherwise stealing focus from the underlying text element.

            var retryCount = 0;

            m_TextField.schedule.Execute(() =>
            {
                if (m_TextField.panel == null)
                    return;

                m_TextField.schedule.Execute(() =>
                {
                    m_InputTextElement.Focus();
                    m_TextField.SelectAll();
                }).StartingIn(0);

            }).Until(() => m_TextField.panel != null && retryCount++ < 10);
        }

        void OnEntryAdded(string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue) || m_Values.Contains(newValue))
                return;

            ChipAdded?.Invoke(newValue);
            m_TextField.value = string.Empty;
        }

        void OnEntryRemoved(string value)
        {
            if (!m_Values.Contains(value))
                return;

            ChipRemoved?.Invoke(value);
        }

        Chip EditChipCreator(string chipText)
        {
            var chip = new Chip(chipText, isDismissable:true);
            chip.ChipDismissed += OnEntryRemoved;

            return chip;
        }

        static Chip CreateMixedValueChip()
        {
            var chip = new Chip("— Mixed", isDismissable: false);
            chip.style.unityFontStyleAndWeight = FontStyle.BoldAndItalic;
            return chip;
        }
    }
}
