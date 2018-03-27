﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Analysis;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Providers
{
    ///<summary>
    /// A provider that allows for searching across multiple indexes
    ///</summary>
    public class MultiIndexSearcher : BaseLuceneSearcher, IDisposable
    {

        #region Constructors

		/// <summary>
		/// Constructor used for config providers
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
        public MultiIndexSearcher()
		{
            _disposer = new DisposableSearcher(this);
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        /// <param name="analyzer"></param>		
		public MultiIndexSearcher(IEnumerable<DirectoryInfo> indexPath, Analyzer analyzer)
            : base(analyzer)
        {
            _disposer = new DisposableSearcher(this);
	        var searchers = new List<LuceneSearcher>();
			//NOTE: DO NOT convert to Linq like this used to be as this breaks security level 2 code because of something Linq is doing.
			foreach (var ip in indexPath)
			{
				searchers.Add(new LuceneSearcher(ip, DefaultLuceneAnalyzer));
			}
	        Searchers = searchers;
        }

		/// <summary>
		/// Constructor to allow for creating an indexer at runtime
		/// </summary>
		/// <param name="luceneDirs"></param>
		/// <param name="analyzer"></param>
		public MultiIndexSearcher(IEnumerable<Lucene.Net.Store.Directory> luceneDirs, Analyzer analyzer)
			: base(analyzer)
		{
            _disposer = new DisposableSearcher(this);
			var searchers = new List<LuceneSearcher>();
			//NOTE: DO NOT convert to Linq like this used to be as this breaks security level 2 code because of something Linq is doing.
			foreach (var luceneDirectory in luceneDirs)
			{
				searchers.Add(new LuceneSearcher(luceneDirectory, DefaultLuceneAnalyzer));
			}
			Searchers = searchers;
		}

		#endregion

        ///<summary>
        /// The underlying LuceneSearchers that will be searched across
        ///</summary>
        public IEnumerable<LuceneSearcher> Searchers { get; protected set; }
        
        /// <summary>
        /// Returns a list of fields to search on based on all distinct fields found in the sub searchers
        /// </summary>
        /// <returns></returns>
        protected internal override string[] GetSearchFields()
        {
            var searchableFields = new List<string>();
            foreach (var searcher in Searchers)
            {
                searchableFields.AddRange(searcher.GetSearchFields());
            }
            return searchableFields.Distinct().ToArray();
        }

        /// <summary>
        /// Gets the searcher for this instance
        /// </summary>
        /// <returns></returns>
		
        public override Searcher GetLuceneSearcher()
        {
	        var searchables = new List<Searchable>();
			//NOTE: Do not convert this to Linq as it will fail the Code Analysis because Linq screws with it.
			foreach(var s in Searchers)
			{
			    var searcher = s.GetLuceneSearcher();
                if (searcher != null)
                    searchables.Add(searcher);
			}
			return new MultiSearcher(searchables.ToArray());
        }

        public override ICriteriaContext GetCriteriaContext()
        {
            return new MultiCriteriaContext(GetLuceneSearcher(), Searchers.Select(s => s.GetCriteriaContext()).ToArray());
        }


        #region IDisposable Members

        private readonly DisposableSearcher _disposer;

        private class DisposableSearcher : DisposableObjectSlim
        {
            private readonly MultiIndexSearcher _searcher;

            public DisposableSearcher(MultiIndexSearcher searcher)
            {
                _searcher = searcher;
            }

            /// <summary>
            /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
            /// </summary>
            
            protected override void DisposeResources()
            {
                foreach (var searcher in _searcher.Searchers)
                {
                    searcher.Dispose();
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _disposer.Dispose();
        }

        #endregion
    }
}
