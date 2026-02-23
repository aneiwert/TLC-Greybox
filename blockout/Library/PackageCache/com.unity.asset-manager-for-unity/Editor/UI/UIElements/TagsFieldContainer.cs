using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AssetManager.Core.Editor;
using Unity.AssetManager.Upload.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class TagsFieldContainer : AssetFieldContainer
    {
        ChipListField TagsField;
        IEnumerable<BaseAssetData> Selection;
        HashSet<string> CommonTags = new();

        public TagsFieldContainer(IEnumerable<UploadAssetData> selection, Func<string, ImportedAssetInfo> getImportedAssetInfo,  Action<IEnumerable<AssetFieldEdit>> onFieldEdited)
            : base(getImportedAssetInfo,  onFieldEdited)
        {
            UpdateField(selection);
        }

        protected override VisualElement CreateFieldElement()
        {
            TagsField = new ChipListField(CommonTags, Constants.TagsText);
            TagsField.ChipAdded += OnTagAdded;
            TagsField.ChipRemoved += OnTagRemoved;
            TagsField.AddToClassList(UssStyle.MultiAssetDetailsPageEntryRow);
            TagsField.AddToClassList(UssStyle.MultiAssetDetailsPageChipField);
            return TagsField;
        }

        public override void UpdateField(IEnumerable<BaseAssetData> assetDataSelection)
        {
            Selection = assetDataSelection;
            CommonTags = GetCommonTags(assetDataSelection, out var hasMixedValueTags);
            TagsField.UpdateChips(CommonTags, hasMixedValueTags);

            if (AreTagsEdited())
            {
                m_Borderline.style.backgroundColor = UssStyle.EditedBorderColor;
                TagsField.style.unityFontStyleAndWeight = FontStyle.Bold;
            }
            else
            {
                m_Borderline.style.backgroundColor = Color.clear;
                TagsField.style.unityFontStyleAndWeight = FontStyle.Normal;
            }
        }

        bool AreTagsEdited()
        {
            foreach (var asset in Selection)
            {
                var assetId = (asset as UploadAssetData)?.ExistingAssetIdentifier?.AssetId ?? asset.Identifier.AssetId;
                var importedAssetData = m_GetImportedAssetInfo?.Invoke(assetId);
                var tagsCollection = asset.Tags ?? Enumerable.Empty<string>();
                var importedTags = importedAssetData?.AssetData?.Tags ?? Enumerable.Empty<string>();

                if (!importedTags.SequenceEqual(tagsCollection))
                    return true;
            }
            return false;
        }

        public override void Enable()
        {
            TagsField.SetEnabled(true);
        }

        public override void Disable()
        {
            TagsField.UpdateChips(Enumerable.Empty<string>());
            TagsField.SetEnabled(false);
        }

        // TODO: Is this scalable for large asset selections?
        static HashSet<string> GetCommonTags(IEnumerable<BaseAssetData> assets, out bool hasMixedValueTags)
        {
            var tagSets = assets.Select(a => a.Tags ?? new List<string>()).ToList();
            if (!tagSets.Any())
            {
                hasMixedValueTags = false;
                return new HashSet<string>();
            }

            var commonTags = tagSets[0].ToList();
            foreach (var tags in tagSets.Skip(1))
            {
                commonTags = commonTags.Intersect(tags).ToList();
            }

            hasMixedValueTags = tagSets.Any(tags => tags.Except(commonTags).Any());
            return commonTags.ToHashSet();
        }

        void OnTagAdded(string tag)
        {
            var edits = new List<AssetFieldEdit>();
            foreach (var asset in Selection.OfType<UploadAssetData>())
            {
                var assetTags = asset.Tags?.ToList() ?? new List<string>();
                assetTags.Add(tag);

                edits.Add(new AssetFieldEdit(asset.Identifier, EditField.Tags, assetTags));
            }

            m_FieldEdited?.Invoke(edits);
        }

        void OnTagRemoved(string tag)
        {
            var edits = new List<AssetFieldEdit>();
            foreach (var asset in Selection.OfType<UploadAssetData>())
            {
                var assetTags = asset.Tags?.ToList() ?? new List<string>();
                assetTags.Remove(tag);

                edits.Add(new AssetFieldEdit(asset.Identifier, EditField.Tags, assetTags));
            }

            m_FieldEdited?.Invoke(edits);
        }
    }
}
