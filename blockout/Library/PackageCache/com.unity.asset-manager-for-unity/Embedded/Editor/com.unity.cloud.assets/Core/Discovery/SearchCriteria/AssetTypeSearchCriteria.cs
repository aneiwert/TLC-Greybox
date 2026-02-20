using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// A simple Asset type search but wrapped in a type.
    /// </summary>
    sealed class AssetTypeSearchCriteria : SearchCriteria<string>
    {
        /// <summary>
        /// The search key for the AssetType.
        /// </summary>
        public new static string SearchKey => "primaryType";
        
        ISearchValue m_IncludedPartial;

        internal AssetTypeSearchCriteria(string propertyName)
            : base(propertyName, SearchKey) { }

        /// <inheritdoc/>
        internal override void Include(Dictionary<string, object> includedValues, string prefix = "")
        {
            if (m_IncludedPartial != null)
            {
                includedValues.Add(SearchKey.BuildSearchKey(prefix), m_IncludedPartial);
                return;
            }

            base.Include(includedValues, prefix);
        }
        
        /// <inheritdoc/>
        public override void Clear()
        {
            base.Clear();

            m_IncludedPartial = null;
        }

        /// <summary>
        /// Sets the value of the <see cref="AssetType"/> criteria.
        /// </summary>
        /// <param name="assetType">The asset type to match. </param>
        public void WithValue(AssetType assetType)
        {
            WithValue(new StringPredicate(assetType.GetValueAsString(), StringSearchOption.ExactMatch));
        }

        /// <summary>
        /// Sets the value of the <see cref="AssetType"/> criteria.
        /// </summary>
        /// <param name="assetTypes">The asset types to match. </param>
        public void WithValue(params AssetType[] assetTypes)
        {
            if (assetTypes.Length == 0) return;
            
            var stringPredicates = assetTypes.Select(x => new StringPredicate(x.GetValueAsString(), StringSearchOption.ExactMatch)).ToArray();
            var combinedPredicate = stringPredicates[0];
            for (var i = 1; i < stringPredicates.Length; i++)
            {
                combinedPredicate = combinedPredicate.Or(stringPredicates[i]);
            }
            
            WithValue(combinedPredicate);
        }

        /// <inheritdoc />
        /// <param name="value">The expected value of the field.</param>
        public override void WithValue(string value)
        {
            if (StringSearchCriteria.k_WildcardChars.Any(value.Contains))
            {
                m_IncludedPartial = SearchStringValue.BuildWildcardQuery(value);
                m_Included = null;
                return;
            }

            base.WithValue(value);
            m_IncludedPartial = null;
        }

        /// <summary>
        /// Sets the pattern of the string search term.
        /// </summary>
        /// <param name="pattern">The string pattern to match. </param>
        public void WithValue(Regex pattern)
        {
            m_IncludedPartial = SearchStringValue.BuildRegexQuery(pattern);
            m_Included = null;
        }

        /// <summary>
        /// Sets the predicate criteria for the string search term.
        /// </summary>
        /// <param name="stringPredicate">The string predicate to match.</param>
        public void WithValue(StringPredicate stringPredicate)
        {
            m_IncludedPartial = stringPredicate.GetSearchValue();
            m_Included = null;
        }
    }
}
