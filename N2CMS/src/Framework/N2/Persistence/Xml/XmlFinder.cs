using System.Collections.Generic;
using N2.Engine;
using N2.Persistence.Finder;
using System;
using System.Linq;

namespace N2.Persistence.Xml
{
    [Service(typeof(IItemFinder), Configuration = "xml", Replaces = typeof(NH.Finder.ItemFinder))]
    public class XmlFinder : IItemFinder
    {
        private XmlContentItemRepository _repository;

        public XmlFinder(IContentItemRepository repository)
        {
            _repository = repository as XmlContentItemRepository; // use cast to avoid direct dependency
        }

        public IQueryBuilder Where
        {
            get { throw new NotImplementedException(); }
        }

        public IQueryEnding All
        {
            get { throw new NotImplementedException(); }
        }

        IEnumerable<T> IItemFinder.AllOfType<T>()
        {
            if (_repository != null)
                return _repository.AllOfType<T>().Where(t => t != null).ToArray();
            
            throw new NotImplementedException();
        }
    }
}
