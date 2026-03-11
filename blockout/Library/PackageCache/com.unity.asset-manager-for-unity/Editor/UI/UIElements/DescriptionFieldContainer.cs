using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AssetManager.Core.Editor;
using Unity.AssetManager.Upload.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class DescriptionFieldContainer : AssetFieldContainer
    {
        MultiValueTextField DescriptionField;
        IEnumerable<BaseAssetData> Selection;

        string m_InitialFieldValue;

        public DescriptionFieldContainer(IEnumerable<UploadAssetData> selection, Func<string, ImportedAssetInfo> getImportedAssetInfo, Action<IEnumerable<AssetFieldEdit>> onFieldEdited)
            : base(getImportedAssetInfo, onFieldEdited)
        {
            UpdateField(selection);
        }

        protected override VisualElement CreateFieldElement()
        {
            DescriptionField = new MultiValueTextField(new())
            {
                label = Constants.DescriptionText,
                tooltip = Constants.DescriptionText
            };
            DescriptionField.AddToClassList(UssStyle.MultiAssetDetailsPageEntryRow);
            DescriptionField.AddToClassList(UssStyle.MultiAssetDetailsPageEntryValue);
            return DescriptionField;
        }

        public override void UpdateField(IEnumerable<BaseAssetData> assetDataSelection)
        {
            Selection = assetDataSelection;
            var descriptions = assetDataSelection.Select(assetData => assetData.Description).ToList();
            DescriptionField?.InitializeFieldAsMultiValue(descriptions);
            m_InitialFieldValue = DescriptionField?.value;

            if (DescriptionField != null)
            {
                if (IsDescriptionEdited())
                {
                    m_Borderline.style.backgroundColor = UssStyle.EditedBorderColor;
                    DescriptionField.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                else
                {
                    m_Borderline.style.backgroundColor = Color.clear;
                    DescriptionField.style.unityFontStyleAndWeight = FontStyle.Normal;
                }
            }
        }

        bool IsDescriptionEdited()
        {
            foreach (var asset in Selection)
            {
                var assetId = (asset as UploadAssetData)?.ExistingAssetIdentifier?.AssetId ?? asset.Identifier.AssetId;
                var importedAssetData = m_GetImportedAssetInfo?.Invoke(assetId);
                if (!string.Equals(importedAssetData?.AssetData?.Description, asset.Description, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        public override void Enable()
        {
            DescriptionField?.RegisterCallback<KeyUpEvent>(OnDescriptionKeyUpEvent);
            DescriptionField?.RegisterCallback<FocusOutEvent>(OnDescriptionFocusOutEvent);
        }

        public override void Disable()
        {
            DescriptionField?.UnregisterCallback<KeyUpEvent>(OnDescriptionKeyUpEvent);
            DescriptionField?.UnregisterCallback<FocusOutEvent>(OnDescriptionFocusOutEvent);
        }

        void OnDescriptionKeyUpEvent(KeyUpEvent evt)
        {
            if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                OnValueDescriptionChanged(DescriptionField.value);
        }

        void OnDescriptionFocusOutEvent(FocusOutEvent evt)
        {
            OnValueDescriptionChanged(DescriptionField.value);
        }

        void OnValueDescriptionChanged(string newValue)
        {
            // Catch the scenario where a user focuses out or hits enter without making an edit
            if (string.Equals(newValue, m_InitialFieldValue, StringComparison.Ordinal))
                return;

            var edits = new List<AssetFieldEdit>();
            foreach (var assetData in Selection)
            {
                edits.Add(new AssetFieldEdit(assetData.Identifier, EditField.Description, newValue));
            }

            m_FieldEdited?.Invoke(edits);
        }
    }
}
