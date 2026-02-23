using System;
using System.Collections.Generic;
using Unity.AssetManager.Core.Editor;
using Unity.AssetManager.Upload.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    abstract class AssetFieldContainer
    {
        protected VisualElement m_Container;
        protected VisualElement m_Borderline;
        protected VisualElement m_FieldElement;

        protected Func<string, ImportedAssetInfo> m_GetImportedAssetInfo;
        protected Action<IEnumerable<AssetFieldEdit>> m_FieldEdited;
        internal VisualElement Root => m_Container;

        protected AssetFieldContainer(Func<string, ImportedAssetInfo> getImportedAssetInfo, Action<IEnumerable<AssetFieldEdit>> onFieldEdited)
        {
            m_GetImportedAssetInfo = getImportedAssetInfo;
            m_FieldEdited = onFieldEdited;

            m_Container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    alignItems = Align.Stretch
                }
            };

            m_Borderline = new VisualElement();
            m_Borderline.AddToClassList("asset-entry-borderline-style");
            m_Container.Add(m_Borderline);

            m_FieldElement = CreateFieldElement();
            m_Container.Add(m_FieldElement);
        }

        protected abstract VisualElement CreateFieldElement();
        public abstract void UpdateField(IEnumerable<BaseAssetData> assetDataSelection);
        public abstract void Enable();
        public abstract void Disable();
    }
}
