using System;
using Unity.AssetManager.Core.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class SideBarAllAssetsFoldout : SideBarFoldout
    {
        readonly Image m_AllAssetsImage;

        internal SideBarAllAssetsFoldout(IStateManager stateManager, IMessageManager messageManager, IProjectOrganizationProvider projectOrganizationProvider,
            string foldoutName, Action onClick = null)
            : base(stateManager, messageManager, projectOrganizationProvider, foldoutName, true)
        {
            AddToClassList("allAssetsFolder");

            RegisterCallback<PointerDownEvent>(e =>
            {
                var target = (VisualElement) e.target;

                // We skip the user's click if they aimed the check mark of the foldout
                // to only select foldouts when they click on it's title/label
                if (e.button != 0 || target.name == k_CheckMarkName)
                    return;

                onClick?.Invoke();
            }, TrickleDown.TrickleDown);
        }

        public override void SetSelected(bool selected)
        {
            base.SetSelected(selected);

            m_Toggle.EnableInClassList(k_UnityListViewItemSelected, selected);
        }
    }
}
