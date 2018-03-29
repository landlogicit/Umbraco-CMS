﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
    using Examine = global::Examine;

    /// <summary>
    /// A class used to query for published content, media items
    /// </summary>
    public class PublishedContentQuery : IPublishedContentQuery
    {
        private readonly IPublishedContentQuery _query;
        private readonly IPublishedContentCache _contentCache;
        private readonly IPublishedMediaCache _mediaCache;

        /// <summary>
        /// Constructor used to return results from the caches
        /// </summary>
        /// <param name="contentCache"></param>
        /// <param name="mediaCache"></param>
        public PublishedContentQuery(IPublishedContentCache contentCache, IPublishedMediaCache mediaCache)
        {
            _contentCache = contentCache ?? throw new ArgumentNullException(nameof(contentCache));
            _mediaCache = mediaCache ?? throw new ArgumentNullException(nameof(mediaCache));
        }

        /// <summary>
        /// Constructor used to wrap the ITypedPublishedContentQuery object passed in
        /// </summary>
        /// <param name="query"></param>
        public PublishedContentQuery(IPublishedContentQuery query)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
        }

        #region Content

        public IPublishedContent Content(int id)
        {
            return _query == null
                ? ItemById(id, _contentCache)
                : _query.Content(id);
        }

        public IPublishedContent Content(Guid id)
        {
            return _query == null
                ? ItemById(id, _contentCache)
                : _query.Content(id);
        }

        public IPublishedContent Content(Udi id)
        {
            if (!(id is GuidUdi udi)) return null;
            return _query == null
                ? ItemById(udi.Guid, _contentCache)
                : _query.Content(udi.Guid);
        }

        public IPublishedContent ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return _query == null
                ? ItemByXPath(xpath, vars, _contentCache)
                : _query.ContentSingleAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids)
        {
            return _query == null
                ? ItemsByIds(_contentCache, ids)
                : _query.Content(ids);
        }

        public IEnumerable<IPublishedContent> Content(IEnumerable<Guid> ids)
        {
            return _query == null
                ? ItemsByIds(_contentCache, ids)
                : _query.Content(ids);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return _query == null
                ? ItemsByXPath(xpath, vars, _contentCache)
                : _query.ContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return _query == null
                ? ItemsByXPath(xpath, vars, _contentCache)
                : _query.ContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> ContentAtRoot()
        {
            return _query == null
                ? ItemsAtRoot(_contentCache)
                : _query.ContentAtRoot();
        }

        #endregion

        #region Media

        public IPublishedContent Media(int id)
        {
            return _query == null
                ? ItemById(id, _mediaCache)
                : _query.Media(id);
        }

        public IPublishedContent Media(Guid id)
        {
            return _query == null
                ? ItemById(id, _mediaCache)
                : _query.Media(id);
        }

        public IPublishedContent Media(Udi id)
        {
            if (!(id is GuidUdi udi)) return null;
            return _query == null
                ? ItemById(udi.Guid, _mediaCache)
                : _query.Media(udi.Guid);
        }

        public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids)
        {
            return _query == null
                ? ItemsByIds(_mediaCache, ids)
                : _query.Media(ids);
        }

        public IEnumerable<IPublishedContent> Media(IEnumerable<Guid> ids)
        {
            return _query == null
                ? ItemsByIds(_mediaCache, ids)
                : _query.Media(ids);
        }

        public IEnumerable<IPublishedContent> MediaAtRoot()
        {
            return _query == null
                ? ItemsAtRoot(_mediaCache)
                : _query.MediaAtRoot();
        }


        #endregion

        #region Used by Content/Media

        private static IPublishedContent ItemById(int id, IPublishedCache cache)
        {
            var doc = cache.GetById(id);
            return doc;
        }

        private static IPublishedContent ItemById(Guid id, IPublishedCache cache)
        {
            var doc = cache.GetById(id);
            return doc;
        }

        private static IPublishedContent ItemByXPath(string xpath, XPathVariable[] vars, IPublishedCache cache)
        {
            var doc = cache.GetSingleByXPath(xpath, vars);
            return doc;
        }

        //NOTE: Not used?
        //private IPublishedContent ItemByXPath(XPathExpression xpath, XPathVariable[] vars, IPublishedCache cache)
        //{
        //    var doc = cache.GetSingleByXPath(xpath, vars);
        //    return doc;
        //}

        private static IEnumerable<IPublishedContent> ItemsByIds(IPublishedCache cache, IEnumerable<int> ids)
        {
            return ids.Select(eachId => ItemById(eachId, cache)).WhereNotNull();
        }

        private IEnumerable<IPublishedContent> ItemsByIds(IPublishedCache cache, IEnumerable<Guid> ids)
        {
            return ids.Select(eachId => ItemById(eachId, cache)).WhereNotNull();
        }

        private static IEnumerable<IPublishedContent> ItemsByXPath(string xpath, XPathVariable[] vars, IPublishedCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private static IEnumerable<IPublishedContent> ItemsByXPath(XPathExpression xpath, XPathVariable[] vars, IPublishedCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private static IEnumerable<IPublishedContent> ItemsAtRoot(IPublishedCache cache)
        {
            return cache.GetAtRoot();
        }

        #endregion

        #region Search

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(string term, bool useWildCards = true, string indexName = null)
        {
            return Search(0, 0, out _, term, useWildCards, searchProvider);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(int skip, int take, out int totalRecords, string term, bool useWildCards = true, string searchProvider = null)
        {
            if (_query != null) return _query.Search(skip, take, out totalRecords, term, useWildCards, searchProvider);

            var searcher = string.IsNullOrWhiteSpace(searchProvider)
                ? Examine.ExamineManager.Instance.DefaultSearchProvider
                : Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];
            
            if (skip == 0 && take == 0)
            {
                var results = searcher.Search(term, useWildCards);
                totalRecords = results.TotalItemCount;
                return results.ToPublishedSearchResults(_contentCache);
            }

            if (!(searcher is BaseLuceneSearcher luceneSearcher))
            {
                var results = searcher.Search(term, useWildCards);
                totalRecords = results.TotalItemCount;
                // Examine skip, Linq take
                return results.Skip(skip).ToPublishedSearchResults(_contentCache).Take(take);
            }

            var criteria = SearchAllFields(term, useWildCards, luceneSearcher);
            return Search(skip, take, out totalRecords, criteria, searcher);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            return Search(0, 0, out _, criteria, searchProvider);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(int skip, int take, out int totalRecords, Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            if (_query != null) return _query.Search(skip, take, out totalRecords, criteria, searchProvider);

            var searcher = searchProvider ?? Examine.ExamineManager.Instance.DefaultSearchProvider;

            var results = skip == 0 && take == 0
                ? searcher.Search(criteria)
                : searcher.Search(criteria, maxResults: skip + take);

            totalRecords = results.TotalItemCount;
            return results.ToPublishedSearchResults(_contentCache);
        }

        /// <summary>
        /// Creates an ISearchCriteria for searching all fields in a <see cref="BaseLuceneSearcher"/>.
        /// </summary>
        /// <remarks>
        /// This is here because some of this stuff is internal in Examine.
        /// </remarks>
        private ISearchCriteria SearchAllFields(string searchText, bool useWildcards, BaseLuceneSearcher searcher)
        {
            var sc = searcher.CreateSearchCriteria();

            if (_examineGetSearchFields == null)
            {
                //get the GetSearchFields method from BaseLuceneSearcher
                _examineGetSearchFields = typeof(BaseLuceneSearcher).GetMethod("GetSearchFields", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }

            //get the results of searcher.BaseLuceneSearcher() using ugly reflection since it's not public
            var searchFields = (IEnumerable<string>) _examineGetSearchFields.Invoke(searcher, null);

            //this is what Examine does internally to create ISearchCriteria for searching all fields
            var strArray = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            sc = useWildcards == false
                ? sc.GroupedOr(searchFields, strArray).Compile()
                : sc.GroupedOr(searchFields, strArray.Select(x => new CustomExamineValue(Examineness.ComplexWildcard, x.MultipleCharacterWildcard().Value)).ToArray<IExamineValue>()).Compile();
            return sc;
        }

        private static MethodInfo _examineGetSearchFields;

        //support class since Examine doesn't expose it's own ExamineValue class publicly
        private class CustomExamineValue : IExamineValue
        {
            public CustomExamineValue(Examineness vagueness, string value)
            {
                this.Examineness = vagueness;
                this.Value = value;
                this.Level = 1f;
            }
            public Examineness Examineness { get; private set; }
            public string Value { get; private set; }
            public float Level { get; private set; }
        }

        #endregion
    }
}
