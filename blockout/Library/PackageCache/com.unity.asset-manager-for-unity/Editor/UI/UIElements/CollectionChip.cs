using Unity.AssetManager.Core.Editor;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class CollectionChip : Chip
    {
        CollectionIdentifier m_CollectionIdentifier;

        public CollectionChip(CollectionIdentifier collection) : base(collection.CollectionPath)
        {
            m_CollectionIdentifier = collection;

            RegisterCallback<ClickEvent>(OnClick);
        }

        void OnClick(ClickEvent evt)
        {
            var organizationProvider = ServicesContainer.instance.Resolve<IProjectOrganizationProvider>();
            organizationProvider.SelectProject(m_CollectionIdentifier.ProjectIdentifier.ProjectId, m_CollectionIdentifier.CollectionPath);
        }
    }
}
