using System;
using Unity.AssetManager.Core.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class SideBarFoldout : Foldout
    {
        protected const string k_UnityListViewItemSelected = "unity-list-view__item--selected";
        protected const string k_CheckMarkName = "unity-checkmark";
        protected const string k_EmptyFoldoutClassName = "sidebar-content-empty";
        const string k_ToggleInputUssClassName = "unity-toggle__input";

        VisualElement m_CheckMark;

        protected readonly IProjectOrganizationProvider m_ProjectOrganizationProvider;
        protected readonly IStateManager m_StateManager;
        protected readonly IMessageManager m_MessageManager;
        protected Toggle m_Toggle;

        protected bool m_HasChild;
        protected bool m_IsPopulated;

        protected SideBarFoldout(IStateManager stateManager, IMessageManager messageManager, IProjectOrganizationProvider projectOrganizationProvider,
            string foldoutName, bool isPopulated)
        {
            m_StateManager = stateManager;
            m_MessageManager = messageManager;
            m_ProjectOrganizationProvider = projectOrganizationProvider;

            text = foldoutName;

            m_HasChild = false;
            m_Toggle = this.Q<Toggle>();
            m_Toggle.tooltip = foldoutName;
            var toggleInput = m_Toggle.Q("", k_ToggleInputUssClassName);
            toggleInput.focusable = false;
            m_CheckMark = m_Toggle.Q<VisualElement>(k_CheckMarkName);

            var iconParent = this.Q(className: inputUssClassName);
            iconParent.pickingMode = PickingMode.Ignore;
            iconParent.Insert(1, new ToolbarSpacer {pickingMode = PickingMode.Ignore, style = {flexShrink = 0}});
            iconParent.Insert(1, new Image {pickingMode = PickingMode.Ignore, style = {flexShrink = 0}});

            MakeFolderOnlyOpenOnCheckMarkClick();
            AddToClassList("removed-arrow");

            SetPopulatedState(isPopulated);
        }

        public virtual void SetSelected(bool selected) { }

        public void SetPopulatedState(bool isPopulated)
        {
            m_IsPopulated = isPopulated;

            // Don't override selection style
            if (!m_Toggle.ClassListContains(k_UnityListViewItemSelected))
            {
                m_Toggle.EnableInClassList(k_EmptyFoldoutClassName, !m_IsPopulated);
            }
        }

        public virtual void AddFoldout(SideBarCollectionFoldout child)
        {
            Add(child);

            if (m_HasChild)
                return;

            m_HasChild = true;
            UIElementsUtils.Show(m_CheckMark);
            RemoveFromClassList("removed-arrow");
        }

        internal void ChangeBackToChildlessFolder()
        {
            if (!m_HasChild)
                return;

            m_HasChild = false;
            UIElementsUtils.Hide(m_CheckMark);
            AddToClassList("removed-arrow");
        }

        void MakeFolderOnlyOpenOnCheckMarkClick()
        {
            var label = m_Toggle.Q<Label>();
            label.pickingMode = PickingMode.Ignore;
            m_Toggle.pickingMode = PickingMode.Ignore;
            m_CheckMark.pickingMode = PickingMode.Position;
            UIElementsUtils.Hide(m_CheckMark);
        }
    }
}
