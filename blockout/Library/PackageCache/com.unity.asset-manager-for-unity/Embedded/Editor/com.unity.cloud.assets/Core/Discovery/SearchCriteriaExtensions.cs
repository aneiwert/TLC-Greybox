using System;

namespace Unity.Cloud.AssetsEmbedded
{
    static class SearchCriteriaExtensions
    {
        internal static string BuildSearchKey(this string searchKey, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return string.IsNullOrEmpty(searchKey) ? "" : $"{searchKey}";
            }

            return string.IsNullOrEmpty(searchKey) ? $"{prefix}" : $"{prefix}.{searchKey}";
        }

        internal static SearchRequestFilter From(this IAssetSearchFilter assetSearchFilter)
        {
            assetSearchFilter ??= new AssetSearchFilter();

            var anyQuery = assetSearchFilter.AccumulateAnyCriteria();

            return new SearchRequestFilter(assetSearchFilter.AccumulateIncludedCriteria(),
                assetSearchFilter.AccumulateExcludedCriteria(),
                anyQuery.criteria,
                anyQuery.criteria is {Count: > 0} ? anyQuery.minimumMatches : null,
                assetSearchFilter.Collections.GetValue());
        }
    }
}
