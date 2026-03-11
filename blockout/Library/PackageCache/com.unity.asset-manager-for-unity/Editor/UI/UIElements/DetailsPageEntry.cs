using System;
using System.Linq;
using Unity.AssetManager.Core.Editor;
using Unity.AssetManager.Upload.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    static partial class UssStyle
    {
        public const string DetailsPageEntry = "details-page-entry";
        public const string DetailsPageEntryLabel = "details-page-entry-label";
        public const string DetailsPageEntryValue = "details-page-entry-value";
        public const string DetailsPageChipContainer = "details-page-chip-container";
        public const string DetailsPageDropdown = "details-page-entry-dropdown";
        public const string DetailsPageEntryValueText = "details-page-entry-value-text";
        public const string DetailsPageEntryValueEdited = "details-page-entry-value-text--edited";
        public const string ClipboardButton = "ClipboardButton";
        public const string ClipboardButtonContainer = "unity-clipboard-button-container";
        public const string ClipboardButtonIcon = "unity-clipboard-button-icon";
        public static readonly Color EditedBorderColor = new(0.02f, 0.58f, 0.88f);
    }

    class DetailsPageEntry : VisualElement
    {
        protected VisualElement m_BorderLine;
        protected Label m_Title;
        protected Label m_Text;
        protected VisualElement m_ChipContainer;
        protected Button m_ClipboardButton;

        public DetailsPageEntry(string title)
        {
            AddToClassList(UssStyle.DetailsPageEntry);

            if (!string.IsNullOrEmpty(title))
            {
                m_BorderLine = new VisualElement();
                m_BorderLine.AddToClassList("asset-entry-borderline-style");

                m_Title = new Label(L10n.Tr(title))
                {
                    name = "entry-label",
                    tooltip = title
                };
                m_Title.AddToClassList(UssStyle.DetailsPageEntryLabel);
                hierarchy.Add(m_BorderLine);
                hierarchy.Add(m_Title);
            }
        }

        public DetailsPageEntry(string title, string details, bool allowSelection = false)
            : this(title)
        {
            m_Text = new Label(L10n.Tr(details))
            {
                name = "entry-value",
                selection = { isSelectable = allowSelection }
            };
            m_Text.AddToClassList(UssStyle.DetailsPageEntryValue);

            if (string.IsNullOrEmpty(title))
            {
                m_Text.AddToClassList(UssStyle.DetailsPageEntryValueText);
            }
            hierarchy.Add(m_Text);

            if (allowSelection)
                SetupCopy();
        }

        public VisualElement AddChipContainer()
        {
            m_ChipContainer = new VisualElement();
            m_ChipContainer.AddToClassList(UssStyle.DetailsPageChipContainer);
            hierarchy.Add(m_ChipContainer);

            return m_ChipContainer;
        }

        public void SetText(string text)
        {
            if (m_Text != null)
            {
                m_Text.text = text;
            }
        }

        void SetupCopy()
        {
            m_ClipboardButton = new Button(CopyTextToClipBoard);
            m_ClipboardButton.AddToClassList(UssStyle.ClipboardButton);
            m_ClipboardButton.focusable = false;
            m_ClipboardButton.style.display = DisplayStyle.None;
            m_ClipboardButton.tooltip = L10n.Tr("Copy field text to clipboard");

            var container = new VisualElement();
            container.AddToClassList(UssStyle.ClipboardButtonContainer);
            m_ClipboardButton.Add(container);

            var icon = new VisualElement();
            icon.AddToClassList(UssStyle.ClipboardButtonIcon);
            container.Add(icon);

            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

            hierarchy.Add(m_ClipboardButton);
        }

        void OnMouseEnter(MouseEnterEvent _)
            => m_ClipboardButton.style.display = DisplayStyle.Flex;

        void OnMouseLeave(MouseLeaveEvent _)
         => m_ClipboardButton.style.display = DisplayStyle.None;

        void CopyTextToClipBoard()
        {
            if (m_Text != null && !string.IsNullOrEmpty(m_Text.text))
            {
                EditorGUIUtility.systemCopyBuffer = m_Text.text;
                Debug.Log($"Copied to clipboard: {m_Text.text}");
            }
        }
    }
}
