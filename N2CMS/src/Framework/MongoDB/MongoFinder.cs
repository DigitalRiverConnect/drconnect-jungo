using N2.Engine;
using N2.Persistence.Finder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace N2.Persistence.MongoDB
{
	[Service(typeof(IItemFinder),
		Configuration = "mongo",
		Replaces = typeof(N2.Persistence.NH.Finder.ItemFinder))]
	public class MongoFinder : IItemFinder
	{
        private MongoContentItemRepository _repository;

        public MongoFinder(IContentItemRepository repository)
        {
            _repository = repository as MongoContentItemRepository; // use cast to avoid direct dependency
        }

		public IQueryBuilder Where
		{
			get { throw new NotImplementedException(); }
		}

		public IQueryEnding All
		{
			get { throw new NotImplementedException(); }
		}

	    public IEnumerable<T> AllOfType<T>() where T : ContentItem
	    {
	        return _repository.FindDescendants(null, typeof(T).Name).Cast<T>();
	    }
	}
}
