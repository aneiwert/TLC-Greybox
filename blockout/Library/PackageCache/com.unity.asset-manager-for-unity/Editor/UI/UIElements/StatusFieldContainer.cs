using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AssetManager.Core.Editor;
using Unity.AssetManager.Upload.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class StatusFieldContainer : AssetFieldContainer
    {
        bool m_HasStatusFlowMismatch;
        string m_InitialFieldValue;
        MultiValueDropdownField m_StatusField;
        Image m_WarningIcon;
        IEnumerable<BaseAssetData> Selection;

        public StatusFieldContainer(IEnumerable<UploadAssetData> selection,
            Func<string, ImportedAssetInfo> getImportedAssetInfo, Action<IEnumerable<AssetFieldEdit>> onFieldEdited)
            : base(getImportedAssetInfo, onFieldEdited)
        {
            UpdateField(selection);
        }

        protected override VisualElement CreateFieldElement()
        {
            var container = new VisualElement();
            container.AddToClassList("status-field-container");

            m_StatusField = new MultiValueDropdownField(new List<string>(), new List<string>())
            {
                label = Constants.StatusText,
                tooltip = Constants.StatusText
            };
            m_StatusField.AddToClassList(UssStyle.MultiAssetDetailsPageEntryRow);
            m_StatusField.AddToClassList(UssStyle.MultiAssetDetailsPageEntryValue);
            m_StatusField.AddToClassList("status-field-dropdown");
            m_StatusField.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            container.Add(m_StatusField);

            m_WarningIcon = new Image();
            m_WarningIcon.AddToClassList("status-flow-mismatch-warning-icon");
            UIElementsUtils.Hide(m_WarningIcon);
            container.Add(m_WarningIcon);

            return container;
        }

        public override void UpdateField(IEnumerable<BaseAssetData> assetDataSelection)
        {
            Selection = assetDataSelection;
            var assetsList = assetDataSelection.ToList();

            if (!assetsList.Any())
                return;

            m_HasStatusFlowMismatch = HasStatusFlowMismatch(assetsList);
            if (m_HasStatusFlowMismatch)
            {
                m_StatusField.SetEnabled(false);
                m_StatusField.value = "Status flow mismatch";
                m_StatusField.showMixedValue = false;
                UIElementsUtils.Show(m_WarningIcon);
                m_Container.tooltip =
                    "The selected assets do not support the same status flows and have different status options. They cannot be bulk edited together.";
            }
            else
            {
                m_StatusField.SetEnabled(true);
                UIElementsUtils.Hide(m_WarningIcon);

                var statuses = assetsList.Select(assetData => assetData.Status).ToList();

                var firstAsset = assetsList.First();
                var availableStatusNames = firstAsset.ReachableStatusNames?.ToList() ?? new List<string>();

                foreach (var status in statuses.Where(s => !string.IsNullOrWhiteSpace(s)))
                    if (!availableStatusNames.Contains(status))
                        availableStatusNames.Insert(0, status);

                m_StatusField.choices = availableStatusNames;
                m_StatusField?.InitializeFieldAsMultiValue(statuses);
                m_InitialFieldValue = m_StatusField?.value;

                if (m_StatusField != null)
                {
                    if (IsStatusEdited())
                    {
                        m_Borderline.style.backgroundColor = UssStyle.EditedBorderColor;
                        m_StatusField.style.unityFontStyleAndWeight = FontStyle.Bold;
                    }
                    else
                    {
                        m_Borderline.style.backgroundColor = Color.clear;
                        m_StatusField.style.unityFontStyleAndWeight = FontStyle.Normal;
                    }
                }
            }
        }

        static bool HasStatusFlowMismatch(List<BaseAssetData> assetsList)
        {
            if (assetsList.Count <= 1)
                return false;

            var firstStatusFlowId = assetsList.First().StatusFlowId;

            return assetsList.Any(asset => asset.StatusFlowId != firstStatusFlowId);
        }

        bool IsStatusEdited()
        {
            foreach (var asset in Selection)
            {
                var assetId = (asset as UploadAssetData)?.ExistingAssetIdentifier?.AssetId ?? asset.Identifier.AssetId;
                var importedAssetData = m_GetImportedAssetInfo?.Invoke(assetId);
                if (!string.Equals(importedAssetData?.AssetData?.Status, asset.Status, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        public override void Enable()
        {
            m_StatusField?.RegisterCallback<ChangeEvent<string>>(OnStatusChangeEvent);
        }

        public override void Disable()
        {
            m_StatusField?.UnregisterCallback<ChangeEvent<string>>(OnStatusChangeEvent);
        }

        void OnStatusChangeEvent(ChangeEvent<string> evt)
        {
            OnStatusChanged(evt.newValue);
        }

        void OnStatusChanged(string newValue)
        {
            if (m_HasStatusFlowMismatch)
                return;

            if (string.Equals(newValue, m_InitialFieldValue, StringComparison.Ordinal))
                return;

            var edits = new List<AssetFieldEdit>();
            foreach (var assetData in Selection)
                edits.Add(new AssetFieldEdit(assetData.Identifier, EditField.Status, newValue));

            m_FieldEdited?.Invoke(edits);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (m_HasStatusFlowMismatch)
                return;

            var visualInput = m_StatusField.Q(className: "unity-base-field__input");
            var textElement = visualInput?.Q<TextElement>();
            if (textElement == null)
                return;

            var labelText = textElement.text ?? string.Empty;
            var measured = textElement.MeasureTextSize(
                labelText,
                float.PositiveInfinity,
                VisualElement.MeasureMode.Undefined,
                1,
                VisualElement.MeasureMode.Undefined
            );
            var available = textElement.contentRect.width;
            var isTruncated = measured.x > available;

            m_Container.tooltip = isTruncated ? labelText : null;
        }
    }
}
