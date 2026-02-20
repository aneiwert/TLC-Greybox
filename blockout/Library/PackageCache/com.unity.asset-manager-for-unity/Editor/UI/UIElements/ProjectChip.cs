using System;
using Unity.AssetManager.Core.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class ProjectChip : Chip
    {
        Image m_Icon;
        ProjectOrLibraryInfo m_ProjectOrLibraryInfo;

        internal event Action<ProjectOrLibraryInfo> ProjectChipClickAction;

        public ProjectChip(ProjectOrLibraryInfo projectOrLibraryInfo)
            : base(projectOrLibraryInfo.Name)
        {
            m_ProjectOrLibraryInfo = projectOrLibraryInfo;

            m_Icon = new Image();
            m_Icon.style.backgroundColor = ProjectIconDownloader.DefaultColor;
            m_Icon.pickingMode = PickingMode.Ignore;
            Add(m_Icon);
            m_Label.PlaceInFront(m_Icon);

            tooltip = projectOrLibraryInfo.Name;

            RegisterCallback<ClickEvent>(OnClick);
        }

        public void SetIcon(Texture texture)
        {
            if (texture != null)
            {
                m_Icon.image = texture;
            }
            else
            {
                m_Icon.AddToClassList("icon-default-project");
                m_Icon.style.backgroundColor = ProjectIconDownloader.GetProjectIconColor(m_ProjectOrLibraryInfo.Id);
            }
        }

        void OnClick(ClickEvent evt)
        {
            ProjectChipClickAction?.Invoke(m_ProjectOrLibraryInfo);
        }
    }
}
