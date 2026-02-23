using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// A class that builds and executes a query to return a set of field definitions.
    /// </summary>
    class FieldDefinitionQueryBuilder
    {
        readonly IAssetDataSource m_AssetDataSource;
        readonly AssetRepositoryCacheConfiguration m_DefaultCacheConfiguration;
        readonly OrganizationId m_OrganizationId = OrganizationId.None;
        readonly AssetLibraryId m_AssetLibraryId = AssetLibraryId.None;
        readonly IEnumerable<string> m_FieldDefinitionKeys;

        FieldDefinitionCacheConfiguration? m_FieldDefinitionCacheConfiguration;
        FieldDefinitionSearchFilter m_SearchFilter;
        Range m_Range = Range.All;
        SortingOrder m_SortingOrder = SortingOrder.Ascending;

        internal FieldDefinitionQueryBuilder(IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration, OrganizationId organizationId)
        {
            m_AssetDataSource = assetDataSource;
            m_DefaultCacheConfiguration = defaultCacheConfiguration;
            m_OrganizationId = organizationId;
        }

        internal FieldDefinitionQueryBuilder(IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration, AssetLibraryId assetLibraryId, IEnumerable<string> fieldDefinitionKeys)
        {
            m_AssetDataSource = assetDataSource;
            m_DefaultCacheConfiguration = defaultCacheConfiguration;
            m_AssetLibraryId = assetLibraryId;

            if (fieldDefinitionKeys == null || !fieldDefinitionKeys.Any())
            {
                throw new ArgumentException("Field definition keys cannot be null or empty.", nameof(fieldDefinitionKeys));
            }

            m_FieldDefinitionKeys = fieldDefinitionKeys;
        }

        /// <summary>
        /// Sets an override to the default cache configuration for the query.
        /// </summary>
        /// <param name="fieldDefinitionCacheConfiguration">The configuration to apply when populating the field definitions. </param>
        /// <returns>The calling <see cref="FieldDefinitionQueryBuilder"/>. </returns>
        public FieldDefinitionQueryBuilder WithCacheConfiguration(FieldDefinitionCacheConfiguration fieldDefinitionCacheConfiguration)
        {
            m_FieldDefinitionCacheConfiguration = fieldDefinitionCacheConfiguration;
            return this;
        }

        /// <summary>
        /// Sets the filter to use for the query.
        /// </summary>
        /// <param name="searchFilter">The search criteria. </param>
        /// <returns>The calling <see cref="FieldDefinitionQueryBuilder"/>. </returns>
        public FieldDefinitionQueryBuilder SelectWhereMatchesFilter(FieldDefinitionSearchFilter searchFilter)
        {
            m_SearchFilter = searchFilter;
            return this;
        }

        /// <summary>
        /// Sets the order in which the results will be returned.
        /// </summary>
        /// <param name="sortingOrder">The sorting order (Ascending|Descending). </param>
        /// <returns>The calling <see cref="FieldDefinitionQueryBuilder"/>. </returns>
        public FieldDefinitionQueryBuilder OrderByName(SortingOrder sortingOrder)
        {
            m_SortingOrder = sortingOrder;
            return this;
        }

        /// <summary>
        /// Sets the range of results to return.
        /// </summary>
        /// <param name="range">The range of results to return. </param>
        /// <returns>The calling <see cref="FieldDefinitionQueryBuilder"/>. </returns>
        public FieldDefinitionQueryBuilder LimitTo(Range range)
        {
            m_Range = range;
            return this;
        }

        /// <summary>
        /// Executes the query and returns the results.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request.</param>
        /// <returns>An async enumeration of <see cref="IFieldDefinition"/>. </returns>
        public async IAsyncEnumerable<IFieldDefinition> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var pagination = new PaginationData
            {
                Range = m_Range,
                SortingOrder = m_SortingOrder
            };

            m_SearchFilter ??= new FieldDefinitionSearchFilter();

            var queryParameters = new Dictionary<string, string[]>
            {
                {"IncludeDeleted", new[] {m_SearchFilter.Deleted.GetValue().ToString()}}
            };
            var fieldOrigin = m_SearchFilter.FieldOrigin.GetValue();
            if (fieldOrigin.HasValue)
            {
                queryParameters.Add("FieldOrigin", new[] {fieldOrigin.ToString()});
            }

            if (m_AssetLibraryId.IsPathToAssetLibraryValid())
            {
                queryParameters.Add("name", m_FieldDefinitionKeys.ToArray());
                var enumerator = m_AssetDataSource.ListFieldDefinitionsAsync(m_AssetLibraryId, pagination, queryParameters, cancellationToken);
                await foreach (var fieldDefinitionData in enumerator)
                {
                    yield return fieldDefinitionData.From(m_AssetDataSource, m_DefaultCacheConfiguration, m_AssetLibraryId, m_FieldDefinitionCacheConfiguration);
                }
            }
            else
            {
                var enumerator = m_AssetDataSource.ListFieldDefinitionsAsync(m_OrganizationId, pagination, queryParameters, cancellationToken);
                await foreach (var fieldDefinitionData in enumerator)
                {
                    yield return fieldDefinitionData.From(m_AssetDataSource, m_DefaultCacheConfiguration, m_OrganizationId, m_FieldDefinitionCacheConfiguration);
                }
            }
        }
    }
}
