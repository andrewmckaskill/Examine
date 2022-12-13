using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Examine.Lucene.Search
{
    public class LuceneSearchResults : ISearchResults, IFacetResults
    {
        public static LuceneSearchResults Empty { get; } = new LuceneSearchResults(Array.Empty<ISearchResult>(), 0, new ReadOnlyDictionary<string, IFacetResult>(new Dictionary<string, IFacetResult>()));

        private readonly IReadOnlyCollection<ISearchResult> _results;

        public LuceneSearchResults(IReadOnlyCollection<ISearchResult> results, int totalItemCount, IReadOnlyDictionary<string, IFacetResult> facets)
        {
            _results = results;
            TotalItemCount = totalItemCount;
            Facets = facets;
        }

        public long TotalItemCount { get; }

        public IReadOnlyDictionary<string, IFacetResult> Facets { get; }

        public IEnumerator<ISearchResult> GetEnumerator() => _results.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
