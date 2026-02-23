using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class Chip : VisualElement
    {
        const string k_ChipUssClass = "details-page-chip";
        const string k_DismissButtonUssClass = "details-page-chip__dismiss_button";
        protected Label m_Label;

        internal event Action<string> ChipDismissed;

        public Chip(string text, bool isSelectable = false, bool isDismissable = false)
        {
            m_Label = new Label(text)
            {
                pickingMode = !isSelectable ? PickingMode.Ignore : PickingMode.Position,
                selection =
                {
                    isSelectable = isSelectable
                }
            };
            Add(m_Label);

            if (isDismissable)
            {
                var dismissButton = new Button(() => ChipDismissed?.Invoke(m_Label.text));
                dismissButton.AddToClassList(k_DismissButtonUssClass);
                Add(dismissButton);
            }

            AddToClassList(k_ChipUssClass);

            RegisterCallback<GeometryChangedEvent>(OnChipGeometryChanged);
        }

        void OnChipGeometryChanged(GeometryChangedEvent evt)
        {
            // Show tooltip only when the label text has been truncated
            var labelText = m_Label.text ?? string.Empty;
            var measured = m_Label.MeasureTextSize(
                labelText,
                float.PositiveInfinity,
                MeasureMode.Undefined,
                1,
                MeasureMode.Undefined
            );
            var available = m_Label.contentRect.width;
            var isTruncated = measured.x > available;
            tooltip = isTruncated ? labelText : null;
        }
    }
}
